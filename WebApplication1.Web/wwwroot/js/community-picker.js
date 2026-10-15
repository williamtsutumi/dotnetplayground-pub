(() => {
    const input = document.getElementById('ComunidadeDestino');
    const list = document.getElementById('community-options');
    const error = document.getElementById('community-error');
    const options = [...list.querySelectorAll('button')];
    let revision = 0;
    function showStatus(message, valid = false) {
        error.textContent = message;
        error.style.color = valid ? '#b9e28c' : '';
    }
    function close() { list.hidden = true; input.setAttribute('aria-expanded', 'false'); }
    function show() {
        list.hidden = false; input.setAttribute('aria-expanded', 'true');
        const term = input.value.trim().toLocaleLowerCase();
        options.forEach(option => option.hidden = !!option.dataset.community && !option.dataset.community.toLocaleLowerCase().includes(term));
        document.getElementById('community-no-match').hidden = options.some(o => o.dataset.community && !o.hidden);
    }
    async function validate() {
        const current = ++revision;
        if (!input.value.trim()) { input.setCustomValidity(''); showStatus(window.playgroundText["PostCreateNoCommunityStatus"], true); return; }
        input.setCustomValidity(window.playgroundText["PostCreateCommunityValidationPending"]);
        showStatus(window.playgroundText["PostCreateCommunityValidationInProgress"]);
        try {
            const url = new URL(input.dataset.validationUrl, location.origin);
            url.searchParams.set('nome', input.value.trim());
            const response = await fetch(url, { headers: { Accept: 'application/json' } });
            if (!response.ok || response.redirected) throw new Error();
            const result = await response.json();
            if (current !== revision) return;
            const message = result.valido ? '' : result.message;
            input.setCustomValidity(message);
            showStatus(result.valido ? window.playgroundText["PostCreateCommunityValidationSuccess"] : message, result.valido);
        } catch {
            if (current !== revision) return;
            const message = window.playgroundText["PostCreateCommunityValidationFailure"];
            input.setCustomValidity(message); showStatus(message);
        }
    }
    input.addEventListener('focus', show);
    input.addEventListener('input', () => { revision++; input.setCustomValidity(''); showStatus(''); show(); });
    input.addEventListener('blur', () => { validate(); setTimeout(() => { if (!list.contains(document.activeElement)) close(); }, 0); });
    options.forEach(option => {
        option.addEventListener('mousedown', event => event.preventDefault());
        option.addEventListener('click', () => { input.value = option.dataset.community; close(); validate(); });
    });
    document.addEventListener('click', event => { if (!input.parentElement.contains(event.target)) close(); });
    input.addEventListener('keydown', event => {
        if (event.key === 'Escape') close();
        if (event.key === 'ArrowDown') { event.preventDefault(); show(); options.find(o => !o.hidden)?.focus(); }
    });
})();
