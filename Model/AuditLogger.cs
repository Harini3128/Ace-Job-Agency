using System;
using System.Threading.Tasks;
using WebApplication1.Model;

public class AuditLogger
{
    private readonly AuthDbContext _context;

    public AuditLogger(AuthDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string userId, string action)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            Timestamp = DateTime.Now
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
