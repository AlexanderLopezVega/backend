using System.IdentityModel.Tokens.Jwt;
using System.Text;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using api.DTO.User;
using api.Data;

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

            if (!LoginWithCredentials(loginRequest.Username, loginRequest.PasswordHash, out string? token))
                return BadRequest();

            return Ok(new { Token = token });
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

            if (!LoginWithCredentials(registerRequest.Username, registerRequest.PasswordHash, out string? token))
                return BadRequest();

            return Ok(new { Token = token });
        }
        private bool LoginWithCredentials(string username, string passwordHash, out string? token)
        {
            token = null;

            if (!ValidateUser(username, passwordHash))
                return false;

            string? securityKey = m_Configuration["SecurityKey"] ?? throw new InvalidOperationException();
            SymmetricSecurityKey secretKey = new(Encoding.UTF8.GetBytes(securityKey));
            SigningCredentials credentials = new(secretKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenOptions = new(
                issuer: "*",
                audience: "*",
                claims: [],
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
            );

            token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return true;
        }
        private bool ValidateUser(string username, string passwordHash)
        {
            User? user = m_Context.Users.Where((user) => user.Username == username).FirstOrDefault();

            if (user == null)
                return false;

            return PasswordHelper.VerifyPassword(passwordHash, user.PasswordHash, user.PasswordSalt);
        }
    }
}