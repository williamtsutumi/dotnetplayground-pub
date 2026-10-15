(() => {
    window.renderPostMarkdown = source => {
        const container = document.createElement('div');
        // Safe, readable fallback when a CDN dependency cannot be loaded.
        if (!window.marked || !window.DOMPurify?.isSupported) {
            const fallback = document.createElement('pre');
            fallback.textContent = source;
            container.append(fallback);
            return container.innerHTML;
        }
        const renderer = new window.marked.Renderer();
        const renderCode = renderer.code;
        renderer.code = function (token) {
            const info = token.lang || '';
            const filenameMatch = /(?:^|\s)filename\s*=\s*(?:"([^"\r\n]*)"|'([^'\r\n]*)')(?=\s|$)/.exec(info);
            if (!filenameMatch) return renderCode.call(this, token);

            const filename = filenameMatch[1] ?? filenameMatch[2];
            const language = info.slice(0, filenameMatch.index).trim();
            const block = document.createElement('div');
            block.innerHTML = renderCode.call(this, { ...token, lang: language });
            if (filename.trim()) block.querySelector('pre').setAttribute('data-code-filename', filename);
            return block.innerHTML;
        };
        const html = window.marked.parse(source, { gfm: true, breaks: false, async: false, renderer });
        const content = window.DOMPurify.sanitize(html, {
            ALLOWED_TAGS: ['p', 'br', 'hr', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
                'strong', 'em', 'del', 'blockquote', 'ul', 'ol', 'li', 'pre', 'code',
                'a', 'img', 'table', 'thead', 'tbody', 'tr', 'th', 'td', 'input'],
            ALLOWED_ATTR: ['href', 'src', 'alt', 'title', 'start', 'type', 'checked', 'disabled', 'align', 'class', 'data-code-filename'],
            ALLOW_DATA_ATTR: false,
            ALLOW_ARIA_ATTR: false,
            RETURN_DOM_FRAGMENT: true
        });
        // Preserve only code languages, never arbitrary application CSS classes.
        content.querySelectorAll('[class]').forEach(element => {
            const language = element.tagName === 'CODE' &&
                Array.from(element.classList).find(name => /^language-[\w#+-]+$/.test(name));
            element.removeAttribute('class');
            if (language) element.classList.add(language);
        });
        content.querySelectorAll('input').forEach(input => {
            if (input.type !== 'checkbox') input.remove();
            else input.disabled = true;
        });
        content.querySelectorAll('a').forEach(link => link.setAttribute('rel', 'nofollow ugc noopener noreferrer'));
        content.querySelectorAll('img').forEach(img => {
            img.loading = 'lazy';
            img.referrerPolicy = 'no-referrer';
        });
        content.querySelectorAll('pre code').forEach(code => {
            const language = Array.from(code.classList).find(name => name.startsWith('language-'))?.slice(9);
            // Unknown languages and unlabelled blocks remain readable plain code.
            if (!window.hljs || !language || !window.hljs.getLanguage(language)) return;
            // Read text only: HTML-looking code is never interpreted as markup.
            code.innerHTML = window.hljs.highlight(code.textContent, { language, ignoreIllegals: true }).value;
            code.classList.add('hljs');
        });
        content.querySelectorAll('pre[data-code-filename]').forEach(pre => {
            const figure = document.createElement('figure');
            figure.className = 'markdown-code-file';
            const caption = document.createElement('figcaption');
            caption.textContent = pre.getAttribute('data-code-filename');
            pre.removeAttribute('data-code-filename');
            pre.replaceWith(figure);
            figure.append(caption, pre);
        });
        container.append(content);
        return container.innerHTML;
    };
})();
