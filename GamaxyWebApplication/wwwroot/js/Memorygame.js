// DOM Elements
const gameContainer = document.getElementById('gameContainer');
const movesEl = document.getElementById('moves');
const matchesEl = document.getElementById('matches');
const timerEl = document.getElementById('timer');
const scoreEl = document.getElementById('score');
const messageEl = document.getElementById('message');
const progressBar = document.getElementById('progressBar');
const progressText = document.getElementById('progressText');
const themeSelect = document.getElementById('themeSelect');
const difficultySelect = document.getElementById('difficultySelect');
const restartBtn = document.getElementById('restartBtn');
const gameOverModal = document.getElementById('gameOverModal');
const modalTitle = document.getElementById('modalTitle');
const modalMessage = document.getElementById('modalMessage');
const nextLevelBtn = document.getElementById('nextLevelBtn');
const viewStatsBtn = document.getElementById('viewStatsBtn');
const bgAnimation = document.getElementById('bgAnimation');

// Game variables
let cards = [];
let hasFlippedCard = false;
let lockBoard = false;
let firstCard, secondCard;
let moves = 0;
let matches = 0;
let score = 0;
let timer = 0;
let timerInterval;
let gameActive = false;
let totalPairs = 0;
let currentLevel = 1;
let maxLevel = 10;
let difficulty = 'easy';
let theme = 'dark';
let gameStartTime;
let bestTimes = JSON.parse(localStorage.getItem('memoryGameBestTimes')) || {};
let highScores = JSON.parse(localStorage.getItem('memoryGameHighScores')) || {};
let totalGamesPlayed = parseInt(localStorage.getItem('memoryGameTotalGames')) || 0;
let totalGamesWon = parseInt(localStorage.getItem('memoryGameTotalWins')) || 0;

