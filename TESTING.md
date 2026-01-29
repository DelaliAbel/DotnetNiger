# 🧪 TESTING Guide

Guide de test pour DotnetNiger.

---

## 🎯 Vue d'ensemble

DotnetNiger utilise une stratégie de test multi-niveaux:

- **Unit Tests** - Logique métier isolée
- **Integration Tests** - Interaction entre composants
- **End-to-End Tests** - Flux utilisateur complets

---

## ✅ Quick Start - Lancer les Tests

```bash
# Tous les tests
dotnet test

# Avec couverture
dotnet test /p:CollectCoverageMetrics=true

# Spécifique projet
dotnet test DotnetNiger.Identity.Tests
```

---

## 📊 Structure des Tests

```
DotnetNiger.Gateway.Tests/
├── Unit/
├── Integration/
└── EndToEnd/

DotnetNiger.Identity.Tests/
├── Unit/
├── Integration/
└── EndToEnd/

DotnetNiger.Community.Tests/
├── Unit/
├── Integration/
└── EndToEnd/
```

---

## 🔧 Frameworks utilisés

- **xUnit** - Test runner
- **Moq** - Mocking
- **FluentAssertions** - Assertions fluentes
- **TestContainers** - Conteneurs pour tests

---

## 📈 Coverage Target

- **Minimum:** 70%
- **Target:** 80%+
- **Critical paths:** 100%

Vérifier la couverture:
```bash
dotnet test /p:CollectCoverage=true
```

---

## 🚀 CI/CD Integration

Les tests tournent automatiquement sur:
- ✅ Chaque push
- ✅ Chaque pull request
- ✅ Avant merge en main

---

## 📚 Documentation Complète

Pour plus de détails techniques et avancés, consultez la documentation interne (disponible pour les contributeurs actifs du projet).

---

## ❓ Questions?

- Consulter [`/FAQ.md`](./FAQ.md) pour des questions générales
