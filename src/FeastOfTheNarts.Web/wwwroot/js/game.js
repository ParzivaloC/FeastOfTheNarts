//игровой клиент
//рисует то, что прислал хаб,
// и отправляет действия (сыграть карту / пас) без мока

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .build();

//последнее состояние, присланное сервером
let currentState = null;



//Запуск соединения и поиск игры!!!!!!!!!
connection.start()
    .then(() => {
        console.log("Подключено к серверу Пир Нартов");
        showOverlay("Поиск соперника...", false);
        connection.invoke("FindGame");
    })
    .catch(err => {
        console.error("Ошибка подключения SignalR:", err.toString());
        showOverlay("Не удалось подключиться к серверу", true);
    });



//События от сервера!!!!!!!!!!!!!!
connection.on("Waiting", () => showOverlay("Ожидание соперника...", false));

connection.on("ReceiveState", (state) => {
    currentState = state;
    hideOverlay();
    renderBoard(state);
    updateTurnBanner(state);
    if (state.phase === "Finished") showResult(state.result);
});

connection.on("OpponentLeft", () => showOverlay("Соперник вышел. Ищем нового соперника...", false));



//Отрисовка карты!!!!!!!!!!!!!!!!!!!!
function createCardHtml(card, isDraggable = false) {
    const heroClass = card.isHero ? "hero-card" : "";
    const dragAttr = isDraggable ? 'draggable="true"' : '';

    let icon = "⚔️";
    if (card.row === "Ranged") icon = "🏹";
    if (card.row === "Siege") icon = "🔮";

    const img = card.imageUrl
        ? `<img class="card-img" src="${encodeURI(card.imageUrl)}" alt="">`
        : '';

    return `
        <div class="game-card ${heroClass}" id="card-${card.id}" ${dragAttr} data-id="${card.id}">
            ${img}
            <div class="card-power">${icon} ${card.power}</div>
            <div class="card-name">${card.name}</div>
        </div>`;
}




//Обновление счётчиков (значения берётся прямо из состояния сервера)
function updateCounters(state) {
    document.getElementById("you-name").innerText = state.you.displayName || "Игрок";
    document.getElementById("opponent-name").innerText = state.opponent.displayName || "Противник";

    document.getElementById("you-lives").innerText = state.you.lives;
    document.getElementById("you-totalPower").innerText = state.you.totalPower;
    document.getElementById("you-deckCount").innerText = state.you.deckCount;
    document.getElementById("you-handCount").innerText = state.you.handCount;

    document.getElementById("opponent-lives").innerText = state.opponent.lives;
    document.getElementById("opponent-totalPower").innerText = state.opponent.totalPower;
    document.getElementById("opponent-handCount").innerText = state.opponent.handCount;

    document.getElementById("you-melee-power").innerText = state.you.melee.power;
    document.getElementById("you-ranged-power").innerText = state.you.ranged.power;
    document.getElementById("you-siege-power").innerText = state.you.siege.power;

    document.getElementById("opponent-melee-power").innerText = state.opponent.melee.power;
    document.getElementById("opponent-ranged-power").innerText = state.opponent.ranged.power;
    document.getElementById("opponent-siege-power").innerText = state.opponent.siege.power;
}




//Главная отрисовка поля!!!!!!!!!!!!!!!
function renderBoard(state) {
    updateCounters(state);

    ['melee', 'ranged', 'siege'].forEach(rowType => {
        const mine = document.getElementById(`you-${rowType}-cards`);
        if (mine) mine.innerHTML = state.you[rowType].cards.map(c => createCardHtml(c, false)).join('');

        const opp = document.getElementById(`opponent-${rowType}-cards`);
        if (opp) opp.innerHTML = state.opponent[rowType].cards.map(c => createCardHtml(c, false)).join('');
    });

    //свою руку можно перетаскивать
    const hand = document.getElementById("you-hand");
    if (hand) hand.innerHTML = state.you.hand.map(c => createCardHtml(c, true)).join('');

    bindCardDrag();
}




function initDropZones() {
    document.querySelectorAll('.player-row').forEach(row => {
        row.addEventListener('dragover', (e) => { e.preventDefault(); row.classList.add('drag-over'); });
        row.addEventListener('dragleave', () => row.classList.remove('drag-over'));
        row.addEventListener('drop', (e) => {
            e.preventDefault();
            row.classList.remove('drag-over');
            if (!currentState || !currentState.isYourTurn) return;
            const cardId = e.dataTransfer.getData('text/plain');
            const rowName = row.getAttribute('data-row');
            connection.invoke("PlayCard", cardId, rowName)
                .catch(err => console.error("Ошибка PlayCard:", err.toString()));
        });
    });
}

// КАЖДЫЙ рендер — карты пересоздаются, поэтому их слушатели вешаем заново (утечки нет)
function bindCardDrag() {
    document.querySelectorAll('.game-card[draggable="true"]').forEach(card => {
        card.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', card.dataset.id);
            setTimeout(() => card.style.opacity = '0.4', 0);
        });
        card.addEventListener('dragend', () => card.style.opacity = '1');
    });
}





//кнопка пас!!!!!!!!!!!
const passBtn = document.getElementById("pass-btn");
if (passBtn) {
    passBtn.addEventListener("click", () => {
        if (currentState && currentState.isYourTurn) {
            connection.invoke("Pass").catch(err => console.error("Ошибка Pass:", err.toString()));
        }
    });
}



//индикатор чей ход сейчас
function updateTurnBanner(state) {
    let banner = document.getElementById("turn-banner");
    if (!banner) {
        banner = document.createElement("div");
        banner.id = "turn-banner";
        banner.style.cssText = "position:fixed;top:10px;left:50%;transform:translateX(-50%);" +
            "padding:8px 20px;border-radius:8px;font-weight:bold;z-index:1500;color:#fff;";
        document.body.appendChild(banner);
    }
    banner.textContent = state.isYourTurn ? "Ваш ход" : "Ход соперника";
    banner.style.background = state.isYourTurn ? "#2e7d32" : "#555";
}



//Оверлей для ожидания и конца игры!!!!!!!!!!!
function showOverlay(message, withMenuButton) {
    let ov = document.getElementById("game-overlay");
    if (!ov) {
        ov = document.createElement("div");
        ov.id = "game-overlay";
        ov.style.cssText = "position:fixed;inset:0;background:rgba(0,0,0,.82);display:flex;" +
            "flex-direction:column;align-items:center;justify-content:center;gap:22px;" +
            "color:#fff;font-size:1.8rem;text-align:center;z-index:2000;";
        document.body.appendChild(ov);
    }
    ov.innerHTML = `<div>${message}</div>` +
        (withMenuButton
            ? `<button class="btn btn-warning fw-bold" onclick="window.location.href='/'">В меню</button>`
            : `<div class="spinner-border text-warning" role="status"></div>`);
    ov.style.display = "flex";
}

function hideOverlay() {
    const ov = document.getElementById("game-overlay");
    if (ov) ov.style.display = "none";
}

function showResult(result) {
    let text = "Ничья 🤝";
    if (result === "Win") text = "🏆 Победа!";
    else if (result === "Loss") text = "💀 Поражение";
    showOverlay(text, true);
}
initDropZones();