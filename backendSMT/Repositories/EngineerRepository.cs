using Dapper;
using Microsoft.Data.SqlClient;
using backendSMT.Interfaces;
using backendSMT.Models;
using System.Data;
using Newtonsoft.Json;

namespace backendSMT.Repositories
{
    public class EngineerRepository : IEngineerRepository
    {
        private readonly string _connectionString;
        private readonly HttpClient _httpClient;

        public EngineerRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:5000/");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<IEnumerable<CompletedTaskEngineerDto>> GetCompletedTasksAsync(DateTime? startDate, DateTime? endDate)
        {
            using var connection = new SqlConnection(_connectionString);

            string query;
            object param;

            if (startDate.HasValue && endDate.HasValue)
            {
                // Query dengan filter tanggal
                query = @"
                    SELECT CAST(EndTime AS DATE) AS WorkDate, COUNT(*) AS CompletedTasks
                    FROM WorkLogNew
                    WHERE StatusId = 2 AND CAST(EndTime AS DATE) BETWEEN @StartDate AND @EndDate
                    GROUP BY CAST(EndTime AS DATE)
                    ORDER BY WorkDate";

                param = new { StartDate = startDate, EndDate = endDate };
            }
            else
            {
                // Query tanpa filter (default 7 hari terakhir)
                query = @"
                    SELECT CAST(EndTime AS DATE) AS WorkDate, COUNT(*) AS CompletedTasks
                    FROM WorkLogNew
                    WHERE StatusId = 2 
                      AND CAST(EndTime AS DATE) >= CAST(DATEADD(DAY, -6, GETDATE()) AS DATE)
                    GROUP BY CAST(EndTime AS DATE)
                    ORDER BY WorkDate";

                param = new { };
            }

            return await connection.QueryAsync<CompletedTaskEngineerDto>(query, param);
        }

        public async Task<IEnumerable<UserPerformanceEngineerDto>> GetUserPerformanceAsync(DateTime? startDate, DateTime? endDate)
        {
            using var connection = new SqlConnection(_connectionString);

            string query;
            object param;

            if (startDate.HasValue && endDate.HasValue)
            {
                // Query dengan filter tanggal
                query = @"
                    SELECT 
                        u.Username, 
                        COUNT(w.WorkLogId) AS CompletedTasks
                    FROM Users u
                    LEFT JOIN WorkLogNew w ON u.UserId = w.UserId 
                        AND w.StatusId = 2
                        AND w.EndTime BETWEEN @StartDate AND @EndDate
                    WHERE u.is_employee = 1
                    GROUP BY u.Username
                    ORDER BY CompletedTasks DESC";

                param = new { StartDate = startDate, EndDate = endDate };
            }
            else
            {
                // Query tanpa filter
                query = @"
                    SELECT 
                        u.Username, 
                        COUNT(w.WorkLogId) AS CompletedTasks
                    FROM Users u
                    LEFT JOIN WorkLogNew w ON u.UserId = w.UserId 
                        AND w.StatusId = 2
                    WHERE u.is_employee = 1
                    GROUP BY u.Username
                    ORDER BY CompletedTasks DESC";

                param = new { };
            }

            return await connection.QueryAsync<UserPerformanceEngineerDto>(query, param);
        }


        public async Task<IEnumerable<OverallPerformanceEngineerDto>> GetOverallPerformanceAsync()
        {
            var result = new List<OverallPerformanceEngineerDto>();

            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT u.UserId, u.Username, 
                    (SELECT COUNT(*) FROM WorkLogNew WHERE UserId = u.UserId AND CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE) AND StatusId = 2) AS FinishedToday,
                    (SELECT ISNULL(AVG(TotalTime), 0) FROM WorkLogNew WHERE UserId = u.UserId AND StatusId = 2 AND CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)) AS AverageWorkmanship,
                    (SELECT COUNT(*) FROM WorkLogNew WHERE UserId = u.UserId AND StatusId = 2) AS TotalWorkmanship
                FROM Users u
                WHERE u.is_employee = 1";

            var data = await connection.QueryAsync(query);

