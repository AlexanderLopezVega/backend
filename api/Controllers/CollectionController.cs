using System.Security.Claims;
using api.Data;
using api.DTO.Collection;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController, Route("api/collections")]
    public class CollectionController(ApplicationDBContext dbContext) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_DBContext = dbContext;

        //  Methods
        [HttpGet("previews")]
        public IActionResult GetAllPreview([FromQuery] int? user, [FromQuery] string? name)
        {
            IQueryable<Collection> query = m_DBContext.Collections.AsQueryable();

            if (user.HasValue)
                query = query.Where(s => s.User.ID == user.Value);
            if (name != null)
                query = query.Where(s => EF.Functions.Like(s.Name, $"%{name}%"));

            return Ok(query.Select(c => c.ToCollectionPreviewDTO()));
        }
        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            Collection? collection = m_DBContext.Collections.Find(id);

            return (collection != null) ? Ok(collection.ToCollectionDTO()) : NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCollectionDTO createCollectionDTO)
        {
            string? userIDString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIDString == null) return BadRequest();

            int userID = int.Parse(userIDString);
            User? user = m_DBContext.Users.Find(userID);

            if (user == null) return BadRequest();

            List<Sample> samples = [.. m_DBContext.Samples.Where(s => s.User == user)];

            Collection collection = new()
            {
                ID = createCollectionDTO.ID,
                Name = createCollectionDTO.Name,
                Description = createCollectionDTO.Description,
                PublicationStatus = createCollectionDTO.PublicationStatus,
                User = user,
                Samples = samples,
            };

            m_DBContext.Collections.Add(collection);
            await m_DBContext.SaveChangesAsync();

            return Ok(collection.ID);
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