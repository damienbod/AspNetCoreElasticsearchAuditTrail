using System.Collections.Generic;

namespace AspNetCoreElasticsearchAuditTrail.ViewModels
{
    public class AuditTrailViewModel
    {
        public List<CustomAuditTrailLog> AuditTrailLogs { get; set; }

        public int Size { get; set; }

        public int Skip { get; set; }

        public string Filter { get; set; }
    }
}
