using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Service.Abstraction;
using Service.Db;
using Service.Dto.Create;
using Service.Exceptions;
using Service.Model;
using Service.OperationResult;
using Service.Settings;

namespace Service.Implementation
{
	public class ToolService : BaseService<Tool, ToolDto>, IToolService
	{
		protected readonly ModelPrecisionSettings _modelPrecisionSettings;

		public ToolService(IMapper mapper, ToolKeeperDbContext dbContext, IOptions<AppSettings> settings) : base(dbContext, mapper)
		{
			_modelPrecisionSettings = settings.Value.ModelPrecisionSettings;
        }

        public async Task<Result<IEnumerable<Tool>>> GetByToolKitIdAsync(long toolKitId)
        {
			var toolKit = await _dbContext.ToolKits
				.Include(toolKit => toolKit.Tools)
				.FirstOrDefaultAsync(toolKit => toolKit.Id == toolKitId);

			return toolKit == null
				? Result<IEnumerable<Tool>>.Failure(FindErrors.NotFound(toolKitId, typeof(ToolKit)))
				: Result<IEnumerable<Tool>>.Success(toolKit.Tools);
        }

        public async Task<Result> TakeToolsAsync(ToolMovementDto[] toolMovements, long employeeId)
		{
			HashSet<long> toolIds = toolMovements.Select(toolMovement => toolMovement.ToolId).ToHashSet();

			var checkResult = await CheckToolsAndEmployeeExistsAsync(toolIds, employeeId);

			if (!checkResult.IsSuccess)
				return checkResult;

			var toolInventories = await _dbContext.Inventory
				.Where(inventory => toolIds.Contains(inventory.ToolId))
				.ToDictionaryAsync(inventory => inventory.ToolId);

			var newToolUsages = new List<ToolUsage>();

			foreach (var toolMovement in toolMovements)
			{
				var toolInventory = toolInventories[toolMovement.ToolId];

				if (toolInventory.RemainQuantity < toolMovement.Quantity)
					return Result.Failure(ToolMovementErrors.NotEnoughToolsInStock(toolMovement.ToolId, toolMovement.Quantity, toolInventory.RemainQuantity));

				toolInventory.RemainQuantity -= toolMovement.Quantity;

				newToolUsages.Add(new ToolUsage()
				{
					EmployeeId = employeeId,
					ToolId = toolMovement.ToolId,
					Quantity = toolMovement.Quantity,
					IssueDate = DateTime.UtcNow
				});
			}

			try
			{
				_dbContext.ToolUsages.AddRange(newToolUsages);
				var updated = await _dbContext.SaveChangesAsync();

				return updated > 0
					? Result.Success()
					: Result.Failure(EntityErrors.SaveToDbError(typeof(ToolUsage)));
			}
			catch (DbUpdateException ex)
			{
				throw new DatabaseException($"Failed to update entity: {ex.Message}", typeof(ToolUsage));
			}
		}

		public async Task<Result> ReturnToolsAsync(ToolMovementDto[] toolMovements, long employeeId)
		{
			HashSet<long> toolIds = toolMovements.Select(toolMovement => toolMovement.ToolId).ToHashSet();

			var checkResult = await CheckToolsAndEmployeeExistsAsync(toolIds, employeeId);

			if (!checkResult.IsSuccess)
				return checkResult;

			var toolInventories = await _dbContext.Inventory
				.Where(inventory => toolIds.Contains(inventory.ToolId))
				.ToDictionaryAsync(inventory => inventory.ToolId);

			var toolUsages = await _dbContext.ToolUsages
				.Where(toolUsage => toolIds.Contains(toolUsage.ToolId) &&
									toolUsage.EmployeeId == employeeId &&
									toolUsage.ReturnDate == null)
				.ToListAsync();

			var groupedToolUsages = toolUsages.GroupBy(toolUsage => toolUsage.ToolId);

			if (toolIds.Count > groupedToolUsages.Count())
			{
				var missedIds = toolIds.Except(groupedToolUsages.Select(groupedToolUsage => groupedToolUsage.Key));

				return Result.Failure(FindErrors.NotFoundCollectionOfEntities(missedIds, typeof(Tool)));
			}

			foreach (var toolUsagesGroup in groupedToolUsages)
			{
				var toolInventory = toolInventories[toolUsagesGroup.Key];

				foreach (var toolUsage in toolUsagesGroup)
				{
					toolInventory.RemainQuantity += toolUsage.Quantity;

					if (toolInventory.RemainQuantity > toolInventory.TotalQuantity)
						return Result.Failure(ToolMovementErrors.ExtraToolQuantity(toolUsagesGroup.Key));

					// TODO: handle not full return

					toolUsage.ReturnDate = DateTime.UtcNow;
                }
			}

            try
            {
                var updated = await _dbContext.SaveChangesAsync();

                return updated > 0
                    ? Result.Success()
                    : Result.Failure(EntityErrors.SaveToDbError(typeof(ToolUsage)));
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException($"Failed to update entity: {ex.Message}", typeof(ToolUsage));
            }
        }

