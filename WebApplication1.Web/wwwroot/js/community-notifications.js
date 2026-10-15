document.querySelectorAll('.community-notification').forEach(element => {
    const text = document.createElement('span');
    text.textContent = element.dataset.message;
    if (typeof Noty === 'function') {
        new Noty({ text: text.innerHTML, type: element.dataset.type, layout: 'bottomRight', theme: 'mint', timeout: 5000, closeWith: ['click', 'button'] }).show();
    } else {
        element.textContent = element.dataset.message;
        element.setAttribute('role', 'status');
        element.hidden = false;
    }
});