// Emoji data with SVG representations
const emojiData = [
    {
        name: "smile",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M64 108c-24.3 0-44-19.7-44-44h88c0 24.3-19.7 44-44 44z" fill="#ED7161"/><path d="M64 90c-14.4 0-26-11.6-26-26h52c0 14.4-11.6 26-26 26z" fill="#FFF"/><circle cx="42" cy="48" r="8" fill="#333"/><circle cx="86" cy="48" r="8" fill="#333"/></svg>`
    },
    {
        name: "laugh",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M64 102c-18.2 0-33-14.8-33-33h66c0 18.2-14.8 33-33 33z" fill="#ED7161"/><path d="M64 96c-14.4 0-26-11.6-26-26h52c0 14.4-11.6 26-26 26z" fill="#FFF"/><path d="M42 42c-4.4 0-8 3.6-8 8s3.6 8 8 8 8-3.6 8-8h-8v-8z" fill="#333"/><path d="M86 42c4.4 0 8 3.6 8 8s-3.6 8-8 8-8-3.6-8-8h8v-8z" fill="#333"/></svg>`
    },
    {
        name: "wink",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M92 72c0 15.5-12.5 28-28 28s-28-12.5-28-28h56z" fill="#ED7161"/><circle cx="42" cy="48" r="8" fill="#333"/><path d="M86 56c-4.4 0-8-3.6-8-8s3.6-8 8-8 8 3.6 8 8-3.6 8-8 8zm0-12v4h4c0-2.2-1.8-4-4-4z" fill="#333"/></svg>`
    },
    {
        name: "cool",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M30 48h68v12H30z" fill="#333"/><path d="M64 96c-13.3 0-24-10.7-24-24h48c0 13.3-10.7 24-24 24z" fill="#ED7161"/><path d="M30 48h68v12H30z" fill="#333"/><path d="M34 42h60v24H34z" fill="#333"/><path d="M38 46h52v16H38z" fill="#699635"/></svg>`
    },
    {
        name: "angry",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FF5A79"/><path d="M64 98c-16.6 0-30-13.4-30-30h60c0 16.6-13.4 30-30 30z" fill="#333"/><path d="M64 92c-13.3 0-24-10.7-24-24h48c0 13.3-10.7 24-24 24z" fill="#FFF"/><path d="M30 48l12-10 12 10H30zm44 0l12-10 12 10H74z" fill="#333"/><circle cx="42" cy="48" r="6" fill="#333"/><circle cx="86" cy="48" r="6" fill="#333"/></svg>`
    },
    {
        name: "sad",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M64 98c-16.6 0-30-13.4-30-30h60c0 16.6-13.4 30-30 30z" transform="rotate(180 64 83)" fill="#699635"/><circle cx="42" cy="48" r="8" fill="#333"/><circle cx="86" cy="48" r="8" fill="#333"/><path d="M64 82c-7.7 0-14-6.3-14-14h28c0 7.7-6.3 14-14 14z" fill="#333"/></svg>`
    },
    {
        name: "surprised",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><circle cx="64" cy="82" r="18" fill="#ED7161"/><circle cx="42" cy="48" r="8" fill="#333"/><circle cx="86" cy="48" r="8" fill="#333"/></svg>`
    },
    {
        name: "confused",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M50 82c0-7.7 6.3-14 14-14s14 6.3 14 14H50z" fill="#699635"/><circle cx="42" cy="48" r="8" fill="#333"/><circle cx="86" cy="48" r="8" fill="#333"/><circle cx="86" cy="88" r="6" fill="#333"/></svg>`
    },
    {
        name: "nerd",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FFCC33"/><path d="M64 96c-13.3 0-24-10.7-24-24h48c0 13.3-10.7 24-24 24z" fill="#699635"/><path d="M32 48h24v12H32zm40 0h24v12H72z" fill="#333"/><path d="M36 52h16v4H36zm40 0h16v4H76z" fill="#FFF"/><path d="M64 48v-8M64 48v8" stroke="#333" stroke-width="4"/></svg>`
    },
    {
        name: "sick",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#A5D6A7"/><path d="M40 80c0-13.3 10.7-24 24-24s24 10.7 24 24H40z" fill="#699635"/><circle cx="42" cy="48" r="8" fill="#333"/><circle cx="86" cy="48" r="8" fill="#333"/><path d="M98 30l-8 8m-8-8l8 8M38 30l-8 8m-8-8l8 8" stroke="#333" stroke-width="4"/></svg>`
    },
    {
        name: "devil",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#FF5A79"/><path d="M64 96c-13.3 0-24-10.7-24-24h48c0 13.3-10.7 24-24 24z" fill="#333"/><path d="M64 90c-10 0-18-8-18-18h36c0 10-8 18-18 18z" fill="#ED7161"/><path d="M42 48l-12-24 24 12-12 12zm44 0l12-24-24 12 12 12z" fill="#FF5A79"/><circle cx="42" cy="48" r="6" fill="#333"/><circle cx="86" cy="48" r="6" fill="#333"/></svg>`
    },
    {
        name: "alien",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#A5D6A7"/><path d="M64 96c-13.3 0-24-10.7-24-24h48c0 13.3-10.7 24-24 24z" fill="#699635"/><circle cx="42" cy="48" r="12" fill="#333"/><circle cx="86" cy="48" r="12" fill="#333"/><circle cx="42" cy="48" r="4" fill="#FFF"/><circle cx="86" cy="48" r="4" fill="#FFF"/><path d="M64 20v-8m-16 16l-6-6m38 6l6-6" stroke="#A5D6A7" stroke-width="4"/></svg>`
    },
    {
        name: "robot",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><rect x="24" y="24" width="80" height="80" rx="8" fill="#699635"/><rect x="32" y="32" width="64" height="64" rx="4" fill="#A5D6A7"/><rect x="40" y="40" width="16" height="16" rx="2" fill="#333"/><rect x="72" y="40" width="16" height="16" rx="2" fill="#333"/><rect x="40" y="72" width="48" height="8" rx="2" fill="#333"/><path d="M40 16v8m48-8v8M32 104v8m64-8v8" stroke="#699635" stroke-width="4"/></svg>`
    },
    {
        name: "ghost",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><path d="M40 120c-8 0-8-16 0-16s8 16 0 16zm24 0c-8 0-8-16 0-16s8 16 0 16zm24 0c-8 0-8-16 0-16s8 16 0 16z" fill="#FFF"/><path d="M104 80V40c0-22-18-40-40-40S24 18 24 40v40c0 22 18 40 40 40h40c0-22-18-40 0-40z" fill="#FFF"/><circle cx="48" cy="48" r="8" fill="#333"/><circle cx="80" cy="48" r="8" fill="#333"/><path d="M64 80c-8.8 0-16-7.2-16-16h32c0 8.8-7.2 16-16 16z" fill="#FF5A79"/></svg>`
    },
    {
        name: "ninja",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="60" fill="#333"/><path d="M24 64c0-22 18-40 40-40s40 18 40 40H24z" fill="#333"/><path d="M24 64h80" stroke="#ED7161" stroke-width="4"/><circle cx="48" cy="48" r="6" fill="#FFF"/><circle cx="80" cy="48" r="6" fill="#FFF"/></svg>`
    },
    {
        name: "heart",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><path d="M64 108l-48-48c-12-12-12-32 0-44s32-12 44 0l4 4 4-4c12-12 32-12 44 0s12 32 0 44l-48 48z" fill="#FF5A79"/></svg>`
    },
    {
        name: "star",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><path d="M64 8l14 42h44l-36 26 14 42-36-26-36 26 14-42-36-26h44z" fill="#FFCC33"/></svg>`
    },
    {
        name: "fire",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><path d="M64 16c0 0 10 24 10 40 0 16-10 24-10 24s-10-8-10-24c0-16 10-40 10-40zm-24 32c0 0 8 16 8 32s-8 32-8 32c16 16 48 16 64 0 0 0-8-16-8-32s8-32 8-32c-16-16-48-16-64 0z" fill="#ED7161"/><path d="M64 56c-8 0-16 8-16 16s8 16 16 16 16-8 16-16-8-16-16-16z" fill="#FFCC33"/></svg>`
    },
    {
        name: "lightning",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><path d="M72 8L40 56h24l-8 64 32-48H64z" fill="#FFCC33"/></svg>`
    },
    {
        name: "moon",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><path d="M96 64c0 26.5-21.5 48-48 48S0 90.5 0 64 21.5 16 48 16c0 26.5 21.5 48 48 48z" fill="#FFCC33"/></svg>`
    },
    {
        name: "sun",
        svg: `<svg class="emoji-svg" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg"><circle cx="64" cy="64" r="32" fill="#FFCC33"/><path d="M64 16v16M64 96v16M16 64h16M96 64h16M32 32l12 12M84 84l12 12M32 96l12-12M84 32l12-12" stroke="#FFCC33" stroke-width="8"/></svg>`
    }
];

