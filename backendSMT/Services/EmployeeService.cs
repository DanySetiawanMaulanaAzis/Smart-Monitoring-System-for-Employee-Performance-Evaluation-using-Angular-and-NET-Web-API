using backendSMT.Interfaces;
using backendSMT.Models;
using backendSMT.Repositories;

namespace backendSMT.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardDto> GetDashboardAsync(int userId)
        {
            DateTime today = DateTime.Today;
            var dashboard = await _repo.GetDashboardDataAsync(userId, today);
            return dashboard;
        }

        public async Task<DailySummaryDto> GetDailySummaryAsync(int userId)
        {
            return await _repo.GetDailySummaryAsync(userId);
        }

        public async Task<List<ProductDto>> GetProductsAsync()
        {
            return await _repo.GetProductsAsync();
        }

        public async Task<List<WorkLogDto>> GetWorkLogsAsync(int userId)
        {
            return await _repo.GetWorkLogsAsync(userId);
        }

        public async Task<int> CreateWorkLogAsync(CreateWorkLogDto workLog)
        {
            return await _repo.CreateWorkLogAsync(workLog);
        }

        public async Task<bool> UpdateWorkLogAsync(UpdateWorkLogDto workLog)
        {
            return await _repo.UpdateWorkLogAsync(workLog);
        }

        public async Task<bool> DeleteWorkLogAsync(int workLogId)
        {
            return await _repo.DeleteWorkLogAsync(workLogId);
        }

        public async Task<IEnumerable<WorkLogChartDto>> GetWorkLogDataForChartAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _repo.GetWorkLogDataForChartAsync(userId, startDate, endDate);
        }

        public async Task UpdateTotalTimeAsync(int workLogId, int elapsedSeconds)
        {
            await _repo.UpdateTotalTimeAsync(workLogId, elapsedSeconds);
        }
    }
}
