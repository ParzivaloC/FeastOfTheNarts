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
builder.Services.AddSingleton<IMatchManager, MatchManager>();

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







////========================================================================================================
//app.MapGet("/test-match", () =>
//{
//    // 1. Создаем матч и двух игроков
//    var engine = new FeastOfTheNarts.Core.Services.GameEngine("match-001", "Player_Soslan", "Player_Batradz");
    
//    // 2. Запускаем раздачу
//    engine.StartMatch();

    // 3. Разыгрываем пару карт для теста (имитируем ход)
    // Достаем первого юнита из руки Сослана (рука хранит BaseCard, нам нужен именно UnitCard)
    //var cardToPlay = engine.Player1State.Hand.OfType<FeastOfTheNarts.Core.Domain.Models.UnitCard>().First();

//     // Сослан кладет свою первую карту на стол в нужный ряд
//    engine.PlayCard("Player_Soslan", cardToPlay.Id, cardToPlay.TargetRow);

//    // 4. Возвращаем срез игры в браузер
//    return new
//    {
//        Message = "Матч успешно запущен и сделан первый ход!",
//        CurrentTurn = engine.CurrentPlayerId,
//        Player1 = new 
//        {
//            CardsInHand = engine.Player1State.Hand.Count,
//            CardsInDeck = engine.Player1State.Deck.Count,
//            BoardScore = engine.Board.Player1Board.GetTotalPower()
//        },
//        Player2 = new 
//        {
//            CardsInHand = engine.Player2State.Hand.Count,
//            CardsInDeck = engine.Player2State.Deck.Count,
//            BoardScore = engine.Board.Player2Board.GetTotalPower()
//        }
//    };
//});

////========================================================================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();