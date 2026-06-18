using FeastOfTheNarts.Core.Domain.Models;
using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Services;
using FeastOfTheNarts.Web.Dtos;
using static FeastOfTheNarts.Web.Dtos.StateOfGameDto;

namespace FeastOfTheNarts.Web.Mapping
{
    public static class GameStateMapper
    {
        // Главный метод для сборки данных о состоянии игры для конкретного зрителя (viewerId)
        public static GameStateDto BuildFor(GameEngine engine, string viewerId)
        {
            bool viewerIsP1 = viewerId == engine.Player1State.PlayerId;

            var you = viewerIsP1 ? engine.Player1State : engine.Player2State;
            var opponent = viewerIsP1 ? engine.Player2State : engine.Player1State;
            var youBoard = viewerIsP1 ? engine.Board.Player1Board : engine.Board.Player2Board;
            var oppBoard = viewerIsP1 ? engine.Board.Player2Board : engine.Board.Player1Board;

            return new GameStateDto
            {
                MatchId = engine.MatchId,
                Phase = engine.Phase.ToString(),        // enum -> строка 
                CurrentPlayerId = engine.CurrentPlayerId,
                WinnerId = engine.WinnerId,
                Result = GetResultFor(engine , viewerId),
                IsYourTurn = engine.CurrentPlayerId == viewerId,
                You = BuildPlayer(you, youBoard, includeHand: true),
                Opponent = BuildPlayer(opponent, oppBoard, includeHand: false) // рука врага скрыта!!!
            };
        }
        private static string? GetResultFor(GameEngine engine, string viewerId)
        {
            if (engine.Phase != GamePhase.Finished) return null; // матч ещё идёт
            if (engine.WinnerId == null) return "Draw";          // ничья — победителя нет
            return engine.WinnerId == viewerId ? "Win" : "Loss"; // ты победитель или нет
        }

        // ВАЖНО: при сборке данных для противника НЕ включаем его руку (includeHand: false),
        private static PlayerStateDto BuildPlayer(PlayerState state, PlayerBoard board, bool includeHand) =>
            new PlayerStateDto
            {
                PlayerId = state.PlayerId,
                DisplayName = state.DisplayName,
                Lives = state.Lives,
                HasPassed = state.HasPassed,
                TotalPower = board.GetTotalPower(),
                DeckCount = state.Deck.Count,
                HandCount = state.Hand.Count,
                Hand = includeHand
                    ? state.Hand.Select(c => ToCardDto(c, onBoard: false)).ToList()
                    : new List<CardDto>(),               // <-- карты соперника НЕ кладём  
                Melee = ToRowDto(board.MeleeRow),
                Ranged = ToRowDto(board.RangedRow),
                Siege = ToRowDto(board.SiegeRow)
            };
        // Метод для преобразования BoardRow в BoardRowDto, включая расчет общей силы ряда и преобразование карт
        private static BoardRowDto ToRowDto(BoardRow row) =>
            new BoardRowDto
            {
                Type = row.Type.ToString(),
                Power = row.GetTotalPower(),
                Cards = row.Cards.Select(c => ToCardDto(c, onBoard: true)).ToList()
            };
        // Метод для преобразования BaseCard в CardDto, с учетом типа карты и текущей силы (для юнитов)
        private static CardDto ToCardDto(BaseCard card, bool onBoard) => card switch
        {
            UnitCard u => new CardDto
            {
                Id = u.Id,
                Name = u.Name,
                Description = u.Description,
                ImageUrl = u.ImageUrl,
                Type = "Unit",
                Power = onBoard ? u.CurrentPower : u.BasePower,  // на столе — текущая, в руке — базовая
                Row = u.TargetRow.ToString(),
                IsHero = u.IsHero
            },
            EventCard e => new CardDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                ImageUrl = e.ImageUrl,
                Type = "Event"
            },
            SpellCard s => new CardDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                ImageUrl = s.ImageUrl,
                Type = "Spell"
            },
            _ => new CardDto { Id = card.Id, Name = card.Name }
        };
    }
}