// Enhanced smooth scrolling
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Ultimate intersection observer
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.animationDelay = '0s';
            entry.target.classList.add('fade-in');
        }
    });
}, observerOptions);

// Ultimate button effects
document.querySelectorAll('button').forEach(button => {
    button.addEventListener('click', function (e) {
        // Create ultimate ripple effect
        const ripple = document.createElement('span');
        const rect = this.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = e.clientX - rect.left - size / 2;
        const y = e.clientY - rect.top - size / 2;

        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';
        ripple.classList.add('ultimate-ripple');

        this.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 1000);
    });
});

// Ultimate counter animation
function animateUltimateCounters() {
    const counters = document.querySelectorAll('.gv-ultimate-counter-f9q');
    counters.forEach(counter => {
        const target = parseInt(counter.textContent.replace(/[^\d]/g, ''));
        const increment = target / 120;
        let current = 0;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                counter.textContent = counter.textContent.replace(/[\d,]+/, target.toLocaleString());
                clearInterval(timer);
            } else {
                counter.textContent = counter.textContent.replace(/[\d,]+/, Math.floor(current).toLocaleString());
            }
        }, 25);
    });
}

// Trigger ultimate counter animation
const counterObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            animateUltimateCounters();
            counterObserver.unobserve(entry.target);
        }
    });
});

document.querySelectorAll('.gv-ultimate-counter-f9q').forEach(counter => {
    counterObserver.observe(counter);
});

// Ultimate CSS for effects
const style = document.createElement('style');
style.textContent = `
            .ultimate-ripple {
                position: absolute;
                border-radius: 50%;
                background: radial-gradient(circle, #D400FF 0%, #008dff 30%, #D400FF 60%, transparent 100%);
                transform: scale(0);
                animation: ultimate-ripple 1s cubic-bezier(0.25, 0.46, 0.45, 0.94);
                pointer-events: none;
                z-index: 1000;
            }

            @keyframes ultimate-ripple {
                to {
                    transform: scale(5);
                    opacity: 0;
                }
            }

            .fade-in {
                opacity: 0;
                transform: translateY(40px);
                animation: ultimate-fade-in 1.2s ease forwards;
            }

            @keyframes ultimate-fade-in {
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }

            @keyframes ultimate-fade-out {
                0% { opacity: 1; transform: scale(1); }
                100% { opacity: 0; transform: scale(0); }
            }

            @keyframes ultimate-entrance {
                0% { 
                    opacity: 0; 
                    transform: translateY(60px) rotateX(90deg); 
                }
                100% { 
                    opacity: 1; 
                    transform: translateY(0) rotateX(0deg); 
                }
            }

            @keyframes ultimate-power {
                0%, 100% { transform: scale(1); }
                25% { transform: scale(1.03) rotateZ(2deg); }
                50% { transform: scale(1.08) rotateZ(-2deg); }
                75% { transform: scale(1.03) rotateZ(2deg); }
            }
        `;
document.head.appendChild(style);

// Ultimate parallax effect
window.addEventListener('scroll', () => {
    const scrolled = window.pageYOffset;
    const parallax = document.querySelectorAll('.gv-ultimate-float-n1c');
    const speed = 0.3;

    parallax.forEach(element => {
        const yPos = -(scrolled * speed);
        element.style.transform = `translateY(${yPos}px)`;
    });
});

// Observe all ultimate elements
document.querySelectorAll('.gv-ultimate-card-w5r, section > div').forEach(el => {
    observer.observe(el);
});

