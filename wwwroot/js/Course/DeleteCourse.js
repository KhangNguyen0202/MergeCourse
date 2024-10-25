document.addEventListener('DOMContentLoaded', function () {
    // Lấy tất cả các phần tử có class "stars"
    const starElements = document.querySelectorAll('.stars');

    // Duyệt qua từng phần tử "stars"
    starElements.forEach(starElement => {
        const rating = parseInt(starElement.getAttribute('data-rating')); // Lấy giá trị xếp hạng
        const starSize = 18; // Đặt kích thước của ngôi sao (px), có thể thay đổi

        // Tạo chuỗi ngôi sao và áp dụng màu và kích thước dựa trên xếp hạng
        starElement.innerHTML = '★★★★★'.split('').map((star, index) => {
            // Màu sao sẽ thay đổi dựa trên xếp hạng
            const starColor = index < rating ? '#ff9800' : '#ccc';
            return `<span style="color: ${starColor}; font-size: ${starSize}px;">${star}</span>`;
        }).join('');
    });
});