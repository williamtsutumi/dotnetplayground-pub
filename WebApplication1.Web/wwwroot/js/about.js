(() => {
    const deploymentTime = document.getElementById('last-deployment-at');
    if (deploymentTime) {
        const date = new Date(deploymentTime.dateTime);
        if (!Number.isNaN(date.getTime())) {
            deploymentTime.textContent = new Intl.DateTimeFormat(undefined, {
                dateStyle: 'medium',
                timeStyle: 'short'
            }).format(date);
        }
    }

    document.getElementById('reload-page-button')?.addEventListener('click', () => {
        const url = new URL(window.location.href);
        url.searchParams.set('_', Date.now().toString());
        url.hash = 'id-who-answered';
        window.location.replace(url.toString());
    });

    function openLinkedSection() {
        if (window.location.hash !== '#id-who-answered') return;
        const panel = document.getElementById('aboutContent');
        const target = document.getElementById('id-who-answered');
        if (!panel || !target) return;

        if (panel.classList.contains('show')) {
            target.scrollIntoView();
            return;
        }
        panel.addEventListener('shown.bs.collapse', () => target.scrollIntoView(), { once: true });
        bootstrap.Collapse.getOrCreateInstance(panel, { toggle: false }).show();
    }

    openLinkedSection();
    window.addEventListener('hashchange', openLinkedSection);
})();
