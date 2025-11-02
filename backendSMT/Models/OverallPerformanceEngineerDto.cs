namespace backendSMT.Models
{
    public class OverallPerformanceEngineerDto
    {
        public string Username { get; set; } = string.Empty;
        public int FinishedToday { get; set; }
        public string AverageWorkmanship { get; set; } = "00:00:00"; // format HH:mm:ss
        public int TotalWorkmanship { get; set; }
        public int PredictedTomorrow { get; set; }
        public string PerformanceResult { get; set; } = "Unknown";
    }
}
