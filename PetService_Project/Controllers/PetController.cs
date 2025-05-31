using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetService_Project.Models;
using Microsoft.EntityFrameworkCore;
using PetService_Project_Api.DTO.PetDTO;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;
        private readonly IWebHostEnvironment _env;

        public PetController(dbPetService_ProjectContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("GetPetData")]
        public async Task<ActionResult<IEnumerable<PetRequestDTO>>> GetPetData()
        {
            // 1. 取得當前登入會員的 ASP.NET Identity ID
            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"aspNetUserId: {aspNetUserId}");

            if (string.IsNullOrEmpty(aspNetUserId))
            {
                return Unauthorized("無法識別使用者身份。");
            }

            // 2. 根據 FAspNetUserId 找到對應的會員
            var member = await _context.TMembers
                .FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member == null)
            {
                return NotFound("找不到對應的會員資料。");
            }

            // 3. 使用會員 FId 去查詢寵物資料
            var memberPets = await _context.TPetLists
                .Where(p => p.FMemberId == member.FId)
                .Where(p => p.FPetTrained == 0 || p.FPetTrained == null)
                .ToListAsync();

            var petDtos = memberPets.Select(p => new PetRequestDTO
            {
                Id=p.FId,
                MemberId = p.FMemberId,
                PetName = p.FPetName,
                PetDelete = p.FPetTrained,//使用此欄位來記錄軟刪除
                PetWeight = p.FPetWeight, // 假設資料庫中是 int 類型
                PetBirthday = p.FPetBirthday, // DateTime 類型
                PetDe=p.FPetDe,
                PetImagePath = p.FPetImagePath
            }).ToList();

            Console.WriteLine("=== 後端準備回傳的 petDtos ===");
            foreach (var pet in petDtos)
            {
                Console.WriteLine($"Id={pet.Id}, Name={pet.PetName}");
            }
            return Ok(petDtos);
        }

        // 更新寵物資料
        [HttpPut("UpdatePetProfile/{petId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePetProfile(int petId, [FromBody] PetUpdateRequestDTO dto)
        {
            string aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 找到對應的會員
            var member = await _context.TMembers
                .FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member == null) return NotFound("會員不存在");

            // 透過 FMemberId 和 PetId 找到該會員的特定寵物
            var pet = await _context.TPetLists
            .FirstOrDefaultAsync(p => p.FMemberId == member.FId && p.FId == petId && (p.FPetTrained == 0 || p.FPetTrained == null));

            if (pet == null) return NotFound("寵物資料不存在或不屬於此會員");

            // 更新寵物資料
            pet.FPetName = dto.PetName;
            pet.FPetWeight = dto.PetWeight;
            pet.FPetDe = dto.PetDe; // 假設這是寵物的編號或分類
            pet.FPetBirthday = dto.PetBirthday;

            // 只在使用者有上傳新頭像時才更新
            if (!string.IsNullOrEmpty(dto.PetAvatarUrl))
            {
                pet.FPetImagePath = dto.PetAvatarUrl;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "寵物資料更新成功" });
        }


        // 上傳寵物照片（用於新增寵物時）
        [HttpPost("UploadPetPhoto")]
        [Authorize]
        public async Task<IActionResult> UploadPetPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("UploadPetPhoto:沒有收到檔案");
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

            var fileName = $"pet_{aspNetUserId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "pets");
            var filePath = Path.Combine(uploadDir, fileName);

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

                string petPhotoUrl = $"/uploads/pets/{fileName}";

                return Ok(new { petPhotoUrl, message = "寵物照片上傳成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"上傳寵物照片時發生錯誤: {ex.Message}");
                return StatusCode(500, "檔案儲存失敗，請稍後再試");
            }
        }

        // 上傳寵物大頭貼圖片（用於編輯現有寵物）
        [HttpPost("UploadPetAvatar/{petId}")]
        [Authorize]
        public async Task<IActionResult> UploadPetAvatar(int petId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("UploadPetAvatar:沒有收到檔案");
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

            // 先找到會員
            var member = await _context.TMembers
                .FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member == null)
                return NotFound("找不到會員資料");

            // 驗證寵物是否屬於該會員
            var pet = await _context.TPetLists
                .FirstOrDefaultAsync(p => p.FId == petId && p.FMemberId == member.FId);

            if (pet == null)
                return NotFound("找不到寵物資料或該寵物不屬於此會員");

            var fileName = $"pet_{petId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "pets");
            var filePath = Path.Combine(uploadDir, fileName);

            // 檢查目錄是否存在
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            try
            {
                // 如果已有舊照片，可選擇刪除舊照片
                if (!string.IsNullOrEmpty(pet.FPetImagePath))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, pet.FPetImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 儲存檔案
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string petAvatarUrl = $"/uploads/pets/{fileName}";

                // 更新寵物的照片資料
                pet.FPetImagePath = petAvatarUrl;
                await _context.SaveChangesAsync();

                return Ok(new { petAvatarUrl, petId, message = "寵物照片更新成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新寵物 {petId} 照片時發生錯誤: {ex.Message}");
                return StatusCode(500, "檔案儲存失敗，請稍後再試");
            }
        }

        [HttpPost("AddPet")]
        [Authorize]
        public async Task<IActionResult> AddPet([FromBody] PetAddPetDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var member = await _context.TMembers
                .FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member == null) return NotFound("會員不存在");

            var newPet = new TPetList
            {
                FMemberId = member.FId,
                FPetName = dto.PetName,
                FPetWeight = dto.PetWeight,
                FPetDe = dto.PetDe,
                FPetBirthday = dto.PetBirthday.HasValue ? dto.PetBirthday.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                FPetImagePath = dto.PetAvatarUrl, // 使用先上傳的照片URL
                FPetTrained = 0
            };

            _context.TPetLists.Add(newPet);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "寵物新增成功", petId = newPet.FId });
        }

        [HttpDelete("DeletePet/{petId}")]
        [Authorize]
        public async Task<IActionResult> DeletePet(int petId)
        {
            string aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 找到對應的會員
            var member = await _context.TMembers
                .FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member == null) return NotFound("會員不存在");

            // 透過 FMemberId 和 PetId 找到該會員的特定寵物
            var pet = await _context.TPetLists
            .FirstOrDefaultAsync(p => p.FMemberId == member.FId && p.FId == petId  && (p.FPetTrained == 0 || p.FPetTrained == null));

            if (pet == null) return NotFound("寵物資料不存在或不屬於此會員");

            // 軟刪除寵物資料
            pet.FPetTrained = 1;


            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "寵物資料刪除成功" });
        }
    }
}
