using Microsoft.AspNetCore.Mvc;

namespace FeastOfTheNarts.Web.Controllers
{
    // Глоссарий/энциклопедия по нартскому эпосу. Открыт всем — без [Authorize].
    public class GlossaryController : Controller
    {
        public IActionResult Index() => View();
    }
}
