document.addEventListener("DOMContentLoaded", function () {
    const page = document.querySelector(".competency-review-page");
    const tokenInput = document.querySelector(
        "#competencyAntiForgeryForm input[name='__RequestVerificationToken']"
    );

    if (!page || !tokenInput) {
        return;
    }

    const token = tokenInput.value;

    async function sendRequest(url, data) {
        const formData = new URLSearchParams();

        Object.keys(data).forEach(function (key) {
            formData.append(key, data[key]);
        });

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
                "RequestVerificationToken": token
            },
            body: formData.toString()
        });

        let result = null;

        try {
            result = await response.json();
        } catch {
            throw new Error("Không thể xử lý phản hồi từ máy chủ.");
        }

        if (!response.ok || !result.success) {
            throw new Error(result.message || "Thao tác không thành công.");
        }

        return result;
    }

    function updateSectionCount(type) {
        const section = document.querySelector(
            type === "language"
                ? ".competency-section:nth-child(1)"
                : ".competency-section:nth-child(2)"
        );

        if (!section) {
            return;
        }

        const countElement = section.querySelector(".competency-count");
        const rows = section.querySelectorAll("tbody tr");

        if (countElement) {
            countElement.textContent = rows.length;
        }

        if (rows.length === 0) {
            const tableWrapper = section.querySelector(".competency-table-wrapper");

            if (tableWrapper) {
                tableWrapper.remove();
            }

            const emptyMessage = document.createElement("div");
            emptyMessage.className = "competency-empty";
            emptyMessage.textContent = type === "language"
                ? "Không có ngôn ngữ nào đang chờ xét duyệt."
                : "Không có kinh nghiệm nào đang chờ xét duyệt.";

            section.appendChild(emptyMessage);
        }
    }

    function removeReviewedRow(type, id) {
        const row = document.getElementById(
            (type === "language" ? "language-row-" : "experience-row-") + id
        );

        if (!row) {
            return;
        }

        const section = row.closest(".competency-section");

        row.remove();
        updateSectionCount(type);

        return section;
    }

    function getActionUrl(type, action) {
        const urls = {
            language: {
                approve: "/AdminCompetencyReview/ApproveLanguage",
                reject: "/AdminCompetencyReview/RejectLanguage"
            },
            experience: {
                approve: "/AdminCompetencyReview/ApproveExperience",
                reject: "/AdminCompetencyReview/RejectExperience"
            }
        };

        return urls[type][action];
    }

    page.addEventListener("click", async function (event) {
        const button = event.target.closest(".competency-btn");

        if (!button || button.disabled) {
            return;
        }

        const type = button.dataset.type;
        const id = button.dataset.id;
        const isApprove = button.classList.contains("competency-btn-approve");

        if ((type !== "language" && type !== "experience") || !id) {
            return;
        }

        let reason = "";

        if (isApprove) {
            const confirmed = confirm(
                "Bạn có chắc chắn muốn phê duyệt năng lực này?"
            );

            if (!confirmed) {
                return;
            }
        } else {
            reason = prompt("Nhập lý do từ chối:");

            if (reason === null) {
                return;
            }

            reason = reason.trim();

            if (!reason) {
                alert("Vui lòng nhập lý do từ chối.");
                return;
            }

            if (reason.length > 1000) {
                alert("Lý do từ chối không được vượt quá 1000 ký tự.");
                return;
            }
        }

        const originalText = button.textContent;

        button.disabled = true;
        button.textContent = "Đang xử lý...";

        const row = button.closest("tr");

        if (row) {
            row.querySelectorAll(".competency-btn").forEach(function (item) {
                item.disabled = true;
            });
        }

        try {
            const action = isApprove ? "approve" : "reject";
            const idField = type === "language"
                ? "guideLanguageId"
                : "experienceId";

            const data = {};
            data[idField] = id;

            if (!isApprove) {
                data.reason = reason;
            }

            const result = await sendRequest(
                getActionUrl(type, action),
                data
            );

            removeReviewedRow(type, id);

            alert(result.message);
        } catch (error) {
            alert(error.message);

            if (row && row.isConnected) {
                row.querySelectorAll(".competency-btn").forEach(function (item) {
                    item.disabled = false;
                });

                button.textContent = originalText;
            }
        }
    });
});