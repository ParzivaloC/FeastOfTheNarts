using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class BoardRow
    {
        public CardRow Type { get; }

        public List<UnitCard> Cards { get; set; } = new List<UnitCard>();// Список карт, размещенных в этом ряду

        public BoardRow(CardRow type)
        {
            Type = type;
        }

        // Метод для расчета общей силы карт в ряду (по ТЕКУЩЕЙ силе, с учётом эффектов)
        public int GetTotalPower()
        {
            return Cards.Sum(c => c.CurrentPower);
        }
    }
}
