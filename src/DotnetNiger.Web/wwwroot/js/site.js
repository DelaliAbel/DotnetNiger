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

document.addEventListener("DOMContentLoaded", () => {
    window.DnTheme.applique();
    armeNavigation();

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") window.DnNav.ferme();
    });

    // La navigation enhanced de Blazor régénère le document sans ré-exécuter le
    // script d'amorçage du <head> ni DOMContentLoaded : on réapplique le thème et
    // on rattache les écouteurs perdus à chaque page suivante.
    if (window.Blazor?.addEventListener) {
        window.Blazor.addEventListener("enhancedload", () => {
            window.DnTheme.applique();
            armeNavigation();
        });
    }
});

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