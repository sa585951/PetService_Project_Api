using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetService_Project.Models;
using Microsoft.EntityFrameworkCore;
using PetService_Project_Api.DTO.MemberDTO;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MemberController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;
        private readonly IWebHostEnvironment _env;

        public MemberController(dbPetService_ProjectContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        // 取得會員資料
        [HttpGet("GetProfile")]
        public IActionResult GetProfile()
        {
            string aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = _context.TMembers.FirstOrDefault(m => m.FAspNetUserId == aspNetUserId);

            if (member == null) return NotFound();

            return Ok(new
            {
                userName = member.FName,
                email = User.FindFirstValue(ClaimTypes.Email),
                phone = member.FPhone,
                address = member.FAddress,
                // 只讀取 FGoogleAvatarUrl，若無則用 FImage
                avatarUrl = !string.IsNullOrEmpty(member.FImage) ? member.FImage : member.FGoogleAvatarUrl
            });
        }

        // 更新會員資料
        [HttpPut("UpdateProfile")]
        public IActionResult UpdateProfile([FromBody] MemberUpdateRequestDTO dto)
        {
            string aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = _context.TMembers.FirstOrDefault(m => m.FAspNetUserId == aspNetUserId);

            if (member == null) return NotFound();
            member.FPhone = dto.Phone;
            member.FAddress = dto.Address;

            // 只在使用者有上傳新頭像時才更新 FImage
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                member.FImage = dto.AvatarUrl;
            }

            _context.SaveChanges();
            return Ok(new { success = true });
        }


        // 上傳大頭貼圖片
        [HttpPost("UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("UploadAvatar:沒有收到檔案");
                return BadRequest("請選擇圖片");
            }



            // 檔案類型檢查
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("只允許上傳 JPG、JPEG、PNG 或 GIF 圖片");

            // 檔案大小檢查 (例如限制為 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("圖片大小不能超過 5MB");

            string aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(aspNetUserId))
                return Unauthorized();

            var fileName = $"avatar_{aspNetUserId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            var filePath = Path.Combine(_env.WebRootPath, "uploads", "avatars", fileName);

            // 檢查目錄是否存在
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            try
            {
                // 儲存檔案
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string avatarUrl = $"/uploads/avatars/{fileName}";
                // 更新會員的頭像資料
                var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);
                if (member == null)
                    return NotFound("找不到會員資料");

                member.FImage = avatarUrl;
                await _context.SaveChangesAsync();

                return Ok(new { avatarUrl });
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                //_logger.LogError(ex, "上傳頭像時發生錯誤");
                return StatusCode(500, "檔案儲存失敗，請稍後再試");
            }
        }
    }
}
