using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    // Мгновенное заклинание: нет собственной силы и ряда. Срабатывает из руки
    // и сразу уходит в сброс (DiscardPile).
    public class SpellCard : BaseCard
    {
        public SpellEffect Effect { get; init; }
    }
}
