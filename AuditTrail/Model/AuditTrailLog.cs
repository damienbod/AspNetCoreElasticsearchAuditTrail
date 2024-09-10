using System;

namespace AuditTrail.Model;

public class AuditTrailLog : IAuditTrailLog
{
    public AuditTrailLog()
    {
        Timestamp = DateTime.UtcNow;
    }

    public DateTime Timestamp { get; set; }

    [Keyword]
    public string Action { get; set; }

    public string Log { get; set; }

    public string Origin { get; set; }

    public string User { get; set; }
}
