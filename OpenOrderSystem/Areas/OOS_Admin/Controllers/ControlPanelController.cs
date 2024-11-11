using Microsoft.AspNetCore.Mvc;

namespace OpenOrderSystem.Areas.OOS_Admin.Controllers
{
    public class ControlPanelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