// Card back SVG
const cardBackSVG = `
            <svg class="back-logo" viewBox="0 0 128 128" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="rgba(255,255,255,0.1)"/>
                <text x="64" y="78" font-size="100" text-anchor="middle" fill="rgba(255,255,255,0.2)">?</text>
            </svg>
        `;

// Initialize the game
function init() {
    // Create background animation
    createBackgroundAnimation();

    // Set theme from localStorage or default
    const savedTheme = localStorage.getItem("memoryGameTheme") || "dark";
    themeSelect.value = savedTheme;
    applyTheme(savedTheme);

    // Set difficulty from localStorage or default
    const savedDifficulty = localStorage.getItem("memoryGameDifficulty") || "easy";
    difficultySelect.value = savedDifficulty;
    setDifficulty(savedDifficulty);

    // Start a new game
    startNewGame();

    // Add event listeners
    themeSelect.addEventListener("change", () => {
        applyTheme(themeSelect.value);
        localStorage.setItem("memoryGameTheme", themeSelect.value);
    });

    difficultySelect.addEventListener("change", () => {
        setDifficulty(difficultySelect.value);
        localStorage.setItem("memoryGameDifficulty", difficultySelect.value);
        startNewGame();
    });

    restartBtn.addEventListener("click", startNewGame);
    nextLevelBtn.addEventListener("click", nextLevel);
    viewStatsBtn.addEventListener("click", showStats);
}

