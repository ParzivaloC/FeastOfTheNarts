namespace FeastOfTheNarts.Core.Domain.Models
{
    public class PlayerState(string playerId)
    {
        public string PlayerId { get; set; } = playerId;
        public string DisplayName { get; set; } = string.Empty;

        public int Lives { get; set; } = 2;

        //Флаг паса (если игрок спасовал, он больше не ходит в этом раунде)
        public bool HasPassed { get; set; } = false;

        // Игровые зоны (колода , рука , сброс). Тип BaseCard — чтобы хранить
        // карты любого вида: юнитов, события и заклинания.
        public List<BaseCard> Deck { get; set; } = new List<BaseCard>();
        public List<BaseCard> Hand { get; set; } = new List<BaseCard>();
        public List<BaseCard> DiscardPile { get; set; } = new List<BaseCard>();

        public void DrawCard()
        {
            if (Deck.Count > 0)
            {
                var card = Deck[0];
                Deck.RemoveAt(0);
                Hand.Add(card);
            }
        }
    }
}