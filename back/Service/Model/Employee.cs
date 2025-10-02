using Swashbuckle.AspNetCore.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Service.Model
{
	public class Employee : BaseModel
	{
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Surname { get; set; }

        public string? Patronymic { get; set; }

        [Required]
        public required string Position { get; set; }

        [Required]
        public string EmployeeUniqueNumber { get; set; }


        [SwaggerIgnore]
        public Collection<ToolUsage> ToolUsages { get; set; }
    }
}
