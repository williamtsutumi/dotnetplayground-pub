(() => {
    const form = document.getElementById('post-form');
    const button = form.querySelector('button[type="submit"]');
    let submitting = false;
    form.addEventListener('submit', event => {
        if (submitting) { event.preventDefault(); return; }
        if (!form.checkValidity() || (window.jQuery && !window.jQuery(form).valid())) return;
        submitting = true;
        button.disabled = true;
        button.textContent = window.playgroundText["PostCreateProgressLabel"];
    });
    window.addEventListener('pageshow', () => {
        submitting = false;
        button.disabled = false;
        button.textContent = window.playgroundText["PostPublishAction"];
    });
})();
