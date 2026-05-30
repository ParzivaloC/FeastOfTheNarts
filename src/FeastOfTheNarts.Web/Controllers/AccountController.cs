using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using FeastOfTheNarts.Core.Services;

namespace FeastOfTheNarts.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly JsonUserService _userService;

        public AccountController(JsonUserService userService)
        {
            _userService = userService;
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string email, string phone)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Логин и пароль обязательны!";
                return View();
            }

            bool isRegistered = _userService.RegisterUser(username, password, email, phone);

            if (!isRegistered)
            {
                ViewBag.Error = "Пользователь с таким логином или почтой уже существует!";
                return View();
            }

            return RedirectToAction("Game");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _userService.VerifyUser(username, password);

            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль!";
                return View();
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //запись куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Game");
        }



        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}