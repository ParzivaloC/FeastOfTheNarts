//инициализация подключения к серверу через SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .build();

//подключение к серверу при старте
connection.start()
    .then(() => {
        console.log("Успешно подключено к игровому серверу Пир Нартов!");
        connection.invoke("JoinMatch", "Player_Soslan"); //сообщаем серверу, кто мы
    })
    .catch(err => console.error("Ошибка подключения: ", err.toString()));

//СЛУШАТЕЛЬ: Сервер присылает обновленное состояние игры всем участникам
connection.on("UpdateGameState", (gameState) => {
    console.log("Получены реальные данные от движка:", gameState);
    saveStateToLocalStorage(gameState); //Сохраняем свежую правду от сервера в браузер
    renderGameState(gameState); //перерисовка экрана
});

//ЛОГИКА СОХРАНЕНИЯ СОСТОЯНИЯ (Local Storage) временно??? (на усмотрение Амридина)
//временный инструмент, чтобы интерфейс не умирал при перезагрузке страницы
function saveStateToLocalStorage(state) {
    localStorage.setItem("nartsMockGameState", JSON.stringify(state));
}

function loadStateFromLocalStorage() {
    const saved = localStorage.getItem("nartsMockGameState");
    return saved ? JSON.parse(saved) : null;
}




