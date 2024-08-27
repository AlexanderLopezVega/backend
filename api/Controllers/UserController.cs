using System.Security.Claims;
using System.Security.Cryptography;
using api.Data;
using api.DTO.User;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api"), ApiController]
    public class UserController(ApplicationDBContext context) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_DBContext = context;

        //  Methods
        [HttpGet("user")]
        public IActionResult Get()
        {
            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine(userIDString == null);

            if (userIDString == null) return BadRequest();

            return Ok(new { userID = userIDString });
        }
        [HttpGet("users/{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            User? user = m_DBContext.Users.Find(id);

            return (user != null) ? Ok(user) : NotFound();
        }
        [HttpPatch("users")]
        public async Task<IActionResult> Update([FromBody] UserPatchDTO userPatchDTO)
        {
            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIDString == null) return BadRequest();

            int userID = int.Parse(userIDString);

            User? user = m_DBContext.Users.Find(userID);

            if (user == null) return BadRequest();

            user.Username = userPatchDTO.Username ?? user.Username;

            m_DBContext.Users.Update(user);
            await m_DBContext.SaveChangesAsync();

            return Ok();
        }
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            User? user = m_DBContext.Users.Find(id);

            if (user == null) return NotFound();

            IQueryable<Sample>? samples = m_DBContext.Samples.Where(s => s.User.ID == user.ID);
            IQueryable<Collection>? collections = m_DBContext.Collections.Where(c => c.User.ID == user.ID);

            m_DBContext.Samples.RemoveRange(samples);
            m_DBContext.Collections.RemoveRange(collections);
            m_DBContext.Users.Remove(user);
            await m_DBContext.SaveChangesAsync();

            return Ok();
        }
    }
}