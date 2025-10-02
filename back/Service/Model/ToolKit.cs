using Swashbuckle.AspNetCore.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Service.Model
{
    public class ToolKit : BaseModel
    {
        [Required]
        public required string SerialNumber { get; set; }

        [Required]
        public required string Description { get; set; }


        [SwaggerIgnore]
        public Collection<Tool> Tools { get; set; }
    }
}
