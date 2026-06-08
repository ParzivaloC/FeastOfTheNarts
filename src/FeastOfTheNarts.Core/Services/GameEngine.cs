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
            GenerateDummyDeck(Player1State);
            GenerateDummyDeck(Player2State);

            Shuffle(Player1State.Deck);
            Shuffle(Player2State.Deck);

            for (int i = 0; i < 10; i++)
            {
                Player1State.DrawCard();
                Player2State.DrawCard();
            }
        }

        // Перемешивание колоды (Фишер–Йейтс)
        private static void Shuffle(List<UnitCard> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        //==========================================================Проверка 
        private void GenerateDummyDeck(PlayerState state)
        {
            int idOffset = state.PlayerId == Player1State.PlayerId ? 1000 : 2000;

            for (int i = 1; i <= 20; i++)
            {
                var row = i % 3 == 0 ? CardRow.Melee : (i % 3 == 1 ? CardRow.Ranged : CardRow.Siege);

                state.Deck.Add(new UnitCard
                {
                    Id = (idOffset + i).ToString(),
                    BasePower = Random.Shared.Next(1, 11),
                    IsHero = i % 10 == 0,
                    TargetRow = row
                });
            }
        }
        //===========================================================




        public bool PlayCard(string playerId, string cardId, CardRow targetRow)
        {
            if (playerId != CurrentPlayerId) return false;// проверка, что ходит текущий игрок

            // получаем состояние игрока и его игровое поле (определяем один раз, чтобы они не разъезжались)
            bool isPlayer1 = playerId == Player1State.PlayerId;
            var state = isPlayer1 ? Player1State : Player2State;
            var playerBoard = isPlayer1 ? Board.Player1Board : Board.Player2Board;

            var cardToPlay = state.Hand.FirstOrDefault(c => c.Id == cardId);
            if (cardToPlay == null) return false; // Карты нет в руке

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
            if (Player1State.HasPassed && Player2State.HasPassed)
            {
                ResolveRound();             
                return;
            }
            
            //смена текущего игрока на противположного после хода 
            if (CurrentPlayerId == Player1State.PlayerId) 
            {
                CurrentPlayerId = Player2State.PlayerId;
            }
            else
            {
                CurrentPlayerId = Player1State.PlayerId;
            }


            //Проверка текущего игрока и если он уже спасовал , возвращаем ход другому игроку
            if (CurrentPlayerId == Player1State.PlayerId && Player1State.HasPassed)
            {
                CurrentPlayerId = Player2State.PlayerId;
            }
            else if (CurrentPlayerId == Player2State.PlayerId && Player2State.HasPassed)
            {
                CurrentPlayerId = Player1State.PlayerId;
            }  
        }

        private void ResolveRound()
        { 
            int p1Score = Board.Player1Board.GetTotalPower();
            int p2Score = Board.Player2Board.GetTotalPower();

            if (p1Score > p2Score)
            {
                Player2State.Lives -= 1;
                CurrentPlayerId = Player1State.PlayerId;

            }
            else if (p2Score > p1Score)
            {
                Player1State.Lives -= 1;
                CurrentPlayerId = Player2State.PlayerId;
            }
            else
            {
                Player1State.Lives -= 1;
                Player2State.Lives -= 1;
            }
            ClearBoard();

            Player1State.HasPassed = false;
            Player2State.HasPassed = false; 
        }

        public void ClearBoard()
        {
            Player1State.DiscardPile.AddRange(Board.Player1Board.MeleeRow.Cards);
            Player1State.DiscardPile.AddRange(Board.Player1Board.RangedRow.Cards);
            Player1State.DiscardPile.AddRange(Board.Player1Board.SiegeRow.Cards);

            Board.Player1Board.MeleeRow.Cards.Clear();
            Board.Player1Board.RangedRow.Cards.Clear();
            Board.Player1Board.SiegeRow.Cards.Clear();

            Player2State.DiscardPile.AddRange(Board.Player2Board.MeleeRow.Cards);
            Player2State.DiscardPile.AddRange(Board.Player2Board.RangedRow.Cards);
            Player2State.DiscardPile.AddRange(Board.Player2Board.SiegeRow.Cards);

            Board.Player2Board.MeleeRow.Cards.Clear();
            Board.Player2Board.RangedRow.Cards.Clear();
            Board.Player2Board.SiegeRow.Cards.Clear();
        }


        public void PassTurn(string playerId)
        {
            if (playerId == Player1State.PlayerId)
            {
                Player1State.HasPassed = true;
            }
            else if (playerId == Player2State.PlayerId)
            {
                Player2State.HasPassed = true;
            }
            SwitchTurn();
        }
    }
}