// Create background animation
function createBackgroundAnimation() {
    bgAnimation.innerHTML = '';
    const count = 20;

    for (let i = 0; i < count; i++) {
        const span = document.createElement('span');
        span.style.width = `${Math.random() * 30 + 10}px`;
        span.style.height = span.style.width;
        span.style.left = `${Math.random() * 100}%`;
        span.style.top = `${Math.random() * 100}%`;
        span.style.animationDelay = `${Math.random() * 15}s`;
        span.style.animationDuration = `${Math.random() * 10 + 10}s`;
        bgAnimation.appendChild(span);
    }
}

// Apply theme
function applyTheme(selectedTheme) {
    theme = selectedTheme;

    // Remove all theme classes
    document.body.classList.remove('theme-light', 'theme-neon', 'theme-retro');

    // Add selected theme class
    if (theme !== 'dark') {
        document.body.classList.add(`theme-${theme}`);
    }
}

// Set difficulty
function setDifficulty(selectedDifficulty) {
    difficulty = selectedDifficulty;

    // Update game grid based on difficulty
    switch (difficulty) {
        case 'easy':
            gameContainer.style.gridTemplateColumns = 'repeat(4, 1fr)';
            break;
        case 'medium':
            gameContainer.style.gridTemplateColumns = 'repeat(5, 1fr)';
            break;
        case 'hard':
            gameContainer.style.gridTemplateColumns = 'repeat(6, 1fr)';
            break;
        case 'expert':
            gameContainer.style.gridTemplateColumns = 'repeat(6, 1fr)';
            break;
    }
}

// Start new game
function startNewGame() {
    // Reset game state
    resetGame();

    // Create cards based on difficulty
    createCards();

    // Start timer
    startTimer();

    // Show initial message
    showMessage('Find all matching pairs!', 'info');

    // Update game state
    gameActive = true;
    gameStartTime = Date.now();

    // Hide modal if open
    gameOverModal.classList.remove('active');
}

// Reset game state
function resetGame() {
    // Clear game container
    gameContainer.innerHTML = '';

    // Reset variables
    cards = [];
    hasFlippedCard = false;
    lockBoard = false;
    firstCard = null;
    secondCard = null;
    moves = 0;
    matches = 0;
    score = 0;
    timer = 0;

    // Reset UI
    movesEl.textContent = '0';
    matchesEl.textContent = '0';
    scoreEl.textContent = '0';
    timerEl.textContent = '00:00';
    progressBar.style.width = '0%';
    progressText.textContent = '0%';

    // Clear timer
    clearInterval(timerInterval);
}

// Create cards based on difficulty
function createCards() {
    let rows, cols;

    switch (difficulty) {
        case 'easy':
            rows = 4;
            cols = 4;
            break;
        case 'medium':
            rows = 4;
            cols = 5;
            break;
        case 'hard':
            rows = 5;
            cols = 6;
            break;
        case 'expert':
            rows = 6;
            cols = 6;
            break;
    }

    // Calculate total pairs needed
    const totalCards = rows * cols;
    totalPairs = totalCards / 2;

    // Get random emojis for the game
    const shuffledEmojis = [...emojiData].sort(() => 0.5 - Math.random()).slice(0, totalPairs);

    // Create pairs
    let cardPairs = [];
    shuffledEmojis.forEach(emoji => {
        cardPairs.push(emoji, emoji);
    });

    // Shuffle the pairs
    cardPairs = cardPairs.sort(() => 0.5 - Math.random());

    // Create card elements
    cardPairs.forEach((emoji, index) => {
        createCard(emoji, index);
    });
}

// Create a single card
function createCard(emoji, index) {
    const card = document.createElement('div');
    card.classList.add('card');
    card.dataset.name = emoji.name;

    // Add level badge to first card if level > 1
    if (index === 0 && currentLevel > 1) {
        const levelBadge = document.createElement('div');
        levelBadge.classList.add('level-badge');
        levelBadge.textContent = currentLevel;
        card.appendChild(levelBadge);
    }

    const front = document.createElement('div');
    front.classList.add('front');
    front.innerHTML = emoji.svg;

    const back = document.createElement('div');
    back.classList.add('back');
    back.innerHTML = cardBackSVG;

    card.appendChild(front);
    card.appendChild(back);

    card.addEventListener('click', flipCard);
    gameContainer.appendChild(card);

    // Add to cards array
    cards.push(card);
}

