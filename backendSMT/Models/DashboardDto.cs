namespace backendSMT.Models
{
    public class DashboardDto
    {
        public int FinishedToday { get; set; }
        public string AverageWorkmanship { get; set; } // format hh:mm:ss
        public int TotalWorkmanship { get; set; }
        public int EfficiencyScore { get; set; }
    }
}
