using Swashbuckle.AspNetCore.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Service.Model
{
	public class Tool : BaseModel
	{
        [Required]
        public required string BatchNumber { get; set; }

        [Required]
        public required string SerialNumber { get; set; }

        [Required]
        public required string Marking { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required string Unit { get; set; }

        [Required]
        public long ToolKitId { get; set; }

        public ToolKit ToolKit { get; set; }


        [SwaggerIgnore]
        public Collection<ToolUsage> ToolUsages { get; set; }
	}
}
