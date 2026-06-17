using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class GameBoard
    {
        public PlayerBoard Player1Board { get; }

        public PlayerBoard Player2Board { get; }

        // Активные глобальные события (напр. Колесо Балсага), лежащие на столе.
        // Они общие для всего поля, поэтому хранятся здесь, а не у игрока.
        public List<EventCard> ActiveEffects { get; } = new();

        public GameBoard(string player1Id, string player2Id)
        {
            Player1Board = new PlayerBoard(player1Id);
            Player2Board = new PlayerBoard(player2Id);
        }
    }
}
