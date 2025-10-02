using Microsoft.AspNetCore.Identity;
using Service.Model;

namespace Service.Dto.Create
{
    public class TestResult
    {
        public string FileName { get; set; }

        public string ToolKitSerialNumber { get; set; }

        public List<Tool> RecognisedTools { get; set; }
    }
}
