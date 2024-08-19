using System.Security.Claims;
using api.Data;
using api.DTO.Sample;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/samples"), ApiController]
    public class SampleController
    (
        ApplicationDBContext dbContext,
        IDbContextFactory<ApplicationDBContext> dbContextFactory,
        ILogger<SampleController> logger
    ) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_DBContext = dbContext;
        private readonly IDbContextFactory<ApplicationDBContext> m_DBContextFactory = dbContextFactory;
        private readonly ILogger<SampleController> m_Logger = logger;

        //  Methods
        [HttpGet("previews")]
        [Authorize]
        public IActionResult GetAllPreview([FromQuery] int? user, [FromQuery] string? name)
        {
            m_Logger.LogInformation("> Getting all previews");
            IQueryable<Sample> query = m_DBContext.Samples.AsQueryable();

            if (user.HasValue)
                query = query.Where(s => s.User.ID == user.Value);
            if (name != null)
                query = query.Where(s => EF.Functions.Like(s.Name, $"%{name}%"));

            return Ok(query.Select(s => s.ToSamplePreviewDTO()));
        }
        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            Sample? sample = m_DBContext.Samples.Find(id);

            return (sample != null) ? Ok(sample.ToSampleDTO()) : NotFound();
        }
        [HttpPost()]
        public async Task<IActionResult> CreateSample([FromBody] CreateSampleDTO createSampleDTO)
        {
            Console.WriteLine("waaaaaaaaaaaaaaaaaaaaah");
            m_Logger.LogInformation("> Posting sample");

            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            m_Logger.LogInformation($"> User ID: {userIDString}");

            if (userIDString == null) return BadRequest();

            int userID = int.Parse(userIDString);
            User? user = m_DBContext.Users.Find(userID);

            if (user == null) return BadRequest();

            string tempFilePath = $"Public/3D Models/Temp/{createSampleDTO.ModelID}.obj";
            bool modelExists = System.IO.File.Exists($"Public/3D Models/Temp/{createSampleDTO.ModelID}.obj");

            string returnJSON = "{\"path\": \"" + tempFilePath + "\"}";

            //  Model does not exist in Temp folder (probably expired)
            if (!modelExists) return BadRequest(returnJSON);

            //  Move model to permanent folder
            string filePath = $"Public/3D Models/{createSampleDTO.ModelID}.obj";
            System.IO.File.Move(tempFilePath, filePath, true);

            //  Create sample
            Sample sample = new()
            {
                Name = createSampleDTO.Name,
                Description = createSampleDTO.Description,
                Tags = createSampleDTO.Tags,
                PublicationStatus = createSampleDTO.PublicationStatus,
                ModelPath = filePath,
                User = user,
                Collections = []
            };

            //  Store sample in database
            using ApplicationDBContext context = m_DBContextFactory.CreateDbContext();
            context.Samples.Add(sample);
            await context.SaveChangesAsync();

            return Ok(sample.ID);
        }
        [HttpDelete()]
        public async Task<IActionResult> DeleteSample([FromBody] DeleteSamplesDTO deleteSamples, CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                IEnumerable<Sample> samples = m_DBContext.Samples.Where(sample => deleteSamples.SampleIDs.Contains(sample.ID));

                foreach (Sample sample in samples)
                    System.IO.File.Delete(sample.ModelPath);

                m_DBContext.Samples.RemoveRange(samples);
                await m_DBContext.SaveChangesAsync();

                return Ok();
            }, token);
        }
    }
}