namespace FeastOfTheNarts.Core.Services
{
    // Реестр активных матчей: хранит "живые" партии в памяти сервера.
    public interface IMatchManager
    {
        // Создаёт новый матч на двух игроков, сохраняет его и возвращает движок.
        GameEngine CreateMatch(string player1Id, string player2Id);

        // Возвращает матч по id или null, если такого матча нет.
        // null (а не исключение) выбран намеренно — вызывающему проще: if (match == null) ...
        GameEngine? GetMatch(string matchId);

        // Удаляет матч (когда партия закончилась или игрок отключился).
        // true — если матч был удалён, false — если его уже не было.
        bool RemoveMatch(string matchId);
    }
}