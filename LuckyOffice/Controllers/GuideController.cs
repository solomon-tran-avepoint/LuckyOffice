using Microsoft.AspNetCore.Mvc;

namespace LuckyOffice.Controllers
{
    public class GuideController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
