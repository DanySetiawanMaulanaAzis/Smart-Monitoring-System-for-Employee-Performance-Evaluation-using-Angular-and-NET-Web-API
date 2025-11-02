using backendSMT.Models;

namespace backendSMT.Interfaces
{
    public interface IEmployeeService
    {
        //ini untuk dashboard yang 4 box itu
        Task<DashboardDto> GetDashboardAsync(int userId);

        
        //ini untuk daily summary yang table itu
        Task<DailySummaryDto> GetDailySummaryAsync(int userId);

        //ini untuk list product di menu dropdown nanti
        Task<List<ProductDto>> GetProductsAsync();

        Task<List<WorkLogDto>> GetWorkLogsAsync(int userId);

        Task<int> CreateWorkLogAsync(CreateWorkLogDto workLog);

        Task<bool> UpdateWorkLogAsync(UpdateWorkLogDto workLog);

        Task<bool> DeleteWorkLogAsync(int workLogId);

        Task<IEnumerable<WorkLogChartDto>> GetWorkLogDataForChartAsync(int userId, DateTime startDate, DateTime endDate);

        Task UpdateTotalTimeAsync(int workLogId, int elapsedSeconds);

    }
}
