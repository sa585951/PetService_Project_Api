using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO;
using SendGrid.Helpers.Mail;

namespace PetService_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly dbPetService_ProjectContext _context;

    public ChatController(dbPetService_ProjectContext context)
    {
        _context = context;
    }

    // ✅ 建立或取得會話
    [HttpPost("CreateOrGetSession")]
    public async Task<IActionResult> CreateOrGetSession([FromBody] ChatSessionDto dto)
    {
        if (dto.FMemberId == dto.FEmployeeId)
        {
            return BadRequest("無法與自己建立對話");
        }

        var session = await _context.TChatSessions
    .FirstOrDefaultAsync(s =>
        s.FMemberId == dto.FMemberId &&
        s.FEmployeeId == dto.FEmployeeId);

        if (session != null)
        {
            // 如果狀態為已結束就重啟
            if (session.Status == "1")
            {
                session.Status = "0";
                session.FEndTime = null;
                session.FStartTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok(session.FSessionId);
        }

        // 若完全沒資料才新增
        var newSession = new TChatSession
        {
            FMemberId = dto.FMemberId,
            FEmployeeId = dto.FEmployeeId,
            FStartTime = DateTime.Now,
            Status = "0"
        };

        _context.TChatSessions.Add(newSession);
        await _context.SaveChangesAsync();

        return Ok(newSession.FSessionId);
    }


    //// ✅ 取得訊息


    [HttpGet("messages/{sessionId}")]
    public async Task<IActionResult> GetMessages(int sessionId)
    {
        var messages = await _context.TChatMessages
            .Where(m => m.FSessionId == sessionId && !m.FIsDeleted)
            .OrderBy(m => m.FSendTime)
            .ToListAsync();

        var senderIds = messages.Select(m => m.FSenderId).Distinct().ToList();

        // 假設 sender 一定是會員
        var senders = await _context.TMembers
            .Where(m => senderIds.Contains(m.FId))
            .ToDictionaryAsync(m => m.FId, m => new {
                m.FName,
                m.FImage
            });

        var result = messages.Select(m => new
        {
            m.FMessageId,
            m.FSessionId,
            m.FSenderId,
            m.FSenderRole,
            m.FMessageText,
            m.FAttachmentUrl,
            m.FMessageType,
            m.FSendTime,
            m.FIsRead,
            m.FIsDeleted,
            senderName = senders.ContainsKey(m.FSenderId) ? senders[m.FSenderId].FName : "未知使用者",
            senderAvatar = senders.ContainsKey(m.FSenderId) ? senders[m.FSenderId].FImage : null
        });

        return Ok(result);
    }

    // ✅ 傳送訊息
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
    {
        var message = new TChatMessage
        {
            FSessionId = dto.FSessionId,
            FSenderId = dto.FSenderId,
            FSenderRole = dto.FSenderRole,
            FMessageText = dto.FMessageText,
            FAttachmentUrl = dto.FAttachmentUrl,
            FMessageType = dto.FMessageType,
            FSendTime = DateTime.Now,
            FIsRead = false,
            FIsDeleted = false,
        };

        _context.TChatMessages.Add(message);

        var session = await _context.TChatSessions.FindAsync(dto.FSessionId);
        if (session != null)
        {
            session.FLastMessageTime = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return Ok(message);
    }

    // ✅ 結束會話
    [HttpPost("EndSession/{sessionId}")]
    public async Task<IActionResult> EndSession(int sessionId)
    {
        var session = await _context.TChatSessions.FindAsync(sessionId);
        if (session == null) return NotFound();

        session.Status = "1";
        session.FEndTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // ✅ 取得使用者資訊（包含角色）
    [HttpGet("GetNameByEmail")]
    public IActionResult GetNameByEmail([FromQuery] string email)
    {
        var member = _context.TMembers.FirstOrDefault(m => m.FEmail == email);
        if (member == null) return NotFound();

        return Ok(new
        {
            id = member.FId,
            name = member.FName,
        });
    }

    //[HttpGet("GetAllMembers")]
    //public IActionResult GetAllMembers()
    //{
    //    var members = _context.TMembers
    //        .Where(m => m.FEmail != null && m.FEmail != "" && m.FIsDeleted == false)
    //        .Select(m => new
    //        {
    //            id = m.FId,
    //            name = m.FName,
    //            email = m.FEmail
    //        })
    //        .ToList();

    //    return Ok(members);
    //}



    [HttpPost("SaveMessage")]
    public async Task<IActionResult> SaveMessage([FromBody] ChatMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FMessageText) || dto.FSenderId <= 0 || dto.FSessionId <= 0)
            return BadRequest("訊息內容、發送者與會話 ID 不可為空");

        var chatMsg = new TChatMessage
        {
            FSessionId = dto.FSessionId,
            FSenderId = dto.FSenderId,
            FSenderRole = dto.FSenderRole,
            FMessageText = dto.FMessageText,
            FAttachmentUrl = dto.FAttachmentUrl ?? "",
            FMessageType = dto.FMessageType ?? "text",
            FSendTime = DateTime.Now,
            FIsRead = false,
            FIsDeleted = false
        };

        _context.TChatMessages.Add(chatMsg);
        // ✅ 同時更新該 Session 狀態為 0（進行中）
        var session = await _context.TChatSessions.FindAsync(dto.FSessionId);
        if (session != null)
        {
            session.FLastMessageTime = DateTime.Now;

            if (session.Status != "0")
            {
                session.Status = "0";
                session.FEndTime = null;
                session.FStartTime = DateTime.Now;
            }
        }
        await _context.SaveChangesAsync();

        return Ok(new { messageId = chatMsg.FMessageId, status = "saved" });
    }

    [HttpGet("GetActiveSessions")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var activeUsers = await _context.TChatSessions
            .Where(s => s.Status == "0")
            .Include(s => s.FMember)
            .Select(s => new {
                id = s.FMember.FId,
                name = s.FMember.FName,
                email = s.FMember.FEmail,
                avatar = s.FMember.FImage ?? "",
                sessionId = s.FSessionId, // ✅ 一定要有這個
                status = s.Status         // ✅ 幫助前端判斷是否結束

            })
            .Distinct()
            .ToListAsync();

        return Ok(activeUsers);
    }

    [HttpGet("GetEndedSessions")]
    public async Task<IActionResult> GetEndedSessions()
    {
        var endedUsers = await _context.TChatSessions
            .Where(s => s.Status == "1")
            .Include(s => s.FMember)
            .Select(s => new {
                id = s.FMember.FId,
                name = s.FMember.FName,
                avatar = s.FMember.FImage ?? "",
                sessionId = s.FSessionId, // ✅ 一定要有這個
                status = s.Status         // ✅ 幫助前端判斷是否結束
            })
            .Distinct()
            .ToListAsync();

        return Ok(endedUsers);
    }

    //抓大頭貼
    [HttpGet("avatar/{memberId}")]
    public async Task<IActionResult> GetAvatar(int memberId)
    {
        var member = await _context.TMembers
            .FirstOrDefaultAsync(m => m.FId == memberId); // ✅ 根據 FId 查

        if (member == null)
            return NotFound(new { message = "找不到該會員" });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var avatarPath = string.IsNullOrWhiteSpace(member.FImage)
            ? "/images/default-avatar.jpg"
            : member.FImage;

        return Ok(new { avatar = baseUrl + avatarPath }); // ✅ 回傳完整圖片網址
    }

    [HttpPost("MarkAsRead")]
    public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkAsReadDto dto)
    {
        var messagesToUpdate = await _context.TChatMessages
            .Where(m => m.FSessionId == dto.SessionId
                        && m.FSenderId != dto.UserId
                        && !m.FIsRead)
            .ToListAsync();

        foreach (var msg in messagesToUpdate)
        {
            msg.FIsRead = true;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}
