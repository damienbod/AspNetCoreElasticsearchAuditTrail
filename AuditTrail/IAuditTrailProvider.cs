using AuditTrail.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuditTrail;

 public interface IAuditTrailProvider<T>
 {
    Task AddLog(T auditTrailLog);

    Task <IEnumerable<T>> QueryAuditLogs(string filter = "*", AuditTrailPaging auditTrailPaging = null);

    Task<long> Count(string filter);
}
