using Microsoft.AspNetCore.Mvc;

namespace FeastOfTheNarts.Web.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
