using backendSMT.Models;

namespace backendSMT.Interfaces
{
    public interface IEngineerService
    {
        Task<IEnumerable<CompletedTaskEngineerDto>> GetCompletedTasksAsync(DateTime? startDate, DateTime? endDate);

        Task<IEnumerable<UserPerformanceEngineerDto>> GetUserPerformanceAsync(DateTime? startDate, DateTime? endDate);

        Task<IEnumerable<OverallPerformanceEngineerDto>> GetOverallPerformanceAsync();
    }
}
