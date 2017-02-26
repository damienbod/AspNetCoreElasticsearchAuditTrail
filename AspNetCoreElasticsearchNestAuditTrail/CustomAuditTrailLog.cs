using AuditTrail.Model;
using Nest;
using System;

namespace AspNetCoreElasticsearchNestAuditTrail
{
    public class CustomAuditTrailLog : IAuditTrailLog
    {
        public CustomAuditTrailLog()
        {
            Timestamp = DateTime.UtcNow;
        }

        public DateTime Timestamp { get; set; }

        [Keyword]
        public string Action { get; set; }

        public string Log { get; set; }

        public string Origin { get; set; }

        public string User { get; set; }

        public string Extra { get; set; }
    }
}
