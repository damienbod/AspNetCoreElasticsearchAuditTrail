using System;

namespace AuditTrail.Model;

public interface IAuditTrailLog
{
    DateTime Timestamp { get; set; }
}
