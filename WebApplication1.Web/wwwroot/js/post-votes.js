document.querySelectorAll('.post-votes').forEach(form => {

    let busy = false;

    form.addEventListener('submit', async event => {
        event.preventDefault();

        if (busy || !event.submitter)
            return;

        const data = new FormData(form);
        const upvoteButton = form.querySelector('[value="0"]');
        const downvoteButton = form.querySelector('[value="1"]');
        data.set('tipo', event.submitter.value);
        data.set('remover', String(event.submitter.getAttribute('aria-pressed') === 'true'));
        busy = true;
        const buttons = form.querySelectorAll('button');
        buttons.forEach(button => button.disabled = true);
        let message;
        let type = 'success';

        try {
            const response = await fetch(form.action, { method: 'POST', body: data, headers: { Accept: 'application/json' } });
            if (response.redirected || response.status === 401)
                throw new Error(form.dataset.voteLogin);

            const result = response.headers.get('content-type')?.includes('application/json') ? await response.json() : null;
            if (!response.ok || !result)
                throw new Error(result?.message || form.dataset.voteRetry);

            form.querySelector('.upvote-total').textContent = result.total;
            upvoteButton.setAttribute('aria-pressed', String(result.voted));

            const label = result.voted ? form.dataset.removeUpvote : form.dataset.upvote;
            upvoteButton.setAttribute('aria-label', label);
            upvoteButton.title = label;
            downvoteButton.setAttribute('aria-pressed', String(result.downvoted));

            const downLabel = result.downvoted ? form.dataset.removeDownvote : form.dataset.downvote;
            downvoteButton.setAttribute('aria-label', downLabel);
            downvoteButton.title = downLabel;
            message = result.message;
        }
        catch (error) {
            message = error.message || form.dataset.voteError; type = 'error';
        }
        finally {
            busy = false; buttons.forEach(button => button.disabled = false);
        }
        form.querySelector('.vote-status').textContent = message;
        const text = document.createElement('span');
        text.textContent = message;
        if (typeof Noty === 'function') new Noty({ text: text.innerHTML, type, layout: 'bottomRight', theme: 'mint', timeout: 3500 }).show();
    });
});
