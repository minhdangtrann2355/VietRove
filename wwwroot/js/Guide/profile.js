// xu ly xac nhan thao tac ho so
document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (event) {
        const confirmElement = event.target.closest('[data-confirm]');
        if (confirmElement !== null) {
            const message = confirmElement.getAttribute('data-confirm');
            if (message && window.confirm(message) === false) {
                event.preventDefault();
            }
        }
    });
});
