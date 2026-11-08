
document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("guideManagementContainer");

    if (!container) {
        return;
    }

    const searchUrl = container.dataset.searchUrl;
    const detailUrl = container.dataset.detailUrl;
    const lockUrl = container.dataset.lockUrl;
    const unlockUrl = container.dataset.unlockUrl;

    const searchInput = document.getElementById("guideSearchInput");
    const accountStatusInput = document.getElementById("accountStatusInput");
    const profileStatusInput = document.getElementById("profileStatusInput");

    const table = document.getElementById("guideManagementTable");
    const recordInfo = document.getElementById("guideRecordInfo");
    const pageInfo = document.getElementById("guidePageInfo");
    const pagination = document.getElementById("guidePagination");

    const searchButton = document.getElementById("guideSearchButton");
    const resetButton = document.getElementById("guideResetButton");
    const refreshButton = document.getElementById("guideRefreshButton");

    const tokenForm = document.getElementById("guideManagementTokenForm");
    const tokenInput = tokenForm.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    const lockModalElement = document.getElementById("lockGuideModal");
    const lockReasonInput = document.getElementById("lockReasonInput");
    const lockReasonError = document.getElementById("lockReasonError");
    const lockReasonCounter = document.getElementById("lockReasonCounter");
    const confirmLockButton = document.getElementById("confirmLockButton");

    const lockModal = new bootstrap.Modal(lockModalElement);

    let currentPage = 1;
    let currentRequest = null;
    let selectedUserId = null;

    function showMessage(message) {
        table.replaceChildren();

        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 7;
        cell.className = "text-center py-4 text-muted";
        cell.textContent = message;

        row.appendChild(cell);
        table.appendChild(row);
    }

    function getProfileStatusText(status) {
        if (status === 0) {
            return "Chờ duyệt";
        }

        if (status === 1) {
            return "Đã duyệt";
        }

        if (status === 2) {
            return "Bị từ chối";
        }

        if (status === 3) {
            return "Đã khóa";
        }

        if (status === 4) {
            return "Yêu cầu chỉnh sửa";
        }

        return "Chưa có hồ sơ";
    }

    function getProfileStatusClass(status) {
        if (status === 1) {
            return "status-approved";
        }

        if (status === 2 || status === 3) {
            return "status-rejected";
        }

        if (status === 4) {
            return "status-warning";
        }

        if (status === 0) {
            return "status-pending";
        }

        return "status-unknown";
    }

    function validateResult(result) {
        if (result == null ||
            Array.isArray(result.items) === false ||
            typeof result.currentPage !== "number" ||
            typeof result.pageSize !== "number" ||
            typeof result.totalRecords !== "number" ||
            typeof result.totalPages !== "number") {
            return false;
        }

        if (result.currentPage < 1 ||
            result.pageSize < 1 ||
            result.totalRecords < 0 ||
            result.totalPages < 1) {
            return false;
        }

        return result.items.every(function (item) {
            return item != null &&
                typeof item.userId === "number" &&
                typeof item.email === "string" &&
                typeof item.fullName === "string" &&
                typeof item.accountStatus === "number" &&
                typeof item.createdAt === "string";
        });
    }

    function createStatusBadge(text, className) {
        const badge = document.createElement("span");

        badge.className = "guide-status-badge " + className;
        badge.textContent = text;

        return badge;
    }

    function renderTable(result) {
        table.replaceChildren();

        if (result.items.length === 0) {
            showMessage("Không tìm thấy hướng dẫn viên.");
            return;
        }

        result.items.forEach(function (item, index) {
            const row = document.createElement("tr");

            const numberCell = document.createElement("td");
            numberCell.textContent =
                (result.currentPage - 1) * result.pageSize + index + 1;

            const nameCell = document.createElement("td");
            nameCell.className = "fw-semibold";
            nameCell.textContent = item.fullName;

            const emailCell = document.createElement("td");
            emailCell.textContent = item.email;

            const phoneCell = document.createElement("td");
            phoneCell.textContent = item.contactPhone || "Chưa cập nhật";

            const accountCell = document.createElement("td");

            if (item.accountStatus === 1) {
                accountCell.appendChild(
                    createStatusBadge("Đang hoạt động", "status-approved")
                );
            } else {
                accountCell.appendChild(
                    createStatusBadge("Đã khóa", "status-rejected")
                );
            }

            const profileCell = document.createElement("td");
            profileCell.appendChild(
                createStatusBadge(
                    getProfileStatusText(item.profileStatus),
                    getProfileStatusClass(item.profileStatus)
                )
            );

            const actionCell = document.createElement("td");
            const actionGroup = document.createElement("div");

            actionGroup.className = "guide-action-group";

            const detailButton = document.createElement("a");
            detailButton.className = "btn btn-sm btn-outline-primary";
            detailButton.textContent = "Chi tiết";
            detailButton.href =
                detailUrl + "?userId=" + encodeURIComponent(item.userId);

            actionGroup.appendChild(detailButton);

            const statusButton = document.createElement("button");
            statusButton.type = "button";
            statusButton.className = "btn btn-sm";

            if (item.accountStatus === 1) {
                statusButton.classList.add("btn-outline-danger");
                statusButton.textContent = "Khóa";

                statusButton.addEventListener("click", function () {
                    openLockModal(item.userId);
                });
            } else {
                statusButton.classList.add("btn-outline-primary");
                statusButton.textContent = "Mở khóa";

                statusButton.addEventListener("click", function () {
                    unlockGuide(item.userId);
                });
            }

            actionGroup.appendChild(statusButton);
            actionCell.appendChild(actionGroup);

            row.appendChild(numberCell);
            row.appendChild(nameCell);
            row.appendChild(emailCell);
            row.appendChild(phoneCell);
            row.appendChild(accountCell);
            row.appendChild(profileCell);
            row.appendChild(actionCell);

            table.appendChild(row);
        });
    }

    function renderPagination(result) {
        pagination.replaceChildren();

        function createButton(label, page, disabled, active) {
            const button = document.createElement("button");

            button.type = "button";
            button.className = "btn btn-sm ";

            if (active) {
                button.className += "btn-primary";
            } else {
                button.className += "btn-outline-primary";
            }

            button.textContent = label;
            button.disabled = disabled;

            button.addEventListener("click", function () {
                loadGuides(page);
            });

            pagination.appendChild(button);
        }

        createButton(
            "Trước",
            result.currentPage - 1,
            result.currentPage <= 1,
            false
        );

        createButton(
            result.currentPage,
            result.currentPage,
            true,
            true
        );

        createButton(
            "Sau",
            result.currentPage + 1,
            result.currentPage >= result.totalPages,
            false
        );
    }

    async function loadGuides(page) {
        if (currentRequest !== null) {
            currentRequest.abort();
        }

        const controller = new AbortController();
        currentRequest = controller;

        const params = new URLSearchParams();

        params.set("Page", page);

        if (searchInput.value.trim() !== "") {
            params.set("Search", searchInput.value.trim());
        }

        if (accountStatusInput.value !== "") {
            params.set("AccountStatus", accountStatusInput.value);
        }

        if (profileStatusInput.value !== "") {
            params.set("ProfileStatus", profileStatusInput.value);
        }

        searchButton.disabled = true;
        resetButton.disabled = true;
        refreshButton.disabled = true;

        showMessage("Đang tải dữ liệu...");

        try {
            const response = await fetch(
                searchUrl + "?" + params.toString(),
                {
                    method: "GET",
                    credentials: "same-origin",
                    headers: {
                        "Accept": "application/json",
                        "X-Requested-With": "XMLHttpRequest"
                    },
                    signal: controller.signal
                }
            );

            if (response.redirected || response.status === 401) {
                throw new Error(
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
            }

            if (response.status === 403) {
                throw new Error(
                    "Bạn không có quyền truy cập chức năng này."
                );
            }

            const contentType = response.headers.get("content-type");

            if (contentType == null ||
                contentType.includes("application/json") === false) {
                throw new Error("Phản hồi từ máy chủ không hợp lệ.");
            }

            const result = await response.json();

            if (response.ok === false) {
                throw new Error(
                    result.message || "Không thể tải danh sách hướng dẫn viên."
                );
            }

            if (validateResult(result) === false) {
                throw new Error("Dữ liệu trả về không hợp lệ.");
            }

            currentPage = result.currentPage;

            renderTable(result);
            renderPagination(result);

            let startRecord = 0;

            if (result.totalRecords > 0) {
                startRecord =
                    (result.currentPage - 1) * result.pageSize + 1;
            }

            const endRecord = Math.min(
                result.currentPage * result.pageSize,
                result.totalRecords
            );

            recordInfo.textContent =
                "Tổng số " + result.totalRecords + " hướng dẫn viên";

            pageInfo.textContent =
                "Hiển thị " + startRecord + " - " + endRecord;
        } catch (error) {
            if (error.name !== "AbortError") {
                showMessage(error.message);

                recordInfo.textContent = "Không thể tải dữ liệu.";
                pageInfo.textContent = "";
                pagination.replaceChildren();
            }
        } finally {
            if (currentRequest === controller) {
                currentRequest = null;

                searchButton.disabled = false;
                resetButton.disabled = false;
                refreshButton.disabled = false;
            }
        }
    }

    function openLockModal(userId) {
        selectedUserId = userId;
        lockReasonInput.value = "";
        lockReasonError.textContent = "";
        lockReasonCounter.textContent = "0/500";

        confirmLockButton.disabled = false;
        confirmLockButton.textContent = "Xác nhận khóa";

        lockModal.show();
    }

    async function unlockGuide(userId) {
        const confirmed = window.confirm(
            "Bạn có chắc chắn muốn mở khóa tài khoản này?"
        );

        if (confirmed === false) {
            return;
        }

        await sendAccountRequest(
            unlockUrl,
            userId,
            null
        );
    }

    
    async function sendAccountRequest(url, userId, reason) {
        const formData = new URLSearchParams();

        formData.set("userId", userId);

        if (reason !== null) {
            formData.set("reason", reason);
        }

        formData.set(
            "__RequestVerificationToken",
            tokenInput.value
        );

        try {
            const response = await fetch(url, {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "Accept": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: formData.toString()
            });

            if (response.redirected || response.status === 401) {
                throw new Error(
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
            }

            if (response.status === 403) {
                throw new Error(
                    "Bạn không có quyền thực hiện thao tác này."
                );
            }

            const contentType = response.headers.get("content-type");

            if (contentType == null ||
                contentType.includes("application/json") === false) {
                throw new Error("Phản hồi từ máy chủ không hợp lệ.");
            }

            const result = await response.json();

            if (response.ok === false || result.success !== true) {
                throw new Error(
                    result.message || "Không thể cập nhật trạng thái tài khoản."
                );
            }

            window.alert(result.message);
            await loadGuides(currentPage);

            return true;
        } catch (error) {
            window.alert(error.message);
            return false;
        }
    }


    searchButton.addEventListener("click", function () {
        loadGuides(1);
    });

    searchInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            loadGuides(1);
        }
    });

    accountStatusInput.addEventListener("change", function () {
        loadGuides(1);
    });

    profileStatusInput.addEventListener("change", function () {
        loadGuides(1);
    });

    refreshButton.addEventListener("click", function () {
        loadGuides(currentPage);
    });

    resetButton.addEventListener("click", function () {
        searchInput.value = "";
        accountStatusInput.value = "";
        profileStatusInput.value = "";

        loadGuides(1);
    });

    lockReasonInput.addEventListener("input", function () {
        lockReasonCounter.textContent =
            lockReasonInput.value.length + "/500";

        lockReasonError.textContent = "";
    });

    
    confirmLockButton.addEventListener("click", async function () {
        const reason = lockReasonInput.value.trim();

        if (reason.length === 0) {
            lockReasonError.textContent = "Vui lòng nhập lý do khóa.";
            lockReasonInput.focus();
            return;
        }

        if (reason.length > 500) {
            lockReasonError.textContent =
                "Lý do khóa không được vượt quá 500 ký tự.";
            lockReasonInput.focus();
            return;
        }

        if (selectedUserId === null) {
            return;
        }

        confirmLockButton.disabled = true;
        confirmLockButton.textContent = "Đang xử lý...";

        const success = await sendAccountRequest(
            lockUrl,
            selectedUserId,
            reason
        );

        if (success) {
            lockModal.hide();
        }

        confirmLockButton.disabled = false;
        confirmLockButton.textContent = "Xác nhận khóa";
    });


    lockModalElement.addEventListener("hidden.bs.modal", function () {
        selectedUserId = null;
        lockReasonInput.value = "";
        lockReasonError.textContent = "";
        lockReasonCounter.textContent = "0/500";
    });

    loadGuides(1);
});