// Flip card
function flipCard() {
    if (lockBoard) return;
    if (this === firstCard) return;

    this.classList.add('flipped');

    if (!hasFlippedCard) {
        // First click
        hasFlippedCard = true;
        firstCard = this;
        return;
    }

    // Second click
    secondCard = this;
    checkForMatch();
}

// Check for match
function checkForMatch() {
    // Increment moves
    moves++;
    movesEl.textContent = moves;

    // Lock the board
    lockBoard = true;

    // Check if cards match
    const isMatch = firstCard.dataset.name === secondCard.dataset.name;

    if (isMatch) {
        // Cards match
        disableCards();
        updateMatches();
        updateScore(true);
    } else {
        // Cards don't match
        unflipCards();
        updateScore(false);
    }
}

// Disable matched cards
function disableCards() {
    firstCard.removeEventListener('click', flipCard);
    secondCard.removeEventListener('click', flipCard);

    // Add match animation
    firstCard.classList.add('match-animation');
    secondCard.classList.add('match-animation');

    // Add matched class after animation
    setTimeout(() => {
        firstCard.classList.add('matched');
        secondCard.classList.add('matched');
        firstCard.classList.remove('match-animation');
        secondCard.classList.remove('match-animation');
        resetBoard();
    }, 600);
}

// Unflip non-matching cards
function unflipCards() {
    // Add no-match animation
    firstCard.classList.add('no-match-animation');
    secondCard.classList.add('no-match-animation');

    setTimeout(() => {
        firstCard.classList.remove('flipped', 'no-match-animation');
        secondCard.classList.remove('flipped', 'no-match-animation');
        resetBoard();
    }, 1000);
}

// Reset board after each turn
function resetBoard() {
    [hasFlippedCard, lockBoard] = [false, false];
    [firstCard, secondCard] = [null, null];
}

// Update matches count
function updateMatches() {
    matches++;
    matchesEl.textContent = matches;

    // Update progress
    const progress = (matches / totalPairs) * 100;
    progressBar.style.width = `${progress}%`;
    progressText.textContent = `${Math.round(progress)}%`;

    // Check if game is complete
    if (matches === totalPairs) {
        gameComplete();
    }
}

// Update score
function updateScore(isMatch) {
    // Calculate score based on match, moves, and time
    if (isMatch) {
        // Base points for match
        const basePoints = difficulty === 'easy' ? 100 :
            difficulty === 'medium' ? 150 :
                difficulty === 'hard' ? 200 : 250;

        // Bonus for fewer moves
        const moveBonus = Math.max(0, 50 - (moves * 2));

        // Bonus for faster time (max 50 points)
        const timeBonus = Math.max(0, 50 - Math.floor(timer / 5));

        // Level multiplier
        const levelMultiplier = 1 + (currentLevel * 0.1);

        // Calculate total points for this match
        const points = Math.round((basePoints + moveBonus + timeBonus) * levelMultiplier);

        // Add to score
        score += points;

        // Show floating score
        showFloatingScore(points);
    } else {
        // Penalty for wrong match
        const penalty = difficulty === 'easy' ? -10 :
            difficulty === 'medium' ? -20 :
                difficulty === 'hard' ? -30 : -40;

        score = Math.max(0, score + penalty);
    }

    // Update score display
    scoreEl.textContent = score;

    // Animate score
    scoreEl.classList.add('pulse');
    setTimeout(() => scoreEl.classList.remove('pulse'), 500);
}

