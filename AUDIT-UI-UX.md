# Audit UI / UX / disposition — site public DotnetNiger

Date : 10 novembre 2026
Périmètre : site public (`/`, `/blog`, `/evenements`, `/ressources`, `/a-propos`,
`/communaute`, `/contact`, `/partenaires`, `/not-found`), thème violet `.NET`,
sans dégradé. Audit fait sur Chrome DevTools (desktop 1440×900 et mobile 390×844),
snapshot d'accessibilité, Lighthouse et script d'audit (`/tmp/opencode/audit.js`).

## Ce qui a été fait dans cette passe

- Suppression de **tous** les dégradés de `theme.css` (16 déclarations) et
  régénération de `app.css` : 0 `linear-gradient` / `radial-gradient` /
  `conic-gradient` restant. Palette violet inchangée.
- Réparation de la page 404 serveur : un chemin inconnu renvoie maintenant `404`
  avec le corps HTML de `/not-found` (et non plus une réponse vide). Cause :
  `UseStatusCodePagesWithReExecute` était placé après le routage implicite de
  `WebApplication`. Correction : `app.UseRouting()` explicite après le middleware
  de ré-exécution.
- Ajout de la page de maintenance (`/maintenance`) et de son interrupteur dans
  **Administration → Paramètres → Disponibilité** (`site.maintenance`).
  Quand elle est active, le public voit la page de maintenance avec un `503` et
  `Retry-After` ; l'administration, la connexion, l'API et les fichiers statiques
  restent accessibles.
- Build : 0 warning, 0 erreur. Tests domaine : 24/24 verts.

## Résultats Lighthouse (page `/contact`, desktop)

| Axe | Score |
|---|---|
| Accessibilité | 96 |
| Bonnes pratiques | 100 |
| SEO | 100 |
| Navigation agentique | 96 |

CLS mesuré : **0,093** (à surveiller, seuil « bon » = 0,1).

## Constats

### P1 — Contrastes insuffisants (WCAG 1.4.3)

| Élément | Couleur | Fond | Ratio | Seuil |
|---|---|---|---|---|
| `.site-brand__accent` (« net ») | violet-500 `#512bd4` | fond sombre `#17171d` | 2,25 | 4,5 |
| `.site-footer__title` (h2) | gris `#5a5a64` | fond sombre | 2,62 | 4,5 |
| Paragraphe copyright | gris `#5a5a64` | fond sombre | 2,62 | 4,5 |
| Liens légaux du pied de page | gris `#7c7c87` | fond sombre | 4,33 | 4,5 |

Note : le label `sr-only` signalé à 1,35 est masqué visuellement ; pas d'action.

**Action** : en mode sombre, remplacer `violet-500` par `violet-200`/`violet-300`
pour l'accent de la marque, et passer les titres et textes du pied de page à
`violet-200` ou un gris plus clair (`#a1a1aa` minimum). Conserver la palette.

### P1 — Cibles tactiles sous 44 px (WCAG 2.5.5)

- Liens de navigation principale : 36 px de haut (`site-nav__link`).
- Bouton de bascule de thème : 32×32 px (`theme-toggle`).
- Liens du pied de page : 17 px de haut (« À propos », « Ressources », etc.).
- Bouton « Aller au contenu » (skip link) : 25 px.

**Action** : porter la hauteur minimale des liens de nav et des liens du pied de
page à 44 px (padding vertical ou `min-height`), et agrandir le `theme-toggle`
à 44×44 px. Le skip link est hors écran par défaut ; lui donner une cible
conforme lorsqu'il est visible.

### P1 — `aria-label` incohérent avec le texte visible

`a.site-brand` porte `aria-label="DotnetNiger — retour à l'accueil"` alors que
le texte visible est « DotnetNiger ». Lighthouse signale
`label-content-name-mismatch`.

**Action** : soit retirer l'`aria-label` (le texte du lien suffit), soit le
réduire à « DotnetNiger » et déplacer l'indication « retour à l'accueil » dans
un `title` ou un texte masqué.

### P2 — Décalage de mise en page (CLS 0,093)

Probablement les images sans dimensions explicites et le chargement de la police
Inter. L'image du héros (`/images/img4.jpeg`) est rendue à 577×547 px sans
attributs `width`/`height` ni `aspect-ratio` stabilisant l'espace.

**Action** : ajouter `width`/`height` (ou `aspect-ratio`) sur les images du
héros et des cartes, et réserver la place des blocs dynamiques. Vérifier avec
Lighthouse après correction.

### P2 — Disposition et rythme vertical

- L'échelle `--spacing` est utilisée de façon cohérente ; pas de débordement
  horizontal détecté (`docOverflow: 0` sur toutes les pages).
- Le débordement signalé par le script est le skip link hors écran (`left: -9999`),
  pas un vrai problème de mise en page.
- 4 éléments `<nav>` par page (en-tête, tiroir mobile, pied de page ×2) : vérifier
  que chacun porte un `aria-label` distinct pour éviter la confusion pour les
  lecteurs d'écran.
- Le tiroir mobile repose sur des attributs `data-nav-*` et du JavaScript ; vérifier
  le piège de focus et la fermeture au clavier (Échap) — non confirmé par l'audit
  automatisé.

### P2 — Responsive

- Les tableaux d'administration passent en cartes sur mobile (déjà traité).
- Les grilles du site passent en colonne sous 40 rem ; pas de débordement.
- À vérifier manuellement dans Firefox : le tiroir de navigation, la barre de
  recherche du blog et les formulaires de contact sur 320 px.

### P3 — SEO / métadonnées

- `noindex` présent sur `/not-found` et `/maintenance` : bon.
- Vérifier que `/maintenance` renvoie bien `503` (confirmé : oui, avec
  `Retry-After: 300`).
- Vérifier l'absence de liens morts dans le pied de page quand un réglage est
  vide (comportement prévu : l'entrée disparaît).

## Ce qui reste à valider par l'utilisateur

1. Ouvrir `http://127.0.0.1:5228/` dans Firefox et vérifier l'absence totale de
   dégradé sur toutes les pages.
2. Tester la bascule de maintenance : Administration → Paramètres → Disponibilité,
   cocher, enregistrer, puis visiter le site en navigation privée (page de
   maintenance attendue, `503`), puis décocher et rouvrir.
3. Vérifier la page 404 : `http://127.0.0.1:5228/page-qui-nexiste-pas`.
4. Décider si les correctifs P1 (contrastes, cibles tactiles, aria-label) sont
   à traiter dans une passe suivante.

## Notes techniques

- Base de données locale : SQLite `/tmp/opencode/dn-audit.db` (EnsureCreated).
- Serveur : `http://127.0.0.1:5228` (PID du process `dotnet run`).
- Aucun commit ni push n'a été fait ; l'arbre reste modifié pour validation.
