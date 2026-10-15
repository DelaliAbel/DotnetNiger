// site.js — interactions légères du site public DotnetNiger.
window.DnTheme = {
    current() {
        return document.documentElement.classList.contains("dark") ? "dark" : "light";
    },

    set(theme) {
        document.documentElement.classList.toggle("dark", theme === "dark");
        document.documentElement.setAttribute("data-theme", theme);
        localStorage.setItem("dn-theme", theme);
        this.sync();
    },

    toggle() {
        this.set(this.current() === "dark" ? "light" : "dark");
    },

    // Réapplique le thème mémorisé sur le <html>. Appelé au chargement et après
    // chaque navigation enhanced de Blazor, qui régénère le document sans
    // ré-exécuter le script d'amorçage du <head>.
    applique() {
        let theme;
        try {
            theme = localStorage.getItem("dn-theme");
        } catch (e) { }
        if (theme !== "dark" && theme !== "light") {
            theme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
        }
        this.set(theme);
    },

    sync() {
        const estSombre = this.current() === "dark";
        document.querySelectorAll("[data-theme-toggle]").forEach((bouton) => {
            bouton.setAttribute("aria-pressed", String(estSombre));
            bouton.setAttribute(
                "aria-label",
                estSombre ? "Activer le mode clair" : "Activer le mode sombre"
            );
            const soleil = bouton.querySelector("[data-icone-soleil]");
            const lune = bouton.querySelector("[data-icone-lune]");
            if (soleil) soleil.style.display = estSombre ? "block" : "none";
            if (lune) lune.style.display = estSombre ? "none" : "block";
        });
    }
};

window.DnNav = {
    estTiroir() {
        const bouton = document.querySelector(".site-nav__toggle");
        if (!bouton) return false;
        return bouton.getBoundingClientRect().width > 0 && getComputedStyle(bouton).display !== "none";
    },

    verrouilleCorps(on) {
        document.body.classList.toggle("nav-ouverte", on);
    },

    ouvre() {
        const nav = document.getElementById("site-nav");
        const fond = document.querySelector("[data-nav-backdrop]");
        if (!nav || nav.classList.contains("is-open")) return;
        nav.classList.add("is-open");
        nav.setAttribute("aria-hidden", "false");
        nav.removeAttribute("inert");
        const bouton = document.querySelector(".site-nav__toggle");
        if (bouton) bouton.setAttribute("aria-expanded", "true");
        if (fond) fond.classList.add("is-visible");
        this.verrouilleCorps(true);
        // La transition du tiroir (250 ms) retient le panneau en visibility:hidden :
        // on attend qu'elle soit terminée pour placer le focus sur le bouton de fermeture.
        const fermeture = nav.querySelector("[data-nav-close]");
        if (fermeture) setTimeout(() => fermeture.focus(), 280);
    },

    ferme() {
        const nav = document.getElementById("site-nav");
        const fond = document.querySelector("[data-nav-backdrop]");
        if (!nav || !nav.classList.contains("is-open")) return;
        const focusDansNav = nav.contains(document.activeElement);
        nav.classList.remove("is-open");
        nav.setAttribute("aria-hidden", "true");
        nav.setAttribute("inert", "");
        const bouton = document.querySelector(".site-nav__toggle");
        if (bouton) bouton.setAttribute("aria-expanded", "false");
        if (fond) fond.classList.remove("is-visible");
        this.verrouilleCorps(false);
        if (focusDansNav && bouton) bouton.focus();
    }
};

window.DnAdminReplie = {
    // Le repli est une classe sur <html>, comme le thème : elle est donc appliquée avant
    // le premier rendu de l'administration et survit à la navigation enhanced de Blazor,
    // qui régénère le document sans rejouer le script d'amorçage.
    applique() {
        let replie = false;

        try {
            replie = localStorage.getItem("dn-admin-replie") === "1";
        } catch {
            // Stockage indisponible : la barre reste dépliée pour cette session.
        }

        document.documentElement.classList.toggle("admin-replie", replie);
        this.majBouton(replie);
    },

    bascule() {
        const replie = !document.documentElement.classList.contains("admin-replie");

        document.documentElement.classList.toggle("admin-replie", replie);

        try {
            localStorage.setItem("dn-admin-replie", replie ? "1" : "0");
        } catch {
            // Stockage indisponible : le repli reste valable pour cette session.
        }

        this.majBouton(replie);
    },

    majBouton(replie) {
        const bouton = document.querySelector("[data-admin-replie]");
        if (bouton) {
            bouton.setAttribute("aria-expanded", replie ? "false" : "true");
        }
    }
};

