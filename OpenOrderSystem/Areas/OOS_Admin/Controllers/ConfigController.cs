using Microsoft.AspNetCore.Mvc;

namespace OpenOrderSystem.Areas.OOS_Admin.Controllers
{
    public class ConfigController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
