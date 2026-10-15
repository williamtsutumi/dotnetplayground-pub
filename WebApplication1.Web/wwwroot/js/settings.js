(() => {
    document.querySelectorAll('input[name="site-language"]').forEach(input => {
        input.addEventListener('change', () => {
            if (input.checked) window.location.assign(input.dataset.languageUrl);
        });
    });
})();
