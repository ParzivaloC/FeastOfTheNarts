//Настройка и запуск SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub") //в Program.cs для GameHub
    .build();

connection.start()
    .then(() => console.log("Успешно подключено к игровому серверу Feast of the Narts!"))
    .catch(err => console.error("Ошибка подключения: ", err.toString()));

//Drag and Drop события для карт и рядов боя
document.addEventListener("DOMContentLoaded", () => {
    initDragAndDrop();
});

function initDragAndDrop() {
    const cards = document.querySelectorAll('.game-card');
    const rows = document.querySelectorAll('.player-row'); //карты можно бросать только в свои ряды

    //навешиваем события на КАРТЫ!!!!!!!!
    cards.forEach(card => {
        card.addEventListener('dragstart', (e) => {
            //запомнить ID перетаскиваемой карты
            e.dataTransfer.setData('text/plain', e.target.id);
            setTimeout(() => card.style.opacity = '0.4', 0);
        });

        card.addEventListener('dragend', () => {
            card.style.opacity = '1';
        });
    });

    //навешиваем события на РЯДЫ БОЯ!!!!!!!!!!!!!!!!!!!
    rows.forEach(row => {
        //обязательно разрешаем сброс карты в этот элемент
        row.addEventListener('dragover', (e) => {
            e.preventDefault();
            row.classList.add('drag-over'); //включаем золотую подсветку из CSS тк больше она не нужна
        });

        //гасим подсветку, если карту унесли с ряда
        row.addEventListener('dragleave', () => {
            row.classList.remove('drag-over');
        });

        //ловим сброс карты в ряд
        row.addEventListener('drop', (e) => {
            e.preventDefault();
            row.classList.remove('drag-over');

            //ID карты из события
            const cardId = e.dataTransfer.getData('text/plain');
            const cardElement = document.getElementById(cardId);

            //получаем инфу о типе ряда (Melee, Ranged или Siege)
            const targetRowName = row.getAttribute('data-row');

            if (cardElement) {
                //Визуально перемещаем карту в ряд на экране (пока бэкенд думает)
                row.appendChild(cardElement);

                //вызов мучеников бэка, отправляет ход в GameHub
                //чистый ID карты (например, из "card-1" берем "1")
                const pureCardId = cardId.replace('card-', '');

                sendMoveToServer(pureCardId, targetRowName);
            }
        });
    });
}

//отправка хода на бэкенд
function sendMoveToServer(cardId, rowName) {
    console.log(`Отправка хода: Карта №${cardId} в ряд ${rowName}`);

    //вызов с бэканда, который проверит легитимность хода (твоя ли это карта и твой ли сейчас ход)
    connection.invoke("PlayCard", cardId, rowName)
        .then(isSuccess => {
            if (!isSuccess) {
                alert("Ход нелегитимен! (Не твой ход или не тот ряд)");
                //здесь в будущем будет логика возвращения карты обратно в руку
            }
        })
        .catch(err => console.error("Ошибка отправки хода: ", err.toString()));
}

//прием обновленного состояния игры от сервера
//когда кто-то походил, GameEngine посчитает всё и Хаб вызовет этот метод у всех клиентов
connection.on("UpdateGameState", (gameState) => {
    console.log("Получено новое состояние игры от сервера:", gameState);

    //Задача: пройтись по gameState и обновить очки на экране:
    // document.getElementById("pl-total").innerText = gameState.player1TotalPower;
    //логика отрисовки карт
});