window.DnAdminTiroir = {
    // En dessous de 64 rem la barre latérale sort du flux et devient un tiroir, comme
    // la navigation du site public : même mécanisme, mêmes classes, mêmes sorties.
    barre() {
        return document.getElementById("admin-barre");
    },

    estOuvert() {
        const barre = this.barre();
        return !!barre && barre.classList.contains("est-ouvert");
    },

    ouvre() {
        const barre = this.barre();
        if (!barre) return;
        barre.classList.add("est-ouvert");
        this.majEtat(true);
    },

    ferme() {
        const barre = this.barre();
        if (!barre) return;
        barre.classList.remove("est-ouvert");
        this.majEtat(false);
    },

    bascule() {
        this.estOuvert() ? this.ferme() : this.ouvre();
    },

    majEtat(ouvert) {
        const bouton = document.querySelector("[data-admin-tiroir]");
        if (bouton) {
            bouton.setAttribute("aria-expanded", ouvert ? "true" : "false");
        }
        const voile = document.querySelector("[data-admin-voile]");
        if (voile) {
            voile.classList.toggle("est-visible", ouvert);
        }
        document.body.classList.toggle("admin-tiroir-ouvert", ouvert);
    }
};

// Un tiroir laissé ouvert cache la page derrière lui au retour du clavier : il se
// referme avec Échap, comme celui du site public.
function armeTiroirAdmin() {
    const voile = document.querySelector("[data-admin-voile]");
    if (voile && !voile.dataset.arme) {
        voile.dataset.arme = "1";
        voile.addEventListener("click", () => window.DnAdminTiroir.ferme());
    }

    const barre = document.getElementById("admin-barre");
    if (barre && !barre.dataset.arme) {
        barre.dataset.arme = "1";
        // Un clic sur une entrée ferme le tiroir : la navigation enhanced recharge la
        // page, mais le tiroir doit être refermé dans tous les cas.
        barre.querySelectorAll("a").forEach((lien) => {
            lien.addEventListener("click", () => window.DnAdminTiroir.ferme());
        });
    }

    window.DnAdminTiroir.majEtat(window.DnAdminTiroir.estOuvert());
}

window.DnAdminTiroir.suitLargeur = window.matchMedia("(min-width: 64rem)");

// Revenir à la largeur large referme le tiroir : sans cela, le corps resterait
// verrouillé alors que la barre est de nouveau dans le flux.
window.DnAdminTiroir.suitLargeur.addEventListener("change", (evenement) => {
    if (evenement.matches) {
        window.DnAdminTiroir.ferme();
    }
});

document.addEventListener("DOMContentLoaded", () => {
    window.DnTheme.applique();
    window.DnAdminReplie.applique();
    armeNavigation();
    armeTiroirAdmin();
    fermeProfil();
    armeProfil();
    armeLiensProfil();
    armeNotifications();

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            window.DnNav.ferme();
            window.DnAdminTiroir.ferme();
            fermeProfil();
        }
    });

    // La navigation enhanced de Blazor régénère le document sans ré-exécuter le
    // script d'amorçage du <head> ni DOMContentLoaded : on réapplique le thème et
    // on rattache les écouteurs perdus à chaque page suivante.
    if (window.Blazor?.addEventListener) {
        window.Blazor.addEventListener("enhancedload", () => {
            window.DnTheme.applique();
            window.DnAdminReplie.applique();
            armeNavigation();
            armeTiroirAdmin();
            fermeProfil();
            armeProfil();
            armeLiensProfil();
            armeNotifications();
        });
    }
});

