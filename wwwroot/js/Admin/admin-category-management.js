
document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("categoryManagement");

    if (!container) {
        return;
    }

    const searchUrl = container.dataset.searchUrl;
    const createUrl = container.dataset.createUrl;
    const updateUrl = container.dataset.updateUrl;
    const toggleUrl = container.dataset.toggleUrl;

    const searchInput = document.getElementById("searchCategory");
    const typeFilter = document.getElementById("filterCategoryType");
    const tableBody = document.getElementById("categoryTableBody");
    const resultInfo = document.getElementById("categoryResultInfo");
    const pagination = document.getElementById("categoryPagination");

    const modal = document.getElementById("categoryModal");
    const modalTitle = document.getElementById("categoryModalTitle");
    const categoryForm = document.getElementById("categoryForm");
    const categoryIdInput = document.getElementById("categoryId");
    const categoryNameInput = document.getElementById("categoryName");
    const categoryTypeInput = document.getElementById("categoryType");
    const categoryTypeGroup = document.getElementById("categoryTypeGroup");
    const categoryNameError = document.getElementById("categoryNameError");
    const categoryTypeError = document.getElementById("categoryTypeError");
    const formMessage = document.getElementById("categoryFormMessage");
    const saveButton = document.getElementById("btnSaveCategory");

    const pageSize = 10;

    let currentPage = 1;
    let totalPages = 0;
    let searchTimer = null;
    let isSubmitting = false;

    function getAntiForgeryToken() {
        const tokenInput = categoryForm.querySelector(
            'input[name="__RequestVerificationToken"]'
        );

        if (!tokenInput) {
            return "";
        }

        return tokenInput.value;
    }

    function getProperty(object, camelName, pascalName) {
        if (object[camelName] !== undefined) {
            return object[camelName];
        }

        return object[pascalName];
    }

    function escapeHtml(value) {
        const element = document.createElement("div");
        element.textContent = value === null || value === undefined
            ? ""
            : String(value);

        return element.innerHTML;
    }

    function getCategoryTypeName(type) {
        if (type === "Province") {
            return "Tỉnh/thành phố";
        }

        if (type === "Language") {
            return "Ngôn ngữ";
        }

        if (type === "CardType") {
            return "Loại thẻ hướng dẫn viên";
        }

        return type;
    }

    function showTableMessage(message) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="5" class="category-empty">
                    ${escapeHtml(message)}
                </td>
            </tr>
        `;
    }

    function showErrorMessage(message) {
        alert(message);
    }

    async function loadCategories() {
        showTableMessage("Đang tải dữ liệu...");
        pagination.innerHTML = "";

        const params = new URLSearchParams();

        params.append("Page", currentPage);
        params.append("Search", searchInput.value.trim());
        params.append("CategoryType", typeFilter.value);

        try {
            const response = await fetch(`${searchUrl}?${params.toString()}`, {
                method: "GET",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error("Không thể tải danh sách danh mục.");
            }

            const result = await response.json();

            const items = getProperty(result, "items", "Items") || [];
            const page = getProperty(result, "currentPage", "CurrentPage") || 1;
            const totalRecords = getProperty(
                result,
                "totalRecords",
                "TotalRecords"
            ) || 0;

            totalPages = getProperty(result, "totalPages", "TotalPages") || 0;
            currentPage = page;

            renderCategories(items);
            renderPagination();
            renderResultInfo(totalRecords);
        } catch (error) {
            showTableMessage(error.message);
            resultInfo.textContent = "Không thể tải dữ liệu.";
        }
    }

    function renderCategories(items) {
        if (!items || items.length === 0) {
            showTableMessage("Không tìm thấy danh mục phù hợp.");
            return;
        }

        let html = "";

        items.forEach(function (item) {
            const id = getProperty(item, "categoryId", "CategoryId");
            const name = getProperty(item, "categoryName", "CategoryName");
            const type = getProperty(item, "categoryType", "CategoryType");
            const isActive = getProperty(item, "isActive", "IsActive");

            let statusHtml = "";
            let toggleButtonHtml = "";

            if (isActive) {
                statusHtml = `
                    <span class="category-status category-status-active">
                        Đang hoạt động
                    </span>
                `;

                toggleButtonHtml = `
                    <button type="button"
                            class="category-action-btn category-toggle-btn"
                            data-action="toggle"
                            data-id="${id}"
                            data-active="true">
                        Ngừng hoạt động
                    </button>
                `;
            } else {
                statusHtml = `
                    <span class="category-status category-status-inactive">
                        Ngừng hoạt động
                    </span>
                `;

                toggleButtonHtml = `
                    <button type="button"
                            class="category-action-btn category-toggle-btn is-active"
                            data-action="toggle"
                            data-id="${id}"
                            data-active="false">
                        Kích hoạt
                    </button>
                `;
            }

            html += `
                <tr>
                    <td>${escapeHtml(id)}</td>
                    <td>${escapeHtml(name)}</td>
                    <td>
                        <span class="category-type-label">
                            ${escapeHtml(getCategoryTypeName(type))}
                        </span>
                    </td>
                    <td>${statusHtml}</td>
                    <td>
                        <div class="category-row-actions">
                            <button type="button"
                                    class="category-action-btn category-edit-btn"
                                    data-action="edit"
                                    data-id="${id}"
                                    data-name="${escapeHtml(name)}"
                                    data-type="${escapeHtml(type)}">
                                Chỉnh sửa
                            </button>

                            ${toggleButtonHtml}
                        </div>
                    </td>
                </tr>
            `;
        });

        tableBody.innerHTML = html;
    }

    function renderResultInfo(totalRecords) {
        if (totalRecords === 0) {
            resultInfo.textContent = "Không có danh mục nào.";
            return;
        }

        const start = (currentPage - 1) * pageSize + 1;
        let end = currentPage * pageSize;

        if (end > totalRecords) {
            end = totalRecords;
        }

        resultInfo.textContent =
            `Hiển thị ${start} - ${end} trong tổng số ${totalRecords} danh mục`;
    }

    function renderPagination() {
        pagination.innerHTML = "";

        if (totalPages <= 1) {
            return;
        }

        const previousButton = document.createElement("button");
        previousButton.type = "button";
        previousButton.className = "category-page-btn";
        previousButton.textContent = "Trước";
        previousButton.disabled = currentPage <= 1;
        previousButton.dataset.page = currentPage - 1;

        pagination.appendChild(previousButton);

        let startPage = currentPage - 2;
        let endPage = currentPage + 2;

        if (startPage < 1) {
            startPage = 1;
            endPage = 5;
        }

        if (endPage > totalPages) {
            endPage = totalPages;
            startPage = totalPages - 4;
        }

        if (startPage < 1) {
            startPage = 1;
        }

        for (let page = startPage; page <= endPage; page++) {
            const button = document.createElement("button");

            button.type = "button";
            button.className = "category-page-btn";

            if (page === currentPage) {
                button.classList.add("active");
            }

            button.textContent = page;
            button.dataset.page = page;

            pagination.appendChild(button);
        }

        const nextButton = document.createElement("button");
        nextButton.type = "button";
        nextButton.className = "category-page-btn";
        nextButton.textContent = "Sau";
        nextButton.disabled = currentPage >= totalPages;
        nextButton.dataset.page = currentPage + 1;

        pagination.appendChild(nextButton);
    }

    function openCreateModal() {
        categoryForm.reset();

        categoryIdInput.value = "";
        categoryTypeInput.disabled = false;
        categoryTypeGroup.style.display = "flex";

        modalTitle.textContent = "Thêm danh mục";
        saveButton.textContent = "Lưu danh mục";

        clearFormMessage();
        clearFieldErrors();

        modal.classList.add("show");
        modal.setAttribute("aria-hidden", "false");

        categoryNameInput.focus();
    }

    function openEditModal(id, name) {
        categoryForm.reset();

        categoryIdInput.value = id;
        categoryNameInput.value = name;

        categoryTypeInput.disabled = true;
        categoryTypeGroup.style.display = "none";

        modalTitle.textContent = "Chỉnh sửa danh mục";
        saveButton.textContent = "Cập nhật";

        clearFormMessage();
        clearFieldErrors();

        modal.classList.add("show");
        modal.setAttribute("aria-hidden", "false");

        categoryNameInput.focus();
    }

    function closeModal() {
        modal.classList.remove("show");
        modal.setAttribute("aria-hidden", "true");
    }

    function clearFieldErrors() {
        categoryNameError.textContent = "";
        categoryTypeError.textContent = "";
    }

    function clearFormMessage() {
        formMessage.textContent = "";
        formMessage.className = "category-form-message";
    }

    function showFormMessage(message, isSuccess) {
        formMessage.textContent = message;
        formMessage.className = "category-form-message";

        if (isSuccess) {
            formMessage.classList.add("success");
        } else {
            formMessage.classList.add("error");
        }
    }

    async function submitCategoryForm(event) {
        event.preventDefault();

        if (isSubmitting) {
            return;
        }

        clearFieldErrors();
        clearFormMessage();

        const categoryName = categoryNameInput.value.trim();
        const categoryId = categoryIdInput.value;
        const isEdit = categoryId !== "";

        if (categoryName.length === 0) {
            categoryNameError.textContent = "Vui lòng nhập tên danh mục.";
            categoryNameInput.focus();
            return;
        }

        if (categoryName.length > 150) {
            categoryNameError.textContent =
                "Tên danh mục không được vượt quá 150 ký tự.";
            categoryNameInput.focus();
            return;
        }

        if (!isEdit && categoryTypeInput.value === "") {
            categoryTypeError.textContent =
                "Vui lòng chọn loại danh mục.";
            categoryTypeInput.focus();
            return;
        }

        const formData = new FormData();

        formData.append("CategoryName", categoryName);
        formData.append("__RequestVerificationToken", getAntiForgeryToken());

        let url = createUrl;

        if (isEdit) {
            url = updateUrl;
            formData.append("categoryId", categoryId);
        } else {
            formData.append("CategoryType", categoryTypeInput.value);
        }

        isSubmitting = true;
        saveButton.disabled = true;
        saveButton.textContent = "Đang lưu...";

        try {
            const response = await fetch(url, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json();

            if (!response.ok || result.success === false) {
                throw new Error(result.message || "Không thể lưu danh mục.");
            }

            showFormMessage(
                result.message || "Lưu danh mục thành công.",
                true
            );

            currentPage = 1;

            setTimeout(function () {
                closeModal();
                loadCategories();
            }, 500);
        } catch (error) {
            showFormMessage(error.message, false);
        } finally {
            isSubmitting = false;
            saveButton.disabled = false;

            if (isEdit) {
                saveButton.textContent = "Cập nhật";
            } else {
                saveButton.textContent = "Lưu danh mục";
            }
        }
    }

    async function toggleCategoryStatus(id, isActive) {
        let confirmMessage = "";

        if (isActive) {
            confirmMessage =
                "Bạn có chắc chắn muốn ngừng hoạt động danh mục này?";
        } else {
            confirmMessage =
                "Bạn có chắc chắn muốn kích hoạt danh mục này?";
        }

        if (!confirm(confirmMessage)) {
            return;
        }

        const formData = new FormData();

        formData.append("categoryId", id);
        formData.append("__RequestVerificationToken", getAntiForgeryToken());

        try {
            const response = await fetch(toggleUrl, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json();

            if (!response.ok || result.success === false) {
                throw new Error(
                    result.message || "Không thể thay đổi trạng thái danh mục."
                );
            }

            alert(result.message || "Cập nhật trạng thái thành công.");

            loadCategories();
        } catch (error) {
            showErrorMessage(error.message);
        }
    }

    document.getElementById("btnOpenCreate").addEventListener(
        "click",
        openCreateModal
    );

    document.getElementById("btnCloseModal").addEventListener(
        "click",
        closeModal
    );

    document.getElementById("btnCancelModal").addEventListener(
        "click",
        closeModal
    );

    document.getElementById("categoryModalOverlay").addEventListener(
        "click",
        closeModal
    );

    categoryForm.addEventListener("submit", submitCategoryForm);

    document.getElementById("btnResetFilter").addEventListener(
        "click",
        function () {
            searchInput.value = "";
            typeFilter.value = "";
            currentPage = 1;

            loadCategories();
        }
    );

    searchInput.addEventListener("input", function () {
        clearTimeout(searchTimer);

        searchTimer = setTimeout(function () {
            currentPage = 1;
            loadCategories();
        }, 400);
    });

    typeFilter.addEventListener("change", function () {
        currentPage = 1;
        loadCategories();
    });

    pagination.addEventListener("click", function (event) {
        const button = event.target.closest("[data-page]");

        if (!button || button.disabled) {
            return;
        }

        const page = Number(button.dataset.page);

        if (page < 1 || page > totalPages || page === currentPage) {
            return;
        }

        currentPage = page;
        loadCategories();
    });

    tableBody.addEventListener("click", function (event) {
        const button = event.target.closest("[data-action]");

        if (!button) {
            return;
        }

        const action = button.dataset.action;
        const id = button.dataset.id;

        if (action === "edit") {
            const name = button.dataset.name;

            openEditModal(id, name);
            return;
        }

        if (action === "toggle") {
            const isActive = button.dataset.active === "true";

            toggleCategoryStatus(id, isActive);
        }
    });

    loadCategories();
});
