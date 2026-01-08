document.addEventListener('DOMContentLoaded', function(){
    var cartModalEl = document.getElementById('cartModal');
    if(!cartModalEl) return;

    cartModalEl.addEventListener('show.bs.modal', loadCartContent);

    async function loadCartContent() {
        var body = document.getElementById('cartModalBody');
        body.innerHTML = '<div class="text-center py-5"><div class="spinner-border" role="status"><span class="visually-hidden">Carregando...</span></div></div>';
        try {
            var resp = await fetch('/Market/CartContent', { credentials: 'same-origin' });
            if (resp.ok) {
                body.innerHTML = await resp.text();
                attachHandlers();
            } else {
                body.innerHTML = '<div class="text-danger>Erro ao carregar o carrinho.</div>';
            }
        } catch (e) {
            body.innerHTML = '<div class="text-danger>Erro de rede.</div>';
        }
    }

    function getToken() {
        var t = document.querySelector('input[name="__RequestVerificationToken"]');
        return t ? t.value : null;
    }

    function attachHandlers() {
        document.querySelectorAll('.btn-remove-item').forEach(btn => {
            btn.addEventListener('click', async function() {
                var id = this.dataset.itemId;
                await postAction('/Market/RemoveFromCart', { itemId: id });
                loadCartContent();
            })
        });

        document.querySelectorAll('.btn-increase-qty').forEach(btn => {
            btn.addEventListener('click', async function() {
                var id = this.dataset.itemId;
                await postAction('/Market/IncreaseCartItem', { itemId: id });
                loadCartContent();
            })
        });
    }

    async function postAction(url, data){
        var token = getToken();
        var form = new FormData();

        for (var k in data) form.apprend(k, data[k]);
        if (token) form.append('__RequestVerificationToken', token);

        try {
            var resp = await fetch(url, {
                method: 'POST',
                body: form,
                credentials: 'same-origin'
            });
            if (resp.ok) {
                var json = await resp.json();
                var el = document.querySelector(`[data-item-id="${json.itemId}"]`);
                if (el && json.count !== undefined) el.textContent = json.count;
                loadCartContent();
            } else {
                console.error('Ação falhou.', await resp.text());
            }
        } catch (e) {
            console.error('Erro de rede.', e);
        }
    }
});