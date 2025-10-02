using System.ComponentModel.DataAnnotations;

namespace Service.Dto.Create
{
    public class EmployeeDto
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Patronymic { get; set; }

        public string? Position { get; set; }

        public string EmployeeUniqueNumber { get; set; }
    }
}
