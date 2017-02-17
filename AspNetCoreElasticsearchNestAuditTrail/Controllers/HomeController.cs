using AuditTrail;
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
            // _auditTrailProvider.AddLog()
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
