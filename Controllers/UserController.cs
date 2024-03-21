using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Entities;
using WebApplication1.Models.User;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) 
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest authenticateRequest)
        {
            AuthenticateResponse authenticateResponse = _userService.Authenticate(authenticateRequest, ipAddress());
            setTokenCookie(authenticateResponse.RefreshToken);

            return Ok(authenticateResponse);
        }

        private void setTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        [HttpGet("get")]
        public IActionResult get()
        {
            User? user = _userService.GetById(1);
            return Ok(user);
        }
    }
}
