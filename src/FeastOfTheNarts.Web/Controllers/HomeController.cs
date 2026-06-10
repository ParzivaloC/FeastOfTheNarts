using Microsoft.AspNetCore.Mvc;

namespace FeastOfTheNarts.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
