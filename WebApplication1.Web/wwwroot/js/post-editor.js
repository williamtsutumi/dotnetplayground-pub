(() => {
    const toggle = document.getElementById('ConteudoMarkdown');
    const textarea = document.getElementById('Conteudo');
    const field = document.getElementById('post-editor-field');
    if (!toggle || !textarea || !field) return;
    const error = document.getElementById('post-editor-error');
    let editor = null;

    function validate() {
        const value = editor.value();
        const message = !value.trim() ? field.dataset.required :
            value.length > textarea.maxLength ? field.dataset.tooLong : '';
        error.textContent = message;
        editor.codemirror.getInputField().setAttribute('aria-invalid', String(!!message));
        return !message;
    }

    function changeMode() {
        error.textContent = '';
        if (toggle.checked && !editor) {
            if (!window.EasyMDE) {
                error.textContent = field.dataset.unavailable;
                return;
            }
            const button = (name, action, icon, title) => ({ name, action, className: `fa fa-${icon}`, title });
            editor = new EasyMDE({
                element: textarea,
                forceSync: true,
                autoDownloadFontAwesome: false,
                spellChecker: false,
                status: false,
                previewRender: source => {
                    if (window.renderPostMarkdown) return window.renderPostMarkdown(source);
                    const fallback = document.createElement('pre');
                    fallback.textContent = source;
                    return fallback.outerHTML;
                },
                previewClass: ['editor-preview', 'post-markdown', 'post-markdown-rendered'],
                shortcuts: { toggleSideBySide: null, toggleFullScreen: null },
                toolbar: [
                    button('bold', EasyMDE.toggleBold, 'bold', field.dataset.bold),
                    button('italic', EasyMDE.toggleItalic, 'italic', field.dataset.italic),
                    button('heading', EasyMDE.toggleHeadingSmaller, 'header', field.dataset.heading), '|',
                    button('quote', EasyMDE.toggleBlockquote, 'quote-left', field.dataset.quote),
                    button('unordered-list', EasyMDE.toggleUnorderedList, 'list-ul', field.dataset.list),
                    button('ordered-list', EasyMDE.toggleOrderedList, 'list-ol', field.dataset.numberedList), '|',
                    button('code', EasyMDE.toggleCodeBlock, 'code', field.dataset.code),
                    button('link', EasyMDE.drawLink, 'link', field.dataset.link), '|',
                    { name: 'preview', action: EasyMDE.togglePreview, className: 'fa fa-eye no-disable', title: field.dataset.preview }
                ]
            });
            // The visible editor handles validation while the original textarea is hidden.
            textarea.required = false;
            const input = editor.codemirror.getInputField();
            input.setAttribute('aria-labelledby', 'post-content-label');
            input.setAttribute('aria-describedby', 'post-editor-error');
            input.setAttribute('aria-required', 'true');
            editor.codemirror.on('change', () => { if (error.textContent) validate(); });
        } else if (!toggle.checked && editor) {
            const value = editor.value();
            editor.toTextArea();
            editor = null;
            textarea.value = value;
            textarea.required = true;
        }
    }

    toggle.addEventListener('change', changeMode);
    textarea.form.addEventListener('submit', event => {
        if (editor && !validate()) {
            event.preventDefault();
            event.stopImmediatePropagation();
            if (editor.isPreviewActive()) EasyMDE.togglePreview(editor);
            editor.codemirror.focus();
        }
    }, true);
    document.getElementById('post-content-label').addEventListener('click', () => {
        if (editor) editor.codemirror.focus();
    });
    changeMode();
})();
