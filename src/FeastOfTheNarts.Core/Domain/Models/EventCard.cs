using FeastOfTheNarts.Core.Domain.Enums;

namespace FeastOfTheNarts.Core.Domain.Models
{
    // Глобальное событие: влияет на всё поле. Длящиеся события (напр. Колесо Балсага)
    // остаются в списке активных эффектов стола, пока их не рассеют.
    public class EventCard : BaseCard
    {
        public EffectType Effect { get; init; }
    }
}
