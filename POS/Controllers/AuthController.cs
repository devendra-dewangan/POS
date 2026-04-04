using Microsoft.AspNetCore.Mvc;
using POS.Services;

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _authService;

        public AuthController(IIdentityService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto request)
        {
            var userId = await _authService.RegisterAsync(request);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Registration failed.");
            }
            return Ok(new { UserId = userId });
        }
    }
}