
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('registerForm');
    const cardTypeSelect = document.getElementById('CardType');
    const languageSection = document.getElementById('language-section');
    const languageContainer = document.getElementById('languageContainer');
    const btnAddLanguage = document.getElementById('btnAddLanguage');
    const languageAsterisk = document.getElementById('languageAsterisk');
    const submitBtn = document.getElementById('btnSubmit');

    if (form === null) {
        return;
    }

    const INTERNATIONAL_CARD = 'Hướng dẫn viên quốc tế';
    const MAX_FILE_SIZE = 10 * 1024 * 1024;

    let isSubmitting = false;

    function isInternationalCard() {
        if (cardTypeSelect === null) {
            return false;
        }

        const val = cardTypeSelect.value.trim().toLowerCase();
        if (val.length === 0) {
            return false;
        }

        if (val.indexOf('quốc tế') !== -1 || val.indexOf('quoc te') !== -1 || val.indexOf('qu?c t?') !== -1) {
            return true;
        }

        return val === 'hướng dẫn viên quốc tế';
    }

    function getLanguageOptionsHtml() {
        if (languageContainer === null) {
            return '<option value="0">-- Chọn Ngoại ngữ --</option>';
        }

        const firstSelect = languageContainer.querySelector(
            'select[name="LanguageIds"]'
        );

        if (firstSelect === null) {
            return '<option value="0">-- Chọn Ngoại ngữ --</option>';
        }

        return firstSelect.innerHTML;
    }

    function clearErrors() {
        const errorSpans = form.querySelectorAll('[data-valmsg-for]');

        errorSpans.forEach(function (span) {
            span.textContent = '';
        });

        form.querySelectorAll('.input-validation-error').forEach(function (field) {
            field.classList.remove('input-validation-error');
        });
    }

    function showClientError(fieldName, message, rowIndex) {
        if (fieldName === 'LanguageIds' ||
            fieldName === 'LanguageCertificates') {

            if (languageContainer === null) {
                return;
            }

            const rows = languageContainer.querySelectorAll('.language-row');

            let row = null;

            if (rowIndex !== undefined && rowIndex >= 0) {
                row = rows[rowIndex];
            } else {
                row = rows[0];
            }

            if (row === null || row === undefined) {
                return;
            }

            let selector = 'select[name="LanguageIds"]';

            if (fieldName === 'LanguageCertificates') {
                selector = 'input[name="LanguageCertificates"]';
            }

            const field = row.querySelector(selector);
            const error = row.querySelector(
                '[data-valmsg-for="' + fieldName + '"]'
            );

            if (field !== null) {
                field.classList.add('input-validation-error');
            }

            if (error !== null) {
                error.textContent = message;
            }

            return;
        }

        const field = form.querySelector('[name="' + fieldName + '"]');
        const error = form.querySelector(
            '[data-valmsg-for="' + fieldName + '"]'
        );

        if (field !== null) {
            field.classList.add('input-validation-error');
        }

        if (error !== null) {
            error.textContent = message;
        }
    }

    function showServerErrors(errors) {
        if (Array.isArray(errors) === false) {
            return;
        }

        errors.forEach(function (error) {
            if (error.field === undefined || error.message === undefined) {
                return;
            }

            const languageIdMatch = error.field.match(
                /^LanguageIds\[(\d+)\]$/
            );

            const certificateMatch = error.field.match(
                /^LanguageCertificates\[(\d+)\]$/
            );

            if (languageIdMatch !== null) {
                showClientError(
                    'LanguageIds',
                    error.message,
                    Number(languageIdMatch[1])
                );

                return;
            }

            if (certificateMatch !== null) {
                showClientError(
                    'LanguageCertificates',
                    error.message,
                    Number(certificateMatch[1])
                );

                return;
            }

            showClientError(error.field, error.message);
        });
    }

    function updateRequiredState() {
        if (cardTypeSelect === null || languageContainer === null) {
            return;
        }

        const international = isInternationalCard();

        if (languageSection !== null) {
            if (international === true) {
                languageSection.style.display = 'block';
            } else {
                languageSection.style.display = 'none';
            }
        }

        if (languageAsterisk !== null) {
            if (international === true) {
                languageAsterisk.style.display = 'inline';
            } else {
                languageAsterisk.style.display = 'none';
            }
        }

        const rows = languageContainer.querySelectorAll('.language-row');

        if (international === false) {
            for (let i = 1; i < rows.length; i++) {
                rows[i].remove();
            }

            if (rows.length > 0) {
                const firstSelect = rows[0].querySelector(
                    'select[name="LanguageIds"]'
                );

                const firstFile = rows[0].querySelector(
                    'input[name="LanguageCertificates"]'
                );

                if (firstSelect !== null) {
                    firstSelect.value = '0';
                    firstSelect.removeAttribute('required');
                    firstSelect.classList.remove('input-validation-error');
                }

                if (firstFile !== null) {
                    firstFile.value = '';
                    firstFile.removeAttribute('required');
                    firstFile.classList.remove('input-validation-error');
                }

                const errorSpans = rows[0].querySelectorAll('[data-valmsg-for]');

                errorSpans.forEach(function (span) {
                    span.textContent = '';
                });
            }

            return;
        }

        rows.forEach(function (row) {
            const select = row.querySelector(
                'select[name="LanguageIds"]'
            );

            const file = row.querySelector(
                'input[name="LanguageCertificates"]'
            );

            if (select !== null) {
                select.setAttribute('required', 'required');
            }

            if (file !== null) {
                file.setAttribute('required', 'required');
            }
        });
    }

    function addLanguageRow() {
        if (languageContainer === null) {
            return;
        }

        const newRow = document.createElement('div');
        newRow.className = 'language-row';

        const selectHtml = getLanguageOptionsHtml();

        newRow.innerHTML = `
            <div class="language-field">
                <select name="LanguageIds" class="language-select" required>
                    ${selectHtml}
                </select>
                <span class="text-danger" data-valmsg-for="LanguageIds"></span>
            </div>

            <div class="language-field">
                <input type="file"
                       name="LanguageCertificates"
                       accept=".jpg,.jpeg,.png,.pdf"
                       required />
                <span class="text-danger" data-valmsg-for="LanguageCertificates"></span>
            </div>

            <div class="language-action">
                <button type="button" class="btn-remove-language">
                    Xóa
                </button>
            </div>
        `;

        languageContainer.appendChild(newRow);
    }

    function isValidDate(value) {
        if (value === '') {
            return false;
        }

        const date = new Date(value + 'T00:00:00');

        return Number.isNaN(date.getTime()) === false;
    }

    function validateBeforeSubmit() {
        let isValid = true;

        clearErrors();

        const requiredTextFields = [
            ['Email', 'Vui lòng nhập Email.'],
            ['Password', 'Vui lòng nhập mật khẩu.'],
            ['ConfirmPassword', 'Vui lòng xác nhận mật khẩu.'],
            ['FullName', 'Vui lòng nhập họ và tên.'],
            ['Phone', 'Vui lòng nhập số điện thoại.'],
            ['Address', 'Vui lòng nhập địa chỉ.'],
            ['IdCard', 'Vui lòng nhập số CCCD/CMND.'],
            ['IssuePlace', 'Vui lòng nhập nơi cấp.'],
            ['CardNumber', 'Vui lòng nhập số thẻ HDV.']
        ];

        requiredTextFields.forEach(function (item) {
            const field = form.querySelector('[name="' + item[0] + '"]');

            if (field !== null && field.value.trim() === '') {
                showClientError(item[0], item[1]);
                isValid = false;
            }
        });

        const email = form.querySelector('[name="Email"]');

        if (email !== null && email.value.trim() !== '') {
            const emailValue = email.value.trim();
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

            if (emailPattern.test(emailValue) === false) {
                showClientError('Email', 'Email không đúng định dạng.');
                isValid = false;
            }
        }

        const password = form.querySelector('[name="Password"]');
        const confirmPassword = form.querySelector('[name="ConfirmPassword"]');
        const fullName = form.querySelector('[name="FullName"]');

        if (password !== null) {
            if (password.value.length < 8 || password.value.length > 128) {
                showClientError(
                    'Password',
                    'Mật khẩu phải từ 8 đến 128 ký tự.'
                );

                isValid = false;
            }

            if (email !== null && email.value.trim() !== '') {
                const emailValue = email.value.trim();

                if (password.value.toLowerCase().includes(emailValue.toLowerCase())) {
                    showClientError(
                        'Password',
                        'Mật khẩu không được chứa địa chỉ email.'
                    );

                    isValid = false;
                } else {
                    const emailPrefix = emailValue.split('@')[0];

                    if (emailPrefix.length >= 3 &&
                        password.value.toLowerCase().includes(emailPrefix.toLowerCase())) {

                        showClientError(
                            'Password',
                            'Mật khẩu không được chứa phần tên định danh của email.'
                        );

                        isValid = false;
                    }
                }
            }

            if (fullName !== null && fullName.value.trim() !== '') {
                const nameParts = fullName.value.trim().split(/\s+/);

                for (let i = 0; i < nameParts.length; i++) {
                    if (nameParts[i].length >= 3 &&
                        password.value.toLowerCase().includes(nameParts[i].toLowerCase())) {

                        showClientError(
                            'Password',
                            'Mật khẩu không được chứa thông tin họ tên của bạn.'
                        );

                        isValid = false;
                        break;
                    }
                }
            }
        }

        if (password !== null && confirmPassword !== null &&
            password.value !== '' &&
            password.value !== confirmPassword.value) {

            showClientError(
                'ConfirmPassword',
                'Mật khẩu xác nhận không khớp.'
            );

            isValid = false;
        }

        const phone = form.querySelector('[name="Phone"]');

        if (phone !== null && phone.value.trim() !== '' &&
            /^0\d{9}$/.test(phone.value.trim()) === false) {

            showClientError(
                'Phone',
                'Số điện thoại phải gồm 10 số và bắt đầu bằng số 0.'
            );

            isValid = false;
        }

        const idCard = form.querySelector('[name="IdCard"]');

        if (idCard !== null && idCard.value.trim() !== '' &&
            /^\d{12}$/.test(idCard.value.trim()) === false) {

            showClientError(
                'IdCard',
                'Số CCCD/CMND phải đúng 12 chữ số.'
            );

            isValid = false;
        }

        const requiredSelectFields = [
            'Gender',
            'DateOfBirth',
            'CardType',
            'ExpiryDate',
            'ProvinceId'
        ];

        requiredSelectFields.forEach(function (fieldName) {
            const field = form.querySelector('[name="' + fieldName + '"]');

            if (field !== null &&
                (field.value === '' || field.value === '0')) {

                showClientError(
                    fieldName,
                    'Vui lòng nhập hoặc chọn thông tin này.'
                );

                isValid = false;
            }
        });

        const dateOfBirth = form.querySelector('[name="DateOfBirth"]');

        if (dateOfBirth !== null && dateOfBirth.value !== '') {
            if (isValidDate(dateOfBirth.value) === false) {
                showClientError(
                    'DateOfBirth',
                    'Ngày sinh không hợp lệ.'
                );

                isValid = false;
            } else {
                const today = new Date();
                today.setHours(0, 0, 0, 0);

                const selectedDate = new Date(
                    dateOfBirth.value + 'T00:00:00'
                );

                if (selectedDate >= today) {
                    showClientError(
                        'DateOfBirth',
                        'Ngày sinh phải trước ngày hiện tại.'
                    );

                    isValid = false;
                }
            }
        }

        const issueDate = form.querySelector('[name="IssueDate"]');

        if (issueDate !== null && issueDate.value !== '') {
            if (isValidDate(issueDate.value) === false) {
                showClientError(
                    'IssueDate',
                    'Ngày cấp thẻ không hợp lệ.'
                );

                isValid = false;
            } else {
                const today = new Date();
                today.setHours(0, 0, 0, 0);

                const selectedDate = new Date(
                    issueDate.value + 'T00:00:00'
                );

                if (selectedDate > today) {
                    showClientError(
                        'IssueDate',
                        'Ngày cấp thẻ không được lớn hơn ngày hiện tại.'
                    );

                    isValid = false;
                }
            }
        }

        const expiryDate = form.querySelector('[name="ExpiryDate"]');

        if (expiryDate !== null && expiryDate.value !== '') {
            if (isValidDate(expiryDate.value) === false) {
                showClientError(
                    'ExpiryDate',
                    'Ngày hết hạn không hợp lệ.'
                );

                isValid = false;
            } else {
                const today = new Date();
                today.setHours(0, 0, 0, 0);

                const selectedDate = new Date(
                    expiryDate.value + 'T00:00:00'
                );

                if (selectedDate <= today) {
                    showClientError(
                        'ExpiryDate',
                        'Ngày hết hạn thẻ phải sau ngày hiện tại.'
                    );

                    isValid = false;
                }

                if (issueDate !== null &&
                    issueDate.value !== '' &&
                    isValidDate(issueDate.value) === true) {

                    const selectedIssueDate = new Date(
                        issueDate.value + 'T00:00:00'
                    );

                    if (selectedDate <= selectedIssueDate) {
                        showClientError(
                            'ExpiryDate',
                            'Ngày hết hạn phải sau ngày cấp thẻ.'
                        );

                        isValid = false;
                    }
                }
            }
        }

        const experienceYears = form.querySelector(
            '[name="ExperienceYears"]'
        );

        if (experienceYears !== null) {
            const years = Number(experienceYears.value);

            if (experienceYears.value === '' ||
                Number.isInteger(years) === false ||
                years < 0 || years > 40) {

                showClientError(
                    'ExperienceYears',
                    'Số năm kinh nghiệm phải từ 0 đến 40.'
                );

                isValid = false;
            }
        }

        const requiredFiles = [
            'AvatarFile',
            'IdCardFrontFile',
            'IdCardBackFile',
            'CardScanFile'
        ];

        requiredFiles.forEach(function (fieldName) {
            const field = form.querySelector('[name="' + fieldName + '"]');

            if (field !== null) {
                if (field.files.length === 0) {
                    showClientError(fieldName, 'Vui lòng tải tệp lên.');
                    isValid = false;
                } else if (field.files[0].size > MAX_FILE_SIZE) {
                    showClientError(
                        fieldName,
                        'Dung lượng tệp không được vượt quá 10MB.'
                    );

                    isValid = false;
                }
            }
        });

        if (isInternationalCard() === true) {
            if (languageContainer === null) {
                isValid = false;
            } else {
                const rows = Array.from(
                    languageContainer.querySelectorAll('.language-row')
                );

                const selectedLanguageIds = [];
                let hasSelectedLanguage = false;

                rows.forEach(function (row, index) {
                    const select = row.querySelector(
                        'select[name="LanguageIds"]'
                    );

                    const file = row.querySelector(
                        'input[name="LanguageCertificates"]'
                    );

                    if (select === null || file === null) {
                        return;
                    }

                    const languageId = Number(select.value);
                    const hasFile = file.files.length > 0;

                    if (languageId > 0) {
                        hasSelectedLanguage = true;

                        if (selectedLanguageIds.includes(languageId)) {
                            showClientError(
                                'LanguageIds',
                                'Không được chọn trùng ngoại ngữ.',
                                index
                            );

                            isValid = false;
                        } else {
                            selectedLanguageIds.push(languageId);
                        }

                        if (hasFile === false) {
                            showClientError(
                                'LanguageCertificates',
                                'Vui lòng tải minh chứng cho ngoại ngữ đã chọn.',
                                index
                            );

                            isValid = false;
                        } else if (file.files[0].size > MAX_FILE_SIZE) {
                            showClientError(
                                'LanguageCertificates',
                                'Dung lượng tệp không được vượt quá 10MB.',
                                index
                            );

                            isValid = false;
                        }
                    } else if (hasFile === true) {
                        showClientError(
                            'LanguageIds',
                            'Vui lòng chọn ngoại ngữ tương ứng với tệp minh chứng.',
                            index
                        );

                        isValid = false;
                    }
                });

                if (hasSelectedLanguage === false) {
                    showClientError(
                        'LanguageIds',
                        'Vui lòng chọn ít nhất một ngoại ngữ.',
                        0
                    );

                    isValid = false;
                }
            }
        }

        return isValid;
    }

    if (cardTypeSelect !== null) {
        cardTypeSelect.addEventListener('change', function () {
            clearErrors();
            updateRequiredState();
        });
    }

    if (btnAddLanguage !== null) {
        btnAddLanguage.addEventListener('click', function () {
            if (isInternationalCard() === false) {
                return;
            }

            addLanguageRow();
        });
    }

    if (languageContainer !== null) {
        languageContainer.addEventListener('click', function (event) {
            const target = event.target;

            if (target instanceof Element === false) {
                return;
            }

            const removeButton = target.closest('.btn-remove-language');

            if (removeButton === null) {
                return;
            }

            const row = removeButton.closest('.language-row');

            if (row !== null) {
                row.remove();
            }
        });
    }

    updateRequiredState();

    form.addEventListener('submit', async function (event) {
        event.preventDefault();

        if (isSubmitting === true) {
            return;
        }

        if (validateBeforeSubmit() === false) {
            return;
        }

        isSubmitting = true;

        let originalText = '';

        if (submitBtn !== null) {
            submitBtn.disabled = true;
            originalText = submitBtn.innerText;
            submitBtn.innerText = 'Đang xử lý hồ sơ...';
        }

        const formData = new FormData(form);

        if (isInternationalCard() === false) {
            formData.delete('LanguageIds');
            formData.delete('LanguageCertificates');
        }

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            let result = null;

            try {
                result = await response.json();
            } catch (error) {
                throw new Error('Máy chủ trả về dữ liệu không hợp lệ.');
            }

            if (response.ok === false) {
                if (result.errors) {
                    showServerErrors(result.errors);
                } else {
                    alert(result.message || 'Có lỗi xảy ra khi đăng ký.');
                }

                return;
            }

            if (result.success === true) {
                alert(result.message || 'Đăng ký thành công!');

                if (result.redirectUrl) {
                    window.location.href = result.redirectUrl;
                }

                return;
            }

            if (result.errors) {
                showServerErrors(result.errors);
            } else {
                alert(
                    result.message ||
                    'Không thể đăng ký. Vui lòng kiểm tra lại thông tin.'
                );
            }
        } catch (error) {
            alert(
                'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối và thử lại.'
            );
        } finally {
            isSubmitting = false;

            if (submitBtn !== null) {
                submitBtn.disabled = false;
                submitBtn.innerText = originalText;
            }
        }
    });
});
