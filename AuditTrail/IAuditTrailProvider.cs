using AuditTrail.Model;
using System.Collections.Generic;

namespace AuditTrail
{
    public interface IAuditTrailProvider
    {
        void AddLog(AuditTrailLog auditTrailLog);

        IEnumerable<AuditTrailLog> SelectItems(string filter, AuditTrailPaging auditTrailPaging = null);

        long Count(string filter);
    }
}
