using FeastOfTheNarts.Core.Domain.Enums;
using FeastOfTheNarts.Core.Domain.Models;
using FeastOfTheNarts.Core.Domain.RepositoryInterfaces;

namespace FeastOfTheNarts.Core.Services
{
    public class GameEngine
    {
        public string MatchId { get; }
        public GameBoard Board { get; }

        public PlayerState Player1State { get; }
        public PlayerState Player2State { get; }

        public string CurrentPlayerId { get; private set; }

        public GamePhase Phase { get; private set; } = GamePhase.NotStarted;

        // Id победителя. null — если ничья или матч ещё не окончен.
        public string? WinnerId { get; private set; }

        // Замок матча: к движку обращаются два игрока из разных потоков,
        // поэтому один ход обрабатываем за раз (см. использование в GameHub).
        public object SyncRoot { get; } = new();

        // Источник определений карт (cards.catalog.json через ICardRepository)
        private readonly ICardRepository _cards;

        public GameEngine(string matchId, string player1Id, string player2Id, ICardRepository cards)
        {
            MatchId = matchId;
            _cards = cards;
            Board = new GameBoard(player1Id, player2Id);

            Player1State = new PlayerState(player1Id);
            Player2State = new PlayerState(player2Id);

            // Имя для отображения = id игрока (это его логин)
            Player1State.DisplayName = player1Id;
            Player2State.DisplayName = player2Id;

            CurrentPlayerId = player1Id;
        }

        public void StartMatch()
        {
            /////////////////////////////Проверка 
            GenerateDummyDeck(Player1State);
            GenerateDummyDeck(Player2State);


            ////////////


            Shuffle(Player1State.Deck);
            Shuffle(Player2State.Deck);

            for (int i = 0; i < 10; i++)
            {
                Player1State.DrawCard();
                Player2State.DrawCard();
            }

            Phase = GamePhase.InProgress; // карты розданы — матч идёт
        }

        // Перемешивание колоды (Фишер–Йейтс)
        private static void Shuffle(List<BaseCard> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }




        public bool PlayCard(string playerId, string cardId, CardRow targetRow)
        {
            if (Phase != GamePhase.InProgress) return false; // нельзя ходить, пока матч не идёт / уже окончен
            if (playerId != CurrentPlayerId) return false;// проверка, что ходит текущий игрок

            // получаем состояние игрока и его игровое поле (определяем один раз, чтобы они не разъезжались)
            bool isPlayer1 = playerId == Player1State.PlayerId;
            var state = isPlayer1 ? Player1State : Player2State;
            var board = isPlayer1 ? Board.Player1Board : Board.Player2Board;
            var enemyState = isPlayer1 ? Player2State : Player1State;

            var card = state.Hand.FirstOrDefault(c => c.Id == cardId);// проверяем, что карта есть в руке
            if (card == null) return false; // Карты нет в руке

            // Разные типы карт играются по-разному
            bool played = card switch
            {
                UnitCard unit => PlayUnit(unit, board, targetRow),
                EventCard ev => PlayEvent(ev, state),
                SpellCard spell => PlaySpell(spell, state, enemyState),
                _ => false
            };

            if (played)
            {
                state.Hand.Remove(card); // убрали из руки разыгранную карту
                SwitchTurn();            // передаём ход
            }

            return played;
        }

        // ---------- Разыгрывание юнита ----------
        private bool PlayUnit(UnitCard unit, PlayerBoard board, CardRow targetRow)
        {
            if (!board.PlaceCard(unit, targetRow)) return false; // PlaceCard ставит CurrentPower = BasePower

            // Сначала на карту действуют уже активные глобальные эффекты (напр. Колесо Балсага)
            ApplyActiveEffectsToCard(unit, targetRow);

            // Затем срабатывает собственная способность "при выходе на поле"
            TriggerEnterAbility(unit, board);
            return true;
        }