            foreach (var row in data)
            {
                int finishedToday = row.FinishedToday ?? 0;
                int avgWorkSeconds = row.AverageWorkmanship ?? 0;
                int totalWork = row.TotalWorkmanship ?? 0;
                int userId = row.UserId;

                // Format avgWorkmanship ke HH:mm:ss
                TimeSpan avgTimeSpan = TimeSpan.FromSeconds(avgWorkSeconds);
                string avgFormatted = $"{avgTimeSpan.Hours:D2}:{avgTimeSpan.Minutes:D2}:{avgTimeSpan.Seconds:D2}";

                // Ambil sequence 7 hari terakhir untuk LSTM
                var sequence = await GetUserSequenceAsync(userId);
                int predictedTomorrow = await GetLSTMPredictionAsync(sequence);

                // Prediksi performance (Low/Medium/High)
                int prediction = await GetPerformancePredictionAsync(finishedToday, avgWorkSeconds, totalWork);
                string performanceLabel = prediction switch
                {
                    0 => "Low",
                    1 => "Medium",
                    2 => "High",
                    _ => "Unknown"
                };

                // Update ke tabel DailyUserSummary2
                await UpdateDailyUserSummaryAsync(userId, predictedTomorrow, performanceLabel);

                result.Add(new OverallPerformanceEngineerDto
                {
                    Username = row.Username,
                    FinishedToday = finishedToday,
                    AverageWorkmanship = avgFormatted,
                    TotalWorkmanship = totalWork,
                    PredictedTomorrow = predictedTomorrow,
                    PerformanceResult = performanceLabel
                });
            }

            return result;
        }


        private async Task<List<float[]>> GetUserSequenceAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                WITH Last7Days AS (
                    SELECT CAST(GETDATE() - number AS DATE) AS WorkDate
                    FROM master..spt_values
                    WHERE type = 'P' AND number < 7
                )
                SELECT 
                    d.WorkDate,
                    ISNULL(s.FinishedToday, 0) AS FinishedToday,
                    ISNULL(s.EfficiencyScore, 0) AS EfficiencyScore,
                    ISNULL(s.TotalWorkmanship, 0) AS TotalWorkmanship
                FROM Last7Days d
                LEFT JOIN DailyUserSummary2 s ON s.WorkDate = d.WorkDate AND s.UserId = @UserId
                ORDER BY d.WorkDate";

            var data = await connection.QueryAsync(query, new { UserId = userId });

            var sequence = new List<float[]>();
            foreach (var row in data)
            {
                sequence.Add(new float[]
                {
                    (float)(row.FinishedToday ?? 0),
                    (float)(row.EfficiencyScore ?? 0),
                    (float)(row.TotalWorkmanship ?? 0)
                });
            }

            sequence.Reverse(); // dari paling lama ke terbaru
            return sequence;
        }


        private async Task<int> GetPerformancePredictionAsync(int finishedToday, int avgWorkInSeconds, int totalWorkmanship)
        {
            var payload = new { FinishedToday = finishedToday, AvgWorkmanship = avgWorkInSeconds, TotalWorkmanship = totalWorkmanship };
            var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("predict/performance", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseJson);
                return (int)result.PerformanceCategory;
            }
            catch { return -1; }
        }


        private async Task<int> GetLSTMPredictionAsync(List<float[]> sequence)
        {
            var payload = new { sequence };
            var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("predict/workload", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseJson);
                return (int)Math.Round((float)result.PredictedFinishedTomorrow);
            }
            catch { return -1; }
        }


        private async Task UpdateDailyUserSummaryAsync(int userId, int predictedTomorrow, string performanceLabel)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                UPDATE DailyUserSummary2
                SET PerformanceCategory = (SELECT CategoryId FROM PerformanceCategory WHERE CategoryLabel = @PerformanceLabel),
                    PredictedTomorrow = @PredictedTomorrow
                WHERE UserId = @UserId AND WorkDate = CAST(GETDATE() AS DATE)";

            await connection.ExecuteAsync(query, new
            {
                PerformanceLabel = performanceLabel,
                PredictedTomorrow = predictedTomorrow,
                UserId = userId
            });
        }
    }
}
