(() => {
    if (!window.renderPostMarkdown) return;
    document.querySelectorAll('[data-post-markdown]').forEach(element => {
        element.innerHTML = window.renderPostMarkdown(element.textContent);
        element.classList.add('post-markdown-rendered');
    });
})();
