# Corrections à faire

## Défaut bloquant

- [ ] **1. Attribuer le rôle administrateur échoue en 405.** Le tiroir Membres appelle
      `PUT /api/admin/accounts/{id}/roles`. Sans cela, personne ne devenir
      administrateur. *L'API répond 200 en appel direct ; l'erreur ne se reproduit pas.
      Le message affiché nomme désormais l'opération et le code pour diagnostiquer au
      prochain essai.*

## Réglages du site

- [x] **2. Paramètres doit piloter le site.** Une table `ReglagesDuSite` porte le logo, les
      contacts et les canaux ; `ServiceReglages` les lit, `AdminSettingsController` les
      écrit. Migration `20261015143158_ReglagesDuSite`.
- [x] **3. Logo du site modifiable** depuis Paramètres, et repris partout : en-tête
      (`DnBrand`), pied de page, courriels (`EmailBranding`).
- [x] **4. WhatsApp remplace Discord** dans le pied de page, l'accueil, la page contact et
      la page communauté.
- [x] **5. Un lien non renseigné ne s'affiche pas.** `ServiceReglages.Canaux()` ne renvoie
      que les canaux renseignés ; la barre d'icônes disparaît entièrement si aucun ne l'est.
- [x] **6. Un lien qui n'existe pas n'est pas une erreur.** La garde du domaine ne contrôle
      que la forme de l'adresse — absolue, en http ou https — et ne va jamais la chercher sur
      le réseau. Une adresse morte reste affichée tant qu'elle est visée.

## Défaut de production

- [x] **16. Un recyclage de l'hébergeur déconnectait tout le monde.** Les clés de
      protection des données vivaient en mémoire : à chaque redémarrage, les cookies de
      connexion devenaient illisibles et les jetons antiforgery déjà émis déclenchaient une
      erreur 400 à corps vide. Le trousseau est désormais écrit en base
      (`DataProtectionKeys`, migration `TrousseauDeDonnees`) et survit au redémarrage.

## Connexion externe

- [ ] **7. Tester la connexion par Google et par GitHub**, une fois les clés renseignées.

## Courriels

- [x] **8. Modèles d'e-mail trop simples.** Bandeau logo + nom sur une ligne, filet violet
      en tête de carte, titre en 21 px, pied de page en quatre blocs.
- [x] **9. Le logo défini dans Paramètres apparaît dans les courriels.** `EmailBranding`
      porte le logo ; `RendreExemple` remplace le domaine fictif par le vrai, pour que
      l'aperçu montre ce que le membre recevra.
- [x] **10. Chaque bouton est doublé du lien en clair.**

## Mise en page

- [x] **11. Cartes de commentaires trop grandes en administration.** La carte tient sur deux
      lignes de plus que le commentaire.
- [x] **12. Cartes de messages trop grandes en administration.** Même cause.
- [x] **13. Administration non adaptative.** Tableaux qui débordent sur téléphone, tiroir qui
      n'occupe plus tout l'écran, barre de filtres qui tient.
- [x] **14. Titres trop grands sur les fiches article et événement** : `clamp()`, trois
      lignes au plus quelle que soit la longueur.

## Manque fonctionnel

- [x] **15. Signalement d'un commentaire depuis le site public.** Bouton « Signaler » replié,
      `POST /commentaires/signaler`, file de modération alimentée.