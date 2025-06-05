// Smooth scrolling for navigation links
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

// Header scroll effect
window.addEventListener('scroll', () => {
    const header = document.querySelector('header');
    if (window.scrollY > 50) {
        header.style.background = 'rgba(0, 0, 0, 0.98)';
        header.style.boxShadow = '0 2px 20px rgba(0, 241, 255, 0.1)';
    } else {
        header.style.background = 'rgba(0, 0, 0, 0.95)';
        header.style.boxShadow = 'none';
    }
});

// Scroll animations
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('visible');
        }
    });
}, observerOptions);

document.querySelectorAll('.fade-in').forEach(el => {
    observer.observe(el);
});

// Create floating particles
function createParticle() {
    const particle = document.createElement('div');
    particle.className = 'particle';
    particle.style.left = Math.random() * 100 + 'vw';
    particle.style.animationDuration = (Math.random() * 3 + 2) + 's';
    particle.style.opacity = Math.random();
    
    const colors = ['#ff0000', '#00f1ff', '#ff6666', '#66f1ff'];
    particle.style.background = colors[Math.floor(Math.random() * colors.length)];
    
    document.querySelector('.hero').appendChild(particle);

    setTimeout(() => {
        particle.remove();
    }, 5000);
}

setInterval(createParticle, 300);

// Mobile menu toggle
const mobileMenu = document.querySelector('.mobile-menu');
const navLinks = document.querySelector('.nav-links');

mobileMenu.addEventListener('click', () => {
    navLinks.style.display = navLinks.style.display === 'flex' ? 'none' : 'flex';
    if (navLinks.style.display === 'flex') {
        navLinks.style.position = 'absolute';
        navLinks.style.top = '100%';
        navLinks.style.left = '0';
        navLinks.style.right = '0';
        navLinks.style.background = 'rgba(0, 0, 0, 0.95)';
        navLinks.style.flexDirection = 'column';
        navLinks.style.padding = '1rem';
        navLinks.style.backdropFilter = 'blur(10px)';
    }
});

// Audio control
const audio = document.getElementById('background-audio');
const audioControl = document.querySelector('.audio-control');
const recordDisc = document.querySelector('.record-disc');

audioControl.addEventListener('click', () => {
    if (audio.paused) {
        audio.play();
        recordDisc.classList.add('rotating');
        recordDisc.classList.remove('muted');
    } else {
        audio.pause();
        recordDisc.classList.remove('rotating');
        recordDisc.classList.add('muted');
    }
});

// Character sliders
document.querySelectorAll('.slider').forEach(slider => {
    const player = slider.getAttribute('data-player');
    const slides = slider.querySelectorAll('.slide');
    let currentIndex = 0;

    const prevBtn = slider.querySelector('.prev');
    const nextBtn = slider.querySelector('.next');

    function updateSlider(direction = null) {
        slides.forEach((slide, index) => {
            slide.classList.remove('active');
            slide.style.transform = '';
            slide.style.opacity = '0';
            if (index === currentIndex) {
                slide.classList.add('active');
                slide.style.opacity = '1';
                if (direction === 'next') {
                    slide.style.animation = 'slideLeft 0.5s ease-in-out';
                } else if (direction === 'prev') {
                    slide.style.animation = 'slideRight 0.5s ease-in-out';
                }
            }
        });
    }

    prevBtn.addEventListener('click', () => {
        currentIndex = (currentIndex - 1 + slides.length) % slides.length;
        updateSlider('prev');
    });

    nextBtn.addEventListener('click', () => {
        currentIndex = (currentIndex + 1) % slides.length;
        updateSlider('next');
    });

    // Character modal functionality
    slides.forEach(slide => {
        slide.addEventListener('click', () => {
            const modal = document.getElementById('character-modal');
            const modalTitle = document.getElementById('character-modal-title');
            const spriteImg = document.getElementById('character-sprite');
            const deathSpriteImg = document.getElementById('character-death-sprite');
            const gunSpriteImg = document.getElementById('character-gun-sprite');

            const characterName = slide.getAttribute('data-character');
            const spriteSrc = slide.getAttribute('data-sprite');
            const deathSpriteSrc = slide.getAttribute('data-death-sprite');
            const gunSpriteSrc = slide.getAttribute('data-gun-sprite');

            modalTitle.textContent = characterName.charAt(0).toUpperCase() + characterName.slice(1);
            spriteImg.src = spriteSrc;
            deathSpriteImg.src = deathSpriteSrc;
            gunSpriteImg.src = gunSpriteSrc;

            modal.style.display = 'flex';
        });
    });

    updateSlider();
});

// Map modal functionality
document.querySelectorAll('.map-card').forEach(card => {
    card.addEventListener('click', () => {
        const modal = document.getElementById('map-modal');
        const modalImage = document.getElementById('modal-image');
        const modalTitle = document.getElementById('modal-title');
        const mapId = card.getAttribute('data-map');
        const mapName = card.querySelector('h1').textContent;
        const modalImageSrc = card.getAttribute('data-modal-image');

        modalImage.src = modalImageSrc;
        modalTitle.textContent = mapName;
        modal.style.display = 'flex';
    });
});

// Power-up modal functionality
document.querySelectorAll('.power-up-card').forEach(card => {
    card.addEventListener('click', () => {
        const modal = document.getElementById('power-up-modal');
        const modalVideo = document.getElementById('power-up-video').querySelector('source');
        const modalTitle = document.getElementById('power-up-title');
        const modalDescription = document.getElementById('power-up-description');
        const powerUpId = card.getAttribute('data-power-up');
        const powerUpName = card.querySelector('.power-up-title').textContent;
        const powerUpVideoSrc = card.getAttribute('data-video');
        const powerUpDescription = card.querySelector('p').textContent;

        modalVideo.src = powerUpVideoSrc;
        modalVideo.parentElement.load(); // Reload video to apply new source
        modalTitle.textContent = powerUpName;
        modalDescription.textContent = powerUpDescription;
        modal.style.display = 'flex';
    });
});

// Close modals
document.querySelectorAll('.close-modal').forEach(button => {
    button.addEventListener('click', () => {
        document.getElementById('map-modal').style.display = 'none';
        document.getElementById('power-up-modal').style.display = 'none';
        document.getElementById('character-modal').style.display = 'none';
    });
});

document.querySelectorAll('.modal').forEach(modal => {
    modal.addEventListener('click', (e) => {
        if (e.target === e.currentTarget) {
            e.currentTarget.style.display = 'none';
        }
    });
});

// Add CSS for ripple and slider animations
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    @keyframes slideLeft {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideRight {
        from { transform: translateX(-100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
`;
document.head.appendChild(style);