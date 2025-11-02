using Microsoft.AspNetCore.Mvc;
using backendSMT.Interfaces;
using backendSMT.Services;

namespace backendSMT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class EngineerController : ControllerBase
    {
        private readonly IEngineerService _service;

        public EngineerController(IEngineerService service)
        {
            _service = service;
        }

        [HttpGet("completed-tasks")]
        public async Task<IActionResult> GetCompletedTasks([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Ambil userId dari JWT claims
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

            if (userId == 0)
                return Unauthorized();

            var data = await _service.GetCompletedTasksAsync(startDate, endDate);
            return Ok(data);
        }



        [HttpGet("user-performance")]
        public async Task<IActionResult> GetUserPerformance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Ambil userId dari JWT claims
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

            if (userId == 0)
                return Unauthorized();

            var data = await _service.GetUserPerformanceAsync(startDate, endDate);
            return Ok(data);
        }

        [HttpGet("user-performance-table")]
        public async Task<IActionResult> GetUserPerformanceTable()
        {
            // Ambil userId dari JWT claims
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

            if (userId == 0)
                return Unauthorized();

            var result = await _service.GetOverallPerformanceAsync();
            return Ok(result);
        }
    }
}
