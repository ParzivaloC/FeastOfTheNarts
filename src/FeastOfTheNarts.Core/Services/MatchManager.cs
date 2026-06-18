using System.Collections.Concurrent;
using FeastOfTheNarts.Core.Domain.RepositoryInterfaces;

namespace FeastOfTheNarts.Core.Services
{
    public class MatchManager : IMatchManager
    {
        private readonly ConcurrentDictionary<string, GameEngine> _matches = new();
        private readonly ICardRepository _cards;

        public MatchManager(ICardRepository cards) => _cards = cards;

        public GameEngine CreateMatch(string player1Id, string player2Id)
        {
            // matchId генерируем сами — Guid гарантирует уникальность ключа.
            var matchId = Guid.NewGuid().ToString();
            var engine = new GameEngine(matchId, player1Id, player2Id, _cards);

            _matches[matchId] = engine; // кладём в реестр
            return engine;
        }

        public GameEngine? GetMatch(string matchId)
        {
            // out-параметр получит движок, если ключ есть; иначе вернётся null.
            _matches.TryGetValue(matchId, out var engine);
            return engine;
        }

        public bool RemoveMatch(string matchId)
        {
            // TryRemove сам потокобезопасно убирает запись и возвращает,
            // был ли там матч. Значение нам не нужно?  отбрасываем через _.
            return _matches.TryRemove(matchId, out _);
        }
    }
}
