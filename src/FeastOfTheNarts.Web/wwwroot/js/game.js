//МОК СОСТОЯНИЯ ИГРЫ (Единый camelCase контракт) тестовый
const mockGameState = {
    you: {
        lives: 2,
        totalPower: 14,
        handCount: 3,
        deckCount: 22,
        melee: {
            power: 6, cards: [
                { id: "card-1", name: "Уастырджи", power: 6, row: "Melee", isHero: true }
            ]
        },
        ranged: {
            power: 8, cards: [
                { id: "card-2", name: "Лучник Нартов", power: 4, row: "Ranged", isHero: false },
                { id: "card-3", name: "Старейшина", power: 4, row: "Ranged", isHero: false }
            ]
        },
        siege: { power: 0, cards: [] },
        hand: [
            { id: "card-4", name: "Всадник", power: 5, row: "Melee", isHero: false },
            { id: "card-5", name: "Нартский Пир", power: 3, row: "Siege", isHero: false }
        ]
    },
    opponent: {
        lives: 1,
        totalPower: 5,
        handCount: 4,
        deckCount: 24,
        melee: {
            power: 5, cards: [
                { id: "card-6", name: "Вражеский воин", power: 5, row: "Melee", isHero: false }
            ]
        },
        ranged: { power: 0, cards: [] },
        siege: { power: 0, cards: [] }
    }
};



//ИНИЦИАЛИЗАЦИЯ SIGNALR СЕРВЕРА
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .build();

connection.start()
    .then(() => {
        console.log("Успешно подключено к игровому серверу Пир Нартов!");
        connection.invoke("JoinMatch", "Gin Sakai");
    })
    .catch(err => console.error("Ошибка подключения SignalR: ", err.toString()));

//слушатель обновлений от сервера
connection.on("UpdateGameState", (gameState) => {
    console.log("Получены реальные данные от движка:", gameState);
    saveStateToLocalStorage(gameState);
    renderBoard(gameState);
});



//РАБОТА С ЛОКАЛЬНЫМ ХРАНИЛИЩЕМ (Local Storage)
function saveStateToLocalStorage(state) {
    localStorage.setItem("nartsGameState", JSON.stringify(state));
}

function loadStateFromLocalStorage() {
    const saved = localStorage.getItem("nartsGameState");
    return saved ? JSON.parse(saved) : null;
}



//ЛОГИКА ПОДСЧЕТА И ОБНОВЛЕНИЯ СЧЕТЧИКОВ
function recalculateScores(state) {
    //считает силу рядов игрока
    state.you.melee.power = state.you.melee.cards.reduce((sum, c) => sum + c.power, 0);
    state.you.ranged.power = state.you.ranged.cards.reduce((sum, c) => sum + c.power, 0);
    state.you.siege.power = state.you.siege.cards.reduce((sum, c) => sum + c.power, 0);
    state.you.totalPower = state.you.melee.power + state.you.ranged.power + state.you.siege.power;
    state.you.handCount = state.you.hand.length;

    //считает силу рядов противника
    state.opponent.melee.power = state.opponent.melee.cards.reduce((sum, c) => sum + c.power, 0);
    state.opponent.ranged.power = state.opponent.ranged.cards.reduce((sum, c) => sum + c.power, 0);
    state.opponent.siege.power = state.opponent.siege.cards.reduce((sum, c) => sum + c.power, 0);
    state.opponent.totalPower = state.opponent.melee.power + state.opponent.ranged.power + state.opponent.siege.power;
}

function updateCounters(state) {
    //синхронизация текстов и цифр на панелях
    document.getElementById("you-lives").innerText = state.you.lives;
    document.getElementById("you-totalPower").innerText = state.you.totalPower;
    document.getElementById("you-deckCount").innerText = state.you.deckCount;
    document.getElementById("you-handCount").innerText = state.you.handCount;

    document.getElementById("opponent-lives").innerText = state.opponent.lives;
    document.getElementById("opponent-totalPower").innerText = state.opponent.totalPower;
    document.getElementById("opponent-handCount").innerText = state.opponent.handCount;

    // Очки рядов
    document.getElementById("you-melee-power").innerText = state.you.melee.power;
    document.getElementById("you-ranged-power").innerText = state.you.ranged.power;
    document.getElementById("you-siege-power").innerText = state.you.siege.power;

    document.getElementById("opponent-melee-power").innerText = state.opponent.melee.power;
    document.getElementById("opponent-ranged-power").innerText = state.opponent.ranged.power;
    document.getElementById("opponent-siege-power").innerText = state.opponent.siege.power;
}

