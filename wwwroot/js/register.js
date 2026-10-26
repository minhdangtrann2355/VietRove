document.addEventListener('DOMContentLoaded', function () {
    const cardTypeSelect = document.getElementById('CardType');
    const languageSection = document.getElementById('language-section');
    const languageContainer = document.getElementById('languageContainer');
    const btnAddLanguage = document.getElementById('btnAddLanguage');
    const languageAsterisk = document.getElementById('languageAsterisk');

    function getLanguageOptionsHtml() {
        let firstSelect = null;
        if (languageContainer !== null) {
            firstSelect = languageContainer.querySelector('select[name="LanguageNames"]');
        }

        if (firstSelect !== null) {
            return firstSelect.innerHTML;
        } else {
            return '<option value="">-- Chọn Ngoại ngữ --</option>';
        }
    }

    function updateRequiredState() {
        if (cardTypeSelect === null) {
            return;
        }

        let isInternational = false;
        const selectedValue = cardTypeSelect.value.trim();
        
        if (selectedValue === 'Quốc tế') {
            isInternational = true;
        }

        if (languageSection !== null) {
            if (isInternational === true) {
                languageSection.style.display = 'block';
            } else {
                languageSection.style.display = 'none';
            }
        }

        if (languageAsterisk !== null) {
            if (isInternational === true) {
                languageAsterisk.style.display = 'inline';
            } else {
                languageAsterisk.style.display = 'none';
            }
        }

        if (languageContainer === null) {
            return;
        }

        if (isInternational === false) {
            const rows = languageContainer.querySelectorAll('.language-row');
            for (let i = 1; i < rows.length; i++) {
                rows[i].remove();
            }

            if (rows.length > 0) {
                const firstRow = rows[0];
                const firstSelect = firstRow.querySelector('select[name="LanguageNames"]');
                const firstFile = firstRow.querySelector('input[name="LanguageCertificates"]');

                if (firstSelect !== null) {
                    firstSelect.value = '';
                    firstSelect.removeAttribute('required');
                }
                if (firstFile !== null) {
                    firstFile.value = '';
                    firstFile.removeAttribute('required');
                }
            }
            return;
        }

        const langSelects = languageContainer.querySelectorAll('select[name="LanguageNames"]');
        const certInputs = languageContainer.querySelectorAll('input[name="LanguageCertificates"]');

        langSelects.forEach(function (select) {
            select.setAttribute('required', 'required');
        });

        certInputs.forEach(function (input) {
            input.setAttribute('required', 'required');
        });
    }

    if (cardTypeSelect !== null) {
        cardTypeSelect.addEventListener('change', updateRequiredState);
    }

    if (btnAddLanguage !== null) {
        btnAddLanguage.addEventListener('click', function () {
            const newRow = document.createElement('div');
            newRow.className = 'language-row g-2 mb-2 align-items-center row';

            const selectHtml = getLanguageOptionsHtml();
            newRow.innerHTML = `
                <div class="col-md-5">
                    <select name="LanguageNames" class="form-select language-select" required>
                        ${selectHtml}
                    </select>
                </div>
                <div class="col-md-5">
                    <input type="file" name="LanguageCertificates" class="form-control" accept=".jpg,.jpeg,.png,.pdf" required />
                </div>
                <div class="col-md-2">
                    <button type="button" class="btn btn-outline-danger w-100 btn-remove-lang">Xóa</button>
                </div>
            `;

            languageContainer.appendChild(newRow);

            const removeBtn = newRow.querySelector('.btn-remove-lang');
            if (removeBtn !== null) {
                removeBtn.addEventListener('click', function () {
                    newRow.remove();
                });
            }
        });
    }

    updateRequiredState();

    const form = document.getElementById('registerForm');
    const submitBtn = document.getElementById('btnSubmit');

    if (form === null) {
        return;
    }

    form.addEventListener('submit', async function (event) {
        event.preventDefault();

        const errorSpans = document.querySelectorAll('.text-danger');
        errorSpans.forEach(function (span) {
            span.innerText = '';
        });

        let originalText = '';
        if (submitBtn !== null) {
            submitBtn.disabled = true;
            originalText = submitBtn.innerText;
            submitBtn.innerText = 'Đang xử lý hồ sơ...';
        }

        const formData = new FormData(form);

        let isInternational = false;
        if (cardTypeSelect !== null) {
            if (cardTypeSelect.value.trim() === 'Quốc tế') {
                isInternational = true;
            }
        }

        if (isInternational === false) {
            formData.delete('LanguageNames');
            formData.delete('LanguageCertificates');
        }

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData
            });

            if (response.ok === false) {
                alert('Lỗi kết nối máy chủ (' + response.status + '). Vui lòng kiểm tra lại SQL Server.');
                return;
            }

            const result = await response.json();

            if (result.success === true) {
                let successMsg = 'Đăng ký thành công!';
                if (result.message) {
                    successMsg = result.message;
                }
                alert(successMsg);

                if (result.redirectUrl) {
                    window.location.href = result.redirectUrl;
                }
            } else {
                let failMsg = 'Vui lòng kiểm tra lại thông tin.';
                if (result.message) {
                    failMsg = result.message;
                }
                alert(failMsg);

                if (result.errors) {
                    result.errors.forEach(function (err) {
                        const targetSpan = document.querySelector('[data-valmsg-for="' + err.field + '"]');
                        if (targetSpan !== null) {
                            targetSpan.innerText = err.message;
                        }
                    });
                }
            }
        } catch (error) {
            console.error('Lỗi Submit:', error);
            alert('Không thể kết nối đến máy chủ. Vui lòng kiểm tra ứng dụng ASP.NET.');
        } finally {
            if (submitBtn !== null) {
                submitBtn.disabled = false;
                submitBtn.innerText = originalText;
            }
        }
    });
});