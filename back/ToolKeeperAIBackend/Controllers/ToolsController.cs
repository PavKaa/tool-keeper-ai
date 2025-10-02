using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service.Abstraction;
using Service.Dto.Create;
using Service.Dto.Patch;
using Service.Model;
using Service.Settings;
using System.Text.Json;
using ToolKeeperAIBackend.Extensions;

namespace ToolKeeperAIBackend.Controllers
{
	public class ToolsController : BaseDataController<Tool, ToolDto, PatchToolDto>
    {
        protected readonly IEmployeeService _employeeService;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ModelAPISettings _settings;

        public ToolsController(IToolService toolService, IEmployeeService employeeService, 
                               IMapper mapper, IHttpClientFactory httpClientFactory, ILogger<ToolsController> logger,
                               IOptions<AppSettings> options) 
        : base(toolService, mapper, logger)
        {
            _employeeService = employeeService;
            _httpClientFactory = httpClientFactory;
            _settings = options.Value.ModelAPISettings;
        }

        [HttpGet("GetByKitId/{kitId:long}")]
        public async Task<IActionResult> GetToolsByKitId([FromRoute] long kitId)
		{
            var result = await ((IToolService)_service).GetByToolKitIdAsync(kitId);

            return this.FromResult(result);
		}

        [HttpPost("CheckToolsPresence/{toolKitSerialNumber}")]
        public async Task<IActionResult> CheckToolsPresence([FromBody] ToolCheckingDto[] toolCheckings, [FromRoute] string toolKitSerialNumber)
        {
            var result = await ((IToolService)_service).CheckToolsPresenceAsync(toolCheckings, toolKitSerialNumber);

            if (result.IsSuccess)
                Console.WriteLine(JsonSerializer.Serialize(result.Data));

            return this.FromResult(result);
        }

        [HttpPost("TakeTools/{employeeId:long}")]
        public async Task<IActionResult> TakeTools([FromBody] ToolMovementDto[] toolMovements, [FromRoute] long employeeId)
        {
            var searchEmployee = await _employeeService.GetByIdAsync(employeeId);

            if (!searchEmployee.IsSuccess)
                return NotFound($"Employee with id - {employeeId} doesnt exist");

            var result = await ((IToolService)_service).TakeToolsAsync(toolMovements, employeeId);

            return this.FromResult(result);
        }

        [HttpPost("ReturnTools/{employeeId:long}")]
        public async Task<IActionResult> ReturnTools([FromBody] ToolMovementDto[] toolMovements, [FromRoute] long employeeId)
        {
            var searchEmployee = await _employeeService.GetByIdAsync(employeeId);

            if (!searchEmployee.IsSuccess)
                return NotFound($"Employee with id - {employeeId} doesnt exist");

            var result = await ((IToolService)_service).ReturnToolsAsync(toolMovements, employeeId);

            return this.FromResult(result);
        }

        [HttpPost("Test")]
        public async Task<IActionResult> TestWorkability(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("There is not uploaded file");

            if (file.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                using var ms = new MemoryStream();
                using var fileStream = file.OpenReadStream();

                await fileStream.CopyToAsync(ms);

                using HttpClient httpClient = _httpClientFactory.CreateClient(nameof(HttpClient));

                using var form = new MultipartFormDataContent();

                form.Add(new ByteArrayContent(ms.ToArray()), $"file", file.FileName);

                var response = await httpClient.PostAsync(_settings.PredictBatchImagesUrl, form);

                if (!response.IsSuccessStatusCode)
                    return BadRequest(await response.Content.ReadAsStringAsync());

                var prediction = await response.Content.ReadAsStringAsync();

                return Ok(new Dictionary<string, string> { { file.FileName, prediction } });
            }
            else
            {
                return BadRequest("File doesnt have png or jpg extension");
            }
        }

        [HttpPost("TestZip")]
        public async Task<IActionResult> TestZipWorkability(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("There is not uploaded archive file");

            var photos = new List<byte[]>();
            var photoNames = new List<string>();

            using (var stream = file.OpenReadStream())
            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        using var entryStream = entry.Open();
                        using var ms = new MemoryStream();

                        await entryStream.CopyToAsync(ms);
                        
                        photos.Add(ms.ToArray());
                        photoNames.Add(entry.FullName);
                    }
                }
            }

            var response = await SendPhotosToApi(photos, photoNames);

            if (!response.IsSuccessStatusCode)
                return BadRequest(await response.Content.ReadAsStringAsync());

            var responseJson = await response.Content.ReadAsStringAsync();

            List<string> predictions = JsonSerializer.Deserialize<List<string>>(responseJson)!;

            var result = photoNames
                .Select((name, i) => new { FileName = name, Result = predictions[i] })
                .ToDictionary(x => x.FileName, x => x.Result);

            return Ok(result);
        }

        private async Task<HttpResponseMessage> SendPhotosToApi(List<byte[]> photos, List<string> photoNames)
        {
            using var form = new MultipartFormDataContent();

            for (int i = 0; i < photos.Count; i++)
            {
                form.Add(new ByteArrayContent(photos[i]), $"files", photoNames[i]);
            }

            using HttpClient httpClient = _httpClientFactory.CreateClient(nameof(HttpClient));

            return await httpClient.PostAsync(_settings.PredictBatchImagesUrl, form);
        }
    }
}
