using InventoryBL.Interfaces;
using InventoryBL.Services;
using InventoryShared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoryManagemetRESTFUL_API.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IUserService _userService;
        ILogger<AuthController> _logger;
        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService=userService;
            _logger=logger;
        }
            
        [HttpPost("Login",Name ="Login")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var user =await _userService.getUserForLogin(request);
            if (user == null) {
                var ip=HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogWarning($"this {request.UserName} has login vails attempt ip={ip}");
                return Unauthorized();
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                 new Claim(ClaimTypes.Name,user.UserName.ToString()),
                 new Claim("Permissions",user.Permissions.ToString()),
            };
            var secretKeyValue = Environment.GetEnvironmentVariable("JWT__KEY");
            if (string.IsNullOrEmpty(secretKeyValue))
            {
                throw new Exception("JWT Secret Key is missing from environment variables");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyValue));
            var cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer:"InventoryAPI",
                audience:"InventoryAPIUsers",
                claims:claims,
                expires:DateTime.UtcNow.AddHours(4),
                signingCredentials: cred);
                return Ok(new {token=new JwtSecurityTokenHandler().WriteToken(token) });
         

        }
    }
}
