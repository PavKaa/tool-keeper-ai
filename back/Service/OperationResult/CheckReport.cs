using Microsoft.VisualBasic;
using System.Collections.ObjectModel;

namespace Service.OperationResult
{
    public class CheckReport
    {
        public bool SuccessfulCheck { get; set; }

        public List<Remark> Remarks { get; set; } = new List<Remark>();
    }

    public sealed record class Remark (string Message, Dictionary<string, object>? Additional);
}
