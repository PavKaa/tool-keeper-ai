using AutoMapper;
using Service.Abstraction;
using Service.Dto.Create;
using Service.Dto.Patch;
using Service.Model;

namespace ToolKeeperAIBackend.Controllers
{
    public class ToolKitsController : BaseDataController<ToolKit, ToolKitDto, PatchToolKitDto>
    {
        public ToolKitsController(IToolKitService service, IMapper mapper, ILogger<ToolKitsController> logger) : base(service, mapper, logger)
        {

        }
    }
}
