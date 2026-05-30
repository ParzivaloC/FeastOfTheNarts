const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .build();

connection.start()
    .then(() => {
        console.log("Успешно подключено к игровому серверу Пир Нартов!");

        connection.invoke("JoinMatch", "Player_Soslan");
    })
    .catch(err => console.error("Ошибка подключения: ", err.toString()));



connection.on("UpdateGameState", (gameState) => {
    console.log("Получены реальные данные от движка:", gameState);
    renderGameState(gameState);
});

// ТЕСТОВЫЙ ЗАПУСК ДЛЯ ПРОВЕРКИ ИНТЕРФЕЙСА
document.addEventListener("DOMContentLoaded", () => {
    console.log("Интерфейс готов к работе.");

    const mockServerResponse = {
        currentPlayerId: "Player_Soslan",

        playerState: {
            playerId: "Player_Soslan",
            lives: 2,
            hand: [
                { id: "1002", name: "Батраз", basePower: 5, targetRow: "Ranged" },
                { id: "1003", name: "Сырдон", basePower: 2, targetRow: "Siege" }
            ],
            deckCount: 18
        },

        opponentState: {
            playerId: "Player_Batradz",
            lives: 2,
            handCount: 9
        },
        //из гейм борд
        board: {
            playerBoard: {
                meleeRow: { totalPower: 7, cards: [{ id: "1001", name: "Сослан", basePower: 7, targetRow: "Melee" }] },
                rangedRow: { totalPower: 0, cards: [] },
                siegeRow: { totalPower: 0, cards: [] },
                totalPower: 7
            },
            opponentBoard: {
                meleeRow: { totalPower: 4, cards: [{ id: "2001", name: "Уастырджи", basePower: 4, targetRow: "Melee" }] },
                rangedRow: { totalPower: 0, cards: [] },
                siegeRow: { totalPower: 0, cards: [] },
                totalPower: 4
            }
        }
    };

    //отрисовываем поле боя стартовыми данными
    renderGameState(mockServerResponse);
});

//обновление
function renderGameState(data) {
    //обновлениек очков, жизни и счетчиков карт на панелях
    document.getElementById("pl-total").innerText = data.board.playerBoard.totalPower;
    document.getElementById("pl-lives").innerText = data.playerState.lives;
    document.getElementById("pl-deck-count").innerText = data.playerState.deckCount;

    document.getElementById("opt-total").innerText = data.board.opponentBoard.totalPower;
    document.getElementById("opt-lives").innerText = data.opponentState.lives;
    document.getElementById("opt-hand-count").innerText = data.opponentState.handCount;

    //очистка и отрисовка руки игрока
    const handContainer = document.getElementById("my-hand");
    handContainer.innerHTML = "";
    data.playerState.hand.forEach(card => {
        handContainer.appendChild(createCardElement(card, true));//true- карту можно тащить
    });

    //очистка и отрисовывка БОЕВЫЕ РЯДЫ ИГРОКА
    renderRowCards(".player-row.melee", data.board.playerBoard.meleeRow, "pl-score-melee");
    renderRowCards(".player-row.ranged", data.board.playerBoard.rangedRow, "pl-score-ranged");
    renderRowCards(".player-row.siege", data.board.playerBoard.siegeRow, "pl-score-siege");

    //очистка и отрисовывка БОЕВЫЕ РЯДЫ СОПЕРНИКА
    renderRowCards(".opponent-row.melee", data.board.opponentBoard.meleeRow, "opt-score-melee");
    renderRowCards(".opponent-row.ranged", data.board.opponentBoard.rangedRow, "opt-score-ranged");
    renderRowCards(".opponent-row.siege", data.board.opponentBoard.siegeRow, "opt-score-siege");

    //перезапускаем слушатели событий Drag & Drop для новых карт в руке
    initDragAndDrop();
}

//для отрисовки карт внутри конкретного ряда
function renderRowCards(rowSelector, rowData, scoreId) {
    const rowElement = document.querySelector(rowSelector);
    const cardsContainer = rowElement.querySelector(".row-cards-container");

    //обновляем счетчик очков конкретного ряда
    document.getElementById(scoreId).innerText = rowData.totalPower;

    //очищаем ряд от старых карт
    cardsContainer.innerHTML = "";

    //рисуем новые карты в этом ряду, их уже нельзя перетаскивать, поэтому false
    rowData.cards.forEach(card => {
        cardsContainer.appendChild(createCardElement(card, false));
    });
}

//геренация HTML карты
function createCardElement(card, isDraggable) {
    const cardDiv = document.createElement("div");
    cardDiv.className = "game-card";
    if (isDraggable) {
        cardDiv.setAttribute("draggable", "true");
    }
    cardDiv.id = `card-${card.id}`;

    let icon = "⚔️";
    if (card.targetRow === "Ranged") icon = "🏹";
    if (card.targetRow === "Siege") icon = "🔮";

    cardDiv.innerHTML = `
        <div class="card-power">${icon} ${card.basePower}</div>
        <div class="card-name">${card.name}</div>
    `;
    return cardDiv;
}


// Логика Drag & Drop
function initDragAndDrop() {
    const cards = document.querySelectorAll('.game-card');
    const rows = document.querySelectorAll('.player-row');

    cards.forEach(card => {
        card.replaceWith(card.cloneNode(true)); //очистка старых слушателей, чтобы не дублировались
    });

    //пересобор актуальных карт после очистки клонированием
    const activeCards = document.querySelectorAll('.game-card');

    activeCards.forEach(card => {
        card.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', e.target.id);
            setTimeout(() => card.style.opacity = '0.4', 0);
        });

        card.addEventListener('dragend', () => {
            card.style.opacity = '1';
        });
    });

    rows.forEach(row => {
        row.addEventListener('dragover', (e) => {
            e.preventDefault();
            row.classList.add('drag-over');
        });

        row.addEventListener('dragleave', () => {
            row.classList.remove('drag-over');
        });

        row.addEventListener('drop', (e) => {
            e.preventDefault();
            row.classList.remove('drag-over');

            const cardId = e.dataTransfer.getData('text/plain');
            const cardElement = document.getElementById(cardId);
            const targetRowName = row.getAttribute('data-row');

            if (cardElement) {
                row.appendChild(cardElement);
                const pureCardId = cardId.replace('card-', '');
                sendMoveToServer(pureCardId, targetRowName);
            }
        });
    });
}

function sendMoveToServer(cardId, rowName) {
    console.log(`Отправка на сервер: Карта ${cardId} в ряд ${rowName}`);

    connection.invoke("PlayCard", cardId, rowName)
        .then(isSuccess => {
            if (!isSuccess) {
                alert("Ход отклонен сервером!");
            }
        })
        .catch(err => console.error("Ошибка метода PlayCard: ", err.toString()));
}