
document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("articleCreateForm");
    const titleInput = document.getElementById("Title");
    const typeInput = document.getElementById("ArticleType");
    const contentInput = document.getElementById("Content");
    const thumbnailInput = document.getElementById("Thumbnail");
    const preview = document.getElementById("thumbnailPreview");
    const previewImage = document.getElementById("thumbnailPreviewImage");
    const submitButton = document.getElementById("btnCreateArticle");

    const titleError = document.getElementById("titleError");
    const typeError = document.getElementById("typeError");
    const contentError = document.getElementById("contentError");
    const thumbnailError = document.getElementById("thumbnailError");

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
            const extension = file.name.substring(file.name.lastIndexOf(".")).toLowerCase();

            if (!allowedTypes.includes(file.type) ||
                !allowedExtensions.includes(extension)) {
                thumbnailError.textContent = "Ảnh phải có định dạng JPG, JPEG hoặc PNG.";
                isValid = false;
            } else if (file.size > 10 * 1024 * 1024) {
                thumbnailError.textContent = "Dung lượng ảnh không được vượt quá 10MB.";
                isValid = false;
            }
        }

        return isValid;
    }

    thumbnailInput.addEventListener("change", function () {
        thumbnailError.textContent = "";

        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
            previewUrl = null;
        }

        preview.style.display = "none";
        previewImage.removeAttribute("src");

        if (thumbnailInput.files.length === 0) {
            return;
        }

        const file = thumbnailInput.files[0];
        const allowedTypes = ["image/jpeg", "image/png"];

        if (!allowedTypes.includes(file.type)) {
            thumbnailError.textContent = "Ảnh phải có định dạng JPG, JPEG hoặc PNG.";
            thumbnailInput.value = "";
            return;
        }

        if (file.size > 10 * 1024 * 1024) {
            thumbnailError.textContent = "Dung lượng ảnh không được vượt quá 10MB.";
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

        const formData = new FormData(form);
        const tokenInput = form.querySelector(
            'input[name="__RequestVerificationToken"]'
        );

        if (!tokenInput) {
            alert("Không tìm thấy mã xác thực yêu cầu.");
            return;
        }

        submitButton.disabled = true;

        try {
            const response = await fetch("/AdminArticle/Create", {
                method: "POST",
                headers: {
                    "RequestVerificationToken": tokenInput.value,
                    "Accept": "application/json"
                },
                body: formData,
                credentials: "same-origin"
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.message || "Không thể tạo bài viết."
                );
            }

            if (result.success) {
                alert(result.message || "Tạo bài viết thành công.");
                window.location.href = "/AdminArticle/Index";
                return;
            }

            alert(result.message || "Tạo bài viết thất bại.");
        } catch (error) {
            console.error("Lỗi khi tạo bài viết:", error);
            alert(error.message || "Đã xảy ra lỗi khi tạo bài viết.");
        } finally {
            submitButton.disabled = false;
        }
    });

    window.addEventListener("beforeunload", function () {
        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
        }
    });
});
