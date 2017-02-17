using AuditTrail.Model;
using System.Collections.Generic;

namespace AuditTrail
{
    public interface IAuditTrailProvider
    {
        void AddLog(AuditTrailLog auditTrailLog);

        IEnumerable<AuditTrailLog> QueryAuditLogs(string filter = "*", AuditTrailPaging auditTrailPaging = null);

        long Count(string filter);
    }
}
