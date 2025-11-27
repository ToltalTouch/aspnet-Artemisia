(function () {
  if (typeof bootstrap === 'undefined') return;

  const DROPDOWN_SELECTOR = '.category-nav .nav-item.dropdown';
  const AJAX_LINK_SELECTOR = '.ajax-category-link';
  const PRODUCT_GRID_ID = 'product-grid';

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
    setupAjaxCategoryLinks();
    window.addEventListener('popstate', onPopState);
  }

  // AJAX category handling (uses categoryId data attribute)
  function setupAjaxCategoryLinks() {
    document.body.addEventListener('click', function (e) {
      const a = e.target.closest(AJAX_LINK_SELECTOR);
      if (!a) return;
      // allow middle-click / ctrl/cmd+click to open in new tab
      if (e.ctrlKey || e.metaKey || e.button === 1) return;
      e.preventDefault();
      const catId = a.dataset.categoryId;
      fetchCategory(catId, true);
    });
  }

  async function fetchCategory(categoryId, pushState) {
    try {
  const url = '/categoria/products?categoryId=' + encodeURIComponent(categoryId);
      const res = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
      if (!res.ok) {
        // fallback: navigate full page
        window.location.href = '/categoria/' + encodeURIComponent(categoryId);
        return;
      }
      const html = await res.text();
      const grid = document.getElementById(PRODUCT_GRID_ID);
      if (grid) grid.innerHTML = html;

      if (pushState) {
  const newUrl = '/categoria/' + encodeURIComponent(categoryId);
        history.pushState({ categoryId }, '', newUrl);
      }
    } catch (err) {
      console.error('Erro ao carregar categoria:', err);
      // fallback full navigation
  window.location.href = '/categoria/' + encodeURIComponent(categoryId);
    }
  }

  function onPopState(ev) {
    const state = ev.state;
    if (state && state.categoryId) {
      fetchCategory(state.categoryId, false);
    } else {
      // if no state, optionally reload full page
      location.reload();
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAll);
  } else {
    initAll();
  }

  function enableHoverDropdowns() {
    const items = document.querySelectorAll('.category-nav .nav-item.dropdown');
    items.forEach(li => {
      const toggle = li.querySelector('[data-bs-toggle="dropdown"]');
      if (!toggle) return;
      const instance = bootstrap.Dropdown.getOrCreateInstance(toggle);

      let enter = () => {
        if (window.matchMedia('(min-width: 992px)').matches) {
          instance.show();
          li.classList.add('show');
          const menu = li.querySelector('.dropdown-menu');
          if (menu) menu.classList.add('show');
        }
      };

      let leave = () => {
        if (window.matchMedia('(min-width: 992px)').matches) {
          instance.hide();
          li.classList.remove('show');
          const menu = li.querySelector('.dropdown-menu');
          if (menu) menu.classList.remove('show');
        }
      };
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    enableHoverDropdowns();
  });
  
})();