(() => {
    const status = document.getElementById('home-status');
    let timer;
    function notify(message) {
        status.textContent = message;
        status.hidden = false;
        clearTimeout(timer);
        timer = setTimeout(() => { status.hidden = true; }, 4500);
    }
    document.querySelectorAll('[data-preview]').forEach(button => button.addEventListener('click', () => notify(button.dataset.preview)));
    document.querySelectorAll('[data-community-toggle]').forEach(button => {
        button.dataset.showMore = button.textContent.trim();
        button.addEventListener('click', () => {
            const expanded = button.getAttribute('aria-expanded') === 'true';
            const listId = button.dataset.communityToggle;
            document.querySelectorAll(`[data-community-list="${listId}"]`).forEach(item => {
                if (Number(item.dataset.communityIndex) < 3) return;
                if (expanded) {
                    item.inert = true;
                    item.classList.add('is-collapsed');
                } else {
                    item.inert = false;
                    item.classList.remove('is-collapsed');
                }
            });
            button.setAttribute('aria-expanded', String(!expanded));
            button.textContent = expanded ? button.dataset.showMore : button.dataset.showLess;
        });
    });
    const menu = document.querySelector('.menu-toggle');
    const sidebar = document.getElementById('sidebar');
    function closeMenu() { sidebar.classList.remove('open'); menu.setAttribute('aria-expanded', 'false'); }
    menu.addEventListener('click', () => menu.setAttribute('aria-expanded', sidebar.classList.toggle('open').toString()));
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape') { closeMenu(); menu.focus(); }
        if (event.key === '/' && !event.ctrlKey && !event.metaKey && !['INPUT', 'TEXTAREA', 'SELECT'].includes(document.activeElement.tagName)) {
            event.preventDefault(); document.getElementById('feed-search').focus();
        }
    });
    document.addEventListener('click', event => { if (!sidebar.contains(event.target) && !menu.contains(event.target)) closeMenu(); });
    const post = document.getElementById('sample-post');
    if (!post) return;
    const search = document.getElementById('feed-search');
    const savedFilter = document.getElementById('saved-filter');
    let saved = false;
    let onlySaved = false;
    const normalize = value => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
    const searchable = normalize(post.querySelector('h2').textContent + ' fotografia marina.luz ' + post.querySelector('.post-description').textContent);
    function filter() {
        post.hidden = !searchable.includes(normalize(search.value.trim())) || (onlySaved && !saved);
        document.getElementById('empty-feed').hidden = !post.hidden;
    }
    document.getElementById('search-form').addEventListener('submit', event => { event.preventDefault(); filter(); });
    search.addEventListener('input', filter);
    savedFilter.addEventListener('click', () => {
        onlySaved = !onlySaved;
        savedFilter.setAttribute('aria-pressed', String(onlySaved));
        savedFilter.classList.toggle('selected', onlySaved);
        document.getElementById('feed-title').textContent = onlySaved ? window.playgroundText["PreviewSavedPostsHeading"] : window.playgroundText["PreviewMainFeedHeading"];
        filter();
    });
    function toggleSave() {
        saved = !saved;
        document.getElementById('save-button').setAttribute('aria-pressed', String(saved));
        document.getElementById('save-button').setAttribute('aria-label', saved ? window.playgroundText["PostBookmarkRemoveAction"] : window.playgroundText["PostBookmarkAddAction"]);
        document.getElementById('save-menu').textContent = saved ? window.playgroundText["PostBookmarkRemoveAction"] : window.playgroundText["PostBookmarkAddAction"];
        notify(saved ? window.playgroundText["PreviewBookmarkAddedNotice"] : window.playgroundText["PreviewBookmarkRemovedNotice"]);
        filter();
    }
    document.getElementById('save-button').addEventListener('click', toggleSave);
    document.getElementById('save-menu').addEventListener('click', toggleSave);
    let vote = 0;
    [ ['upvote', 1], ['downvote', -1] ].forEach(([id, value]) => {
        document.getElementById(id).addEventListener('click', () => {
            vote = vote === value ? 0 : value;
            document.getElementById('vote-count').textContent = 128 + vote;
            document.getElementById('upvote').setAttribute('aria-pressed', String(vote === 1));
            document.getElementById('downvote').setAttribute('aria-pressed', String(vote === -1));
        });
    });
    const join = document.getElementById('join-button');
    join.addEventListener('click', () => {
        const joined = join.getAttribute('aria-pressed') !== 'true';
        join.setAttribute('aria-pressed', String(joined));
        join.textContent = joined ? window.playgroundText["CommunityJoinStatusLabel"] : window.playgroundText["CommunityJoinActionLabel"];
        notify(joined ? window.playgroundText["PreviewCommunityJoinedNotice"] : window.playgroundText["PreviewCommunityLeftNotice"]);
    });
    const view = document.getElementById('view-toggle');
    view.addEventListener('click', () => {
        const compact = post.classList.toggle('compact');
        view.setAttribute('aria-pressed', String(compact));
        view.setAttribute('aria-label', compact ? window.playgroundText["MarkdownExpandViewAction"] : window.playgroundText["MarkdownCollapseViewAction"]);
    });
    document.getElementById('feed-sort').addEventListener('change', () => notify(window.playgroundText["PreviewSinglePostSortNotice"]));
    const comments = document.getElementById('comments');
    document.getElementById('comments-toggle').addEventListener('click', event => {
        comments.hidden = !comments.hidden;
        event.currentTarget.setAttribute('aria-expanded', String(!comments.hidden));
        if (!comments.hidden) document.getElementById('comment-text').focus();
    });
    document.getElementById('comment-form').addEventListener('submit', event => {
        event.preventDefault();
        const input = document.getElementById('comment-text');
        if (!input.value.trim()) return;
        const comment = document.createElement('p');
        comment.className = 'local-comment';
        comment.textContent = input.value.trim();
        document.getElementById('comment-list').appendChild(comment);
        document.getElementById('comment-count').textContent = document.getElementById('comment-list').children.length;
        input.value = '';
        notify(window.playgroundText["PreviewCommentAddedNotice"]);
    });
    document.getElementById('share-button').addEventListener('click', async () => {
        try {
            await navigator.clipboard.writeText(window.location.origin + window.location.pathname + '#sample-post');
            notify(window.playgroundText["PostLinkCopySuccessNotice"]);
        } catch { notify(window.playgroundText["ClipboardCopyFailureNotice"]); }
    });
})();
