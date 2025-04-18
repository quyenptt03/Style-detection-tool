namespace StyleDetectionTool.Models
{
    public class UsageInfo
    {
        public bool Used { get; set; } = false;
        public List<string> InUsed { get; set; } = new List<string>();
        
        public UsageInfo() { }

        public UsageInfo(bool used, List<string> inUsed)
        {
            Used = used;
            InUsed = inUsed;
        }
    }
}
