using backendSMT.Interfaces;
using backendSMT.Models;
using backendSMT.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace backendSMT.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeeController(IEmployeeService service)
        {
            _service = service;
        }

        // GET api/employee/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            // Ambil userId dari JWT claims
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

            if (userId == 0)
                return Unauthorized();

            var dashboard = await _service.GetDashboardAsync(userId);
            return Ok(dashboard);
        }


        // GET api/employee/daily-summary
        [HttpGet("daily-summary")]
        public async Task<ActionResult<DailySummaryDto>> GetDailySummary()
        {
            // Ambil userId dari token JWT (claims)
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

            if (userId == 0) return Unauthorized();

            var summary = await _service.GetDailySummaryAsync(userId);
            return Ok(summary);
        }


        // GET api/employee/product
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _service.GetProductsAsync();
            return Ok(products);
        }


        // GET api/employee/worklogs
        [HttpGet]
        public async Task<ActionResult<List<WorkLogDto>>> GetWorkLogs()
        {
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var result = await _service.GetWorkLogsAsync(userId);
            return Ok(result);
        }


        // POST api/employee/CreateWorkLog
        [HttpPost]
        public async Task<ActionResult<int>> CreateWorkLog(CreateWorkLogDto workLog)
        {
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            workLog.UserId = userId;

            int newId = await _service.CreateWorkLogAsync(workLog);
            return Ok(newId);
        }


        // PUT api/employee/UpdateWorkLog
        [HttpPut]
        public async Task<ActionResult> UpdateWorkLog(UpdateWorkLogDto workLog)
        {
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            // overwrite userId biar pasti benar
            workLog.UserId = userId;

            bool updated = await _service.UpdateWorkLogAsync(workLog);
            if (!updated) return NotFound();
            return NoContent();
        }


        // DETELE api/employee/DeleteWorkLog
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkLog(int id)
        {
            bool deleted = await _service.DeleteWorkLogAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }


        // GET api/employee/WorkLogChart
        //[HttpGet("worklog/chart")]
        //public async Task<IActionResult> GetWorkLogChart([FromQuery] int userId, [FromQuery] string startDate, [FromQuery] string endDate)
        //{
        //    if (!DateTime.TryParseExact(startDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var start))
        //        return BadRequest("Invalid startDate format, use dd-MM-yyyy");

        //    if (!DateTime.TryParseExact(endDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var end))
        //        return BadRequest("Invalid endDate format, use dd-MM-yyyy");

        //    var result = await _service.GetWorkLogDataForChartAsync(userId, start, end);
        //    return Ok(result);
        //}


        // GET api/employee/worklog/chart (tanpa userId, ambil dari token)

        [HttpGet("worklog/chart")]
        public async Task<IActionResult> GetWorkLogChart([FromQuery] string startDate, [FromQuery] string endDate)
        {
            int userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            if (!DateTime.TryParseExact(startDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var start))
                return BadRequest("Invalid startDate format, use dd-MM-yyyy");

            if (!DateTime.TryParseExact(endDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var end))
                return BadRequest("Invalid endDate format, use dd-MM-yyyy");

            var result = await _service.GetWorkLogDataForChartAsync(userId, start, end);
            return Ok(result);
        }


        // POST api/employee/update-totaltime
        [HttpPost("update-totaltime")]
        public async Task<IActionResult> UpdateTotalTime([FromBody] UpdateTotalTimeDto dto)
        {
            if (dto == null || dto.WorkLogId <= 0 || dto.ElapsedSeconds <= 0)
                return BadRequest("Invalid request");

            await _service.UpdateTotalTimeAsync(dto.WorkLogId, dto.ElapsedSeconds);
            return Ok(new { message = "TotalTime updated successfully" });
        }

    }
}
