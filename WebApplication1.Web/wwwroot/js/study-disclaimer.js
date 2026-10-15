(() => {
    const disclaimer = document.getElementById('study-disclaimer');
    if (!disclaimer) return;
    const remember = document.getElementById('study-disclaimer-remember');
    const storageKey = 'playground.studyDisclaimer.dismissed';

    function updateSpace() {
        document.body.style.setProperty('--disclaimer-height', `${disclaimer.hidden ? 0 : disclaimer.offsetHeight}px`);
    }

    function restorePreference() {
        let dismissed = false;
        try { dismissed = window.localStorage.getItem(storageKey) === 'true'; }
        catch { /* Storage may be unavailable; keep the disclaimer usable. */ }
        disclaimer.hidden = dismissed;
        remember.checked = dismissed;
        updateSpace();
    }

    document.getElementById('study-disclaimer-accept').addEventListener('click', () => {
        if (remember.checked) {
            try { window.localStorage.setItem(storageKey, 'true'); }
            catch { /* Still dismiss the current page when persistence is unavailable. */ }
        }
        disclaimer.hidden = true;
        updateSpace();
    });
    new ResizeObserver(updateSpace).observe(disclaimer);
    window.addEventListener('pageshow', restorePreference);
    window.addEventListener('storage', event => {
        if (event.key === storageKey || event.key === null) restorePreference();
    });
    restorePreference();
})();
