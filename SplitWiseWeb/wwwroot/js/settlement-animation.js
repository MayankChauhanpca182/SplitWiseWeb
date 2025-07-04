function settlementSuccess() {
    for (let i = 0; i < 80; i++) {

        const type = getRandomType();

        createFallItem(type);

    }
}

function getRandomType() {

    const types = ['heart', 'ribbon', 'money'];

    return types[Math.floor(Math.random() * types.length)];

}

function createFallItem(type) {

    const el = document.createElement('div');

    el.classList.add('fall-item', type);

    // Style and content depending on type

    switch (type) {

        case 'heart':

            el.style.backgroundColor = getRandomColor();

            break;

        case 'ribbon':

            el.style.backgroundColor = getRandomColor();

            break;

        case 'money':

            el.textContent = getRandomMoneyEmoji();

            break;

    }

    el.style.left = Math.random() * window.innerWidth + 'px';

    el.style.top = '-30px';

    el.style.animationDuration = `${3 + Math.random() * 2}s`;

    document.body.appendChild(el);
    // document.getElementById("regularModalContent").appendChild(el);

    setTimeout(() => el.remove(), 6000);

}

function getRandomColor() {

    const colors = ['#e74c3c', '#f1c40f', '#2ecc71', '#9b59b6', '#ff69b4'];

    return colors[Math.floor(Math.random() * colors.length)];

}

function getRandomMoneyEmoji() {

    const moneyEmojis = ['💸', '💰', '🪙', '💵'];

    return moneyEmojis[Math.floor(Math.random() * moneyEmojis.length)];

}
