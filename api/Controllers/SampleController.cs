using System.Security.Claims;
using api.Data;
using api.DTO.Sample;
using api.Mappers;
using api.Models;
using api.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueryCondition = System.Linq.Expressions.Expression<System.Func<api.Models.Sample, bool>>;

namespace api.Controllers
{
    [Route("api/samples"), ApiController]
    [Authorize]
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
        public IActionResult GetAllPreview([FromQuery] bool? localOnly, [FromQuery] string? name)
        {
            m_Logger.LogInformation("> Getting all previews");

            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIDString == null) return BadRequest();

            int clientID = int.Parse(userIDString);

            m_Logger.LogInformation("> Client ID: {ClientID}", clientID);

            IQueryable<Sample> query = m_DBContext.Samples.AsQueryable();

            QueryCondition whereCondition = localOnly.HasValue
            ? (s) => s.User.ID == clientID
            : (s) => s.User.ID == clientID || s.PublicationStatus == PublicationStatus.Public;

            query = query.Where(whereCondition);

            if (name != null)
                query = query.Where(s => EF.Functions.Like(s.Name, $"%{name}%"));

            SamplePreviewDTO[] samples = query.Select(s => s.ToSamplePreviewDTO()).ToArray();

            m_Logger.LogInformation("> Samples: {samples}", samples.Length);

            return Ok(samples);
        }
        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            Sample? sample = m_DBContext.Samples.Find(id);

            if (sample == null) return BadRequest();

            if (sample.PublicationStatus != PublicationStatus.Public)
            {
                string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIDString == null) return BadRequest();

                int userID = int.Parse(userIDString);

                User? user = m_DBContext.Users.Find(userID);

                if (user == null) return BadRequest();
                if (!sample.User.Equals(user)) return BadRequest();
            }

            return Ok(sample.ToSampleDTO());
        }
        [HttpPost()]
        public async Task<IActionResult> CreateSample([FromBody] CreateSampleDTO createSampleDTO)
        {
            m_Logger.LogInformation("> Posting sample");

            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            m_Logger.LogInformation("> User ID: {UserIDString}", userIDString);

            if (userIDString == null) return BadRequest();

            int userID = int.Parse(userIDString);
            User? user = m_DBContext.Users.Find(userID);

            m_Logger.LogInformation("> User: {User}", user);

            if (user == null) return BadRequest();

            string tempFilePath = $"Public/3D Models/Temp/{createSampleDTO.ModelID}.obj";
            bool modelExists = System.IO.File.Exists($"Public/3D Models/Temp/{createSampleDTO.ModelID}.obj");

            string returnJSON = "{\"path\": \"" + tempFilePath + "\"}";

            m_Logger.LogInformation("> Model temporary filepath: {TempFilePath}", tempFilePath);

            //  Model does not exist in Temp folder (probably expired)
            if (!modelExists) return BadRequest(returnJSON);

            //  Move model to permanent folder
            string filePath = $"Public/3D Models/{createSampleDTO.ModelID}.obj";
            System.IO.File.Move(tempFilePath, filePath, true);

            m_Logger.LogInformation("> Moved model to permanent folder");

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
            context.Attach(user);
            context.Samples.Add(sample);
            await context.SaveChangesAsync();

            m_Logger.LogInformation("> Created new sample {Sample}", sample);

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
        [HttpPatch("")]
        public async Task<IActionResult> Update([FromBody] SamplePatchDTO samplePatchDTO)
        {
            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIDString == null) return BadRequest();

            Sample? sample = m_DBContext.Samples.Find(samplePatchDTO.ID);

            if (sample == null) return BadRequest();

            sample.Name = samplePatchDTO.Name ?? sample.Name;
            sample.Description = samplePatchDTO.Description ?? sample.Description;
            sample.Tags = samplePatchDTO.Tags ?? sample.Tags;
            sample.PublicationStatus = samplePatchDTO.PublicationStatus ?? sample.PublicationStatus;

            m_DBContext.Samples.Update(sample);
            await m_DBContext.SaveChangesAsync();

            return Ok();
        }
    }
}