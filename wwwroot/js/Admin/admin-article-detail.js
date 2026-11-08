
document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("articleUpdateForm");
    const titleInput = document.getElementById("Title");
    const typeInput = document.getElementById("ArticleType");
    const contentInput = document.getElementById("Content");
    const thumbnailInput = document.getElementById("Thumbnail");

    const titleError = document.getElementById("titleError");
    const typeError = document.getElementById("typeError");
    const contentError = document.getElementById("contentError");
    const thumbnailError = document.getElementById("thumbnailError");

    const preview = document.getElementById("thumbnailPreview");
    const previewImage = document.getElementById("thumbnailPreviewImage");

    const updateButton = document.getElementById("btnUpdateArticle");
    const statusSelect = document.getElementById("articleStatus");
    const statusButton = document.getElementById("btnChangeArticleStatus");
    const statusMessage = document.getElementById("statusMessage");

    const articleId = form.querySelector('input[name="ArticleId"]').value;
    const tokenInput = form.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    let previewUrl = null;

    function clearErrors() {
        titleError.textContent = "";
        typeError.textContent = "";
        contentError.textContent = "";
        thumbnailError.textContent = "";
    }

    function validateForm() {
        clearErrors();

        let isValid = true;

        if (titleInput.value.trim() === "") {
            titleError.textContent = "Vui lòng nhập tiêu đề bài viết.";
            isValid = false;
        } else if (titleInput.value.trim().length > 250) {
            titleError.textContent = "Tiêu đề không được vượt quá 250 ký tự.";
            isValid = false;
        }

        if (!["1", "2", "3"].includes(typeInput.value)) {
            typeError.textContent = "Vui lòng chọn loại bài viết.";
            isValid = false;
        }

        if (contentInput.value.trim() === "") {
            contentError.textContent = "Vui lòng nhập nội dung bài viết.";
            isValid = false;
        }

        if (thumbnailInput.files.length > 0) {
            const file = thumbnailInput.files[0];
            const allowedTypes = ["image/jpeg", "image/png"];
            const allowedExtensions = [".jpg", ".jpeg", ".png"];
            const extension = file.name.substring(
                file.name.lastIndexOf(".")
            ).toLowerCase();

            if (!allowedTypes.includes(file.type) ||
                !allowedExtensions.includes(extension)) {
                thumbnailError.textContent =
                    "Ảnh phải có định dạng JPG, JPEG hoặc PNG.";
                isValid = false;
            } else if (file.size > 10 * 1024 * 1024) {
                thumbnailError.textContent =
                    "Dung lượng ảnh không được vượt quá 10MB.";
                isValid = false;
            }
        }

        return isValid;
    }

    function releasePreview() {
        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
            previewUrl = null;
        }

        preview.style.display = "none";
        previewImage.removeAttribute("src");
    }

    thumbnailInput.addEventListener("change", function () {
        thumbnailError.textContent = "";
        releasePreview();

        if (thumbnailInput.files.length === 0) {
            return;
        }

        const file = thumbnailInput.files[0];
        const allowedTypes = ["image/jpeg", "image/png"];
        const allowedExtensions = [".jpg", ".jpeg", ".png"];
        const extension = file.name.substring(
            file.name.lastIndexOf(".")
        ).toLowerCase();

        if (!allowedTypes.includes(file.type) ||
            !allowedExtensions.includes(extension)) {
            thumbnailError.textContent =
                "Ảnh phải có định dạng JPG, JPEG hoặc PNG.";
            thumbnailInput.value = "";
            return;
        }

        if (file.size > 10 * 1024 * 1024) {
            thumbnailError.textContent =
                "Dung lượng ảnh không được vượt quá 10MB.";
            thumbnailInput.value = "";
            return;
        }

        previewUrl = URL.createObjectURL(file);
        previewImage.src = previewUrl;
        preview.style.display = "block";
    });

    form.addEventListener("submit", async function (event) {
        event.preventDefault();

        if (!validateForm()) {
            return;
        }

        if (!tokenInput) {
            alert("Không tìm thấy mã xác thực yêu cầu.");
            return;
        }

        const formData = new FormData(form);

        updateButton.disabled = true;

        try {
            const response = await fetch("/AdminArticle/Update", {
                method: "POST",
                headers: {
                    "Accept": "application/json"
                },
                body: formData,
                credentials: "same-origin"
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.message || "Không thể cập nhật bài viết."
                );
            }

            if (result.success) {
                alert(result.message || "Cập nhật bài viết thành công.");
                window.location.reload();
                return;
            }

            alert(result.message || "Cập nhật bài viết thất bại.");
        } catch (error) {
            console.error("Lỗi khi cập nhật bài viết:", error);
            alert(error.message || "Đã xảy ra lỗi khi cập nhật bài viết.");
        } finally {
            updateButton.disabled = false;
        }
    });

    statusButton.addEventListener("click", async function () {
        if (!tokenInput) {
            alert("Không tìm thấy mã xác thực yêu cầu.");
            return;
        }

        const status = statusSelect.value;

        if (!["0", "1", "2"].includes(status)) {
            statusMessage.textContent = "Trạng thái không hợp lệ.";
            return;
        }

        const confirmed = confirm(
            "Bạn có chắc chắn muốn thay đổi trạng thái bài viết?"
        );

        if (!confirmed) {
            return;
        }

        const formData = new URLSearchParams();
        formData.append("articleId", articleId);
        formData.append("status", status);
        formData.append(
            "__RequestVerificationToken",
            tokenInput.value
        );

        statusButton.disabled = true;
        statusMessage.textContent = "";

        try {
            const response = await fetch("/AdminArticle/ChangeStatus", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "Accept": "application/json"
                },
                body: formData.toString(),
                credentials: "same-origin"
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.message || "Không thể thay đổi trạng thái."
                );
            }

            if (result.success) {
                alert(result.message || "Cập nhật trạng thái thành công.");
                window.location.reload();
                return;
            }

            statusMessage.textContent =
                result.message || "Cập nhật trạng thái thất bại.";
        } catch (error) {
            console.error("Lỗi khi thay đổi trạng thái:", error);
            statusMessage.textContent =
                error.message || "Đã xảy ra lỗi khi cập nhật trạng thái.";
        } finally {
            statusButton.disabled = false;
        }
    });

    window.addEventListener("beforeunload", releasePreview);
});
