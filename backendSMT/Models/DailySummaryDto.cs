namespace backendSMT.Models
{
    public class DailySummaryDto
    {
        public int FinishedToday { get; set; }
        public string AverageWorkmanship { get; set; } = "00:00:00";
        public int TotalWorkmanship { get; set; }
        public int EfficiencyScore { get; set; }
    }
}