//КОМПОНЕНТЫ ОТРИСОВКИ КАРТ
function createCardHtml(card, isDraggable = false) {
    const heroClass = card.isHero ? "hero-card" : "";
    const dragAttribute = isDraggable ? 'draggable="true"' : '';

    let icon = "⚔️";
    if (card.row === "Ranged") icon = "🏹";
    if (card.row === "Siege") icon = "🔮";

    return `
        <div class="game-card ${heroClass}" id="card-${card.id}" ${dragAttribute} data-id="${card.id}">
            <div class="card-power">${icon} ${card.power}</div>
            <div class="card-name">${card.name}</div>
        </div>
    `;
}

//главная функция рендеринга поля
function renderBoard(state) {
    console.log("Отрисовка игрового поля...");

    //Пересчитывает очки на основе массивов карт
    recalculateScores(state);

    //Обновляет текстовые интерфейсы
    updateCounters(state);

    //Отрисовывает карты в рядах (твоих и чужих)
    const rowTypes = ['melee', 'ranged', 'siege'];
    rowTypes.forEach(rowType => {
        const myContainer = document.getElementById(`you-${rowType}-cards`);
        if (myContainer) {
            myContainer.innerHTML = state.you[rowType].cards.map(c => createCardHtml(c, false)).join('');
        }

        const optContainer = document.getElementById(`opponent-${rowType}-cards`);
        if (optContainer) {
            optContainer.innerHTML = state.opponent[rowType].cards.map(c => createCardHtml(c, false)).join('');
        }
    });

    //отрисовываем твою руку (карты можно перетаскивать — true)
    const handContainer = document.getElementById("you-hand");
    if (handContainer) {
        handContainer.innerHTML = state.you.hand.map(c => createCardHtml(c, true)).join('');
    }

    //Перепривязываем события перетаскивания к новым элементам
    initDragAndDrop();
}

//МЕХАНИКА DRAG & DROP
function initDragAndDrop() {
    const cards = document.querySelectorAll('.game-card[draggable="true"]');
    const rows = document.querySelectorAll('.player-row');

    cards.forEach(card => {
        card.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', e.target.id);
            setTimeout(() => card.style.opacity = '0.4', 0);
        });
        card.addEventListener('dragend', () => { card.style.opacity = '1'; });
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
            const pureCardId = cardId.replace('card-', '');
            const targetRowName = row.getAttribute('data-row'); // "Melee", "Ranged" или "Siege"
            const targetRowKey = targetRowName.toLowerCase();

            let currentState = loadStateFromLocalStorage() || mockGameState;
            const cardIndex = currentState.you.hand.findIndex(c => c.id === pureCardId);

            if (cardIndex !== -1) {
                const card = currentState.you.hand[cardIndex];

                //перемещает карту из руки в выбранный ряд
                currentState.you.hand.splice(cardIndex, 1);
                card.row = targetRowName; //обновляет тип ряда у карты
                currentState.you[targetRowKey].cards.push(card);

                //сохраняем локально и перерисовываем поле
                saveStateToLocalStorage(currentState);
                renderBoard(currentState);

                //отправляем ход на бэкенд-хаб
                sendMoveToServer(pureCardId, targetRowName);
            }
        });
    });
}

function sendMoveToServer(cardId, rowName) {
    console.log(`Отправка на сервер: Карта ${cardId} в ряд ${rowName}`);
    if (connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("PlayCard", cardId, rowName)
            .then(isSuccess => { if (!isSuccess) console.warn("Ход не одобрен сервером."); })
            .catch(err => console.error("Ошибка метода PlayCard: ", err.toString()));
    } else {
        console.log("Хаб пока не подключен, ход обработан локально в моке.");
    }
}

//СТАРТ ПРИ ЗАГРУЗКЕ СТРАНИЦЫ
document.addEventListener("DOMContentLoaded", () => {
    console.log("Интерфейс «Пир Нартов» инициализирован.");

    //для чистых тестов мока можно раскомментировать строку ниже, чтобы сбрасывать кэш:
    //localStorage.removeItem("nartsGameState");

    let currentState = loadStateFromLocalStorage();
    if (!currentState) {
        currentState = mockGameState;
        saveStateToLocalStorage(currentState);
    }

    //первичный рендер сцены
    renderBoard(currentState);
});