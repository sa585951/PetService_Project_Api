using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PetService_Project.Models;
using PetService_Project_Api.Models;
using PetService_Project_Api.Service;
using Microsoft.EntityFrameworkCore;
using PetService_Project_Api.DTO.MemberDTO;
using System.Threading.Tasks;

namespace PetService_Project_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly dbPetService_ProjectContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ICodeService _codeService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            dbPetService_ProjectContext context,
            IConfiguration config,
            IEmailService emailService,
            ICodeService codeService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _config = config;
            _emailService = emailService;
            _codeService = codeService;
        }

        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] AccountGoogleRequestDTO dto)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            };

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Google Token 驗證失敗");
            }

            var email = payload.Email;
            var name = payload.Name;
            var avatarUrl = payload.Picture;
            var providerKey = payload.Subject;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return BadRequest("Identity 建立失敗: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            bool isNewGoogleUser = false;
            var member = _context.TMembers.FirstOrDefault(m => m.FEmail == email);
            if (member == null)
            {
                isNewGoogleUser = true;
                member = new TMember
                {
                    FName = name,
                    FEmail = email,
                    FGoogleAvatarUrl = avatarUrl,
                    FProviderKey = providerKey,
                    FLoginProvider = "Google",
                    FAspNetUserId = user.Id,
                    FCreatedDate = DateTime.Now,
                    FLastLoginDate = DateTime.Now,
                    FIsDeleted = false,
                    FBlackList = false
                };
                _context.TMembers.Add(member);
            }
            else
            {
                member.FLastLoginDate = DateTime.Now;
                member.FGoogleAvatarUrl = avatarUrl;
                // TODO: **將 Google 登入的 Provider 信息也記錄到現有用戶上**
                if (string.IsNullOrEmpty(member.FProviderKey) || member.FLoginProvider != "Google")
                {
                    member.FProviderKey = providerKey;
                    member.FLoginProvider = "Google";
                    // 如果你允許用戶更新姓名，也可以在這裡用 Google 提供的姓名更新 FName
                    // member.FName = name;
                }
            }

            bool needSupplement = false;
            if (string.IsNullOrEmpty(member.FPhone) || string.IsNullOrEmpty(member.FAddress))
            {
                needSupplement = true;
            }

            await _context.SaveChangesAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            //5.回傳JWT token+name
            var token = GenerateJwtToken(user, userRoles);

            return Ok(new
            {
                token,
                userName = string.IsNullOrEmpty(member.FName) ? member.FEmail : member.FName,
                needSupplement = needSupplement,
                memberId = member.FId,
            });
        }

        [HttpPost("CompleteProfile")]
        public async Task<IActionResult> CompleteProfile([FromBody] AccountCompleteGoogleSignupDTO dto)
        {

            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(aspNetUserId))
            {

                return Unauthorized("無法識別使用者身份");
            }

            // 找到對應的 TMember 記錄
            // 使用 FirstOrDefaultAsync 避免封鎖線程，同時處理找不到的情況
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member == null)
            {
                // 找不到對應的 TMember 記錄，這不應該發生在已登入且有 TMember 的用戶上，可能是資料問題
                return NotFound("找不到對應的使用者記錄");
            }


            if (string.IsNullOrWhiteSpace(dto.Phone))
            {
                return BadRequest("手機是必填的");
            }


            if (string.IsNullOrWhiteSpace(dto.Address))
            {
                return BadRequest("地址是必填的");
            }

            // ****** 更新 TMember 記錄 ******
            member.FPhone = dto.Phone;
            member.FAddress = dto.Address;

            if (dto.Sources != null && dto.Sources.Any())
            {
                foreach (var sourceId in dto.Sources)
                {
                    // 儲存每個來源到 TMemberSource 關聯表中
                    var memberSource = new TMemberSource
                    {
                        FMemberId = member.FId,  // TMember 的主鍵
                        FSourceId = sourceId
                    };
                    _context.TMemberSources.Add(memberSource);
                }
                await _context.SaveChangesAsync(); // 儲存來源資料
            }

            try
            {
                _context.TMembers.Update(member); // EF Core 會自動追蹤變化，通常調用這個是明確表示更新
                await _context.SaveChangesAsync(); // 儲存變更

                // 返回成功響應
                // 你可以回傳更新後的用戶資料，或者只是一個成功訊息
                return Ok(new { message = "資料更新成功" });
            }
            catch (DbUpdateConcurrencyException)
            {
                // 處理並發衝突 (Concurrency Conflict) - 兩個用戶同時修改同一條記錄
                return Conflict("資料已被修改，請重新提交");
            }
            catch (Exception ex)
            {
                // 處理其他可能的資料庫錯誤
                // 記錄錯誤 ex
                return StatusCode(500, "儲存資料時發生錯誤");
            }
        }


        [HttpGet("CheckEmailExists")]
        public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email 不可為空");

            bool exists = await _context.TMembers.AnyAsync(m => m.FEmail == email);
            return Ok(new { exists });
        }

        [HttpGet("CheckPhoneExists")]
        public async Task<IActionResult> CheckPhoneExists([FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest("電話不可為空");

            bool exists = await _context.TMembers.AnyAsync(m => m.FPhone == phone);
            return Ok(new { exists });
        }



        [HttpPost("EmailRegister")]
        public async Task<IActionResult> EmailRegister([FromBody] AccountEmailSignupRequestDTO dto)
        {
            // 使用事務來保證兩者的資料同步
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. 創建 ApplicationUser
                    var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
                    var result = await _userManager.CreateAsync(user, dto.Password);

                    if (!result.Succeeded)
                    {
                        return BadRequest("Email 註冊失敗: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }

                    //加入角色
                    await _userManager.AddToRoleAsync(user, "user");
                    // 2. 創建 TMember 並關聯到 ApplicationUser
                    var member = new TMember
                    {
                        FEmail = dto.Email,
                        FName = dto.Name,
                        FPhone = dto.Phone,
                        FAddress = dto.Address,
                        FAspNetUserId = user.Id,
                        FLoginProvider = "Email",
                        FCreatedDate = DateTime.Now,
                        FIsDeleted = false,
                        FBlackList = false
                    };

                    // 儲存 TMember
                    _context.TMembers.Add(member);
                    await _context.SaveChangesAsync();

                    // 3. 處理 'sources' 欄位並儲存至 TMemberSource
                    if (dto.Sources != null && dto.Sources.Any())
                    {
                        foreach (var sourceId in dto.Sources)
                        {
                            // 儲存每個來源到 TMemberSource 關聯表中
                            var memberSource = new TMemberSource
                            {
                                FMemberId = member.FId,  // TMember 的主鍵
                                FSourceId = sourceId
                            };
                            _context.TMemberSources.Add(memberSource);
                        }
                        await _context.SaveChangesAsync(); // 儲存來源資料
                    }

                    // 4.提交事務
                    await transaction.CommitAsync();

                    var userRoles = await _userManager.GetRolesAsync(user);
                    //5.回傳JWT token+name
                    var token = GenerateJwtToken(user, userRoles);
                    return Ok(new
                    {
                        message = "註冊成功",
                        token = token,
                        name = dto.Name,
                    });
                }
                catch (Exception ex)
                {
                    // 如果有錯誤，撤回所有操作
                    await transaction.RollbackAsync();
                    return BadRequest($"註冊失敗: {ex.Message}");
                }
            }
        }

        [HttpPost("SendEmailVerificationCode")]
        public async Task<IActionResult> SendEmailVerificationCode([FromBody] AccountEmailVerificationRequestDTO dto)
        {
            try
            {
                var code = new Random().Next(100000, 999999).ToString();
                Console.WriteLine($"驗證碼：{code}");

                var subject = "毛孩管家註冊驗證碼";
                var content = $"您好，感謝您使用毛孩管家服務，您的驗證碼是：{code}，請於5分鐘內完成驗證，避免失效。";

                // 儲存驗證碼到 MemoryCacheService，設定 5 分鐘過期時間
                await _codeService.SetCodeAsync(dto.Email, code, TimeSpan.FromMinutes(5));

                await _emailService.SendEmailAsync(dto.Email, subject, content);
                return Ok(new { message = "驗證碼已寄出" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發送驗證碼失敗: {ex.Message}");
                return StatusCode(500, $"發送失敗: {ex.Message}");
            }
        }

        [HttpPost("VerifyEmailVerificationCode")]
        public async Task<IActionResult> VerifyEmailVerificationCode([FromBody] AccountVerifyEmailCodeRequestDTO dto)
        {
            try
            {
                // 從 MemoryCacheService 中讀取存儲的驗證碼
                var cachedCode = await _codeService.GetCodeAsync(dto.Email);
                if (cachedCode != null && cachedCode == dto.Code)
                {
                    return Ok(new { message = "驗證成功" });
                }
                return BadRequest("驗證碼錯誤或已過期");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"驗證失敗: {ex.Message}");
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user, IList<string> userRoles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(userRoles.Select(role=>new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),  // Token 有效期設定為 7 天
                signingCredentials: creds);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] AccountLoginRequestDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            //上線後應該用此方法較安全
            //if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            //    return Unauthorized("帳號或密碼錯誤");
            if (user == null)
                return Unauthorized("此 Email 尚未註冊");
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password,false);
            if (!result.Succeeded)
                return Unauthorized("密碼錯誤");

            var roles = await _userManager.GetRolesAsync(user);
            await Task.Yield();

            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FAspNetUserId == user.Id);
            if (member == null) return NotFound("找不到會員資料");

            var token = GenerateJwtToken(user, roles);
            member.FLastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
                userName = member.FName,
                memberId = member.FId
            });
        }

        // AccountController.cs
        [Authorize] // 需要驗證 JWT 才能呼叫
        [HttpGet("CheckLoginStatus")]
        public IActionResult CheckLoginStatus()
        {
            var userName = User.Identity?.Name; // 從 JWT 中取得登入者名稱
            return Ok(new { message = $"你有登入成功，歡迎 {userName}！" });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] AccountForgotPasswordRequestDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest(new { message = "無法找到該電子郵件地址。" });
            }

            if (user.ProviderKey != "Email" && user.ProviderKey != null)
            {
                return BadRequest(new { message = "無法找到該電子郵件地址。" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 3. 組合 reset 連結（前端要有對應的 reset 頁面）
            var resetLink = $"{dto.FrontendUrl}/resetpassword?email={user.Email}&token={Uri.EscapeDataString(token)}";

            // 4. 寄出信件
            await _emailService.SendEmailAsync(user.Email, "重設密碼連結", $"請點擊以下連結重設密碼，連結有效時間20分鐘：<a href='{resetLink}'>重設密碼</a>");
            //Console.Write("重設密碼連結" + resetLink);
            return Ok(new { message = "已寄出重設密碼連結，請查看信箱。頁面3秒後自動跳轉。" });
        }

        [HttpPost("ValidateNewPassword")]
        public async Task<IActionResult> ValidateNewPassword([FromBody] AccountResetPasswordRequestDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "無效的帳號" });

            var passwordHasher = new PasswordHasher<IdentityUser>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.NewPassword);
            if (verificationResult == PasswordVerificationResult.Success)
            {
                return BadRequest(new { message = "新密碼不能與舊密碼相同" });
            }

            return Ok();
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] AccountResetPasswordRequestDTO dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return BadRequest(new { message = "無效的帳號" });

                var result = await _userManager.ResetPasswordAsync(user, WebUtility.UrlDecode(dto.Token), dto.NewPassword);
                if (!result.Succeeded)
                {
                    // 若錯誤原因是 token 無效
                    if (result.Errors.Any(e => e.Code == "InvalidToken"))
                    {
                        return BadRequest(new { message = "無效的或已過期的重設密碼連結，請重新申請。" });
                    }

                    return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }

                return Ok(new { message = "密碼重設成功，頁面在3秒後自動跳轉" });
            }
            catch (Exception ex)
            {
                // 其他異常情況
                return StatusCode(500, new { message = "伺服器錯誤", error = ex.Message });
            }


        }
    }
}
