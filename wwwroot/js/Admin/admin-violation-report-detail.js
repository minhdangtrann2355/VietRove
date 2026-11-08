document.addEventListener("DOMContentLoaded", function () {
    const page = document.getElementById("violationReportDetailPage");

    if (!page) {
        return;
    }

    const updateUrl = page.dataset.updateUrl;

    const statusSelect = document.getElementById("reviewStatus");
    const noteInput = document.getElementById("reviewNote");
    const noteHint = document.getElementById("reviewNoteHint");
    const warning = document.getElementById("reviewWarning");
    const message = document.getElementById("reviewMessage");
    const updateButton = document.getElementById("btnUpdateViolationStatus");

    const tokenInput = document.querySelector(
        '#adminAntiForgeryForm input[name="__RequestVerificationToken"]'
    );

    const currentStatusElement = document.querySelector(".detail-status");

    if (
        !updateUrl ||
        !statusSelect ||
        !noteInput ||
        !updateButton ||
        !tokenInput
    ) {
        return;
    }

    const statusText = currentStatusElement
        ? currentStatusElement.textContent.trim()
        : "";

    const statusMap = {
        "Chờ xử lý": "0",
        "Đang xử lý": "1",
        "Đã xử lý": "2",
        "Không vi phạm": "3"
    };

    if (Object.prototype.hasOwnProperty.call(statusMap, statusText)) {
        statusSelect.value = statusMap[statusText];
    }

    function showMessage(text, type) {
        message.textContent = text;
        message.className = "review-message message-" + type;
    }

    function clearMessage() {
        message.textContent = "";
        message.className = "review-message";
    }

    function updateReviewHint() {
        const isViolation = statusSelect.value === "2";

        warning.hidden = !isViolation;

        if (isViolation) {
            noteInput.maxLength = 500;
            noteHint.textContent =
                "Bắt buộc nhập lý do khóa tài khoản Guide. Tối đa 500 ký tự.";
        } else {
            noteInput.maxLength = 1000;
            noteHint.textContent = "Tối đa 1000 ký tự.";
        }
    }

    statusSelect.addEventListener("change", function () {
        clearMessage();
        updateReviewHint();
    });

    noteInput.addEventListener("input", clearMessage);

    updateReviewHint();

    updateButton.addEventListener("click", async function () {
        clearMessage();

        const status = Number(statusSelect.value);
        const note = noteInput.value.trim();

        if (!Number.isInteger(status) || status < 0 || status > 3) {
            showMessage("Trạng thái xử lý không hợp lệ.", "error");
            return;
        }

        if (status === 2 && note.length === 0) {
            showMessage(
                "Vui lòng nhập lý do xác nhận vi phạm để khóa tài khoản Guide.",
                "error"
            );
            noteInput.focus();
            return;
        }

        if (status === 2 && note.length > 500) {
            showMessage(
                "Lý do khóa tài khoản không được vượt quá 500 ký tự.",
                "error"
            );
            noteInput.focus();
            return;
        }

        if (status !== 2 && note.length > 1000) {
            showMessage(
                "Ghi chú không được vượt quá 1000 ký tự.",
                "error"
            );
            noteInput.focus();
            return;
        }

        const currentStatus = statusMap[statusText];

        if (currentStatus === String(status)) {
            showMessage(
                "Trạng thái báo cáo hiện tại đã được chọn.",
                "error"
            );
            return;
        }

        if (status === 2) {
            const confirmed = window.confirm(
                "Xác nhận báo cáo có vi phạm? Tài khoản Guide sẽ bị khóa sau khi lưu."
            );

            if (!confirmed) {
                return;
            }
        }

        updateButton.disabled = true;
        updateButton.textContent = "Đang lưu...";

        try {
            const response = await fetch(updateUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": tokenInput.value
                },
                body: JSON.stringify({
                    status: status,
                    note: note
                })
            });

            const result = await response.json();

            if (!response.ok || result.success !== true) {
                showMessage(
                    result.message || "Không thể cập nhật trạng thái báo cáo.",
                    "error"
                );
                return;
            }

            showMessage(
                result.message || "Cập nhật trạng thái thành công.",
                "success"
            );

            window.location.reload();
        } catch (error) {
            console.error("Lỗi khi cập nhật báo cáo:", error);

            showMessage(
                "Đã xảy ra lỗi khi kết nối đến máy chủ.",
                "error"
            );
        } finally {
            updateButton.disabled = false;
            updateButton.textContent = "Lưu kết quả xét duyệt";
        }
    });
});