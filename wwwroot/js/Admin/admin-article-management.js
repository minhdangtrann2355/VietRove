
document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("articleSearch");
    const typeFilter = document.getElementById("articleTypeFilter");
    const statusFilter = document.getElementById("articleStatusFilter");
    const searchButton = document.getElementById("btnSearchArticle");
    const resetButton = document.getElementById("btnResetArticle");
    const tableBody = document.getElementById("articleTableBody");
    const pagination = document.getElementById("articlePagination");
    const totalRecords = document.getElementById("articleTotalRecords");

    let currentPage = 1;

    function getArticleTypeName(type) {
        switch (type) {
            case 1:
                return "Tin tức";
            case 2:
                return "Trải nghiệm du lịch";
            case 3:
                return "Thông báo";
            default:
                return "Không xác định";
        }
    }

    function getStatusInfo(status) {
        switch (status) {
            case 0:
                return {
                    text: "Bản nháp",
                    className: "status-draft"
                };
            case 1:
                return {
                    text: "Đã xuất bản",
                    className: "status-published"
                };
            case 2:
                return {
                    text: "Đã ẩn",
                    className: "status-hidden"
                };
            default:
                return {
                    text: "Không xác định",
                    className: "status-draft"
                };
        }
    }

    function formatDate(value) {
        if (!value) {
            return "-";
        }

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return "-";
        }

        return date.toLocaleDateString("vi-VN");
    }

    function createCell(value, className) {
        const cell = document.createElement("td");
        cell.textContent = value;

        if (className) {
            cell.className = className;
        }

        return cell;
    }

    function renderArticles(items) {
        tableBody.replaceChildren();

        if (!Array.isArray(items) || items.length === 0) {
            const row = document.createElement("tr");
            const cell = document.createElement("td");

            cell.colSpan = 7;
            cell.className = "table-message";
            cell.textContent = "Không tìm thấy bài viết phù hợp.";

            row.appendChild(cell);
            tableBody.appendChild(row);
            return;
        }

        items.forEach(function (article) {
            const row = document.createElement("tr");

            row.appendChild(createCell(article.articleId));

            const titleCell = createCell(
                article.title || "Không có tiêu đề",
                "article-title-cell"
            );
            row.appendChild(titleCell);

            row.appendChild(
                createCell(getArticleTypeName(article.articleType))
            );

            row.appendChild(
                createCell(article.authorName || "-")
            );

            row.appendChild(
                createCell(formatDate(article.createdAt))
            );

            const statusCell = document.createElement("td");
            const statusInfo = getStatusInfo(article.status);
            const statusLabel = document.createElement("span");

            statusLabel.className =
                "status-label " + statusInfo.className;
            statusLabel.textContent = statusInfo.text;

            statusCell.appendChild(statusLabel);
            row.appendChild(statusCell);

            const actionCell = document.createElement("td");
            actionCell.className = "col-actions";

            const detailLink = document.createElement("a");
            detailLink.className = "article-action-link";
            detailLink.href =
                "/AdminArticle/Detail?articleId=" +
                encodeURIComponent(article.articleId);
            detailLink.textContent = "Chi tiết";

            actionCell.appendChild(detailLink);
            row.appendChild(actionCell);

            tableBody.appendChild(row);
        });
    }

    function renderPagination(data) {
        pagination.replaceChildren();

        const totalPages = Number(data.totalPages) || 0;
        const page = Number(data.currentPage) || 1;

        if (totalPages <= 1) {
            return;
        }

        const previousButton = document.createElement("button");
        previousButton.type = "button";
        previousButton.textContent = "Trước";
        previousButton.disabled = page <= 1;
        previousButton.addEventListener("click", function () {
            loadArticles(page - 1);
        });

        pagination.appendChild(previousButton);

        for (let i = 1; i <= totalPages; i++) {
            const pageButton = document.createElement("button");

            pageButton.type = "button";
            pageButton.textContent = i;

            if (i === page) {
                pageButton.classList.add("active");
                pageButton.disabled = true;
            }

            pageButton.addEventListener("click", function () {
                loadArticles(i);
            });

            pagination.appendChild(pageButton);
        }

        const nextButton = document.createElement("button");
        nextButton.type = "button";
        nextButton.textContent = "Sau";
        nextButton.disabled = page >= totalPages;
        nextButton.addEventListener("click", function () {
            loadArticles(page + 1);
        });

        pagination.appendChild(nextButton);
    }

    async function loadArticles(page) {
        currentPage = page;

        const params = new URLSearchParams();

        params.set("Page", currentPage);

        if (searchInput.value.trim() !== "") {
            params.set("Search", searchInput.value.trim());
        }

        if (typeFilter.value !== "") {
            params.set("ArticleType", typeFilter.value);
        }

        if (statusFilter.value !== "") {
            params.set("Status", statusFilter.value);
        }

        tableBody.innerHTML =
            '<tr><td colspan="7" class="table-message">Đang tải dữ liệu...</td></tr>';

        pagination.replaceChildren();

        try {
            const response = await fetch(
                "/AdminArticle/Search?" + params.toString(),
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    credentials: "same-origin"
                }
            );

            if (!response.ok) {
                throw new Error("Không thể tải danh sách bài viết.");
            }

            const result = await response.json();

            if (!result || !Array.isArray(result.items)) {
                throw new Error("Dữ liệu trả về không hợp lệ.");
            }

            renderArticles(result.items);
            renderPagination(result);

            totalRecords.textContent =
                "Tổng số: " + (result.totalRecords || 0) + " bài viết";
        } catch (error) {
            console.error("Lỗi tải danh sách bài viết:", error);

            tableBody.replaceChildren();

            const row = document.createElement("tr");
            const cell = document.createElement("td");

            cell.colSpan = 7;
            cell.className = "table-message";
            cell.textContent = "Không thể tải danh sách bài viết. Vui lòng thử lại.";

            row.appendChild(cell);
            tableBody.appendChild(row);

            totalRecords.textContent = "Không thể tải dữ liệu.";
        }
    }

    searchButton.addEventListener("click", function () {
        loadArticles(1);
    });

    resetButton.addEventListener("click", function () {
        searchInput.value = "";
        typeFilter.value = "";
        statusFilter.value = "";

        loadArticles(1);
    });

    searchInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
            loadArticles(1);
        }
    });

    typeFilter.addEventListener("change", function () {
        loadArticles(1);
    });

    statusFilter.addEventListener("change", function () {
        loadArticles(1);
    });

    loadArticles(currentPage);
});
