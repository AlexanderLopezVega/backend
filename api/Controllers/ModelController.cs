using api.Data;
using api.DTO.Model;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/model"), ApiController]
    public class ModelController(ApplicationDBContext context) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_Context = context;

        //  Methods
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Sample? sample = m_Context.Samples.Where(s => s.ID == id).FirstOrDefault();

            if (sample == null) return NotFound();

            byte[] modelFile = System.IO.File.ReadAllBytes(sample.ModelPath);

            return Ok(new ModelDTO() { ModelFile = modelFile });
        }
    }
}