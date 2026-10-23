document.querySelectorAll('.sidebar-nav a').forEach(link => {
    link.addEventListener('click', e => {
        e.preventDefault();
        document.body.classList.add('page-leave');

        setTimeout(() => {
            window.location = link.href;
        }, 200);
    });
});