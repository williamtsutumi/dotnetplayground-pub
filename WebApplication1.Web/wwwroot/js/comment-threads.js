document.querySelectorAll('.comment-thread-list').forEach(list => {
    const svg = list.querySelector('.comment-thread-lines');
    const items = [...list.querySelectorAll('[data-comment-id]')];
    const byId = new Map(items.map(item => [item.dataset.commentId, item]));
    let frame;
    function draw() {
        const bounds = list.getBoundingClientRect();
        svg.setAttribute('viewBox', `0 0 ${bounds.width} ${bounds.height}`);
        const paths = [];
        for (const item of items) {
            const parent = byId.get(item.dataset.parentId);
            if (!parent) continue;
            const from = parent.querySelector('.comment-avatar').getBoundingClientRect();
            const to = item.querySelector('.comment-avatar').getBoundingClientRect();
            const x = from.left + from.width / 2 - bounds.left;
            const y = from.bottom - bounds.top + 3;
            const endX = to.left - bounds.left - 3;
            const endY = to.top + to.height / 2 - bounds.top;
            const radius = Math.min(10, Math.abs(endX - x), Math.max(0, (endY - y) / 2));
            const direction = endX >= x ? 1 : -1;
            const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
            path.setAttribute('d', `M ${x} ${y} V ${endY - radius} Q ${x} ${endY} ${x + radius * direction} ${endY} H ${endX}`);
            paths.push(path);
        }
        svg.replaceChildren(...paths);
    }
    function schedule() {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(draw);
    }
    const observer = new ResizeObserver(schedule);
    observer.observe(list);
    items.forEach(item => observer.observe(item));
    schedule();
});


document.querySelector("#input-comment-need-login").addEventListener("submit", function (event) {
    event.preventDefault();

    debugger;

    new Noty({
        text: event.currentTarget.getAttribute("comment-login"),
        type: "error",
        layout: 'bottomRight',
        theme: 'mint',
        timeout: 3500
    }).show();
});