// Menu de profil : `<details>` donne l'ouverture sans script, mais rien ne le
// referme. Sans ces deux sorties, le menu reste ouvert après un clic dans la
// page — et la navigation enhanced le laisse ouvert sur la page suivante, ce qui
// donne l'impression que l'en-tête est cassé. Les deux entrées du menu
// (liens, déconnexion) referment déjà `details` implicitement en validity.
function fermeProfil() {
    document.querySelectorAll("details.profil[open]").forEach((menu) => menu.removeAttribute("open"));
}

// Marqueurs d'écouteurs déjà posés.
//
// Ils vivent sur `window`, pas sur `<html>`. La navigation enhanced de Blazor remplace
// l'élément `<html>` : un marqueur posé dessus repart à zéro à chaque page, et le script
// rattache un écouteur supplémentaire à chaque visite. Après trois pages, un clic
// déclenchait trois fois l'action — c'est ainsi qu'un unique « Ajouter un lien » en
// créait trois ou quatre. `window`, lui, survit au remplacement.
function dejaArme(nom) {
    if (!window.__dnArmes) window.__dnArmes = {};
    if (window.__dnArmes[nom]) return true;
    window.__dnArmes[nom] = true;
    return false;
}

function armeProfil() {
    // Un seul écouteur sur le document : le menu n'existe qu'après la connexion,
    // et il est régénéré à chaque navigation enhanced.
    if (dejaArme("profil")) return;

    document.addEventListener("click", (event) => {
        document.querySelectorAll("details.profil[open]").forEach((menu) => {
            if (!menu.contains(event.target)) menu.removeAttribute("open");
        });
    });
}

// Liens multiples du profil : le formulaire reste un POST HTML normal, et cette
// petite amélioration progressive ne fait qu'ajouter ou retirer des lignes. Le
// serveur reçoit toujours `Liens[index].Label` et `Liens[index].Url` contigus.
function armeLiensProfil() {
    if (dejaArme("liensProfil")) return;

    document.addEventListener("click", (event) => {
        const ajouter = event.target.closest("[data-profil-lien-ajouter]");
        if (ajouter) {
            const conteneur = document.querySelector("[data-profil-liens]");
            const modele = document.getElementById("profil-lien-modele");
            if (!conteneur || !modele || conteneur.children.length >= 12) return;

            const ligne = modele.content.firstElementChild.cloneNode(true);
            conteneur.appendChild(ligne);
            renumeroteLiensProfil(conteneur);
            ligne.querySelector("input")?.focus();
            return;
        }

        const retirer = event.target.closest("[data-profil-lien-retirer]");
        if (!retirer) return;

        const conteneur = retirer.closest("[data-profil-liens]");
        retirer.closest("[data-profil-lien]")?.remove();
        if (conteneur && conteneur.children.length === 0) {
            const modele = document.getElementById("profil-lien-modele");
            if (modele) conteneur.appendChild(modele.content.firstElementChild.cloneNode(true));
        }
        if (conteneur) renumeroteLiensProfil(conteneur);
    });
}

function renumeroteLiensProfil(conteneur) {
    // Le gabarit est marqué d'un attribut, pas seulement par son texte : le nom du champ
    // y contient `__index__`, un texte qui ressemble à un index mais n'en est pas un.
    // Sans cette distinction, le remplacement visait `Liens[__index__]` — il ne changeait
    // donc rien, et le serveur recevait tous les champs sous le même index.
    Array.from(conteneur.children).forEach((ligne, index) => {
        ligne.querySelectorAll("input[name]").forEach((champ) => {
            champ.name = champ.name.replace(/Liens\[(?:__index__|\d+)\]/, `Liens[${index}]`);
        });
    });
}

// Les messages de redirection (confirmation/erreur) sont des retours ponctuels, pas des
// bandeaux permanents. Ils disparaissent après lecture, y compris après une navigation
// enhanced où Blazor remplace la page sans rejouer DOMContentLoaded.
function armeNotifications() {
    document.querySelectorAll(
        ".alerte, p.rounded-sm.bg-vert-100, p.rounded-sm.bg-orange-100"
    ).forEach((message) => {
        if (message.dataset.autoMasque === "1") return;
        message.dataset.autoMasque = "1";

        window.setTimeout(() => {
            if (!message.isConnected) return;
            message.style.transition = "opacity 250ms ease";
            message.style.opacity = "0";
            window.setTimeout(() => message.remove(), 260);
        }, 7000);
    });
}

