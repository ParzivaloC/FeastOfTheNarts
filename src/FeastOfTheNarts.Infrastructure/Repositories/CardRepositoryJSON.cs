using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Domain.Models;
using FeastOfTheNarts.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeastOfTheNarts.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly List<UnitCard> _unitCards;
        private readonly List<EventCard> _eventCards;
        private readonly List<SpellCard> _spellCards;

        public CardRepository()
        {
            _unitCards = [];
            _eventCards = [];
            _spellCards = [];

            var json = File.ReadAllText("../Data/cards.catalog.json");
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("unitCards", out var unitCardsElement))
                foreach (var element in unitCardsElement.EnumerateArray())
                    _unitCards.Add(new UnitCard
                    {
                        Id = element.GetProperty("id").GetString()!,
                        Name = element.GetProperty("name").GetString()!,
                        Description = element.GetProperty("description").GetString()!,
                        Faction = Enum.Parse<Faction>(element.GetProperty("faction").GetString()!),
                        BasePower = element.GetProperty("basePower").GetInt32(),
                        TargetRow = Enum.Parse<CardRow>(element.GetProperty("targetRow").GetString()!),
                        IsHero = element.GetProperty("isHero").GetBoolean(),
                        Ability = Enum.Parse<HeroAbility>(element.GetProperty("ability").GetString()!)
                    });

            if (root.TryGetProperty("eventCards", out var eventCardsElement))
                foreach (var element in eventCardsElement.EnumerateArray())
                    _eventCards.Add(new EventCard
                    {
                        Id = element.GetProperty("id").GetString()!,
                        Name = element.GetProperty("name").GetString()!,
                        Description = element.GetProperty("description").GetString()!,
                        Faction = Enum.Parse<Faction>(element.GetProperty("faction").GetString()!),
                        Effect = Enum.Parse<EffectType>(element.GetProperty("effect").GetString()!)
                    });

            if (root.TryGetProperty("spellCards", out var spellCardsElement))
                foreach (var element in spellCardsElement.EnumerateArray())
                    _spellCards.Add(new SpellCard
                    {
                        Id = element.GetProperty("id").GetString()!,
                        Name = element.GetProperty("name").GetString()!,
                        Description = element.GetProperty("description").GetString()!,
                        Faction = Enum.Parse<Faction>(element.GetProperty("faction").GetString()!),
                        Effect = Enum.Parse<SpellEffect>(element.GetProperty("effect").GetString()!)
                    });
        }

        public IEnumerable<UnitCard> GetUnitCards() => _unitCards;
        public IEnumerable<EventCard> GetEventCards() => _eventCards;
        public IEnumerable<SpellCard> GetSpellCards() => _spellCards;

        public UnitCard? GetUnitCard(string id) => _unitCards.FirstOrDefault(c => c.Id == id);
        public EventCard? GetEventCard(string id) => _eventCards.FirstOrDefault(c => c.Id == id);
        public SpellCard? GetSpellCard(string id) => _spellCards.FirstOrDefault(c => c.Id == id);
    }

}
