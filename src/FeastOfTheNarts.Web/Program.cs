using FeastOfTheNarts.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

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


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapControllers();

app.MapHub<GameHub>("/gamehub");







//========================================================================================================
app.MapGet("/test-match", () =>
{
    // 1. Создаем матч и двух игроков
    var engine = new FeastOfTheNarts.Core.Services.GameEngine("match-001", "Player_Soslan", "Player_Batradz");
    
    // 2. Запускаем раздачу
    engine.StartMatch();

    // 3. Разыгрываем пару карт для теста (имитируем ход)
    // Достаем первую карту из руки Сослана
    var cardToPlay = engine.Player1State.Hand.First();

    // Сослан кладет свою первую карту на стол в нужный ряд
    engine.PlayCard("Player_Soslan", cardToPlay.Id, cardToPlay.TargetRow);

    // 4. Возвращаем срез игры в браузер
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

//========================================================================================================

app.Run();