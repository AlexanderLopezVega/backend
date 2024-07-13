using api.data;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/sample"), ApiController]
    public class SampleController(ApplicationDBContext context) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_Context = context;

        //  Methods
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(m_Context.Samples.Select(s => s.ToSampleDTO()));
        }

        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            Sample? sample = m_Context.Samples.Find(id);

            return (sample != null) ? Ok(sample.ToSampleDTO()) : NotFound();
        }
    }
}