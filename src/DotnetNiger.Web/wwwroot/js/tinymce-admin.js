// tinymce-admin.js — éditeur de contenu riche de l'administration DotnetNiger.
// Le pack embarqué n'apporte que les plugins utilisés (code, link, lists, table) ;
// tout le reste est retiré de la graine livrée au navigateur.
window.DnTinymce = {
    // Initialise un éditeur TinyMCE sur le <textarea> identifié par « id ».
    // Attend patiemment que la graine soit chargée (jusqu'à cinq secondes), puis
    // renvoie true dès que l'éditeur est prêt, false si le délai est dépassé.
    async initialiser(id, contenuInitial, options) {
        if (window.tinymce === undefined) {
            const delai = 5000;
            const pas = 100;
            for (let essai = 0; essai < delai / pas; essai++) {
                await new Promise((resoudre) => setTimeout(resoudre, pas));
                if (window.tinymce !== undefined) {
                    break;
                }
            }
        }

        if (window.tinymce === undefined) {
            return false;
        }

        const opts = options || {};

        const editeur = window.tinymce.get(id);
        if (editeur) {
            editeur.setContent(contenuInitial || "");
            return true;
        }

        window.tinymce.init({
            selector: "#" + id,
            license_key: "gpl",
            plugins: "code link lists table",
            toolbar:
                "undo redo | blocks | bold italic underline strikethrough | " +
                "bullist numlist | blockquote | table dnImage link | code",
            menubar: "edit insert view table tools",
            height: opts.hauteur || 440,
            width: "100%",
            branding: false,
            promotion: false,
            setup(editeur) {
                // Le pack embarqué ne contient pas le plugin « image » : on enregistre donc
                // notre propre bouton. Il ouvre un sélecteur de fichiers, dépose l'image
                // auprès du serveur, et l'insère à la position du curseur. Aucune adresse
                // n'est saisie à la main — c'est le fichier qui devient l'image.
                editeur.ui.registry.addButton("dnImage", {
                    icon: "image",
                    tooltip: "Insérer une image",
                    onAction: () => window.DnTinymce.insereImage(id),
                });

                editeur.on("init", () => {
                    if (contenuInitial) {
                        editeur.setContent(contenuInitial);
                    }
                });
            }
        });

        return true;
    },

    // Dépose une image choisie puis l'insère dans l'éditeur.
    async insereImage(id) {
        const editeur = window.tinymce && window.tinymce.get(id);
        if (!editeur) {
            return;
        }

        const champ = document.createElement("input");
        champ.type = "file";
        champ.accept = "image/jpeg,image/png,image/webp";

        champ.addEventListener("change", async () => {
            const fichier = champ.files && champ.files[0];

            if (!fichier) {
                return;
            }

            editeur.windowManager.open({
                title: "Dépôt de l'image",
                body: { type: "panel", items: [{ type: "htmlpanel", html: "Téléversement en cours…" }] },
                buttons: [{ type: "cancel", text: "Annuler" }]
            });

            const donnees = new FormData();
            donnees.append("file", fichier, fichier.name);

            let url = null;

            try {
                const reponse = await fetch("/api/images", {
                    method: "POST",
                    credentials: "same-origin",
                    headers: { Accept: "application/json" },
                    body: donnees
                });

                if (reponse.ok) {
                    const lu = await reponse.json();
                    url = lu.url || null;
                } else {
                    const lu = await reponse.json().catch(() => null);
                    editeur.windowManager.alert((lu && lu.detail) || "Le dépôt a échoué.");
                }
            } catch (e) {
                editeur.windowManager.alert("Le serveur n'a pas répondu.");
            } finally {
                editeur.windowManager.close();
            }

            if (url) {
                editeur.insertContent('<img src="' + url + '" alt="" />');
            }
        });

        champ.click();
    },

    // Lit le corps courant de l'éditeur désigné.
    obtenir(id) {
        const editeur = window.tinymce && window.tinymce.get(id);
        return editeur ? editeur.getContent() : "";
    },

    // Insère du HTML à la position du curseur.
    inserer(id, html) {
        const editeur = window.tinymce && window.tinymce.get(id);
        if (editeur) {
            editeur.insertContent(html);
        }
    },

    // Détruit proprement l'éditeur, à la fermeture d'un formulaire.
    detruire(id) {
        const editeur = window.tinymce && window.tinymce.get(id);
        if (editeur) {
            editeur.remove();
        }
    }
};