// ULTIMATE MATRIX DIGITAL RAIN EFFECT
function createUltimateMatrixRain() {
    const matrixContainer = document.getElementById('gv-ultimate-matrix-rain');
    const characters = '01アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンABCDEFGHIJKLMNOPQRSTUVWXYZ';

    for (let i = 0; i < 60; i++) {
        const column = document.createElement('div');
        column.className = 'gv-ultimate-matrix-column-c5n';
        column.style.left = Math.random() * 100 + '%';
        column.style.animationDuration = (Math.random() * 4 + 3) + 's';
        column.style.animationDelay = Math.random() * 3 + 's';

        let columnText = '';
        for (let j = 0; j < 25; j++) {
            columnText += characters.charAt(Math.floor(Math.random() * characters.length)) + '<br>';
        }
        column.innerHTML = columnText;
        matrixContainer.appendChild(column);
    }
}

// ULTIMATE WARP SPEED LINES EFFECT
function createUltimateWarpLines() {
    const warpContainer = document.getElementById('gv-ultimate-warp-lines');

    for (let i = 0; i < 20; i++) {
        const line = document.createElement('div');
        line.className = 'gv-ultimate-warp-line-u1h';
        line.style.top = Math.random() * 100 + '%';
        line.style.animationDuration = (Math.random() * 3 + 2) + 's';
        line.style.animationDelay = Math.random() * 4 + 's';
        warpContainer.appendChild(line);
    }
}

// ULTIMATE MOUSE TRAIL EFFECT
let mouseTrail = [];
document.addEventListener('mousemove', (e) => {
    mouseTrail.push({
        x: e.clientX,
        y: e.clientY,
        time: Date.now()
    });

    // Keep only recent trail points
    mouseTrail = mouseTrail.filter(point => Date.now() - point.time < 1200);

    // Create trail particle
    const particle = document.createElement('div');
    particle.style.position = 'fixed';
    particle.style.left = e.clientX + 'px';
    particle.style.top = e.clientY + 'px';
    particle.style.width = '6px';
    particle.style.height = '6px';
    particle.style.background = 'radial-gradient(circle, #008dff, #D400FF)';
    particle.style.borderRadius = '50%';
    particle.style.pointerEvents = 'none';
    particle.style.zIndex = '9999';
    particle.style.animation = 'ultimate-fade-out 1s ease-out forwards';

    document.body.appendChild(particle);

    setTimeout(() => {
        if (particle.parentNode) {
            particle.parentNode.removeChild(particle);
        }
    }, 1000);
});

// ULTIMATE SCROLL TRIGGERED ANIMATIONS
const ultimateElements = document.querySelectorAll('.gv-ultimate-card-w5r, .gv-ultimate-text-l2s, .gv-ultimate-text-e4p');
const ultimateObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.animation = 'ultimate-entrance 1.2s ease-out forwards';
        }
    });
}, { threshold: 0.1 });

ultimateElements.forEach(el => ultimateObserver.observe(el));

// ULTIMATE KEYBOARD SHORTCUTS
document.addEventListener('keydown', (e) => {
    // Press 'U' for Ultimate Mode
    if (e.key.toLowerCase() === 'u') {
        document.body.style.filter = 'hue-rotate(60deg) saturate(1.8) brightness(1.3)';
        setTimeout(() => {
            document.body.style.filter = '';
        }, 3000);
    }

    // Press 'Q' for Quantum Mode
    if (e.key.toLowerCase() === 'q') {
        document.body.style.animation = 'ultimate-power 3s ease-in-out';
        setTimeout(() => {
            document.body.style.animation = '';
        }, 3000);
    }
});

// Initialize ultimate effects
createUltimateMatrixRain();
createUltimateWarpLines();

// ULTIMATE DYNAMIC TITLE EFFECT
const title = document.querySelector('h1');
if (title) {
    setInterval(() => {
        title.style.textShadow = `
                    0 0 15px #008dff,
                    0 0 30px #008dff,
                    0 0 45px #D400FF,
                    0 0 60px #D400FF,
                    0 0 90px #D400FF,
                    0 0 120px #D400FF,
                    0 0 150px #008dff,
                    0 0 200px #008dff
                `;
        setTimeout(() => {
            title.style.textShadow = '';
        }, 300);
    }, 4000);
}

