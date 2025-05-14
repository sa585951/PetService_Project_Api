using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetService_Project.Models;
using PetService_Project_Api.DTO;
using Microsoft.EntityFrameworkCore;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public PetController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        [HttpGet("GetPetData")]
        public async Task<ActionResult<IEnumerable<PetDataRequestDTO>>> GetPetData()
        {
            {
                // 1. 取得當前登入會員的 ID
                // 在 [Authorize] 保護的 Action 中，User 物件包含了登入使用者的 Claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // ClaimTypes.NameIdentifier 通常用於儲存使用者 ID

                if (userIdClaim == null)
                {
                    // 理論上 [Authorize] 會處理未登入的情況，但為了保險起見或如果授權配置有問題，可以加這個檢查
                    // 已經通過 Authorize 的請求，理論上 userIdClaim 不會是 null
                    return Unauthorized("無法識別使用者身份。");
                }

                // 將取得的使用者 ID 轉換為資料庫中 fMemberId 的類型 (假設是 int)
                if (!int.TryParse(userIdClaim.Value, out int currentMemberId))
                {
                    // 如果使用者 ID 的 Claim 值不是有效的 int，表示身份驗證系統配置可能有問題
                    return StatusCode(500, "無法解析使用者 ID。");
                }

                // 2. 使用會員 ID 去資料庫查詢寵物資料
                var memberPets = await _context.TPetLists
                    .Where(p => p.FMemberId == currentMemberId) // 篩選出 fMemberId 符合當前使用者 ID 的寵物
                    .ToListAsync(); // 異步執行查詢並獲取結果列表

                // 3. 將資料庫實體 (Entity) 映射到 DTO (Data Transfer Object)
                // 這是一個好的做法，只返回前端需要的資料，避免暴露過多資料庫細節
                var petDtos = memberPets.Select(p => new PetDataRequestDTO // 假設您定義了一個 PetDto 類別
                {
                    MemberId = p.FMemberId, // 對應 fMemberId
                    PetName = p.FPetName, // 對應 fPetName
                    PetWeight = p.FPetWeight, // 對應 fPetWeight
                    PetAge = p.FPetAge, // 對應 fPetAge
                                        // ... 繼續映射其他需要的屬性
                    PetBirthday = p.FPetBirthday, // 對應 fPetBirthday
                    PetImagePath = p.FPetImagePath // 對應 fPetImagePath
                }).ToList();


                // 4. 返回結果
                // 如果找不到寵物，memberPets 和 petDtos 將是空列表，這也是 OK (200) 的回應
                return Ok(petDtos);
            }

        }
    }
}
