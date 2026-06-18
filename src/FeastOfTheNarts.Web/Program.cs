using Microsoft.AspNetCore.Authentication.Cookies;
using FeastOfTheNarts.Web.Hubs;
using FeastOfTheNarts.Core.Services;
using FeastOfTheNarts.Core.Domain.RepositoryInterfaces;
using FeastOfTheNarts.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Регистрация репозитория пользователей
builder.Services.AddSingleton<IUserRepository, UserRepositoryJSON>();

// Регистрация репозитория для карточек
builder.Services.AddSingleton<ICardRepository, CardRepository>();

builder.Services.AddSingleton<UserService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //если игрок попытается зайти на защищенную страницу игры без логина,
        //его автоматически перенаправит сюда:
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        // поля поедут в camelCase: фронт читает state.you.lives, а не state.You.Lives
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddSingleton<FeastOfTheNarts.Web.Services.MatchmakingService>();

//один реестр матчей на всё приложение (Singleton): его должны видеть все
//запросы и все соединения, и он обязан помнить матчи между вызовами.
builder.Services.AddSingleton<IMatchManager, MatchManager>(); //Soya: раскомитил тк выдает ошибку с билдером var app = builder.Build();

// Настройки для фронта
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");






app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();