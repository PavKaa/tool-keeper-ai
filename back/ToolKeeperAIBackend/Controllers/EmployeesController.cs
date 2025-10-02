using AutoMapper;
using Service.Abstraction;
using Service.Dto.Create;
using Service.Dto.Patch;
using Service.Model;

namespace ToolKeeperAIBackend.Controllers
{
    public class EmployeesController : BaseDataController<Employee, EmployeeDto, PatchEmployeeDto>
    {
        public EmployeesController(IEmployeeService service, IMapper mapper, ILogger<EmployeesController> logger) : base(service, mapper, logger)
        {
            
        }
    }
}
