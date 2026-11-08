
document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("violationReportForm");
    const fileInput = document.getElementById("EvidenceFiles");
    const selectedFiles = document.getElementById("selectedFiles");
    const reportMessage = document.getElementById("reportMessage");
    const submitButton = document.getElementById("btnSubmitReport");

    if (!form || !fileInput || !selectedFiles || !reportMessage || !submitButton) {
        return;
    }

    const maxFileSize = 10 * 1024 * 1024;
    const maxTotalSize = 25 * 1024 * 1024;
    const maxFileCount = 5;
    const allowedExtensions = [".jpg", ".jpeg", ".png", ".pdf"];

    function formatFileSize(size) {
        return (size / (1024 * 1024)).toFixed(2) + " MB";
    }

    function showMessage(message, type) {
        reportMessage.textContent = message;
        reportMessage.className = "report-message " + type;
    }

    function clearMessage() {
        reportMessage.textContent = "";
        reportMessage.className = "report-message";
    }

    function validateFiles(files) {
        if (files.length > maxFileCount) {
            return "Mỗi báo cáo chỉ được đính kèm tối đa 5 tệp minh chứng.";
        }

        let totalSize = 0;

        for (const file of files) {
            const fileName = file.name.toLowerCase();
            const extension = fileName.substring(fileName.lastIndexOf("."));

            if (!allowedExtensions.includes(extension)) {
                return "Tệp " + file.name + " không đúng định dạng hỗ trợ.";
            }

            if (file.size > maxFileSize) {
                return "Tệp " + file.name + " vượt quá dung lượng 10 MB.";
            }

            totalSize += file.size;

            if (totalSize > maxTotalSize) {
                return "Tổng dung lượng các tệp minh chứng không được vượt quá 25 MB.";
            }
        }

        return "";
    }

    function displaySelectedFiles() {
        selectedFiles.innerHTML = "";
        clearMessage();

        const files = Array.from(fileInput.files);
        const error = validateFiles(files);

        if (error) {
            showMessage(error, "error");
            fileInput.value = "";
            return;
        }

        files.forEach(function (file) {
            const item = document.createElement("div");
            item.className = "selected-file-item";

            const name = document.createElement("span");
            name.textContent = file.name;

            const size = document.createElement("span");
            size.className = "selected-file-size";
            size.textContent = formatFileSize(file.size);

            item.appendChild(name);
            item.appendChild(size);

            selectedFiles.appendChild(item);
        });
    }

    fileInput.addEventListener("change", displaySelectedFiles);

    form.addEventListener("submit", async function (event) {
        event.preventDefault();

        clearMessage();

        if (!form.reportValidity()) {
            return;
        }

        const files = Array.from(fileInput.files);
        const fileError = validateFiles(files);

        if (fileError) {
            showMessage(fileError, "error");
            return;
        }

        submitButton.disabled = true;
        submitButton.textContent = "Đang gửi...";

        try {
            const formData = new FormData(form);

            const response = await fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const responseText = await response.text();
            let result;

            try {
                result = JSON.parse(responseText);
            } catch {
                throw new Error("Không thể xử lý phản hồi từ máy chủ.");
            }

            if (!response.ok || result.success === false) {
                throw new Error(result.message || "Không thể gửi báo cáo.");
            }

            showMessage(result.message, "success");

            if (result.redirectUrl) {
                window.location.href = result.redirectUrl;
                return;
            }
        } catch (error) {
            showMessage(
                error.message || "Đã xảy ra lỗi khi gửi báo cáo.",
                "error"
            );
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = "Gửi báo cáo";
        }
    });
});