        // Применяет к ОДНОЙ только что выставленной карте уже действующие на столе эффекты
        private void ApplyActiveEffectsToCard(UnitCard card, CardRow row)
        {
            foreach (var effect in Board.ActiveEffects)
            {
                if (effect.Effect == EffectType.BalsagWheel && row == CardRow.Melee && !card.IsHero)
                    card.CurrentPower = 1;
            }
        }

        // Запускает способность героя "при выходе на поле"
        private void TriggerEnterAbility(UnitCard unit, PlayerBoard board)
        {
            switch (unit.Ability)
            {
                case HeroAbility.BatradzRage:
                    ResolveBatradzRage(unit);
                    break;
                case HeroAbility.KhamytsRally:
                    ResolveKhamytsRally(board);
                    break;
                case HeroAbility.AtsamazMelody:
                    ResolveAtsamazMelody(unit, board);
                    break;
                // SoslanImmunity — пассивная, на выход не реагирует
            }
        }

        // Ярость Батрадза: 2 урона всем в обоих рядах воинов (кроме себя), +1 за каждый удар;
        // отряд с силой <= 0 погибает и уходит в сброс владельца.
        private void ResolveBatradzRage(UnitCard batradz)
        {
            int rage = 0;

            foreach (var owner in new[] { Player1State, Player2State })
            {
                var melee = BoardOf(owner).MeleeRow;
                var killed = new List<UnitCard>();

                foreach (var card in melee.Cards)
                {
                    if (card == batradz) continue;
                    card.CurrentPower -= 2;
                    rage++;
                    if (card.CurrentPower <= 0) killed.Add(card);
                }

                foreach (var dead in killed)
                {
                    melee.Cards.Remove(dead);
                    owner.DiscardPile.Add(dead);
                }
            }

            batradz.CurrentPower += rage;
        }

        //==========================================================Проверка 
        private void GenerateDummyDeck(PlayerState state)
        {
            int idOffset = state.PlayerId == Player1State.PlayerId ? 1000 : 2000;
            int[] imgs = { 1, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }; // доступные картинки из wwwroot/images

            for (int i = 1; i <= 20; i++)
            {
                var row = i % 3 == 0 ? CardRow.Melee : (i % 3 == 1 ? CardRow.Ranged : CardRow.Siege);

                state.Deck.Add(new UnitCard
                {
                    Id = (idOffset + i).ToString(),
                    Name = $"Нарт {i}",
                    BasePower = Random.Shared.Next(1, 11),
                    IsHero = i % 10 == 0,
                    TargetRow = row,
                    ImageUrl = $"/images/Pokrov_fullllly {imgs[i % imgs.Length]}.png"
                });
            }

            // Именные карты "Пира Нартов" берём из каталога (cards.catalog.json),
            // клонируя под каждого игрока (см. CloneCard)
            state.Deck.AddRange(BuildCatalogCards());
        }
        //===========================================================

        // Свежие копии всех карт каталога для одной колоды
        private List<BaseCard> BuildCatalogCards() =>
            _cards.GetUnitCards().Cast<BaseCard>()
                  .Concat(_cards.GetEventCards())
                  .Concat(_cards.GetSpellCards())
                  .Select(CloneCard)
                  .ToList();

        // Клон карты: репозиторий хранит один экземпляр на определение, а на столе
        // силу карты меняют эффекты — поэтому каждому игроку нужна своя копия
        private static BaseCard CloneCard(BaseCard card) => card switch
        {
            UnitCard u => new UnitCard
            {
                Id = u.Id, Name = u.Name, Description = u.Description, Faction = u.Faction,
                ImageUrl = u.ImageUrl, BasePower = u.BasePower, TargetRow = u.TargetRow,
                IsHero = u.IsHero, Ability = u.Ability
            },
            EventCard e => new EventCard
            {
                Id = e.Id, Name = e.Name, Description = e.Description, Faction = e.Faction,
                ImageUrl = e.ImageUrl, Effect = e.Effect
            },
            SpellCard s => new SpellCard
            {
                Id = s.Id, Name = s.Name, Description = s.Description, Faction = s.Faction,
                ImageUrl = s.ImageUrl, Effect = s.Effect
            },
            _ => card
        };

