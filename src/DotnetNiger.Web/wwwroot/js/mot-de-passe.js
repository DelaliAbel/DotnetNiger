/*
    Affichage du contenu d'un champ de mot de passe.

    Le script ne fait que retourner l'état. Le passage d'une icône à l'autre est fait en CSS
    à partir de l'attribut `aria-pressed` : les icônes sont rendues par le serveur, et Blazor
    ne redessine pas un composant qu'un script a modifié — l'icône serait alors figée.
*/
window.DnMotDePasse = (function () {
    function bascule(bouton) {
        var champ = bouton.parentElement && bouton.parentElement.querySelector("input");

        if (!champ) {
            return;
        }

        var visible = champ.type === "text";

        champ.type = visible ? "password" : "text";

        // L'état est annoncé : un bouton dont le sens ne change pas est un bouton dont on ne
        // sait pas ce qu'il fait.
        bouton.setAttribute("aria-pressed", visible ? "false" : "true");
        bouton.setAttribute(
            "aria-label",
            visible ? "Afficher le mot de passe" : "Masquer le mot de passe");
        bouton.setAttribute("title", visible ? "Afficher" : "Masquer");
    }

    return { bascule: bascule };
})();