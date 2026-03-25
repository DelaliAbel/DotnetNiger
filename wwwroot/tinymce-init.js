// Attendre que TinyMCE soit chargé
function waitForTinyMCE(callback, timeout = 5000) {
      const startTime = Date.now();
      const checkTinyMCE = () => {
            if (typeof tinymce !== 'undefined') {
                  callback();
            } else if (Date.now() - startTime < timeout) {
                  setTimeout(checkTinyMCE, 100);
            } else {
                  console.error('TinyMCE failed to load');
            }
      };
      checkTinyMCE();
}

window.initTinyMCE = (id) => {
      waitForTinyMCE(() => {
            try {
                  const element = document.getElementById(id);
                  if (!element) {
                        console.error(`Element with id "${id}" not found`);
                        return;
                  }
                  
                  tinymce.init({
                        selector: '#' + id,
                        base_url: '/lib/tinymce',  // Chemin vers les fichiers locaux
                        license_key: 'gpl',  // Licence Community (GPLv2)
                        height: 300,
                        menubar: true,
                        plugins: 'lists link image table code',
                        toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | bullist numlist | code',
                        init_instance_callback: (editor) => {
                              console.log('TinyMCE initialized for:', id);
                        }
                  });
            } catch (error) {
                  console.error('Error initializing TinyMCE:', error);
            }
      });
};

window.getTinyMCEContent = (id) => {
      try {
            if (typeof tinymce === 'undefined') {
                  console.error('TinyMCE is not loaded');
                  return '';
            }
            
            const editor = tinymce.get(id);
            if (!editor) {
                  console.error(`Editor with id "${id}" not found`);
                  return '';
            }
            
            return editor.getContent();
      } catch (error) {
            console.error('Error getting TinyMCE content:', error);
            return '';
      }
};

window.setTinyMCEContent = (id, content) => {
      try {
            if (typeof tinymce === 'undefined') {
                  return;
            }

            const editor = tinymce.get(id);
            if (!editor) {
                  return;
            }

            editor.setContent(content || '');
      } catch (error) {
            console.error('Error setting TinyMCE content:', error);
      }
};