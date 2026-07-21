# TODO Frontend (Blazor WASM) — RBAC & UI

## Règles métier
- **SuperAdmin** : accès complet à tout
- **Admin** : gère tout (users, blog, events, projets, ressources, partenaires, équipe) **sauf Settings** (SuperAdmin uniquement)
- **Collaborateur** : accède à `/admin`, voit son dashboard (ses propres stats), gère son contenu (Mes Articles, Mes Événements, Mes Ressources, Mes Projets), commentaires, newsletter. Ses events doivent être approuvés par Admin/SuperAdmin avant publication.
- **User** : pas d'accès admin

---

### 1. Dashboard collaborateur
- [ ] Afficher uniquement les stats personnelles du collaborateur (ses événements, ses articles, ses ressources, ses projets) quand `IsCollaborator`
- [ ] Garder les stats globales pour Admin/SuperAdmin
- [ ] Ajouter une section "Mes soumissions en attente d'approbation" pour les events

### 2. Workflow approbation des événements
- [ ] Quand un collaborateur crée/modifie un événement, l'event doit être soumis pour approbation (IsPublished = false, en attente)
- [ ] Page Admin/Events doit montrer les events en attente (GetPendingEventsAsync)
- [ ] Boutons "Approuver" / "Rejeter" sur les events en attente (ApproveEventAsync / RejectEventAsync)
- [ ] Notifier le collaborateur quand son event est approuvé/rejeté

### 3. Cacher les actions Admin pour les collaborateurs
- [ ] Sur toutes les pages "full" (Blog, Events, etc.), cacher les boutons Créer/Modifier/Supprimer pour les collaborateurs (qui utilisent leurs pages "Mes" à la place)
- [ ] Sur les pages détails admin, cacher les actions de management si le user n'est pas Admin
- [x] Dans le TopBarProfileDropdown, l'entrée "Administration" doit aussi être visible pour les collaborateurs (vers `/admin`) — *déjà ok, RedirectToLogin redirige vers /admin*

### 4. Sidebar — améliorations
- [ ] Vérifier que les collaborateurs certifiés (IsCertificateValidated) voient bien toutes leurs sections "Mes"
- [ ] Ajouter badge ou indicateur "en attente" pour les events du collaborateur

### 5. Tests de vérification
- [ ] Se connecter en tant que Collaborateur : vérifier que seul le dashboard + "Mes" sections sont accessibles
- [ ] Se connecter en tant qu'Admin : vérifier accès à tout sauf Settings
- [ ] Se connecter en tant que SuperAdmin : vérifier accès complet
- [ ] Vérifier que taper une URL admin non autorisée redirige correctement (AuthorizeRouteView)