//ИНИЦИАЛИЗАЦИЯ ИНТЕРФЕЙСА
document.addEventListener("DOMContentLoaded", () => {
    console.log("Интерфейс инициализирован.");

    //очистка старого кэша, чтобы проверить, появятся ли карты
    localStorage.removeItem("nartsMockGameState"); 

    let currentState = loadStateFromLocalStorage();

    if (!currentState) {
        console.log("Состояние пустое, загружаю начальные карты...");
        currentState = {
            currentPlayerId: "Gin Sakai",
            playerState: {
                lives: 2,
                hand: [
                    { id: "1002", name: "Батраз", basePower: 5, targetRow: "Ranged" },
                    { id: "1003", name: "Сырдон", basePower: 2, targetRow: "Siege" },
                    { id: "1004", name: "Ацамаз", basePower: 4, targetRow: "Melee" }
                ],
                deckCount: 18
            },
            opponentState: { lives: 2, handCount: 9 },
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
        saveStateToLocalStorage(currentState);
    } else {
        console.log("Загружено состояние из памяти:", currentState);
    }

    //вызов отрисовка с задержкой чтобы ДОМ успел подгрузить всё(НЕ ТРОГАТЬ)
    setTimeout(() => {
        renderGameState(currentState);
    }, 100);
});



//ЛОГИКА ПОДСЧЕТА ОЧКОВ
function recalculateScores(state) {
    //Очки в каждом ряду игрока (Авангард, Стрелки, Артиллерия)
    state.board.playerBoard.meleeRow.totalPower = state.board.playerBoard.meleeRow.cards.reduce((sum, card) => sum + card.basePower, 0);
    state.board.playerBoard.rangedRow.totalPower = state.board.playerBoard.rangedRow.cards.reduce((sum, card) => sum + card.basePower, 0);
    state.board.playerBoard.siegeRow.totalPower = state.board.playerBoard.siegeRow.cards.reduce((sum, card) => sum + card.basePower, 0);

    //Общий счет игрока (сумма всех 3 полей)
    state.board.playerBoard.totalPower =
        state.board.playerBoard.meleeRow.totalPower +
        state.board.playerBoard.rangedRow.totalPower +
        state.board.playerBoard.siegeRow.totalPower;


    //То же самое для противника
    state.board.opponentBoard.meleeRow.totalPower = state.board.opponentBoard.meleeRow.cards.reduce((sum, card) => sum + card.basePower, 0);
    state.board.opponentBoard.rangedRow.totalPower = state.board.opponentBoard.rangedRow.cards.reduce((sum, card) => sum + card.basePower, 0);
    state.board.opponentBoard.siegeRow.totalPower = state.board.opponentBoard.siegeRow.cards.reduce((sum, card) => sum + card.basePower, 0);

    state.board.opponentBoard.totalPower =
        state.board.opponentBoard.meleeRow.totalPower +
        state.board.opponentBoard.rangedRow.totalPower +
        state.board.opponentBoard.siegeRow.totalPower;
}


//ГЛАВНАЯ ФУНКЦИЯ ОТРИСОВКИ
//берет gameState и обновляет весь html: очки, жизни, колоду и ряды карт и тд тп
function renderGameState(data) {
    console.log("Отрисовка состояния:", data);

    //пересчёт очков ПЕРЕД отрисовкой
    recalculateScores(data);

    //обновление счетчика игрока
    document.getElementById("pl-total").innerText = data.board.playerBoard.totalPower;
    document.getElementById("pl-lives").innerText = data.playerState.lives;
    document.getElementById("pl-deck-count").innerText = data.playerState.deckCount;
    document.getElementById("pl-hand-count").innerText = data.playerState.hand.length; //кол-во в руке

    //обновление счетчика противника
    document.getElementById("opt-lives").innerText = data.opponentState.lives;
    document.getElementById("opt-hand-count").innerText = data.opponentState.handCount;
    document.getElementById("opt-total").innerText = data.board.opponentBoard.totalPower;

    //отрисовка руки
    const handContainer = document.getElementById("my-hand");
    handContainer.innerHTML = ""; //очистка
    data.playerState.hand.forEach(card => {
        handContainer.appendChild(createCardElement(card, true));
    });

    //отрисовка полей
    renderRowCards(".player-row.melee", data.board.playerBoard.meleeRow, "pl-score-melee");
    renderRowCards(".player-row.ranged", data.board.playerBoard.rangedRow, "pl-score-ranged");
    renderRowCards(".player-row.siege", data.board.playerBoard.siegeRow, "pl-score-siege");

    renderRowCards(".opponent-row.melee", data.board.opponentBoard.meleeRow, "opt-score-melee");
    renderRowCards(".opponent-row.ranged", data.board.opponentBoard.rangedRow, "opt-score-ranged");
    renderRowCards(".opponent-row.siege", data.board.opponentBoard.siegeRow, "opt-score-siege");

    initDragAndDrop();
}



//вспомогательная функция для наполнения конкретного ряда картами
function renderRowCards(rowSelector, rowData, scoreId) {
    const rowElement = document.querySelector(rowSelector);
    const cardsContainer = rowElement.querySelector(".row-cards-container");
    document.getElementById(scoreId).innerText = rowData.totalPower;
    cardsContainer.innerHTML = "";
    rowData.cards.forEach(card => {
        cardsContainer.appendChild(createCardElement(card, false)); //false - на поле карту двигать нельзя
    });
}

//создает html элемент карты, шаблон, который мы используем везде
function createCardElement(card, isDraggable) {
    const cardDiv = document.createElement("div");
    cardDiv.className = "game-card";
    if (isDraggable) cardDiv.setAttribute("draggable", "true");
    cardDiv.id = `card-${card.id}`;

    let icon = "⚔️";
    if (card.targetRow === "Ranged") icon = "🏹";
    if (card.targetRow === "Siege") icon = "🔮";

    cardDiv.innerHTML = `<div class="card-power">${icon} ${card.basePower}</div><div class="card-name">${card.name}</div>`;
    return cardDiv;
}

//ЛОГИКА DRAG & DROP (Перетаскивание)
function initDragAndDrop() {
    const cards = document.querySelectorAll('.game-card[draggable="true"]');
    const rows = document.querySelectorAll('.player-row');

    //привязываем событие начала перетаскивания(хватаем карту)
    cards.forEach(card => {
        card.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', e.target.id); //запоминает ID карты
            setTimeout(() => card.style.opacity = '0.4', 0); //делает прозрачной
        });
        card.addEventListener('dragend', () => { card.style.opacity = '1'; });
    });

    //обработка полей, куда можно бросить карту
    rows.forEach(row => {
        row.addEventListener('dragover', (e) => { e.preventDefault(); row.classList.add('drag-over'); });
        row.addEventListener('dragleave', () => { row.classList.remove('drag-over'); });

        //когда бросили карту
        row.addEventListener('drop', (e) => {
            e.preventDefault();
            row.classList.remove('drag-over');

            const cardId = e.dataTransfer.getData('text/plain');
            const pureCardId = cardId.replace('card-', '');
            const targetRowName = row.getAttribute('data-row'); //получает ряд из html атрибута

            //локально обновляет массив руки и ряда
            let currentState = loadStateFromLocalStorage();
            const cardIndex = currentState.playerState.hand.findIndex(c => c.id === pureCardId);

            if (cardIndex !== -1) {
                const card = currentState.playerState.hand[cardIndex];
                currentState.playerState.hand.splice(cardIndex, 1); // Удаляем из руки

                //просто кладет карту в нужный ряд (очки посчитаются сами при рендере)
                if (targetRowName === "Melee") currentState.board.playerBoard.meleeRow.cards.push(card);
                else if (targetRowName === "Ranged") currentState.board.playerBoard.rangedRow.cards.push(card);
                else if (targetRowName === "Siege") currentState.board.playerBoard.siegeRow.cards.push(card);

                saveStateToLocalStorage(currentState); //сохранение
                renderGameState(currentState); //перерисовываем (и очки пересчитаются!)
                sendMoveToServer(pureCardId, targetRowName); //сообщает серверу о ходе
            }
        });
    });
}

//отправка команды серверу
function sendMoveToServer(cardId, rowName) {
    console.log(`Отправка на сервер: Карта ${cardId} в ряд ${rowName}`);
    connection.invoke("PlayCard", cardId, rowName)
        .then(isSuccess => { if (!isSuccess) alert("Ход отклонен сервером!"); })
        .catch(err => console.error("Ошибка метода PlayCard: ", err.toString()));
}