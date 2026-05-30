using Microsoft.AspNetCore.Authentication.Cookies;
using FeastOfTheNarts.Core.Services;
using FeastOfTheNarts.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<JsonUserService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Если игрок попытается зайти на защищенную страницу игры без логина,
        // его автоматически перенаправит сюда:
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

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
app.UseCors("AllowFrontend");


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<GameHub>("/gamehub");

//ТЕСТ
app.MapGet("/test-match", () =>
{
    //создание матча и игроков
    var engine = new GameEngine("match-001", "Player_Soslan", "Player_Batradz");

    //запуск
    engine.StartMatch();

    //розыгрыш
    var cardToPlay = engine.Player1State.Hand.First();
    engine.PlayCard("Player_Soslan", cardToPlay.Id, cardToPlay.TargetRow);

    return new
    {
        Message = "Матч успешно запущен и сделан первый ход!",
        CurrentTurn = engine.CurrentPlayerId,
        Player1 = new
        {
            CardsInHand = engine.Player1State.Hand.Count,
            CardsInDeck = engine.Player1State.Deck.Count,
            BoardScore = engine.Board.Player1Board.GetTotalPower()
        },
        Player2 = new
        {
            CardsInHand = engine.Player2State.Hand.Count,
            CardsInDeck = engine.Player2State.Deck.Count,
            BoardScore = engine.Board.Player2Board.GetTotalPower()
        }
    };
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Game}/{action=Index}/{id?}");

app.Run();