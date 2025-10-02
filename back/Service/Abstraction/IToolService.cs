using Microsoft.AspNetCore.Mvc;
using Service.Dto.Create;
using Service.Model;
using Service.OperationResult;

namespace Service.Abstraction
{
	public interface IToolService : IBaseEntityService<Tool, ToolDto>
	{
		Task<Result<IEnumerable<Tool>>> GetByToolKitIdAsync(long toolKitId);

		Task<Result> TakeToolsAsync(ToolMovementDto[] toolMovements, long employeeId);

		Task<Result> ReturnToolsAsync(ToolMovementDto[] toolMovements, long employeeId);

		Task<Result<CheckReport>> CheckToolsPresenceAsync(ToolCheckingDto[] toolCheckings, string toolKitSerialNumber);

    }
}
