function loadScript(src) {
  return new Promise((resolve, reject) => {
    if (document.querySelector(`script[src="${src}"]`)) {
      resolve();
      return;
    }
    const script = document.createElement('script');
    script.src = src;
    script.onload = resolve;
    script.onerror = () => reject(new Error(`Échec du chargement de ${src}`));
    document.head.appendChild(script);
  });
}

function loadCSS(href) {
  return new Promise((resolve) => {
    if (document.querySelector(`link[href="${href}"]`)) {
      resolve();
      return;
    }
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    link.onload = resolve;
    document.head.appendChild(link);
  });
}

function swapSkinCSS(theme) {
  const skinName = theme === 'dark' ? 'oxide-dark' : 'oxide';
  document.querySelectorAll('link[href*="/skins/ui/"]').forEach(el => el.remove());
  loadCSS(`/lib/tinymce/skins/ui/${skinName}/skin.min.css`);
  loadCSS(`/lib/tinymce/skins/ui/${skinName}/content.min.css`);
}

window.__tinyMCEInstances = window.__tinyMCEInstances || {};

window.loadTinyMCE = async (id, initialContent = '') => {
  if (typeof tinymce === 'undefined') {
    const theme = document.documentElement.getAttribute('data-theme') || 'light';
    const skinName = theme === 'dark' ? 'oxide-dark' : 'oxide';
    await Promise.all([
      loadScript('/lib/tinymce/tinymce.min.js'),
      loadCSS(`/lib/tinymce/skins/ui/${skinName}/skin.min.css`),
      loadCSS(`/lib/tinymce/skins/ui/${skinName}/content.min.css`)
    ]);
  }

  const existing = tinymce.get(id);
  if (existing) {
    existing.destroy();
  }

  const element = document.getElementById(id);
  if (!element) {
    console.error(`[TinyMCE] Élément #${id} introuvable.`);
    return;
  }

  const theme = document.documentElement.getAttribute('data-theme') || 'light';
  const isDark = theme === 'dark';

  tinymce.init({
    selector: '#' + id,
    base_url: '/lib/tinymce',
    license_key: 'gpl',
    height: 380,
    menubar: true,
    plugins: 'lists link image table code',
    toolbar: 'undo redo | bold italic underline | alignleft aligncenter alignright | bullist numlist | link image | code',
    skin_url: isDark ? '/lib/tinymce/skins/ui/oxide-dark' : '/lib/tinymce/skins/ui/oxide',
    content_css: isDark ? '/lib/tinymce/skins/content/dark/content.min.css' : '/lib/tinymce/skins/content/default/content.min.css',
    init_instance_callback: (editor) => {
      if (initialContent) {
        editor.setContent(initialContent);
      }
    },
    setup: (editor) => {
      editor.on('remove', () => {
        const ta = document.getElementById(id);
        if (ta) ta.value = '';
        delete window.__tinyMCEInstances[id];
      });
    }
  });

  window.__tinyMCEInstances[id] = { initialContent };
};

window.updateTinyMCETheme = (theme) => {
  const isDark = theme === 'dark';

  swapSkinCSS(theme);

  Object.keys(window.__tinyMCEInstances).forEach(id => {
    const editor = tinymce.get(id);
    if (!editor) return;

    const doc = editor.getDoc();
    if (!doc) return;

    const oldLink = doc.querySelector('link[href*="/skins/content/"]');
    if (oldLink) oldLink.remove();

    const newLink = doc.createElement('link');
    newLink.rel = 'stylesheet';
    newLink.href = isDark
      ? '/lib/tinymce/skins/content/dark/content.min.css'
      : '/lib/tinymce/skins/content/default/content.min.css';
    doc.head.appendChild(newLink);
  });
};

window.getTinyMCEContent = (id) => {
  if (typeof tinymce === 'undefined') return '';
  const editor = tinymce.get(id);
  if (!editor) return '';
  return editor.getContent();
};

window.setTinyMCEContent = (id, content) => {
  if (typeof tinymce === 'undefined') return;
  const editor = tinymce.get(id);
  if (!editor) return;
  editor.setContent(content || '');
};

window.destroyTinyMCE = (id) => {
  if (typeof tinymce === 'undefined') return;
  const editor = tinymce.get(id);
  if (editor) {
    editor.destroy();
  }
};
