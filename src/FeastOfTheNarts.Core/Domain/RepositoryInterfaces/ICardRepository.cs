using FeastOfTheNarts.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeastOfTheNarts.Core.Domain.RepositoryInterfaces
{
    public interface ICardRepository
    {
        IEnumerable<UnitCard> GetUnitCards();
        IEnumerable<EventCard> GetEventCards();
        IEnumerable<SpellCard> GetSpellCards();
        UnitCard GetUnitCard(string id);
        EventCard GetEventCard(string id);
        SpellCard GetSpellCard(string id);
    }

    public class SpellCard
    {
    }

    public class EventCard
    {
    }
}
