
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("adminSidebar");
    const overlay = document.getElementById("adminSidebarOverlay");
    const toggleButton = document.getElementById("adminMobileToggle");

    if (!sidebar || !overlay || !toggleButton) {
        return;
    }

    let previousBodyOverflow = "";

    function closeSidebar() {
        sidebar.classList.remove("show");
        overlay.classList.remove("show");
        toggleButton.setAttribute("aria-expanded", "false");

        document.body.style.overflow = previousBodyOverflow;
    }

    function openSidebar() {
        previousBodyOverflow = document.body.style.overflow;

        sidebar.classList.add("show");
        overlay.classList.add("show");
        toggleButton.setAttribute("aria-expanded", "true");

        document.body.style.overflow = "hidden";
    }

    toggleButton.addEventListener("click", function () {
        if (sidebar.classList.contains("show")) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });

    overlay.addEventListener("click", closeSidebar);

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" &&
            sidebar.classList.contains("show")) {
            closeSidebar();
            toggleButton.focus();
        }
    });

    window.addEventListener("resize", function () {
        if (window.innerWidth > 991 &&
            sidebar.classList.contains("show")) {
            closeSidebar();
        }
    });
});