// Show floating score
function showFloatingScore(points) {
    const floatingScore = document.createElement('div');
    floatingScore.textContent = `+${points}`;
    floatingScore.style.position = 'absolute';
    floatingScore.style.left = `${secondCard.getBoundingClientRect().left + secondCard.offsetWidth / 2}px`;
    floatingScore.style.top = `${secondCard.getBoundingClientRect().top}px`;
    floatingScore.style.color = 'var(--success)';
    floatingScore.style.fontWeight = 'bold';
    floatingScore.style.fontSize = '1.5rem';
    floatingScore.style.pointerEvents = 'none';
    floatingScore.style.zIndex = '100';
    floatingScore.style.textShadow = '0 0 10px rgba(16, 185, 129, 0.5)';
    floatingScore.style.animation = 'floatUp 1s ease-out forwards';

    document.body.appendChild(floatingScore);

    // Add keyframe animation
    const style = document.createElement('style');
    style.textContent = `
        @keyframes floatUp {
                    0% { transform: translateY(0) scale(1); opacity: 1; }
                    100% { transform: translateY(-50px) scale(1.5); opacity: 0; }
                }
            `;
    document.head.appendChild(style);

    // Remove after animation
    setTimeout(() => {
        document.body.removeChild(floatingScore);
        document.head.removeChild(style);
    }, 1000);
}

// Start timer
function startTimer() {
    clearInterval(timerInterval);
    timer = 0;

    timerInterval = setInterval(() => {
        timer++;

        // Format time as MM:SS
        const minutes = Math.floor(timer / 60).toString().padStart(2, '0');
        const seconds = (timer % 60).toString().padStart(2, '0');
        timerEl.textContent = `${minutes}:${seconds}`;

        // Add warning class when time gets high
        if (timer > 60) {
            timerEl.classList.add('timer-warning');
        } else {
            timerEl.classList.remove('timer-warning');
        }
    }, 1000);
}

// Game complete
function gameComplete() {
    // Stop timer
    clearInterval(timerInterval);

    // Update game state
    gameActive = false;

    // Calculate final score
    const timeBonus = Math.max(0, 300 - timer);
    const movesBonus = Math.max(0, 300 - (moves * 2));
    const levelBonus = currentLevel * 100;
    const finalScore = score + timeBonus + movesBonus + levelBonus;

    // Update total score
    score = finalScore;
    scoreEl.textContent = score;

    // Update stats
    totalGamesPlayed++;
    totalGamesWon++;
    localStorage.setItem('memoryGameTotalGames', totalGamesPlayed);
    localStorage.setItem('memoryGameTotalWins', totalGamesWon);

    // Check for high score
    if (!highScores[difficulty] || score > highScores[difficulty]) {
        highScores[difficulty] = score;
        localStorage.setItem('memoryGameHighScores', JSON.stringify(highScores));
    }

    // Check for best time
    const gameTime = timer;
    if (!bestTimes[difficulty] || gameTime < bestTimes[difficulty]) {
        bestTimes[difficulty] = gameTime;
        localStorage.setItem('memoryGameBestTimes', JSON.stringify(bestTimes));
    }

    // Show success message
    showMessage('Level Complete! Great job!', 'success');

    // Create confetti effect
    createConfetti();

    // Show modal after a delay
    setTimeout(() => {
        modalTitle.textContent = 'Level Complete!';
        modalTitle.style.color = 'var(--success)';

        // Format time
        const minutes = Math.floor(timer / 60).toString().padStart(2, '0');
        const seconds = (timer % 60).toString().padStart(2, '0');
        const timeString = `${minutes}:${seconds}`;

        modalMessage.innerHTML = `
                    <div class="stats-grid">
                        <div class="stats-item">
                            <div class="stats-item-label">Final Score</div>
                            <div class="stats-item-value">${score}</div>
                        </div>
                        <div class="stats-item">
                            <div class="stats-item-label">Time</div>
                            <div class="stats-item-value">${timeString}</div>
                        </div>
                        <div class="stats-item">
                            <div class="stats-item-label">Moves</div>
                            <div class="stats-item-value">${moves}</div>
                        </div>
                        <div class="stats-item">
                            <div class="stats-item-label">Level</div>
                            <div class="stats-item-value">${currentLevel}</div>
                        </div>
                    </div>
                    <p style="margin-top: 1rem;">Ready for the next challenge?</p>
                `;

        // Update button text based on level
        if (currentLevel >= maxLevel) {
            nextLevelBtn.textContent = 'Play Again';
        } else {
            nextLevelBtn.textContent = 'Next Level';
        }

        gameOverModal.classList.add('active');
    }, 1500);
}

