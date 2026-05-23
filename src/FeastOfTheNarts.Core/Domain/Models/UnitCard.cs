using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Domain.Enums.Models;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class UnitCard : BaseCard
    {
        public int BasePower { get; init; }
        public CardRow TargetRow { get; init; }
        public bool IsHero { get; init; }

    }
}
