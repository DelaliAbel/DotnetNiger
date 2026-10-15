// api-admin.js — appels aux points d'extermination de l'API depuis le navigateur.
// Les écritures de l'administration passent par l'API existante, et non par une
// nouvelle route : la session du visiteur, portée par son cookie, est transmise
// telle quelle, comme le serait un appel de page classique.
window.DnApi = {
    // Envoie une requête JSON. Renvoie { ok, statut, corps } où « corps » est le JSON
    // décodé de la réponse (ou son texte brut si elle n'est pas du JSON).
    async envoyer(methode, chemin, corps) {
        const options = {
            method: methode,
            credentials: "same-origin",
            headers: { Accept: "application/json" }
        };

        if (corps !== null && corps !== undefined) {
            options.headers["Content-Type"] = "application/json";
            options.body = JSON.stringify(corps);
        }

        return await window.DnApi.repond(await window.DnApi.appelle(chemin, options));
    },

    // Dépose un fichier auprès d'un point d'extermination multipart. Le champ « fichier »
    // est l'élément <input type="file"> : le fichier choisi est lu ici plutôt que
    // transporté par Blazor, pour que la requête parte du navigateur avec le cookie de
    // session — exactement comme le ferait un formulaire de la page.
    async televerseFichier(chemin, nomChamp, element) {
        const choisi = element && element.files && element.files[0];

        if (!choisi) {
            return { ok: false, statut: 0, corps: { title: "Aucun fichier n'a été choisi." } };
        }

        const donnees = new FormData();
        donnees.append(nomChamp, choisi, choisi.name);

        const options = {
            method: "POST",
            // Ni Content-Type ni boundary : le navigateur les déduit de la frontière.
            // Les fixer à la main produirait un corps que le serveur refuse de lire.
            credentials: "same-origin",
            headers: { Accept: "application/json" },
            body: donnees
        };

        return await window.DnApi.repond(await window.DnApi.appelle(chemin, options));
    },

    // Exécute l'appel, sans distinction de réussite : une coupure réseau doit se
    // traduire par la même forme de réponse qu'un refus du serveur, sinon chaque
    // appelant devrait traiter deux erreurs différentes.
    async appelle(chemin, options) {
        try {
            return await fetch(chemin, options);
        } catch (e) {
            return null;
        }
    },

    // Normalise une réponse en { ok, statut, corps }.
    async repond(reponse) {
        if (reponse === null) {
            return { ok: false, statut: 0, corps: { title: "Le serveur n'a pas répondu." } };
        }

        const texte = await reponse.text();

        let corps = null;
        if (texte) {
            try {
                corps = JSON.parse(texte);
            } catch (e) {
                corps = texte;
            }
        }

        return { ok: reponse.ok, statut: reponse.status, corps };
    }
};