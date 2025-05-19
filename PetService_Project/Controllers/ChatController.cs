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

    // ✅ 建立或取得會話（僅由會員發起）
    [HttpPost("CreateOrGetSession")]
    public async Task<IActionResult> CreateOrGetSession([FromBody] ChatSessionDto dto)
    {
        // ✅ 僅限會員發起對話（由前端傳入角色判斷）
        if (dto.Role != "member")
            return BadRequest("只有會員可以發起對話");

        var existingSession = await _context.TChatSessions
            .FirstOrDefaultAsync(s =>
                s.FMemberId == dto.FMemberId &&
                s.FEmployeeId == dto.FEmployeeId &&
                s.Status == "0");

        if (existingSession != null)
        {
            Console.WriteLine($"🔁 已存在進行中對話：SessionId = {existingSession.FSessionId}");
            return Ok(existingSession.FSessionId);
        }

        var newSession = new TChatSession
        {
            FMemberId = dto.FMemberId,
            FEmployeeId = dto.FEmployeeId,
            FStartTime = DateTime.Now,
            Status = "0"
        };

        Console.WriteLine($"🆕 建立新對話：memberId = {dto.FMemberId}, employeeId = {dto.FEmployeeId}");

        _context.TChatSessions.Add(newSession);
        await _context.SaveChangesAsync();

        return Ok(newSession.FSessionId);
    }

    // ✅ 取得訊息
    [HttpGet("messages/{sessionId}")]
    public async Task<IActionResult> GetMessages(int sessionId)
    {
        var messages = await _context.TChatMessages
            .Where(m => m.FSessionId == sessionId && !m.FIsDeleted)
            .OrderBy(m => m.FSendTime)
            .ToListAsync();

        return Ok(messages);
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
    [HttpPost("EndSession")]
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

    [HttpGet("GetAllMembers")]
    public IActionResult GetAllMembers()
    {
        var members = _context.TMembers
            .Where(m => m.FEmail != null && m.FEmail != "" && m.FIsDeleted == false)
            .Select(m => new
            {
                id = m.FId,
                name = m.FName,
                email = m.FEmail
            })
            .ToList();

        return Ok(members);
    }

    // ✅ 抓任一客服
    //[HttpGet("GetAnyEmployee")]
    //public IActionResult GetAnyEmployee()
    //{
    //    var employee = _context.TMembers.FirstOrDefault(m => m.Role == "employee");
    //    if (employee == null) return NotFound("找不到客服人員");

    //    return Ok(new
    //    {
    //        id = employee.FId,
    //        name = employee.FName
    //    });
    //}
}
