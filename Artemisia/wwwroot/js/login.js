document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('loginForm');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        var formData = new FormData(form);

        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        var headers = {};
        if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;

        try {
            var resp = await fetch(form.action, {
                method: 'POST',
                headers: headers,
                body: formData,
                credentials: 'same-origin'
            });

            if (resp.ok) {
                // tenta JSON { success: true, redirect: "/..." } ou recarrega por padrão
                try {
                    var data = await resp.json();
                    var modalEl = document.getElementById('loginModal');
                    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    modal.hide();
                    if (data && data.redirect) location.href = data.redirect;
                    else location.reload();
                    return;
                } catch (err) {
                    // não JSON -> recarrega
                    var modalEl = document.getElementById('loginModal');
                    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    modal.hide();
                    location.reload();
                }
            } else {
                var errText = await resp.text();
                var errDiv = document.getElementById('loginErrors');
                errDiv.style.display = 'block';
                errDiv.innerHTML = errText || 'Erro no login';
            }
        } catch (ex) {
            var errDiv = document.getElementById('loginErrors');
            errDiv.style.display = 'block';
            errDiv.innerHTML = 'Erro de rede';
        }
    });
});