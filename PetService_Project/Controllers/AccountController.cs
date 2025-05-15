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
using PetService_Project_Api.DTO;
using Microsoft.EntityFrameworkCore;

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
            }

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userName = string.IsNullOrEmpty(member.FName) ? member.FEmail : member.FName,
                needSupplement = isNewGoogleUser
            });
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

                    //5.回傳JWT token+name
                    var token = GenerateJwtToken(user);
                    return Ok(new
                    {
                        message = "註冊成功",
                        token = GenerateJwtToken(user),
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

                var subject = "您的驗證碼";
                var content = $"您好，您的驗證碼是：{code}，5分鐘內有效。";

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

        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

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
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("帳號或密碼錯誤");

            var token = GenerateJwtToken(user);
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FAspNetUserId == user.Id);
            if (member == null) return NotFound("找不到會員資料");

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
