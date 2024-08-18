using api.Data;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/tags"), ApiController]
    public class TagController(ApplicationDBContext context) : ControllerBase
    {
        //  Fields
        private ApplicationDBContext m_Context = context;

        //  Methods
        [HttpGet("{value}")]
        public async Task<IActionResult> Get(string value)
        {
            return await Task.Run(() =>
            {
                Tag[] tags = [.. m_Context.Tags .Where(tag => EF.Functions.Like(tag.Value, $"%{value}%"))];

                return Ok(tags);
            });
        }
    }
}