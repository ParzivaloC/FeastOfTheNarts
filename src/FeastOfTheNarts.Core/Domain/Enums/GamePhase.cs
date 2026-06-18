namespace FeastOfTheNarts.Core.Domain.Enums
{
    // Фаза матча — в каком состоянии находится партия.
    public enum GamePhase
    {
        NotStarted,  // матч создан, но StartMatch ещё не вызван
        InProgress,  // идёт игра
        Finished     // партия окончена (есть победитель или ничья)
    }
}
