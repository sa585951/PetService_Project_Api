using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly dbPetService_ProjectContext _context;

        protected BaseController(dbPetService_ProjectContext context)
        {
            _context = context;
        }
        protected async Task<int?> GetMemberId()
        {
            string aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(aspNetUserId))
                return null;

            var member = await _context.TMembers.FirstOrDefaultAsync(m=>m.FAspNetUserId == aspNetUserId);

            return member?.FId;
        }

        //回傳整個TMember物件(全部欄位)
        protected async Task<TMember?> GetMember()
        {
            string aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(aspNetUserId))
                return null;
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            return member;
        }
    }
}
