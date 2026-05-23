using FeastOfTheNarts.Core.Domain.Enums;


namespace FeastOfTheNarts.Core.Domain.Enums.Models
{
    public abstract class BaseCard
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Faction Faction { get; init; }
        public string ImageUrl { get; init; } = string.Empty;
    }
}