// Next level
function nextLevel() {
    if (currentLevel >= maxLevel) {
        // Reset to level 1 if max level reached
        currentLevel = 1;
    } else {
        // Increment level
        currentLevel++;
    }

    // Start new game with current level
    startNewGame();
}

// Show stats
function showStats() {
    // Format best time
    const formatTime = (seconds) => {
        if (!seconds) return 'N/A';
        const mins = Math.floor(seconds / 60).toString().padStart(2, '0');
        const secs = (seconds % 60).toString().padStart(2, '0');
        return `${mins}:${secs}`;
    };

    // Calculate win rate
    const winRate = totalGamesPlayed > 0 ? Math.round((totalGamesWon / totalGamesPlayed) * 100) : 0;

    modalTitle.textContent = 'Your Statistics';
    modalTitle.style.color = 'var(--info)';

    modalMessage.innerHTML = `
                <div class="stats-grid">
                    <div class="stats-item">
                        <div class="stats-item-label">Games Played</div>
                        <div class="stats-item-value">${totalGamesPlayed}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Games Won</div>
                        <div class="stats-item-value">${totalGamesWon}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Win Rate</div>
                        <div class="stats-item-value">${winRate}%</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Current Level</div>
                        <div class="stats-item-value">${currentLevel}</div>
                    </div>
                </div>
                <h3 style="margin: 1rem 0 0.5rem;">High Scores</h3>
                <div class="stats-grid">
                    <div class="stats-item">
                        <div class="stats-item-label">Easy</div>
                        <div class="stats-item-value">${highScores.easy || 0}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Medium</div>
                        <div class="stats-item-value">${highScores.medium || 0}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Hard</div>
                        <div class="stats-item-value">${highScores.hard || 0}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Expert</div>
                        <div class="stats-item-value">${highScores.expert || 0}</div>
                    </div>
                </div>
                <h3 style="margin: 1rem 0 0.5rem;">Best Times</h3>
                <div class="stats-grid">
                    <div class="stats-item">
                        <div class="stats-item-label">Easy</div>
                        <div class="stats-item-value">${formatTime(bestTimes.easy)}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Medium</div>
                        <div class="stats-item-value">${formatTime(bestTimes.medium)}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Hard</div>
                        <div class="stats-item-value">${formatTime(bestTimes.hard)}</div>
                    </div>
                    <div class="stats-item">
                        <div class="stats-item-label">Expert</div>
                        <div class="stats-item-value">${formatTime(bestTimes.expert)}</div>
                    </div>
                </div>
            `;

    // Change button text
    nextLevelBtn.textContent = 'Continue';
}

// Show message
function showMessage(text, type = '') {
    messageEl.textContent = text;
    messageEl.className = 'message-display';

    if (type) {
        messageEl.classList.add(type);
    }

    // Clear message after 3 seconds
    setTimeout(() => {
        messageEl.textContent = '';
        messageEl.className = 'message-display';
    }, 3000);
}

// Create confetti effect
function createConfetti() {
    const colors = ["#ff0000", "#00ff00", "#0000ff", "#ffff00", "#ff00ff", "#00ffff", "#ff8800", "#00ff88"];

    for (let i = 0; i < 150; i++) {
        const confetti = document.createElement("div");
        confetti.classList.add("confetti");
        confetti.style.left = `${Math.random() * 100}%`;
        confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
        confetti.style.width = `${Math.random() * 10 + 5}px`;
        confetti.style.height = `${Math.random() * 10 + 5}px`;
        confetti.style.animationDelay = `${Math.random() * 2}s`;
        confetti.style.animationDuration = `${Math.random() * 2 + 2}s`;

        document.body.appendChild(confetti);

        // Remove after animation completes
        setTimeout(() => {
            confetti.remove();
        }, 5000);
    }
}

// Initialize the game
init();