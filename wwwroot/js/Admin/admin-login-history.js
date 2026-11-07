
document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("loginHistoryContainer");

    if (!container) {
        return;
    }

    const searchUrl = container.dataset.url;

    const searchInput = document.getElementById("searchInput");
    const roleInput = document.getElementById("roleInput");
    const fromDateInput = document.getElementById("fromDateInput");
    const toDateInput = document.getElementById("toDateInput");

    const historyTable = document.getElementById("historyTable");
    const recordInfo = document.getElementById("recordInfo");
    const pageInfo = document.getElementById("pageInfo");
    const pagination = document.getElementById("pagination");

    const searchButton = document.getElementById("searchButton");
    const resetButton = document.getElementById("resetButton");
    const refreshButton = document.getElementById("refreshButton");

    let currentPage = 1;
    let currentRequest = null;

    function showMessage(message) {
        historyTable.replaceChildren();

        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 4;
        cell.className = "text-center py-4 text-muted";
        cell.textContent = message;

        row.appendChild(cell);
        historyTable.appendChild(row);
    }

    function formatDate(value) {
        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return "Không xác định";
        }

        return new Intl.DateTimeFormat("vi-VN", {
            dateStyle: "short",
            timeStyle: "medium"
        }).format(date);
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
                typeof item.roleId === "number" &&
                typeof item.roleName === "string" &&
                typeof item.loginTime === "string";
        });
    }

    function renderTable(result) {
        historyTable.replaceChildren();

        if (result.items.length === 0) {
            showMessage("Không tìm thấy lịch sử đăng nhập.");
            return;
        }

        result.items.forEach(function (item, index) {
            const row = document.createElement("tr");

            const numberCell = document.createElement("td");
            numberCell.textContent =
                (result.currentPage - 1) * result.pageSize + index + 1;

            const emailCell = document.createElement("td");
            emailCell.textContent = item.email;

            const roleCell = document.createElement("td");
            const roleBadge = document.createElement("span");

            roleBadge.className = "role-badge";

            if (item.roleId === 1) {
                roleBadge.classList.add("role-admin");
            } else if (item.roleId === 2) {
                roleBadge.classList.add("role-guide");
            } else {
                roleBadge.classList.add("role-unknown");
            }

            roleBadge.textContent = item.roleName;
            roleCell.appendChild(roleBadge);

            const timeCell = document.createElement("td");
            timeCell.textContent = formatDate(item.loginTime);

            row.appendChild(numberCell);
            row.appendChild(emailCell);
            row.appendChild(roleCell);
            row.appendChild(timeCell);

            historyTable.appendChild(row);
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
                loadHistory(page);
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

    async function loadHistory(page) {
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

        if (roleInput.value !== "") {
            params.set("RoleId", roleInput.value);
        }

        if (fromDateInput.value !== "") {
            params.set("FromDate", fromDateInput.value);
        }

        if (toDateInput.value !== "") {
            params.set("ToDate", toDateInput.value);
        }

        searchButton.disabled = true;
        refreshButton.disabled = true;
        resetButton.disabled = true;

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

            if (response.redirected) {
                throw new Error(
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
            }

            if (response.status === 401) {
                throw new Error(
                    "Bạn cần đăng nhập lại để tiếp tục."
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
                throw new Error(
                    "Phản hồi từ máy chủ không hợp lệ."
                );
            }

            const result = await response.json();

            if (response.ok === false) {
                if (result.message) {
                    throw new Error(result.message);
                }

                throw new Error(
                    "Không thể tải lịch sử đăng nhập."
                );
            }

            if (validateResult(result) === false) {
                throw new Error(
                    "Dữ liệu trả về không hợp lệ."
                );
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
                "Tổng số " + result.totalRecords + " lượt đăng nhập";

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
                refreshButton.disabled = false;
                resetButton.disabled = false;
            }
        }
    }

    searchButton.addEventListener("click", function () {
        loadHistory(1);
    });

    searchInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            loadHistory(1);
        }
    });

    refreshButton.addEventListener("click", function () {
        loadHistory(currentPage);
    });

    resetButton.addEventListener("click", function () {
        searchInput.value = "";
        roleInput.value = "";
        fromDateInput.value = "";
        toDateInput.value = "";

        loadHistory(1);
    });

    loadHistory(1);
});
