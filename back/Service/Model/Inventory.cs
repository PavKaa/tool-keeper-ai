using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Service.Model
{
	public class Inventory : BaseModel
	{
        [Required]
        public int TotalQuantity { get; set; }

        [Required]
        public int RemainQuantity { get; set; }

        [Required]
        public long ToolId { get; set; }

        public Tool Tool { get; set; }
    }
}
