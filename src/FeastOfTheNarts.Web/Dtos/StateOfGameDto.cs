using System.Numerics;

namespace FeastOfTheNarts.Web.Dtos
{
    public class StateOfGameDto
    {
        //одна карта
        public class CardDto
        {
            public string Id { get; init; } = "";
            public string Name { get; init; } = "";
            public string Description { get; init; } = "";
            public string ImageUrl { get; init; } = "";
            public string Type { get; init; } = "";  // "Unit", "Event", "Spell"
            public int Power { get; init; }
            public string? Row { get; init; }
            public bool IsHero { get; init; }
        }

        //один ряд 
        public class BoardRowDto
        {
            public string Type { get; init; } = ""; //"Melee", "Ranged", "Siege"
            public int Power { get; init; } //сумма сил в ряду
            public List<CardDto> Cards { get; init; } = new();
        }

        //Состояния игрока со стороны просмотра зрителя 
        public class PlayerStateDto
        {
            public string PlayerId { get; init; } = "";
            public string DisplayName { get; init; } = "";
            public int Lives { get; init; }
            public bool HasPassed { get; init; }
            public int TotalPower { get; init; }
            public int DeckCount { get; init; }
            public int HandCount { get; init; }


            //Замена только для игрока , соперник не видит руки тк она скрыта 
            public List<CardDto> Hand { get; init; } = new();
            public BoardRowDto Melee { get; init; } = new();
            public BoardRowDto Ranged { get; init; } = new();
            public BoardRowDto Siege { get; init; } = new();

        }
        public class GameStateDto
        {
            public string MatchId { get; init; } = "";
            public string Phase { get; init; } = ""; // "NotStarted", "InProgress", "Finished"
            public string CurrentPlayerId { get; init; } = "";
            public string? WinnerId { get; init; }
            public string? Result { get; init; } // "Victory", "Defeat", "Draw" - для зрителя, может быть null если игра не закончена
            public bool IsYourTurn { get; init; }

            public PlayerStateDto You { get; init; } = new();
            public PlayerStateDto Opponent { get; init; } = new();


        }


    }
}
