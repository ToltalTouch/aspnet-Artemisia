(function () {
  // garante que o bootstrap esteja disponível
  if (typeof bootstrap === 'undefined') return;

  const DROPDOWN_SELECTOR = '.category-nav .nav-item.dropdown';

  function setupDropdown(li) {
    const toggle = li.querySelector('[data-bs-toggle="dropdown"]');
    if (!toggle) return;
    const instance = bootstrap.Dropdown.getOrCreateInstance(toggle);

    function show() {
      instance.show();
      li.classList.add('show');
      const menu = li.querySelector('.dropdown-menu');
      if (menu) menu.classList.add('show');
    }
    function hide() {
      instance.hide();
      li.classList.remove('show');
      const menu = li.querySelector('.dropdown-menu');
      if (menu) menu.classList.remove('show');
    }

    // hover em desktop, clique/hover nativo em mobile
    function applyMode() {
      if (window.matchMedia('(min-width: 992px)').matches) {
        li.addEventListener('mouseenter', show);
        li.addEventListener('mouseleave', hide);
      } else {
        li.removeEventListener('mouseenter', show);
        li.removeEventListener('mouseleave', hide);
      }
    }

    applyMode();
    window.addEventListener('resize', applyMode);
  }

  function initAll() {
    document.querySelectorAll(DROPDOWN_SELECTOR).forEach(setupDropdown);
  }

  // init on DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAll);
  } else {
    initAll();
  }
})();