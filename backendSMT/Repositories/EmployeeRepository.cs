using backendSMT.Interfaces;
using backendSMT.Models;
using Microsoft.Data.SqlClient; // lebih direkomendasikan di .NET Core
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace backendSMT.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // Inject IConfiguration
        public EmployeeRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:5000/");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<DashboardDto> GetDashboardDataAsync(int userId, DateTime workDate)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            SqlCommand cmd = new SqlCommand(@"
                SELECT FinishedToday, AvgWorkmanship, TotalWorkmanship, EfficiencyScore
                FROM DailyUserSummary2
                WHERE UserId = @UserId AND WorkDate = @WorkDate", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@WorkDate", workDate);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            DashboardDto dto = new DashboardDto();

            if (await reader.ReadAsync())
            {
                int avgWorkmanshipSeconds = reader.GetInt32(1);
                TimeSpan avgTimeSpan = TimeSpan.FromSeconds(avgWorkmanshipSeconds);

                dto.FinishedToday = reader.GetInt32(0);
                dto.AverageWorkmanship = $"{avgTimeSpan.Hours:D2}:{avgTimeSpan.Minutes:D2}:{avgTimeSpan.Seconds:D2}";
                dto.TotalWorkmanship = reader.GetInt32(2);
                dto.EfficiencyScore = reader.GetInt32(3);
            }

            reader.Close();
            return dto;
        }




        public async Task<DailySummaryDto> GetDailySummaryAsync(int userId)
        {
            await UpdateOrInsertDailySummaryAsync(userId);

            var summary = new DailySummaryDto();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT FinishedToday, AvgWorkmanship, TotalWorkmanship, EfficiencyScore
                FROM DailyUserSummary2
                WHERE UserId = @UserId AND WorkDate = CAST(GETDATE() AS DATE)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                summary.FinishedToday = reader.GetInt32(0);
                int avgSeconds = reader.GetInt32(1);
                summary.AverageWorkmanship = TimeSpan.FromSeconds(avgSeconds).ToString(@"hh\:mm\:ss");
                summary.TotalWorkmanship = reader.GetInt32(2);
                summary.EfficiencyScore = reader.GetInt32(3);
            }

            return summary;
        }



        public async Task UpdateOrInsertDailySummaryAsync(int userId)
        {
            // Gunakan _connectionString yang sudah di-inject, jangan lagi memanggil _configuration.GetConnectionString
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Ambil pekerjaan hari ini (FinishedToday & AvgWorkmanship)
            string query = @"
        SELECT CAST(StartTime AS DATE) AS WorkDate,
               COUNT(*) AS FinishedToday,
               ISNULL(AVG(CAST(TotalTime AS INT)), 0) AS AvgWorkmanship
        FROM WorkLogNew
        WHERE UserId = @UserId AND StatusId = 2
        GROUP BY CAST(StartTime AS DATE)";

            string query2 = @"
    SELECT CAST(EndTime AS DATE) AS WorkDate,
           COUNT(*) AS FinishedToday,
           ISNULL(AVG(CAST(TotalTime AS INT)), 0) AS AvgWorkmanship
    FROM WorkLogNew
    WHERE UserId = @UserId AND StatusId = 2
          AND CAST(EndTime AS DATE) = CAST(GETDATE() AS DATE)
    GROUP BY CAST(EndTime AS DATE)";

            int finishedToday = 0;
            int avgWorkmanship = 0;
            DateTime workDate = DateTime.Today;

            using (var cmd = new SqlCommand(query2, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    workDate = reader.GetDateTime(0);
                    finishedToday = reader.GetInt32(1);
                    avgWorkmanship = reader.GetInt32(2);
                }
            }

            // 2. Hitung TotalWorkmanship kumulatif hingga hari ini
            int totalWorkmanship = 0;
            string totalQuery = @"SELECT ISNULL(SUM(FinishedToday),0) FROM DailyUserSummary2 WHERE UserId=@UserId AND WorkDate<=CAST(GETDATE() AS DATE)";
            using (var totalCmd = new SqlCommand(totalQuery, conn))
            {
                totalCmd.Parameters.AddWithValue("@UserId", userId);
                totalWorkmanship = (int)await totalCmd.ExecuteScalarAsync();
            }

            // 3. Hitung EfficiencyScore via API eksternal
            int efficiencyScore = await GetEfficiencyScoreAsync(finishedToday, avgWorkmanship, totalWorkmanship);

            // 4. Cek apakah data DailyUserSummary2 hari ini sudah ada
            string checkQuery = @"SELECT COUNT(*) FROM DailyUserSummary2 WHERE UserId=@UserId AND WorkDate=@WorkDate";
            using (var checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                checkCmd.Parameters.AddWithValue("@WorkDate", workDate);

                int count = (int)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    // Update jika sudah ada
                    string updateQuery = @"
                UPDATE DailyUserSummary2
                SET FinishedToday=@FinishedToday,
                    AvgWorkmanship=@AvgWorkmanship,
                    TotalWorkmanship=@TotalWorkmanship,
                    EfficiencyScore=@EfficiencyScore
                WHERE UserId=@UserId AND WorkDate=@WorkDate";

                    using var updateCmd = new SqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                    updateCmd.Parameters.AddWithValue("@WorkDate", workDate);
                    updateCmd.Parameters.AddWithValue("@FinishedToday", finishedToday);
                    updateCmd.Parameters.AddWithValue("@AvgWorkmanship", avgWorkmanship);
                    updateCmd.Parameters.AddWithValue("@TotalWorkmanship", totalWorkmanship);
                    updateCmd.Parameters.AddWithValue("@EfficiencyScore", efficiencyScore);

                    await updateCmd.ExecuteNonQueryAsync();
                }
                else
                {
                    // Insert jika belum ada
                    string insertQuery = @"
                INSERT INTO DailyUserSummary2
                (UserId, WorkDate, FinishedToday, AvgWorkmanship, TotalWorkmanship, EfficiencyScore)
                VALUES (@UserId,@WorkDate,@FinishedToday,@AvgWorkmanship,@TotalWorkmanship,@EfficiencyScore)";

                    using var insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@WorkDate", workDate);
                    insertCmd.Parameters.AddWithValue("@FinishedToday", finishedToday);
                    insertCmd.Parameters.AddWithValue("@AvgWorkmanship", avgWorkmanship);
                    insertCmd.Parameters.AddWithValue("@TotalWorkmanship", totalWorkmanship);
                    insertCmd.Parameters.AddWithValue("@EfficiencyScore", efficiencyScore);

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
        }



        private async Task<int> GetEfficiencyScoreAsync(int finishedToday, int avgWorkmanship, int totalWorkmanship)
        {
            var payload = new { FinishedToday = finishedToday, AvgWorkmanship = avgWorkmanship, TotalWorkmanship = totalWorkmanship };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("predict/efficiency", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseJson);
                return (int)result.EfficiencyScore;
            }
            catch
            {
                return 0;
            }
        }



        public async Task<List<ProductDto>> GetProductsAsync()
        {
            var products = new List<ProductDto>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT productId, productName FROM Products ORDER BY productName";
            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new ProductDto
                {
                    ProductId = reader.GetInt32(0),
                    ProductName = reader.GetString(1)
                });
            }

            return products;
        }



        // GET all worklogs for a user
        public async Task<List<WorkLogDto>> GetWorkLogsAsync(int userId)
        {
            var workLogs = new List<WorkLogDto>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT w.WorkLogId, w.ProductId, p.ProductName, w.StartTime, w.EndTime, w.TotalTime, s.StatusName
                FROM WorkLogNew w
                INNER JOIN Products p ON w.ProductId = p.ProductId
                INNER JOIN Statuses s ON w.StatusId = s.StatusId
                WHERE w.UserId = @UserId
                ORDER BY w.CreatedAt DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                workLogs.Add(new WorkLogDto
                {
                    WorkLogId = reader.GetInt32(0),
                    ProductId = reader.GetInt32(1),
                    ProductName = reader.GetString(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    TotalTime = reader.GetInt32(5),
                    StatusName = reader.GetString(6)
                });
            }

            return workLogs;
        }



        // CREATE new worklog
        public async Task<int> CreateWorkLogAsync(CreateWorkLogDto workLog)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                INSERT INTO WorkLogNew (UserId, ProductId, StartTime, StatusId, TotalTime)
                VALUES (@UserId, @ProductId, @StartTime, 1, 0);
                SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", workLog.UserId);
            cmd.Parameters.AddWithValue("@ProductId", workLog.ProductId);
            cmd.Parameters.AddWithValue("@StartTime", DateTime.Now);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }



        // UPDATE worklog (total time or mark completed)
        public async Task<bool> UpdateWorkLogAsync(UpdateWorkLogDto workLog)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "UPDATE WorkLogNew SET ";
            bool needComma = false;

            if (workLog.TotalTime.HasValue)
            {
                query += "TotalTime=@TotalTime";
                needComma = true;
            }

            if (workLog.MarkCompleted.HasValue && workLog.MarkCompleted.Value)
            {
                if (needComma) query += ",";
                query += "EndTime=@EndTime, StatusId=2";
            }

            query += " WHERE WorkLogId=@WorkLogId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@WorkLogId", workLog.WorkLogId);
            if (workLog.TotalTime.HasValue) cmd.Parameters.AddWithValue("@TotalTime", workLog.TotalTime.Value);
            if (workLog.MarkCompleted.HasValue && workLog.MarkCompleted.Value)
                cmd.Parameters.AddWithValue("@EndTime", DateTime.Now);

            int affected = await cmd.ExecuteNonQueryAsync();
            if (affected > 0)
            {
                // 🔹 Update summary setelah worklog berubah
                await UpdateOrInsertDailySummaryAsync(workLog.UserId);
                return true;
            }

            return false;
        }



        // DELETE worklog
        public async Task<bool> DeleteWorkLogAsync(int workLogId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "DELETE FROM WorkLogNew WHERE WorkLogId=@WorkLogId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@WorkLogId", workLogId);

            int affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }


        // Buat Charts
        public async Task<IEnumerable<WorkLogChartDto>> GetWorkLogDataForChartAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var results = new List<WorkLogChartDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    CAST(StartTime AS DATE) AS WorkDate, 
                    COUNT(*) AS TotalWork
                FROM WorkLogNew
                WHERE UserId = @UserId 
                  AND StartTime BETWEEN @StartDate AND @EndDate
                GROUP BY CAST(StartTime AS DATE)
                ORDER BY WorkDate";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        results.Add(new WorkLogChartDto
                        {
                            Label = reader.GetDateTime(0).ToString("dd-MM-yyyy"),
                            Data = reader.GetInt32(1)
                        });
                    }
                }
            }

            return results;
        }


        //untuk update total time tiap 5 detik sekali
        public async Task UpdateTotalTimeAsync(int workLogId, int elapsedSeconds)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                UPDATE WorkLogNew
                SET TotalTime = TotalTime + @ElapsedSeconds
                WHERE WorkLogId = @WorkLogId AND StatusId <> 2", conn);

            cmd.Parameters.AddWithValue("@ElapsedSeconds", elapsedSeconds);
            cmd.Parameters.AddWithValue("@WorkLogId", workLogId);

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
