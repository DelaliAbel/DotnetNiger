// site.js — interactions légères du site public DotnetNiger.
window.DnTheme = {
    current() {
        return document.documentElement.classList.contains("dark") ? "dark" : "light";
    },

    set(theme) {
        document.documentElement.classList.toggle("dark", theme === "dark");
        localStorage.setItem("dn-theme", theme);
        this.sync();
    },

    toggle() {
        this.set(this.current() === "dark" ? "light" : "dark");
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
    window.DnTheme.sync();
    const nav = document.getElementById("site-nav");
    if (!nav) return;
    if (window.DnNav.estTiroir()) {
        nav.setAttribute("aria-hidden", "true");
        nav.setAttribute("inert", "");
    }

    nav.querySelectorAll("a").forEach((lien) =>
        lien.addEventListener("click", () => window.DnNav.ferme())
    );

    const fond = document.querySelector("[data-nav-backdrop]");
    fond?.addEventListener("click", () => window.DnNav.ferme());

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") window.DnNav.ferme();
    });
});