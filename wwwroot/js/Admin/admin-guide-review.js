
document.addEventListener("DOMContentLoaded", function () {
    const approveButton = document.getElementById("btnApproveGuide");
    const rejectButton = document.getElementById("btnRejectGuide");
    const rejectReason = document.getElementById("rejectReason");
    const tokenInput = document.querySelector(
        '#admin-review-antiforgery input[name="__RequestVerificationToken"]'
    );

    if (!tokenInput) {
        return;
    }

    const token = tokenInput.value;

    async function sendRequest(url, data) {
        const body = new URLSearchParams();

        for (const key in data) {
            body.append(key, data[key]);
        }

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "RequestVerificationToken": token
            },
            body: body.toString()
        });

        const result = await response.json();

        if (!response.ok || !result.success) {
            throw new Error(result.message || "Không thể thực hiện thao tác.");
        }

        return result;
    }

    if (approveButton) {
        approveButton.addEventListener("click", async function () {
            const guideId = approveButton.dataset.guideId;

            if (!confirm("Bạn có chắc chắn muốn duyệt hồ sơ này?")) {
                return;
            }

            approveButton.disabled = true;

            try {
                const result = await sendRequest("/AdminGuideReview/Approve", {
                    guideId: guideId
                });

                alert(result.message);
                window.location.href = "/AdminGuideReview/Index";
            } catch (error) {
                alert(error.message);
                approveButton.disabled = false;
            }
        });
    }

    if (rejectButton) {
        rejectButton.addEventListener("click", async function () {
            const guideId = rejectButton.dataset.guideId;
            const reason = rejectReason.value.trim();

            if (reason.length === 0) {
                alert("Vui lòng nhập lý do từ chối.");
                rejectReason.focus();
                return;
            }

            if (reason.length > 1000) {
                alert("Lý do từ chối không được vượt quá 1000 ký tự.");
                rejectReason.focus();
                return;
            }

            if (!confirm("Bạn có chắc chắn muốn từ chối hồ sơ này?")) {
                return;
            }

            rejectButton.disabled = true;

            try {
                const result = await sendRequest("/AdminGuideReview/Reject", {
                    guideId: guideId,
                    reason: reason
                });

                alert(result.message);
                window.location.href = "/AdminGuideReview/Index";
            } catch (error) {
                alert(error.message);
                rejectButton.disabled = false;
            }
        });
    }
});
