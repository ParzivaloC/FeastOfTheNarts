using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Services;
using FeastOfTheNarts.Web.Mapping;
using FeastOfTheNarts.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FeastOfTheNarts.Web.Hubs
{
    [Authorize] // в хаб пускаем только залогиненных
    public class GameHub : Hub
    {
        private readonly IMatchManager _matches;
        private readonly MatchmakingService _matchmaking;

        public GameHub(IMatchManager matches, MatchmakingService matchmaking)
        {
            _matches = matches;
            _matchmaking = matchmaking;
        }

        // Id игрока берём из авторизации, а не из параметра
        private string PlayerId => Context.User?.Identity?.Name ?? Context.ConnectionId;

        public async Task FindGame()
        {
            var engine = _matchmaking.TryJoin(Context.ConnectionId, PlayerId);
            if (engine == null)
            {
                await Clients.Caller.SendAsync("Waiting");
                return;
            }

            lock (engine.SyncRoot) { engine.StartMatch(); }
            await BroadcastState(engine);   // оба игрока получают свой стартовый снимок
        }

        public async Task PlayCard(string cardId, string row)
        {
            var engine = CurrentEngine();
            if (engine == null) return;
            if (!Enum.TryParse<CardRow>(row, out var targetRow)) return;

            lock (engine.SyncRoot) { engine.PlayCard(PlayerId, cardId, targetRow); } // движок сам проверит ход/фазу
            await BroadcastState(engine);
        }

        public async Task Pass()
        {
            var engine = CurrentEngine();
            if (engine == null) return;

            lock (engine.SyncRoot) { engine.PassTurn(PlayerId); }
            await BroadcastState(engine);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var left = _matchmaking.Leave(Context.ConnectionId);
            if (left != null)
            {
                _matches.RemoveMatch(left.Value.MatchId);

                // Оставшегося игрока уведомляем и возвращаем в комнату ожидания (снова в очередь)
                foreach (var p in left.Value.Parts)
                {
                    if (p.ConnectionId == Context.ConnectionId) continue; // это ушедший игрок

                    await Clients.Client(p.ConnectionId).SendAsync("OpponentLeft");

                    var engine = _matchmaking.TryJoin(p.ConnectionId, p.PlayerId);
                    if (engine == null)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("Waiting");
                    }
                    else
                    {
                        lock (engine.SyncRoot) { engine.StartMatch(); }
                        await BroadcastState(engine);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        private GameEngine? CurrentEngine()
        {
            var matchId = _matchmaking.GetMatchId(Context.ConnectionId);
            return matchId == null ? null : _matches.GetMatch(matchId);
        }

        // Каждому игроку — СВОЙ снимок (рука соперника скрыта)
        private async Task BroadcastState(GameEngine engine)
        {
            var parts = _matchmaking.GetParticipants(engine.MatchId);
            if (parts == null) return;

            foreach (var p in parts)
            {
                var state = GameStateMapper.BuildFor(engine, p.PlayerId);
                await Clients.Client(p.ConnectionId).SendAsync("ReceiveState", state);
            }
        }
    }
}