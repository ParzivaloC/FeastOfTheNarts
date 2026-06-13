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
        public async Task<IActionResult> Register(string username, string email, string password, string returnUrl = "/")
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Логин и пароль обязательны!";
                return RedirectToAction("Index", "Home");
            }

            //передает пустую строку вместо телефона, так как мы от него отказались
            bool isRegistered = _userService.RegisterUser(username, password, email, "");

            if (!isRegistered)
            {
                ViewBag.Error = "Пользователь с таким логином или почтой уже существует!";
                return RedirectToAction("Index", "Home");
            }

            //АВТОМАТИЧЕСКИЙ ВХОД ПОСЛЕ УСПЕШНОЙ РЕГИСТРАЦИИ
            var user = _userService.VerifyUser(username, password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                //запись куки(временное или постоянное решение: На усмотрение Амридину)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            }

            //перенаправляем туда, откуда была вызвана модалка (в Игру или в Меню)
            return Redirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = "/")
        {
            var user = _userService.VerifyUser(username, password);

            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль!";
                return RedirectToAction("Index", "Home");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //запись куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            //перенаправляет туда, откуда была вызвана модалка (в Игру или в Меню)
            return Redirect(returnUrl);
        }



        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //после выхода возвращаем игрока на Главное меню
            return RedirectToAction("Index", "Home");
        }
    }
}