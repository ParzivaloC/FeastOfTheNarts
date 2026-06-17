using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Domain.Models;

namespace FeastOfTheNarts.Core.Services
{
    // Каталог-фабрика особых карт "Пира Нартов".
    // Каждый метод создаёт НОВЫЙ экземпляр — нельзя отдавать один объект двум
    // игрокам, иначе изменение силы у одного отразится у другого.
    public static class CardCatalog
    {
        // Полный набор особых карт (по одному экземпляру каждой).
        public static List<BaseCard> CreateSpecialCards() => new()
        {
            Soslan(), Batradz(), Khamyts(), Atsamaz(),
            BalsagWheel(), ShatanaWisdom(),
            SyrdonTheft(), SoslansKnees()
        };

        // ===== Легендарные герои (UnitCard со способностями) =====

        public static UnitCard Soslan() => new()
        {
            Id = "hero_soslan",
            Name = "Сослан",
            Description = "Бесстрашный нарт, закалённый в волчьем молоке и расплавленном железе кузнецом Курдалагоном.",
            Faction = Faction.Ahsartaggata,
            BasePower = 10,
            TargetRow = CardRow.Melee,
            IsHero = true,
            Ability = HeroAbility.SoslanImmunity
        };

        public static UnitCard Batradz() => new()
        {
            Id = "hero_batradz",
            Name = "Батрадз",
            Description = "Стальной нарт, который в гневе раскаляется докрасна и становится неостановимым буйством природы.",
            Faction = Faction.Ahsartaggata,
            BasePower = 7,
            TargetRow = CardRow.Melee,
            IsHero = true,
            Ability = HeroAbility.BatradzRage
        };

        public static UnitCard Khamyts() => new()
        {
            Id = "hero_khamyts",
            Name = "Хамыц",
            Description = "Брат Сослана, бесстрашный наездник и мастер внезапных кавалерийских атак.",
            Faction = Faction.Borata,
            BasePower = 6,
            TargetRow = CardRow.Ranged,
            IsHero = true,
            Ability = HeroAbility.KhamytsRally
        };

        public static UnitCard Atsamaz() => new()
        {
            Id = "hero_atsamaz",
            Name = "Ацамаз",
            Description = "Легендарный нартский музыкант: от его золотой свирели таяли ледники и пробуждались звери.",
            Faction = Faction.Borata,
            BasePower = 5,
            TargetRow = CardRow.Ranged,
            IsHero = true,
            Ability = HeroAbility.AtsamazMelody
        };

        // ===== Глобальные события (EventCard) =====

        public static EventCard BalsagWheel() => new()
        {
            Id = "event_balsag_wheel",
            Name = "Колесо Балсага",
            Description = "Мифическое огненное колесо, покатившееся с небес по воле небожителей.",
            Faction = Faction.Neutral,
            Effect = EffectType.BalsagWheel
        };

        public static EventCard ShatanaWisdom() => new()
        {
            Id = "event_shatana_wisdom",
            Name = "Мудрость Шатаны",
            Description = "Великая вещая матерь нартов, способная предотвратить любую беду.",
            Faction = Faction.Neutral,
            Effect = EffectType.ShatanaWisdom
        };

        // ===== Мгновенные заклинания (SpellCard) =====

        public static SpellCard SyrdonTheft() => new()
        {
            Id = "spell_syrdon_theft",
            Name = "Интрига Сырдона",
            Description = "Главный трикстер нартских сказаний, мастер тайных похищений.",
            Faction = Faction.Neutral,
            Effect = SpellEffect.SyrdonTheft
        };

        public static SpellCard SoslansKnees() => new()
        {
            Id = "spell_soslans_knees",
            Name = "Колени Сослана",
            Description = "Единственное уязвимое место великого героя, до которого не дошло закалочное железо.",
            Faction = Faction.Neutral,
            Effect = SpellEffect.SoslansKnees
        };
    }
}
