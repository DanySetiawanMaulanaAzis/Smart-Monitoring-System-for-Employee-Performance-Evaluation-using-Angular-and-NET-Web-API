namespace backendSMT.Models
{
    public class WorkLogDto
    {
        public int WorkLogId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } // nullable karena mungkin belum selesai
        public int TotalTime { get; set; } // dalam detik
        public string StatusName { get; set; } // On Progress / Completed
    }
}
