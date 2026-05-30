using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Domain.Models;

namespace FeastOfTheNarts.Core.Services
{
    public class GameEngine
    {
        public string MatchId { get; }
        public GameBoard Board { get; }

        public PlayerState Player1State { get; }
        public PlayerState Player2State { get; }

        public string CurrentPlayerId { get; private set; }


        public GameEngine(string matchId, string player1Id, string player2Id)
        {
            MatchId = matchId;
            Board = new GameBoard(player1Id, player2Id);

            Player1State = new PlayerState(player1Id);
            Player2State = new PlayerState(player2Id);

            CurrentPlayerId = player1Id;
        }

        public void StartMatch()
        {
            //GenerateDummyDeck(Player1State);
            //GenerateDummyDeck(Player2State);

            for (int i = 0; i < 10; i++)
            {
                Player1State.DrawCard();
                Player2State.DrawCard();
            }
        }

        ////==========================================================Проверка 
        //private void GenerateDummyDeck(PlayerState state)
        //{
        //    int idOffset = state.PlayerId == Player1State.PlayerId ? 1000 : 2000;

        //    for (int i = 1; i <= 20; i++)
        //    {
        //        var row = i % 3 == 0 ? CardRow.Melee : (i % 3 == 1 ? CardRow.Ranged : CardRow.Siege);

        //        state.Deck.Add(new UnitCard
        //        {
        //            Id = (idOffset + i).ToString(),
        //            BasePower = Random.Shared.Next(1, 11),
        //            IsHero = i % 10 == 0,
        //            TargetRow = row
        //        });
        //    }
        //}
        ////===========================================================




        public bool PlayCard(string playerId, string cardId, CardRow targetRow)
        {
            if (playerId != CurrentPlayerId) return false;// проверка, что ходит текущий игрок

            var state = playerId == Player1State.PlayerId ? Player1State : Player2State;//проверка, что игрок играет своей картой
            var playerBoard = playerId == Board.Player1Board.PlayerId ? Board.Player1Board : Board.Player2Board;//получаем состояние игрока и его игровое поле

            var cardToPlay = state.Hand.FirstOrDefault( c => c.Id == cardId) as UnitCard;
            if (cardToPlay == null) return false; // Карты нет в руке или это не юнит

            // Пытаемся положить на стол
            bool isPlaced = playerBoard.PlaceCard(cardToPlay, targetRow);

            if (isPlaced)
            {
                // Убираем из руки, если успешно легла на стол
                state.Hand.Remove(cardToPlay);

                // Передаем ход оппоненту
                SwitchTurn();
            }

            return isPlaced;
        }

        private void SwitchTurn()
        {
            CurrentPlayerId = CurrentPlayerId == Player1State.PlayerId ? Player2State.PlayerId : Player1State.PlayerId;
               
        }
    }
}
