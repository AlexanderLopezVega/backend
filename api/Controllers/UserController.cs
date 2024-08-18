using api.Data;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/users"), ApiController]
    public class UserController(ApplicationDBContext context) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_Context = context;

        //  Methods
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(m_Context.Users.ToList());
        }
        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            User? user = m_Context.Users.Find(id);

            return (user != null) ? Ok(user) : NotFound();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            User? user = m_Context.Users.Find(id);

            if (user == null) return NotFound();

            m_Context.Users.Remove(user);
            await m_Context.SaveChangesAsync();

            return Ok();
        }
    }
}