        // Быстрый наскок Хамыца: призвать из своей колоды всех обычных нартов с базовой силой <= 4 в ряд Бората (Ranged)
        private void ResolveKhamytsRally(PlayerBoard board)
        {
            var ownerState = StateOf(board);

            var recruits = ownerState.Deck.OfType<UnitCard>()
                                          .Where(u => !u.IsHero && u.BasePower <= 4)
                                          .ToList();
            foreach (var recruit in recruits)
            {
                ownerState.Deck.Remove(recruit);
                recruit.CurrentPower = recruit.BasePower;
                board.RangedRow.Cards.Add(recruit);              // призыв минует обычную проверку ряда
                ApplyActiveEffectsToCard(recruit, CardRow.Ranged);
            }
        }

        // Песнь Ацамаза: +2 всем ОСТАЛЬНЫМ отрядам в рядах Бората (Ranged) и Алагата (Siege) на своей стороне
        private void ResolveAtsamazMelody(UnitCard atsamaz, PlayerBoard board)
        {
            foreach (var card in board.RangedRow.Cards.Concat(board.SiegeRow.Cards))
            {
                if (card == atsamaz) continue;
                card.CurrentPower += 2;
            }
        }

        // ---------- Разыгрывание глобального события ----------
        private bool PlayEvent(EventCard ev, PlayerState state)
        {
            switch (ev.Effect)
            {
                case EffectType.BalsagWheel:
                    Board.ActiveEffects.Add(ev);   // событие остаётся лежать на столе
                    ApplyBalsagWheel();
                    break;

                case EffectType.ShatanaWisdom:
                    Board.ActiveEffects.Clear();    // рассеивает все глобальные бедствия
                    ResetAllUnitsToBase();
                    state.DiscardPile.Add(ev);      // сама уходит в сброс после применения
                    break;

                default:
                    return false;
            }
            return true;
        }

        // Снижает текущую силу всех обычных (не геройских) карт в обоих рядах воинов до 1
        private void ApplyBalsagWheel()
        {
            foreach (var board in new[] { Board.Player1Board, Board.Player2Board })
                foreach (var card in board.MeleeRow.Cards)
                    if (!card.IsHero) card.CurrentPower = 1;
        }

        // Возвращает всем простым картам базовую силу (используется Мудростью Шатаны)
        private void ResetAllUnitsToBase()
        {
            foreach (var board in new[] { Board.Player1Board, Board.Player2Board })
                foreach (var row in board.Rows)
                    foreach (var card in row.Cards)
                        if (!card.IsHero) card.CurrentPower = card.BasePower;
        }

        // ---------- Разыгрывание мгновенного заклинания ----------
        private bool PlaySpell(SpellCard spell, PlayerState caster, PlayerState enemy)
        {
            switch (spell.Effect)
            {
                case SpellEffect.SyrdonTheft:
                    // Сильнейший юнит из сброса врага — себе в руку
                    var loot = enemy.DiscardPile.OfType<UnitCard>()
                                                .OrderByDescending(u => u.BasePower)
                                                .FirstOrDefault();
                    if (loot != null)
                    {
                        enemy.DiscardPile.Remove(loot);
                        caster.Hand.Add(loot);
                    }
                    break;

                case SpellEffect.SoslansKnees:
                    KillSoslan();
                    break;

                default:
                    return false;
            }

            caster.DiscardPile.Add(spell); // заклинание уходит в сброс
            return true;
        }

        // Находит Сослана где угодно на столе и уничтожает, игнорируя иммунитет
        private void KillSoslan()
        {
            foreach (var owner in new[] { Player1State, Player2State })
            {
                foreach (var row in BoardOf(owner).Rows)
                {
                    var soslan = row.Cards.FirstOrDefault(c => c.Ability == HeroAbility.SoslanImmunity);
                    if (soslan != null)
                    {
                        row.Cards.Remove(soslan);
                        owner.DiscardPile.Add(soslan);
                        return;
                    }
                }
            }
        }

