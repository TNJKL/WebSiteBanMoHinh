const reviewsContainer = document.querySelector('.reviews-container');
const reviewItems = document.querySelectorAll('.review-item');
const indicators = document.querySelectorAll('.reviews-slider .carousel-indicators span');
let currentIndex = 0;
const totalItems = reviewItems.length; // Tổng số đánh giá (8)
const itemsToShow = 3; // Số lượng hiển thị cùng lúc trên desktop
const maxIndex = Math.ceil(totalItems - itemsToShow); // Số vị trí tối đa (8 - 3 = 5)
let isDragging = false;
let startPos = 0;
let currentTranslate = 0;
let prevTranslate = 0;

function updateSlider() {
    const itemWidth = reviewItems[0].offsetWidth;
    currentTranslate = -currentIndex * itemWidth;
    reviewsContainer.style.transform = `translateX(${currentTranslate}px)`;

    // Cập nhật indicator
    indicators.forEach((indicator, index) => {
        indicator.classList.toggle('active', index === currentIndex);
    });
}

// Xử lý sự kiện kéo chuột
reviewsContainer.addEventListener('mousedown', (e) => {
    isDragging = true;
    startPos = e.clientX;
    prevTranslate = currentTranslate;
    reviewsContainer.classList.add('dragging');
});

reviewsContainer.addEventListener('mousemove', (e) => {
    if (isDragging) {
        const currentPos = e.clientX;
        const diff = currentPos - startPos;
        currentTranslate = prevTranslate + diff;

        // Giới hạn kéo để không vượt quá đầu hoặc cuối
        const itemWidth = reviewItems[0].offsetWidth;
        const maxTranslate = 0; // Vị trí đầu tiên
        const minTranslate = -itemWidth * maxIndex; // Vị trí cuối cùng
        currentTranslate = Math.min(maxTranslate, Math.max(currentTranslate, minTranslate));

        reviewsContainer.style.transform = `translateX(${currentTranslate}px)`;
    }
});

reviewsContainer.addEventListener('mouseup', () => {
    isDragging = false;
    reviewsContainer.classList.remove('dragging');

    const itemWidth = reviewItems[0].offsetWidth;
    const movedBy = currentTranslate - prevTranslate;

    // Tính toán vị trí mới dựa trên khoảng cách kéo
    if (movedBy < -itemWidth / 2 && currentIndex < maxIndex) {
        currentIndex++;
    } else if (movedBy > itemWidth / 2 && currentIndex > 0) {
        currentIndex--;
    }

    // Giới hạn currentIndex
    currentIndex = Math.max(0, Math.min(currentIndex, maxIndex));
    updateSlider();
});

reviewsContainer.addEventListener('mouseleave', () => {
    if (isDragging) {
        isDragging = false;
        reviewsContainer.classList.remove('dragging');

        const itemWidth = reviewItems[0].offsetWidth;
        const movedBy = currentTranslate - prevTranslate;

        if (movedBy < -itemWidth / 2 && currentIndex < maxIndex) {
            currentIndex++;
        } else if (movedBy > itemWidth / 2 && currentIndex > 0) {
            currentIndex--;
        }

        currentIndex = Math.max(0, Math.min(currentIndex, maxIndex));
        updateSlider();
    }
});

// Xử lý nhấp vào indicator
indicators.forEach((indicator, index) => {
    indicator.addEventListener('click', () => {
        currentIndex = index;
        updateSlider();
    });
});

// Điều chỉnh khi thay đổi kích thước màn hình
window.addEventListener('resize', updateSlider);
updateSlider();