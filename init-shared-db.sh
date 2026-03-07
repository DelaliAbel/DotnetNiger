#!/bin/bash
#
# DotnetNiger - Initialiser la base de données partagée (SQLite)
# Synchronise les migrations d'Identity et Community vers une seule BD
#
# Usage: ./init-shared-db.sh
#

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
IDENTITY_PATH="$SCRIPT_DIR/DotnetNiger.Identity"
COMMUNITY_PATH="$SCRIPT_DIR/DotnetNiger.Community"

echo -e "${CYAN}🗄️  Initialisation de la base de données partagée (DotnetNiger.db)${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

# Vérifier dotnet
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ Erreur: .NET SDK non trouvé. Veuillez installer le .NET SDK.${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✅ .NET SDK détecté: $DOTNET_VERSION${NC}"
echo ""

# ÉTAPE 1: Migrations Identity
echo -e "${YELLOW}📋 Étape 1: Appliquer les migrations d'Identity${NC}"
echo -e "${YELLOW}================================================${NC}"

pushd "$IDENTITY_PATH" > /dev/null
echo -e "${GRAY}📍 Localisation: $IDENTITY_PATH${NC}"
echo -e "⏳ Exécution de dotnet ef database update..."

if dotnet ef database update; then
    echo -e "${GREEN}✅ Identity migrations appliquées avec succès!${NC}"
else
    echo -e "${YELLOW}⚠️  Erreur lors de l'application des migrations Identity${NC}"
fi

popd > /dev/null

echo ""

# ÉTAPE 2: Migrations Community
echo -e "${YELLOW}📋 Étape 2: Appliquer les migrations de Community${NC}"
echo -e "${YELLOW}================================================${NC}"

pushd "$COMMUNITY_PATH" > /dev/null
echo -e "${GRAY}📍 Localisation: $COMMUNITY_PATH${NC}"
echo -e "⏳ Exécution de dotnet ef database update..."

if dotnet ef database update; then
    echo -e "${GREEN}✅ Community migrations appliquées avec succès!${NC}"
else
    echo -e "${YELLOW}⚠️  Erreur lors de l'application des migrations Community${NC}"
fi

popd > /dev/null

echo ""
echo -e "${CYAN}================================================${NC}"
echo -e "${GREEN}✅ Initialisation terminée!${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

# Vérifier existence BD
DB_PATH="$SCRIPT_DIR/DotnetNiger.db"
if [ -f "$DB_PATH" ]; then
    DB_SIZE=$(du -h "$DB_PATH" | cut -f1)
    echo -e "${CYAN}📁 Fichier BD créé: $DB_PATH${NC}"
    echo -e "${GRAY}📊 Taille: $DB_SIZE${NC}"
else
    echo -e "${YELLOW}⚠️  Fichier BD non trouvé à: $DB_PATH${NC}"
fi

echo ""
echo -e "${CYAN}📝 Prochaines étapes:${NC}"
echo -e "${GRAY}   • Exécuter Identity:   cd DotnetNiger.Identity && dotnet run${NC}"
echo -e "${GRAY}   • Exécuter Community:  cd DotnetNiger.Community && dotnet run${NC}"
echo -e "${GRAY}   • Tester l'API avec:   http://localhost:5000 (Gateway)${NC}"
