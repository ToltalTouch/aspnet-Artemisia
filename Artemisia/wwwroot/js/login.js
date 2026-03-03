document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('loginForm');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        var formData = new FormData(form);
        var errDiv = document.getElementById('loginErrors');
        errDiv.style.display = 'none';
        errDiv.innerHTML = '';

        try {
            var resp = await fetch(form.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            });

            var data = await resp.json();

            if (data.success) {
                // Login bem-sucedido: redireciona para a URL retornada pelo servidor
                location.href = data.redirect || '/';
            } else {
                // Exibe a mensagem de erro retornada pelo servidor
                errDiv.style.display = 'block';
                errDiv.innerHTML = data.message || 'E-mail ou senha inválidos.';
            }
        } catch (ex) {
            errDiv.style.display = 'block';
            errDiv.innerHTML = 'Erro de rede. Tente novamente.';
        }
    });
});