# Proposition — Prochaines actions

## Préambule
Chaque proposition ci-dessous décrit le **quoi**, le **pourquoi**, et le **risque**.
Tu valides ou rejettes point par point avant que je n'écrive la moindre ligne de code.

---

## 1. Créer les rôles SuperAdmin et Collaborator à l'initialisation du tenant

**Quoi :** Ajouter `SuperAdmin` et `Collaborator` à `TenantInitializationService.CreateRolesAsync()` (actuellement crée seulement `Admin` et `User`). Assigner `SuperAdmin` (et pas seulement `Admin`) au créateur du tenant.

**Pourquoi :** Les rôles `SuperAdmin` et `Collaborator` sont utilisés partout (contrôleurs backend, pages frontend) mais ne sont jamais créés en base. L'utilisateur qui crée un tenant doit être son SuperAdmin.

**Risque :** Aucun — création idempotente.

**Décision :** ✅ Accepté — faire ce changement backend.

---

## 2. Dashboard Collaborator (stats de base)

**Quoi :** Créer une UI de Dashboard pour Collaborator avec stats personnelles (nombre events, projets, etc.) au lieu de rediriger vers `/admin/profile`.

**Pourquoi :** Un collaborateur doit voir ses données rapidement.

**Risque :** Nécessite des endpoints d'agrégation côté Community.

**Décision :** ✅ Accepté — specs UI ajoutées dans `SPECS_FRONTEND.md`. Backend à voir si endpoints existent.

---

## 3. Nettoyer ClaimsTransformer + uniformiser DTOs/Models/Entities

**Quoi :** 
- Supprimer `ClaimsTransformer.cs` (plus de doublon `"role"`)
- Nettoyer les DTOs, Models et Entities qui traînent (incohérences de nommage, propriétés inutilisées, doublons)

**Pourquoi :** Uniformité des claims (plus que `ClaimTypes.Role`). Propreté du code : des DTOs/Models ont été modifiés en cours de route et certains vestiges existent.

**Risque :** Moyen — vérifier qu'aucun code ne dépend du claim `"role"` court.

**Décision :** ✅ Accepté — backend.

---

## 4. Harmoniser les politiques CORS

**Quoi :** Aligner les 3 services (Gateway, Identity, Community) sur la même politique CORS. Documenter les origines autorisées.

**Pourquoi :** Cohérence et sécurité. Actuellement Gateway utilise `AllowCredentials()` mais Identity/Community non. Si un jour on accède directement à Identity/Community, le CORS bloquera les requêtes avec credentials.

**Risque :** Faible.

**Décision :** ✅ Accepté — aligner Identity/Community sur Gateway avec `AllowCredentials()` et lister les origines.

---

## 5. Composant UI "Permission refusée" (frontend)

**Quoi :** Créer un petit composant/popup qui s'affiche quand l'utilisateur tente une action sans avoir la permission requise.

**Pourquoi :** Améliorer l'UX — au lieu d'un 403 silencieux ou d'une page blanche, l'utilisateur voit un message clair.

**Risque :** Faible.

**Décision :** ✅ Accepté — specs dans `SPECS_FRONTEND.md` pour le dev frontend.

---

## 6. Page admin de gestion des permissions par rôle

**Quoi :** Page Blazor dans `/admin/permissions` (ou dans Settings) accessible aux SuperAdmin uniquement. Liste les rôles et permet d'activer/désactiver chaque permission.

**Pourquoi :** Un SuperAdmin doit pouvoir gérer les permissions depuis l'interface.

**Risque :** Moyen.

**Décision :** ✅ Accepté — specs UI dans `SPECS_FRONTEND.md`. Backend : vérifier que les endpoints nécessaires existent.
