using Swashbuckle.AspNetCore.Annotations;

namespace Service.Model
{
    [SwaggerSchema]
    public class ToolUsage : BaseModel
	{
        public int Quantity { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime? ReturnDate { get; set; }


        public long EmployeeId { get; set; }

        public Employee Employee { get; set; }


        public long ToolId { get; set; }

        public Tool Tool { get; set; }
    }
}
