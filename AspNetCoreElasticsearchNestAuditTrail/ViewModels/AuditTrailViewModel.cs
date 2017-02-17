using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreElasticsearchNestAuditTrail.ViewModels
{
    public class AuditTrailViewModel
    {
        public List<AuditTrail.Model.AuditTrailLog> AuditTrailLogs { get; set; }

        public int Size { get; set; }

        public int Skip { get; set; }

        public string Filter  { get; set; }
    }
}
