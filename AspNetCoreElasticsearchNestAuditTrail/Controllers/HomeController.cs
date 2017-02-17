using AuditTrail;
using AuditTrail.Model;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreElasticsearchNestAuditTrail.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuditTrailProvider _auditTrailProvider;

        public HomeController(IAuditTrailProvider auditTrailProvider)
        {
            _auditTrailProvider = auditTrailProvider;
        }

        public IActionResult Index()
        {
            var auditTrailLog = new AuditTrailLog()
            {
                User = User.ToString(),
                Origin = "HomeController:Index",
                Action = "Home GET",
                Log = "home page called doing something important enough to be added to the audit log."
            };

            _auditTrailProvider.AddLog(auditTrailLog);
            return View();
        }

        public IActionResult About()
        {
            var auditTrailLog = new AuditTrailLog()
            {
                User = User.ToString(),
                Origin = "HomeController:About",
                Action = "About GET",
                Log = "user clicked the about nav."
            };

            _auditTrailProvider.AddLog(auditTrailLog);
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            var auditTrailLog = new AuditTrailLog()
            {
                User = User.ToString(),
                Origin = "HomeController:Contact",
                Action = "Contact GET",
                Log = "user clicked the about nav."
            };

            _auditTrailProvider.AddLog(auditTrailLog);
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            var auditTrailLog = new AuditTrailLog()
            {
                User = User.ToString(),
                Origin = "HomeController:Error",
                Action = "Error GET",
                Log = "something has gone wrong"
            };

            _auditTrailProvider.AddLog(auditTrailLog);
            ViewData["Message"] = "Your contact page.";
            return View();
        }
    }
}
