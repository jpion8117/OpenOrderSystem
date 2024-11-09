using Microsoft.AspNetCore.Mvc;

namespace OpenOrderSystem.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    public class RecoveryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
