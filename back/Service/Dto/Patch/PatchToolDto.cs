namespace Service.Dto.Patch
{
    public class PatchToolDto
    {
        public string? BatchNumber { get; set; }

        public string? SerialNumber { get; set; }

        public string? Marking { get; set; }

        public string? Description { get; set; }

        public string? Unit { get; set; }

        public long? ToolKitId { get; set; }
    }
}
