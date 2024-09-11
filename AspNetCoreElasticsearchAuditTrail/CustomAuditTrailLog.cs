using AuditTrail.Model;

namespace AspNetCoreElasticsearchAuditTrail;

public class CustomAuditTrailLog : IAuditTrailLog
{
    public CustomAuditTrailLog()
    {
        Timestamp = DateTime.UtcNow;
    }

    public DateTime Timestamp { get; set; }

    // TODO add elastic keyword definition for the property
    //[Keyword]
    public string Action { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Extra { get; set; } = string.Empty;
}
