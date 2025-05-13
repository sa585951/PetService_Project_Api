using Microsoft.AspNetCore.Mvc;
using PetService_Project.Models;
using PetService_Project_Api.DTO;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SourceController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public SourceController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        // GET: api/Source/GetSources
        [HttpGet("GetSources")]
        public ActionResult<IEnumerable<object>> GetSources()
        {
            var sources = _context.TSourceLists
                .Select(s => new SourceRequestDTO
                {
                    Id = s.FSourceId,
                    Name = s.FSourceName
                })
                .ToList();

            return Ok(sources);
        }
    }
}
