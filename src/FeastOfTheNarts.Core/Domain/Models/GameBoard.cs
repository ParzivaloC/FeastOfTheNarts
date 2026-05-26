using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class GameBoard
    {
        public PlayerBoard Player1Board { get; }

        public PlayerBoard Player2Board { get; }

        public GameBoard(string player1Id, string player2Id)
        {
            Player1Board = new PlayerBoard(player1Id);
            Player2Board = new PlayerBoard(player2Id);
        }
    }
}
