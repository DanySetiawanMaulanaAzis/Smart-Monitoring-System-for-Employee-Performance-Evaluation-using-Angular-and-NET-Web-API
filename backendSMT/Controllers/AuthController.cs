using backendSMT.Services;
using Microsoft.AspNetCore.Mvc;
using backendSMT.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace backendSMT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
                return Unauthorized("Username atau password salah");

            var token = _authService.GenerateJwtToken(user);

            // TODO: generate JWT token
            return Ok(new
            {
                user.UserId,
                user.Username,
                user.IsEmployee,
                user.IsEngineer,
                Token = token,
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var username = User.Identity?.Name ?? "Unknown"; // claim Name sudah di-set
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var isEmployee = User.Claims.FirstOrDefault(c => c.Type == "isEmployee")?.Value;
            var isEngineer = User.Claims.FirstOrDefault(c => c.Type == "isEngineer")?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username,
                Role = isEngineer == "True" ? "Engineer" : "Employee"
            });
        }

    }
}
