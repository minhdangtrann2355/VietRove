document.addEventListener("DOMContentLoaded", function () {
    const page = document.getElementById("violationReportPage");

    if (page === null) {
        return;
    }

    const searchUrl = page.dataset.searchUrl;
    const detailUrl = page.dataset.detailUrl;

    const searchInput = document.getElementById("reportSearch");
    const statusSelect = document.getElementById("reportStatus");
    const fromDateInput = document.getElementById("reportFromDate");
    const toDateInput = document.getElementById("reportToDate");

    const searchButton = document.getElementById("btnSearchReports");
    const resetButton = document.getElementById("btnResetReports");

    const tableBody = document.getElementById("reportTableBody");
    const totalText = document.getElementById("reportTotalText");
    const paginationInfo = document.getElementById("reportPaginationInfo");
    const pagination = document.getElementById("reportPagination");

    let currentPage = 1;
    let totalPages = 0;

    function getStatusInfo(status) {
        const statuses = {
            0: {
                text: "Chờ xử lý",
                className: "violation-status-pending"
            },
            1: {
                text: "Đang xử lý",
                className: "violation-status-processing"
            },
            2: {
                text: "Đã xử lý",
                className: "violation-status-resolved"
            },
            3: {
                text: "Không vi phạm",
                className: "violation-status-invalid"
            }
        };

        if (statuses[status] === undefined) {
            return {
                text: "Không xác định",
                className: "violation-status-invalid"
            };
        }

        return statuses[status];
    }

    function formatDate(value) {
        if (value === null || value === undefined || value === "") {
            return "";
        }

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return "";
        }

        return date.toLocaleString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function createCell(text, className) {
        const cell = document.createElement("td");
        cell.textContent = text;

        if (className) {
            cell.className = className;
        }

        return cell;
    }

    function showTableMessage(message) {
        tableBody.replaceChildren();

        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 7;
        cell.className = "violation-table-message";
        cell.textContent = message;

        row.appendChild(cell);
        tableBody.appendChild(row);
    }

    function renderReports(items) {
        tableBody.replaceChildren();

        if (items.length === 0) {
            showTableMessage("Không tìm thấy báo cáo vi phạm.");
            return;
        }

        items.forEach(function (item) {
            const row = document.createElement("tr");

            const reportIdCell = createCell(
                "#" + item.reportId,
                "violation-report-id"
            );

            row.appendChild(reportIdCell);

            const reporterCell = createCell(item.reporterName || "Không có thông tin");
            row.appendChild(reporterCell);

            const guideCell = createCell(item.guideName || "Không có thông tin");
            row.appendChild(guideCell);

            const dateCell = createCell(formatDate(item.createdAt));
            row.appendChild(dateCell);

            const evidenceCell = createCell(
                item.evidenceCount + " tệp",
                "violation-evidence-count"
            );

            row.appendChild(evidenceCell);

            const statusCell = document.createElement("td");
            const statusInfo = getStatusInfo(item.status);
            const statusBadge = document.createElement("span");

            statusBadge.className =
                "violation-status " + statusInfo.className;

            statusBadge.textContent = statusInfo.text;

            statusCell.appendChild(statusBadge);
            row.appendChild(statusCell);

            const actionCell = document.createElement("td");
            const detailLink = document.createElement("a");

            detailLink.className = "violation-detail-link";
            detailLink.textContent = "Chi tiết";

            detailLink.href =
                detailUrl + "?id=" + encodeURIComponent(item.reportId);

            actionCell.appendChild(detailLink);
            row.appendChild(actionCell);

            tableBody.appendChild(row);
        });
    }

    function renderPagination(data) {
        pagination.replaceChildren();

        currentPage = data.currentPage;
        totalPages = data.totalPages;

        const startRecord = data.totalRecords === 0
            ? 0
            : (currentPage - 1) * data.pageSize + 1;

        let endRecord = currentPage * data.pageSize;

        if (endRecord > data.totalRecords) {
            endRecord = data.totalRecords;
        }

        paginationInfo.textContent =
            "Hiển thị " + startRecord + " - " + endRecord +
            " / " + data.totalRecords + " báo cáo";

        function createPageButton(text, pageNumber, disabled, active) {
            const button = document.createElement("button");

            button.type = "button";
            button.className = "violation-page-btn";
            button.textContent = text;
            button.disabled = disabled;

            if (active) {
                button.classList.add("active");
            }

            button.addEventListener("click", function () {
                loadReports(pageNumber);
            });

            pagination.appendChild(button);
        }

        createPageButton(
            "Trước",
            currentPage - 1,
            currentPage <= 1,
            false
        );

        if (totalPages > 0) {
            let startPage = currentPage - 2;
            let endPage = currentPage + 2;

            if (startPage < 1) {
                startPage = 1;
            }

            if (endPage > totalPages) {
                endPage = totalPages;
            }

            if (endPage - startPage < 4) {
                if (startPage === 1) {
                    endPage = Math.min(totalPages, startPage + 4);
                } else {
                    startPage = Math.max(1, endPage - 4);
                }
            }

            for (let i = startPage; i <= endPage; i++) {
                createPageButton(
                    String(i),
                    i,
                    false,
                    i === currentPage
                );
            }
        }

        createPageButton(
            "Sau",
            currentPage + 1,
            currentPage >= totalPages || totalPages === 0,
            false
        );
    }

    async function loadReports(pageNumber) {
        if (fromDateInput.value !== "" &&
            toDateInput.value !== "" &&
            fromDateInput.value > toDateInput.value) {
            showTableMessage("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            totalText.textContent = "Dữ liệu tìm kiếm không hợp lệ.";
            paginationInfo.textContent = "";
            pagination.replaceChildren();
            return;
        }

        currentPage = pageNumber;

        const params = new URLSearchParams();

        params.set("Page", String(pageNumber));

        if (searchInput.value.trim() !== "") {
            params.set("Search", searchInput.value.trim());
        }

        if (statusSelect.value !== "") {
            params.set("Status", statusSelect.value);
        }

        if (fromDateInput.value !== "") {
            params.set("FromDate", fromDateInput.value);
        }

        if (toDateInput.value !== "") {
            params.set("ToDate", toDateInput.value);
        }

        searchButton.disabled = true;
        resetButton.disabled = true;

        showTableMessage("Đang tải dữ liệu...");
        totalText.textContent = "Đang tải dữ liệu...";
        paginationInfo.textContent = "";
        pagination.replaceChildren();

        try {
            const response = await fetch(
                searchUrl + "?" + params.toString(),
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    credentials: "same-origin"
                }
            );

            const result = await response.json();

            if (!response.ok || result.success !== true) {
                throw new Error(
                    result.message || "Không thể tải danh sách báo cáo."
                );
            }

            const data = result.data;

            renderReports(data.items);
            renderPagination(data);

            totalText.textContent =
                "Tổng cộng " + data.totalRecords + " báo cáo";
        } catch (error) {
            showTableMessage(
                error.message || "Đã xảy ra lỗi khi tải dữ liệu."
            );

            totalText.textContent = "Không thể tải dữ liệu.";
            paginationInfo.textContent = "";
            pagination.replaceChildren();
        } finally {
            searchButton.disabled = false;
            resetButton.disabled = false;
        }
    }

    searchButton.addEventListener("click", function () {
        loadReports(1);
    });

    resetButton.addEventListener("click", function () {
        searchInput.value = "";
        statusSelect.value = "";
        fromDateInput.value = "";
        toDateInput.value = "";

        loadReports(1);
    });

    searchInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            loadReports(1);
        }
    });

    loadReports(1);
});