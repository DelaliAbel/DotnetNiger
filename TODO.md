# Corrections à faire

## Défaut bloquant

- [x] **1. Attribuer le rôle administrateur échoue en 405.** *Cause trouvée.* Le middleware
      `UseStatusCodePagesWithReExecute` rejouait toute requête en échec vers `/not-found` **en
      conservant la méthode** : un `PUT` refusé à 403 revenait donc en `PUT /not-found`, auquel
      Blazor répond 405. L'erreur réelle disparaissait derrière « méthode non autorisée ».
      La ré-exécution est désormais écartée de l'API.

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

## Ergonomie de l'administration

- [x] **17. Le repli de la barre cassait l'administration et le site public.** Le nom de classe
      `admin-replie` servait au bouton *et* à `<html>` : une règle d'affichage du bouton
      s'appliquait au document entier. La classe d'état est devenue `admin-barre-replie`.
- [x] **18. La barre latérale défilait avec la page.** Elle est fixe au défilement.
- [x] **19. Suppression de compte sans déconnexion.** La redirection ne révoquait pas le
      cookie : la personne restait « connectée » à un compte inexistant.
- [x] **20. Signaler son propre commentaire.** Refusé par le domaine, et le bouton disparaît.
- [x] **21. Déconnexion absente de la barre.** Ajoutée en bas, avec son icône.
- [x] **22. Cartes KPI sans issue.** Chacune mène à son écran.
- [x] **23. Pas d'action rapide.** Six cartes, tiroir de création ouvert d'office.
- [x] **24. Bloc serveur dans Paramètres.** Retiré : il affichait en clair le relais SMTP et
      l'adresse du compte expéditeur, sans rien permettre de changer.
- [x] **25. Pas de numéro de téléphone.** Réglage ajouté, affiché en pied de page.

## Contenu et présentation

- [x] **26. Fiche d'équipe.** Une case à cocher et une fonction : la photo et les liens
      viennent du profil du membre, que lui seul renseigne.
- [x] **27. Couverture de blog qui déborde.** L'image est posée en absolu ; un
      `height: 100 %` ne se résout pas quand la hauteur vient d'un `aspect-ratio`.
- [x] **28. Chapô affiché sur la fiche.** Le chapô sert aux cartes, au référencement et au
      partage ; le corps commence la fiche.
- [x] **29. Pas d'aperçu à l'image sur les partages.** `og:image` était en adresse relative,
      qu'aucun réseau social ne sait résoudre. Composant `DnSeo` : adresses absolues,
      canonical par page, `twitter:card`.
- [x] **30. Pas de plan du site.** `sitemap.xml` généré depuis la base, `robots.txt` qui
      exclut l'administration, l'espace membre et l'API.
- [x] **31. Favicon indépendant du logo.** Il suit désormais le logo des réglages.
- [x] **32. Pas d'interface pour répondre aux messages.** Tiroir d rédaction, envoi par le
      relais du site, message marqué traité après acceptation seulement.
- [x] **33. Boutons de connexion externe sans logo et trop bas.** Logos Google et GitHub,
      replacés au-dessus du formulaire.
- [x] **34. Écritures PUT rejetées en 405.** *Cause trouvée* : la ré-exécution d'erreur
      rejouait la requête vers `/not-found` en conservant la méthode, transformant tout 403
      ou 400 d'API en 405. Écartée de l'API.

## Connexion externe

- [ ] **7. Tester la connexion par Google et par GitHub**, une fois les clés renseignées.
      `ExternalProviders__Providers__{Google,GitHub}__ClientId` et `__ClientSecret`.
      Les adresses de retour sont `/connexion/google/retour` et `/connexion/github/retour`.