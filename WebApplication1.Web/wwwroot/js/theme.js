(() => {
    const storageKey = 'playground-theme';
    const normalize = value => value === 'light' ? 'light' : 'dark';
    function apply(value) {
        const theme = normalize(value);
        document.documentElement.dataset.theme = theme;
        document.querySelector('meta[name="theme-color"]')?.setAttribute('content', theme === 'light' ? '#f5f5f1' : '#101415');
        document.querySelectorAll('input[name="site-theme"]').forEach(input => {
            input.checked = input.value === theme;
        });
    }
    let initialTheme = 'dark';
    try { initialTheme = localStorage.getItem(storageKey); } catch { /* Storage may be unavailable. */ }
    apply(initialTheme);
    document.addEventListener('DOMContentLoaded', () => {
        apply(document.documentElement.dataset.theme);
        document.querySelectorAll('input[name="site-theme"]').forEach(input => {
            input.addEventListener('change', () => {
                if (!input.checked) return;
                apply(input.value);
                try { localStorage.setItem(storageKey, input.value); } catch { /* Keep the theme for this page. */ }
            });
        });
    });
    window.addEventListener('storage', event => {
        if (event.key === storageKey || event.key === null) apply(event.newValue);
    });
})();