        // ---------- Вспомогательные сопоставления игрок <-> поле ----------
        private PlayerBoard BoardOf(PlayerState state) =>
            state == Player1State ? Board.Player1Board : Board.Player2Board;

        private PlayerState StateOf(PlayerBoard board) =>
            board == Board.Player1Board ? Player1State : Player2State;

        private void SwitchTurn()
        {
            if (Player1State.HasPassed && Player2State.HasPassed)
            {
                ResolveRound();
                return;
            }

            //смена текущего игрока на противположного после хода
            if (CurrentPlayerId == Player1State.PlayerId)
            {
                CurrentPlayerId = Player2State.PlayerId;
            }
            else
            {
                CurrentPlayerId = Player1State.PlayerId;
            }


            //Проверка текущего игрока и если он уже спасовал , возвращаем ход другому игроку
            if (CurrentPlayerId == Player1State.PlayerId && Player1State.HasPassed)
            {
                CurrentPlayerId = Player2State.PlayerId;
            }
            else if (CurrentPlayerId == Player2State.PlayerId && Player2State.HasPassed)
            {
                CurrentPlayerId = Player1State.PlayerId;
            }
        }

        private void ResolveRound()
        {
            int p1Score = Board.Player1Board.GetTotalPower();
            int p2Score = Board.Player2Board.GetTotalPower();

            if (p1Score > p2Score)
            {
                Player2State.Lives -= 1;
                CurrentPlayerId = Player1State.PlayerId;

            }
            else if (p2Score > p1Score)
            {
                Player1State.Lives -= 1;
                CurrentPlayerId = Player2State.PlayerId;
            }
            else
            {
                Player1State.Lives -= 1;
                Player2State.Lives -= 1;
            }
            ClearBoard();

            Player1State.HasPassed = false;
            Player2State.HasPassed = false;

            CheckForGameEnd();
        }

        // Проверяет, не закончился ли матч после потери жизней
        private void CheckForGameEnd()
        {
            bool p1Dead = Player1State.Lives <= 0;
            bool p2Dead = Player2State.Lives <= 0;

            if (!p1Dead && !p2Dead) return; // оба живы — матч продолжается

            Phase = GamePhase.Finished;

            if (p1Dead && p2Dead) WinnerId = null;                 // ничья — оба проиграли в один раунд
            else if (p2Dead) WinnerId = Player1State.PlayerId; // выжил первый
            else WinnerId = Player2State.PlayerId; // выжил второй
        }

        public void ClearBoard()
        {
            Player1State.DiscardPile.AddRange(Board.Player1Board.MeleeRow.Cards);
            Player1State.DiscardPile.AddRange(Board.Player1Board.RangedRow.Cards);
            Player1State.DiscardPile.AddRange(Board.Player1Board.SiegeRow.Cards);

            Board.Player1Board.MeleeRow.Cards.Clear();
            Board.Player1Board.RangedRow.Cards.Clear();
            Board.Player1Board.SiegeRow.Cards.Clear();

            Player2State.DiscardPile.AddRange(Board.Player2Board.MeleeRow.Cards);
            Player2State.DiscardPile.AddRange(Board.Player2Board.RangedRow.Cards);
            Player2State.DiscardPile.AddRange(Board.Player2Board.SiegeRow.Cards);

            Board.Player2Board.MeleeRow.Cards.Clear();
            Board.Player2Board.RangedRow.Cards.Clear();
            Board.Player2Board.SiegeRow.Cards.Clear();

            Board.ActiveEffects.Clear(); // в новый раунд глобальные события не переносятся
        }


        public void PassTurn(string playerId)
        {
            if (Phase != GamePhase.InProgress) return;
            if (playerId != CurrentPlayerId) return; // пасовать можно только в свой ход

            if (playerId == Player1State.PlayerId)
            {
                Player1State.HasPassed = true;
            }
            else if (playerId == Player2State.PlayerId)
            {
                Player2State.HasPassed = true;
            }
            SwitchTurn();
        }
    }
}
