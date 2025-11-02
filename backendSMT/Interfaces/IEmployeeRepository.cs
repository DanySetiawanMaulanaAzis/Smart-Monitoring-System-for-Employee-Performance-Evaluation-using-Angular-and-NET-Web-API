using backendSMT.Models;

namespace backendSMT.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<DashboardDto> GetDashboardDataAsync(int userId, DateTime workDate);

        Task<DailySummaryDto> GetDailySummaryAsync(int userId);
        
        Task UpdateOrInsertDailySummaryAsync(int userId);

        Task<List<ProductDto>> GetProductsAsync();

        Task<List<WorkLogDto>> GetWorkLogsAsync(int userId);

        Task<int> CreateWorkLogAsync(CreateWorkLogDto workLog);

        Task<bool> UpdateWorkLogAsync(UpdateWorkLogDto workLog);

        Task<bool> DeleteWorkLogAsync(int workLogId);

        Task<IEnumerable<WorkLogChartDto>> GetWorkLogDataForChartAsync(int userId, DateTime startDate, DateTime endDate);

        Task UpdateTotalTimeAsync(int workLogId, int elapsedSeconds);

    }
}