function armeNavigation() {
    const nav = document.getElementById("site-nav");
    if (!nav) return;
    synchroniseNavigation(nav);
    nav.querySelectorAll("a").forEach((lien) =>
        lien.addEventListener("click", () => window.DnNav.ferme())
    );

    const fond = document.querySelector("[data-nav-backdrop]");
    fond?.addEventListener("click", () => window.DnNav.ferme());
}

// Aligne l'état du tiroir sur la largeur réelle de la fenêtre.
//
// `inert` rend les liens non cliquables, non focusables et invisibles pour les
// technologies d'assistance. Posé au chargement, il doit être retiré dès que la
// fenêtre s'élargit au-dessus du point de bascule : sinon la barre horizontale
// s'affiche bien mais ses liens restent inertes, et le redimensionnement de la
// fenêtre semble casser la navigation. Blazor ne rejouant ni DOMContentLoaded ni
// enhancedload sur un simple redimensionnement, l'écouteur est posé une seule fois
// et se sert des deux lignes ci-dessous pour se resynchroniser.
function synchroniseNavigation(nav) {
    if (window.DnNav.estTiroir()) {
        if (!nav.classList.contains("is-open")) {
            nav.setAttribute("aria-hidden", "true");
            nav.setAttribute("inert", "");
        }
    } else {
        // Au-dessus du point de bascule la navigation est affichée en ligne : plus
        // rien ne doit la bloquer. On referme aussi un tiroir resté ouvert, pour ne
        // pas laisser le corps verrouillé par une classe `nav-ouverte`.
        nav.classList.remove("is-open");
        nav.removeAttribute("inert");
        nav.removeAttribute("aria-hidden");
        const bouton = document.querySelector(".site-nav__toggle");
        if (bouton) bouton.setAttribute("aria-expanded", "false");
        document.querySelector("[data-nav-backdrop]")?.classList.remove("is-visible");
        window.DnNav.verrouilleCorps(false);
    }
}

let minuteurRedimensionnement = null;
window.addEventListener("resize", () => {
    // Le rafraichissement est regroupé : un redimensionnement à la souris génère
    // des centaines d'événements, et la resynchronisation ne sert qu'une fois
    // l'opération terminée.
    clearTimeout(minuteurRedimensionnement);
    minuteurRedimensionnement = setTimeout(() => {
        const nav = document.getElementById("site-nav");
        if (nav) synchroniseNavigation(nav);
    }, 150);
});
// Copie du lien de la page courante. Le presse-papiers n'est atteignable que depuis un
// geste de l'utilisateur, d'où l'écoute directe sur le bouton plutôt qu'une fonction
// appelée depuis Blazor. Le repli sur execCommand couvre les navigateurs anciens, qui
// refusent l'API moderne hors contexte sécurisé.
window.DnPartage = (function () {
    function copie(texte) {
        if (navigator.clipboard?.writeText) {
            return navigator.clipboard.writeText(texte);
        }

        const champ = document.createElement("textarea");
        champ.value = texte;
        champ.setAttribute("readonly", "");
        champ.style.position = "absolute";
        champ.style.left = "-9999px";
        document.body.appendChild(champ);
        champ.select();
        document.execCommand("copy");
        document.body.removeChild(champ);
        return Promise.resolve();
    }

    function confirme(bouton) {
        const titre = bouton.getAttribute("title");
        if (titre) {
            bouton.setAttribute("title", "Lien copié");
            window.setTimeout(() => bouton.setAttribute("title", titre), 2000);
        }
    }

    function installe() {
        document.querySelectorAll("[data-partage-copie]").forEach((bouton) => {
            if (bouton.dataset.partagePret) {
                return;
            }

            bouton.dataset.partagePret = "1";
            bouton.addEventListener("click", () => {
                copie(window.location.href).then(() => confirme(bouton));
            });
        });
    }

    return { installe: installe };
})();
