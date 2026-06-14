using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeastOfTheNarts.Web.Controllers
{
    [Authorize] // Теперь сюда пустят ТОЛЬКО тех, у кого есть кука!
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
