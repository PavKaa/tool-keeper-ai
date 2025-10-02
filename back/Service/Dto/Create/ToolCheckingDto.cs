using System.ComponentModel.DataAnnotations;

namespace Service.Dto.Create
{
    public class ToolCheckingDto
    {
        [Required]
        public string ToolSerialNumber { get; set; }

        public double ModelPrediction { get; set; }
    }
}
