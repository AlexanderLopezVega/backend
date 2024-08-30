using System.Security.Claims;
using System.Text.Json;
using api.Data;
using api.DTO.Collection;
using api.Mappers;
using api.Models;
using api.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController, Route("api/collections")]
    [Authorize]
    public class CollectionController(ApplicationDBContext dbContext) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_DBContext = dbContext;

        //  Methods
        [HttpGet("previews")]
        public IActionResult GetAllPreview([FromQuery] int? userID, [FromQuery] string? name)
        {
            //  Get client ID
            string? clientIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //  If not found, bad request
            if (clientIDString == null || !int.TryParse(clientIDString, out int clientID))
                return BadRequest();

            IEnumerable<Collection> query = m_DBContext.Collections.Include(c => c.User).AsEnumerable();

            //  Filter query by provided filters
            if (userID.HasValue)
                query = query.Where(s => s.User.ID == userID.Value);
            if (name != null)
                query = query.Where(s => s.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));

            if (userID != clientID)
                query = query.Where(s => s.User.ID == clientID || s.PublicationStatus == PublicationStatus.Public);

            return Ok(query.Select(c => c.ToCollectionPreviewDTO()));
        }
        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            //  Get client ID
            string? clientIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //  If not found, bad request
            if (clientIDString == null || !int.TryParse(clientIDString, out int clientID))
                return BadRequest();

            Collection? collection = m_DBContext.Collections.Include(c => c.User).Include(c => c.Samples).FirstOrDefault(c => c.ID == id);

            if (collection == null)
                return NotFound();

            return (collection.User.ID != clientID) ? BadRequest() : Ok(collection.ToCollectionDTO());
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCollectionDTO createCollectionDTO)
        {
            Console.WriteLine("\n\n\n");
            Console.WriteLine(JsonSerializer.Serialize(createCollectionDTO));
            Console.WriteLine("\n\n\n");

            //  Get client ID
            string? clientIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //  If not found, bad request
            if (clientIDString == null || !int.TryParse(clientIDString, out int clientID))
                return BadRequest("a");

            User? user = m_DBContext.Users.Find(clientID);

            //  Get user
            if (user == null)
                return BadRequest("b");

            // bool collectionAlreadyExists = m_DBContext.Collections.Where((c) => c.User == user && c.Name == createCollectionDTO.Name).Any();

            // if (collectionAlreadyExists) return BadRequest();

            //  Fetch samples
            List<Sample> samples = [];

            if (createCollectionDTO.SamplesID != null)
                samples = [.. m_DBContext.Samples.Include(s => s.User).Where(s => s.User.ID == clientID && createCollectionDTO.SamplesID.Contains(s.ID))];

            //  Create collection
            Collection collection = new()
            {
                Name = createCollectionDTO.Name,
                Description = createCollectionDTO.Description,
                PublicationStatus = createCollectionDTO.PublicationStatus,
                User = user,
                Samples = samples,
            };

            //  Add and save changes
            m_DBContext.Collections.Add(collection);
            await m_DBContext.SaveChangesAsync();

            //  Return result
            return Ok(new CreateCollectionResponseDTO() { ID = collection.ID });
        }
        
        [HttpPatch("")]
        public async Task<IActionResult> UpdateCollection([FromBody] CollectionPatchDTO collectionPatchDTO)
        {
            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIDString == null)
                return BadRequest();

            Collection? collection = await m_DBContext.Collections
            .Include(c => c.Samples) 
            .FirstOrDefaultAsync(c => c.ID == collectionPatchDTO.ID);

            if (collection == null)
                return BadRequest();
            
            List<Sample>? samples = m_DBContext.Samples.Where(s => collectionPatchDTO.SampleIDs.Contains(s.ID)).ToList();

            collection.Samples?.Clear();

            collection.Name = collectionPatchDTO.Name ?? collection.Name;
            collection.Description = collectionPatchDTO.Description ?? collection.Description;
            collection.Tags = collectionPatchDTO.Tags ?? collection.Tags;
            collection.PublicationStatus = collectionPatchDTO.PublicationStatus ?? collection.PublicationStatus;
            foreach (var sample in samples) {
                collection.Samples?.Add(sample);
            }

            m_DBContext.Collections.Update(collection);
            await m_DBContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIDString == null) return BadRequest();

            Collection? collection = m_DBContext.Collections.Find(id);

            if (collection == null) return BadRequest();

            m_DBContext.Collections.Remove(collection);
            await m_DBContext.SaveChangesAsync();

            return Ok();
        }
    }
}