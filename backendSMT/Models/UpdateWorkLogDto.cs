namespace backendSMT.Models
{
    public class UpdateWorkLogDto
    {
        public int WorkLogId { get; set; }
        public int? TotalTime { get; set; } // opsional, kalau mau update total time
        public bool? MarkCompleted { get; set; } // true jika ingin tandai selesai
        public int UserId { get; set; }   // 🔹 tambahan
    }
}
