using Microsoft.AspNetCore.Mvc;

namespace PolyGPS.Controllers
{
    public class MapController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