        public async Task<Result<CheckReport>> CheckToolsPresenceAsync(ToolCheckingDto[] toolCheckings, string toolKitSerialNumber)
        {
			// TODO: add index to toolkit and tool tables
			var toolKits = await _dbContext.ToolKits
				.Where(toolKit => toolKit.SerialNumber == toolKitSerialNumber)
				.Include(toolKit => toolKit.Tools)
				.ToListAsync();

			if (toolKits.Count != 1)
				return Result<CheckReport>.Failure(FindErrors.NotFoundBySerialNumber(toolKitSerialNumber, typeof(ToolKit)));

			var checkReport = new CheckReport();
			var dbTools = toolKits[0].Tools.ToDictionary(tool => tool.SerialNumber);
			var presentTools = new List<long>();
			var unpresentTools = new List<long>();

			//TODO: handle multiple tools with same id
			var toolIdModelPredictionLink = new Dictionary<long, double>();

            foreach (var toolChecking in toolCheckings)
			{
				var toolSerialNumber = toolChecking.ToolSerialNumber;

                if (!dbTools.ContainsKey(toolSerialNumber))
				{
					checkReport.Remarks.Add(new Remark
					(
                        Message: "Tool with serial number not present in db",
						Additional: new Dictionary<string, object> { { "SerialNumber", toolChecking.ToolSerialNumber } }
					));
                }

				var toolId = dbTools[toolSerialNumber].Id;

                if (toolChecking.ModelPrediction >= _modelPrecisionSettings.ConfidenceTreshold)
					presentTools.Add(toolId);
				else
                    unpresentTools.Add(toolId);

                toolIdModelPredictionLink.Add(toolId, toolChecking.ModelPrediction);
            }

			checkReport.Remarks.AddRange(await HandlePresentToolsAsync(presentTools, toolIdModelPredictionLink));
			checkReport.Remarks.AddRange(await HandleUnpresentToolsAsync(unpresentTools, toolIdModelPredictionLink));

			checkReport.SuccessfulCheck = !(checkReport.Remarks.Count > 0);

			return Result<CheckReport>.Success(checkReport);
        }

        protected async Task<IEnumerable<Remark>> HandleUnpresentToolsAsync(List<long> unpresentTools, Dictionary<long, double> toolIdModelPredictionLink)
        {
            var remarks = new List<Remark>();

            var tools = await _dbContext.Tools
				.Where(tool => unpresentTools.Contains(tool.Id) && !tool.ToolUsages.Any())
				.ToListAsync();

            foreach (var tool in tools)
            {
                remarks.Add(new Remark
                (
                    Message: "Model recognition error - the absence of the present instrument is predicted",
                    Additional: new Dictionary<string, object>
                    {
                        { "ToolId", tool.Id },
                        { "ModelPrediction", toolIdModelPredictionLink[tool.Id] }
                    }
                ));
            }

            return remarks;
        }

        protected async Task<IEnumerable<Remark>> HandlePresentToolsAsync(List<long> presentTools, Dictionary<long, double> toolIdModelPredictionLink)
        {
			var remarks = new List<Remark>();

            var presentToolsUsages = await _dbContext.ToolUsages
                .Where(toolUsage => presentTools.Contains(toolUsage.ToolId) && toolUsage.ReturnDate == null)
                .ToListAsync();

            foreach (var presentToolsUsage in presentToolsUsages)
            {
                remarks.Add(new Remark
                (
                    Message: "Model recognition error - the presence of a taking instrument is predicted",
                    Additional: new Dictionary<string, object>
                    {
                        { "ToolId", presentToolsUsage.ToolId },
						{ "EmployeeId", presentToolsUsage.EmployeeId },
                        { "ModelPrediction", toolIdModelPredictionLink[presentToolsUsage.ToolId] },
                        { "UsageInfoId", presentToolsUsage.Id },
                        { "UsageQuantity", presentToolsUsage.Quantity },
						{ "UsageDateTime", presentToolsUsage.IssueDate }
                    }
                ));
            }

            return remarks;
        }

        protected async Task<Result> CheckToolsAndEmployeeExistsAsync(HashSet<long> toolIds, long employeeId)
		{
			var exists = await _dbContext.Employees.AnyAsync(employee => employee.Id == employeeId);

			if (!exists)
				return Result.Failure(FindErrors.NotFound(employeeId, typeof(Employee)));

			Result<IEnumerable<Tool>> findingResult = await this.GetByExpressionAsEnumerableAsync(tool => toolIds.Contains(tool.Id));

			var findingResultData = findingResult.Data;

			if (!findingResult.IsSuccess)
				return Result.Failure(FindErrors.NotFoundCollectionOfEntities(typeof(Tool)));

			// TODO: ignore tool ids that have not found (maybe). Partial update

			if (toolIds.Count > findingResultData!.Count())
			{
				var missedIds = toolIds.Except(findingResultData!.Select(tool => tool.Id));

				return Result.Failure(FindErrors.NotFoundCollectionOfEntities(missedIds, typeof(Tool)));
			}

			return Result.Success();
		}
    }
}
