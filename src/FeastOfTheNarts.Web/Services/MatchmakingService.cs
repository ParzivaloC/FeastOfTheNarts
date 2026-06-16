using System.Collections.Concurrent;
using FeastOfTheNarts.Core.Services;

namespace FeastOfTheNarts.Web.Services
{
    public record Participant(string ConnectionId, string PlayerId);

    public class MatchmakingService
    {
        private readonly IMatchManager _matches;
        private readonly object _lock = new();

        private Participant? _waiting;// единственный участник, который сейчас ждет соперника
        private readonly ConcurrentDictionary<string, List<Participant>> _byMatch = new(); // для быстрого поиска участников по MatchId
        private readonly ConcurrentDictionary<string, string> _connToMatch = new();

        public MatchmakingService(IMatchManager matches) => _matches = matches;

        public GameEngine? TryJoin(string connId, string playerId)
        {
            lock (_lock)
            {
                if (_waiting == null || _waiting.PlayerId == playerId)
                {
                    _waiting = new Participant(connId, playerId);
                    return null;
                }

                var first = _waiting;
                _waiting = null;

                var engine = _matches.CreateMatch(first.PlayerId, playerId);
                _byMatch[engine.MatchId] = new List<Participant>
                    {
                        first,
                        new Participant(connId, playerId)
                    };
                _connToMatch[first.ConnectionId] = engine.MatchId;
                _connToMatch[connId] = engine.MatchId;
                return engine;
            }

        }

        public string? GetMatchId(string connId) =>
            _connToMatch.TryGetValue(connId, out var id) ? id : null;

        public List<Participant>? GetParticipants(string matchId) =>
            _byMatch.TryGetValue(matchId, out var list) ? list : null;

        // Игрок ушёл. Вернёт (matchId, участники), если он был в матче.
        public (string MatchId, List<Participant> Parts)? Leave(string connId)
        {
            lock (_lock)
            {
                if (_waiting?.ConnectionId == connId) { _waiting = null; return null; }

                if (!_connToMatch.TryRemove(connId, out var matchId)) return null;
                _byMatch.TryRemove(matchId, out var parts);
                if (parts != null)
                    foreach (var p in parts) _connToMatch.TryRemove(p.ConnectionId, out _);

                return parts == null ? null : (matchId, parts);
            }
        }

    }
}
