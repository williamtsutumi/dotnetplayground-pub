(() => {
    const form = document.getElementById('registration-form');
    const password = document.getElementById('senha');
    const confirmation = document.getElementById('confirmar-senha');
    const username = document.getElementById('nome');

    function validateUsername() {
        username.setCustomValidity(/\s/u.test(username.value) ? username.dataset.whitespaceError : '');
    }

    username.addEventListener('input', validateUsername);
    const status = document.getElementById('registration-status');

    function notify(message, type) {
        status.textContent = '';

        if (typeof Noty !== 'function') {
            status.textContent = message;
            return;
        }

        const text = document.createElement('span');
        text.textContent = message;

        new Noty({
            text: text.innerHTML,
            type,
            layout: 'bottomRight',
            theme: 'mint',
            timeout: 5000,
            progressBar: false,
            closeWith: ['click', 'button']
        }).show();
    }

    function validateConfirmation() {
        confirmation.setCustomValidity(password.value === confirmation.value ? '' : form.dataset.passwordMismatch);
    }

    password.addEventListener('input', validateConfirmation);
    confirmation.addEventListener('input', validateConfirmation);

    form.addEventListener('submit', async event => {

        event.preventDefault();
        validateUsername();
        validateConfirmation();

        if (!form.reportValidity()) return;

        const button = form.querySelector('button[type="submit"]');
        button.disabled = true;
        status.textContent = form.dataset.saving;

        try {
            const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: { Accept: 'application/json' } });
            if (response.redirected)
                throw new Error(form.dataset.accessError);

            if (!response.ok) {
                const error = response.headers.get('content-type')?.includes('application/json') ? await response.json() : null;
                throw new Error(error?.message || form.dataset.connectionError);
            }

            const result = await response.json();
            form.reset();
            notify(result.message || form.dataset.registrationSuccess, result.type);

        } catch (error) {
            notify(error.message || form.dataset.saveError, 'error');
        }
        finally {
            button.disabled = false;
        }
    });
})();
