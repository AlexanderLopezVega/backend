using System.IdentityModel.Tokens.Jwt;
using System.Text;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using api.DTO.User;
using api.Data;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/auth"), ApiController]
    public class AuthController(ApplicationDBContext context, IConfiguration config) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_Context = context;
        private readonly IConfiguration m_Configuration = config;

        //  Methods
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (loginRequest == null)
                return BadRequest();

            if (!LoginWithCredentials(loginRequest.Username, loginRequest.PasswordHash, out string? token, out int userID))
                return BadRequest();

            return Ok(new { Token = token, UserID = userID });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequest)
        {
            if (registerRequest == null)
                return BadRequest();

            bool usernameInUse = m_Context.Users.Any((user) => user.Username == registerRequest.Username);

            Console.WriteLine(usernameInUse);

            if (usernameInUse)
                return BadRequest();

            PasswordHelper.CreatePasswordHash(registerRequest.PasswordHash, out string passwordDoubleHash, out string passwordSalt);

            User user = new()
            {
                Username = registerRequest.Username,
                PasswordHash = passwordDoubleHash,
                PasswordSalt = passwordSalt
            };

            await m_Context.Users.AddAsync(user);
            await m_Context.SaveChangesAsync();

            if (!LoginWithCredentials(registerRequest.Username, registerRequest.PasswordHash, out string? token, out int userID))
                return BadRequest();

            return Ok(new { Token = token, UserID = userID });
        }
        private bool LoginWithCredentials(string username, string passwordHash, out string? token, out int userID)
        {
            token = null;

            if (!ValidateUser(username, passwordHash, out userID))
                return false;

            token = GenerateJwtToken(username);

            return true;
        }
        private bool ValidateUser(string username, string passwordHash, out int userID)
        {
            userID = -1;
            User? user = m_Context.Users.Where((user) => user.Username == username).FirstOrDefault();

            if (user == null) return false;

            bool passwordValid = PasswordHelper.VerifyPassword(passwordHash, user.PasswordHash, user.PasswordSalt);

            if (!passwordValid) return false;

            userID = user.ID;

            return true;
        }
        private string GenerateJwtToken(string username)
        {
            string securityKeyConfig = m_Configuration["SecurityKey"] ?? throw new InvalidOperationException("Security key not found");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(securityKeyConfig));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            SecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}