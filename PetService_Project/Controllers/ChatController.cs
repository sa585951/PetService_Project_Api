using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO;

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

    // ✅ 取得某個會話的所有訊息
    [HttpGet("messages/{sessionId}")]
    public async Task<IActionResult> GetMessages(int sessionId)
    {
        var messages = await _context.TChatMessages
            .Where(m => m.FSessionId == sessionId && !m.FIsDeleted)
            .OrderBy(m => m.FSendTime)
            .ToListAsync();

        return Ok(messages);
    }

    // ✅ 新增一筆訊息
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

        // 更新會話最後訊息時間
        var session = await _context.TChatSessions.FindAsync(dto.FSessionId);
        if (session != null)
        {
            session.FLastMessageTime = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return Ok(message);
    }

    // ✅ 建立新會話（會員發起聊天）
    [HttpPost("session")]
    public async Task<IActionResult> CreateSession([FromBody] ChatSessionDto dto)
    {
        var session = new TChatSession
        {
            FMemberId = dto.FMemberId,
            FEmployeeId = dto.FEmployeeId,
            FStartTime = DateTime.Now,
            Status = "active",
        };

        _context.TChatSessions.Add(session);
        await _context.SaveChangesAsync();

        return Ok(session);
    }

    // ✅ 關閉會話
    [HttpPost("session/close/{sessionId}")]
    public async Task<IActionResult> CloseSession(int sessionId)
    {
        var session = await _context.TChatSessions.FindAsync(sessionId);
        if (session == null) return NotFound();

        session.FEndTime = DateTime.Now;
        session.Status = "closed";

        await _context.SaveChangesAsync();
        return Ok(session);
    }
}

