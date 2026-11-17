// Password visibility toggle for buttons marked with data-password-toggle
// and data-target="#inputId".
document.addEventListener('DOMContentLoaded', function () {

    const toggleButtons =
        document.querySelectorAll('[data-password-toggle]');

    toggleButtons.forEach(function (button) {

        const targetSelector =
            button.dataset.target;

        const passwordField =
            document.querySelector(targetSelector);

        if (!passwordField) {
            return;
        }

        const icon =
            button.querySelector('i');

        button.addEventListener('click', function () {

            const isPassword =
                passwordField.type === 'password';

            passwordField.type =
                isPassword
                    ? 'text'
                    : 'password';

            icon.classList.toggle('bi-eye');
            icon.classList.toggle('bi-eye-slash');

            button.setAttribute(
                'aria-label',
                isPassword
                    ? 'Hide password'
                    : 'Show password');
        });
    });
});
