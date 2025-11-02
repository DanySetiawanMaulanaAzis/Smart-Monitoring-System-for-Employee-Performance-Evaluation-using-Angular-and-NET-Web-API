using backendSMT.Interfaces;
using backendSMT.Models;

namespace backendSMT.Services
{
    public class EngineerService : IEngineerService
    {
        private readonly IEngineerRepository _repo;

        public EngineerService(IEngineerRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<CompletedTaskEngineerDto>> GetCompletedTasksAsync(DateTime? startDate, DateTime? endDate)
        {
            return _repo.GetCompletedTasksAsync(startDate, endDate);
        }

        public Task<IEnumerable<UserPerformanceEngineerDto>> GetUserPerformanceAsync(DateTime? startDate, DateTime? endDate)
        {
            return _repo.GetUserPerformanceAsync(startDate, endDate);
        }

        public Task<IEnumerable<OverallPerformanceEngineerDto>> GetOverallPerformanceAsync()
        {
            return _repo.GetOverallPerformanceAsync();
        }
    }
}
