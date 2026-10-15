(() => {
    document.querySelectorAll('[data-confirm-delete]').forEach(form => {
        let confirmed = false;
        let dialog = null;
        form.addEventListener('submit', event => {
            if (confirmed) return;
            event.preventDefault();
            if (dialog) return;
            const trigger = event.submitter || form.querySelector('button[type="submit"]');
            const submit = () => {
                confirmed = true;
                form.requestSubmit(trigger);
                trigger.disabled = true;
            };
            if (typeof window.Noty !== 'function') {
                if (window.confirm(form.dataset.confirmMessage)) submit();
                return;
            }
            dialog = new Noty({
                text: form.dataset.confirmMessage,
                type: 'warning', layout: 'center', theme: 'mint',
                modal: true, timeout: false, closeWith: [],
                buttons: [
                    Noty.button(form.dataset.confirmYes, 'btn btn-danger', () => {
                        if (confirmed) return;
                        dialog.close();
                        submit();
                    }, { type: 'button' }),
                    Noty.button(form.dataset.confirmNo, 'btn btn-secondary', () => dialog.close(), { type: 'button' })
                ],
                callbacks: {
                    onShow: function () {
                        this.barDom.classList.add('delete-confirmation');
                        this.barDom.setAttribute('role', 'alertdialog');
                        this.barDom.setAttribute('aria-modal', 'true');
                        this.barDom.setAttribute('aria-label', form.dataset.confirmMessage);
                        const buttons = this.barDom.querySelectorAll('button');
                        buttons[buttons.length - 1].focus();
                        this.barDom.addEventListener('keydown', event => {
                            if (event.key === 'Escape') { event.preventDefault(); this.close(); }
                            if (event.key === 'Tab') {
                                event.preventDefault();
                                const index = Array.from(buttons).indexOf(document.activeElement);
                                buttons[(index + (event.shiftKey ? -1 : 1) + buttons.length) % buttons.length].focus();
                            }
                        });
                    },
                    afterClose: () => { dialog = null; if (!confirmed) trigger.focus(); }
                }
            }).show();
        });
        window.addEventListener('pageshow', () => {
            confirmed = false;
            form.querySelector('button[type="submit"]').disabled = false;
        });
    });
})();
