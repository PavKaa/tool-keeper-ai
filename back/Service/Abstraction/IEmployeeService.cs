using Service.Dto.Create;
using Service.Model;

namespace Service.Abstraction
{
	public interface IEmployeeService : IBaseEntityService<Employee, EmployeeDto>
	{

	}
}
