using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class UnitCard : BaseCard
    {
        public int BasePower { get; init; }      // неизменная "паспортная" сила карты
        public CardRow TargetRow { get; init; }
        public bool IsHero { get; init; }
        public HeroAbility Ability { get; init; } = HeroAbility.None;

        // Текущая сила на столе: меняется эффектами (погода, бафы, урон).
        // Выставляется в BasePower в момент выхода карты на поле.
        public int CurrentPower { get; set; }
    }
}
