using System.ComponentModel.DataAnnotations;

namespace Service.Dto.Create
{
    public class ToolDto
    {
        public string BatchNumber { get; set; }

        public string SerialNumber { get; set; }

        public string Marking { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

        public long ToolKitId { get; set; }
    }
}
