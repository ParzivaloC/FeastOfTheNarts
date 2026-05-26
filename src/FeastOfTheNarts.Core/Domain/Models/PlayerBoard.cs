using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class PlayerBoard
    {
        public string PlayerId { get; }

        public BoardRow MeleeRow { get; } = new (CardRow.Melee);
        public BoardRow RangedRow { get; } = new(CardRow.Ranged);
        public BoardRow SiegeRow { get; } = new(CardRow.Siege);

        public PlayerBoard(string playerId)
        {
            PlayerId = playerId;
        }

        public int GetTotalPower()
        {
            return MeleeRow.GetTotalPower() + RangedRow.GetTotalPower() + SiegeRow.GetTotalPower();
        }

        public bool PlaceCard(UnitCard card , CardRow targetRow)
        {
            if (card.TargetRow != targetRow && card.TargetRow != CardRow.Special)
                return false; // Карта не может быть размещена в этом ряду)

            switch (card.TargetRow)
            {
                case CardRow.Melee:
                    MeleeRow.Cards.Add(card);
                    break;
                case CardRow.Ranged:
                    RangedRow.Cards.Add(card);
                    break;
                case CardRow.Siege:
                    SiegeRow.Cards.Add(card);
                    break;
                default:
                    return false; // Невозможно разместить карту
            }
            return true;
        }


    }
}
