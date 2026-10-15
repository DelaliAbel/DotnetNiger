# Corrections à faire

## Défaut bloquant

- [ ] **1. Attribuer le rôle administrateur échoue en 405.** Le tiroir Membres appelle
      `PUT /api/admin/accounts/{id}/roles`. Sans cela, personne ne peut devenir
      administrateur.

## Réglages du site

- [ ] **2. Paramètres doit piloter le site.** Aujourd'hui il ne fait que lire des fichiers.
      Il faut une table de réglages — logo, contacts, liens sociaux — sinon « changer le
      logo depuis Paramètres » est impossible.
- [ ] **3. Logo du site modifiable** depuis Paramètres, et repris partout : en-tête, pied
      de page, courriels.
- [ ] **4. WhatsApp remplace Discord** dans le pied de page et la navigation.
- [ ] **5. Un lien non renseigné ne s'affiche pas.** Si l'adresse est absente des réglages,
      l'entrée disparaît : pas de lien vide, pas d'icône orpheline.
- [ ] **6. Un lien qui n'existe pas n'est pas une erreur.** La validation accepte un lien
      mort ; son entretien revient à l'administrateur.

## Connexion externe

- [ ] **7. Tester la connexion par Google et par GitHub**, une fois les clés renseignées.

## Courriels

- [ ] **8. Modèles d'e-mail trop simples.** Reprendre la mise en page : identité du site,
      hiérarchie lisible, pied de page.
- [ ] **9. Le logo défini dans Paramètres apparaît dans les courriels.**
- [ ] **10. Chaque bouton est doublé du lien en clair.** Un lecteur qui n'affiche pas les
      images doit pouvoir agir quand même.

## Mise en page

- [ ] **11. Cartes de commentaires trop grandes en administration.** La carte doit tenir
      sur deux lignes de plus que le commentaire.
- [ ] **12. Cartes de messages trop grandes en administration.** Même cause.
- [ ] **13. Administration non adaptative.** Tableaux qui débordent sur téléphone, tiroir
      qui occupe tout l'écran, barre de filtres qui se casse.
- [ ] **14. Titres trop grands sur les fiches article et événement**, surtout sur mobile :
      trois lignes au plus, quelle que soit la longueur.

## Manque fonctionnel

- [ ] **15. Signalement d'un commentaire depuis le site public.** Le point d'exPRT
      `POST /api/reports` existe, mais aucun bouton ne l'appelle.