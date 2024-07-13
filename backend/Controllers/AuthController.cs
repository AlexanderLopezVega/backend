using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [Route("[controller]"), ApiController]
    public class AuthController : ControllerBase
    {
        //  Methods
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null)
                return BadRequest("Invalid client request");

            if (!ValidateUser(loginRequest.Username, loginRequest.Password))
                return Unauthorized();

            SymmetricSecurityKey scretKey = new(Encoding.UTF8.GetBytes("YourSecretKey"));
            SigningCredentials credentials = new(scretKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenOptions = new(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: new List<Claim>(),
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
            );

            string tokenString = new JwtSecurityTokenHandler ().WriteToken(tokenOptions);
            return Ok(new { Token = tokenString });
        }
        private static bool ValidateUser(string username, string password)
        {
            return username == "foo" && password == "bar";
        }
    }
}