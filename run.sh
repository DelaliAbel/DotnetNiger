#!/bin/bash

# Script pour lancer tous les services DotnetNiger
# Usage: 
#   ./run.sh              # Mode watch (défaut)
#   ./run.sh run          # Mode run simple
#   ./run.sh build        # Build uniquement
#   ./run.sh clean        # Nettoyage
#   ./run.sh dev          # Mode développement (watch + logs détaillés)

set -e

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Ports configurés
GATEWAY_PORT=5000
IDENTITY_PORT=5075
COMMUNITY_PORT=5269

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}   DotnetNiger - Gestion des services${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

# Déterminer le mode
MODE="watch"
VERBOSE=""
if [ "$1" == "run" ]; then
    MODE="run"
elif [ "$1" == "build" ]; then
    echo -e "${YELLOW}🔨 Build des projets...${NC}"
    dotnet build DotnetNiger.Gateway/DotnetNiger.Gateway.csproj
    dotnet build DotnetNiger.Identity/DotnetNiger.Identity.csproj
    dotnet build DotnetNiger.Community/DotnetNiger.Community.csproj
    echo -e "${GREEN}✓ Build terminé${NC}"
    exit 0
elif [ "$1" == "clean" ]; then
    echo -e "${YELLOW}🧹 Nettoyage des projets...${NC}"
    dotnet clean DotnetNiger.Gateway/DotnetNiger.Gateway.csproj
    dotnet clean DotnetNiger.Identity/DotnetNiger.Identity.csproj
    dotnet clean DotnetNiger.Community/DotnetNiger.Community.csproj
    echo -e "${GREEN}✓ Nettoyage terminé${NC}"
    exit 0
elif [ "$1" == "dev" ]; then
    MODE="watch"
    VERBOSE="--verbosity detailed"
fi

# Déterminer la commande
if [ "$MODE" == "run" ]; then
    DOTNET_CMD="dotnet run"
    MODE_LABEL="Run"
else
    DOTNET_CMD="dotnet watch run"
    MODE_LABEL="Watch"
fi

# Clear screen
clear

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}   Démarrage des services - Mode: $MODE_LABEL${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""
echo -e "${GREEN}🚀 Gateway    :${NC} http://localhost:$GATEWAY_PORT"
echo -e "${GREEN}🔐 Identity   :${NC} http://localhost:$IDENTITY_PORT"
echo -e "${GREEN}👥 Community  :${NC} http://localhost:$COMMUNITY_PORT"
echo ""
echo -e "${YELLOW}💡 Appuyez sur Ctrl+C pour arrêter tous les services${NC}"
echo ""

# Créer un fichier temporaire pour les PIDs
PIDS_FILE="/tmp/dotnetniger_pids.txt"
> "$PIDS_FILE"

# Fonction pour nettoyer à la sortie
cleanup() {
    echo ""
    echo -e "${RED}🛑 Arrêt des services...${NC}"
    
    # Tuer tous les processus enfants
    if [ -f "$PIDS_FILE" ]; then
        while read pid; do
            if ps -p $pid > /dev/null 2>&1; then
                kill $pid 2>/dev/null || true
            fi
        done < "$PIDS_FILE"
        rm -f "$PIDS_FILE"
    fi
    
    # Tuer tous les processus dotnet watch
    pkill -f "dotnet watch" || true
    
    echo -e "${GREEN}✓ Services arrêtés${NC}"
    exit 0
}

# Capturer Ctrl+C
trap cleanup INT TERM EXIT

# Lancer Gateway
echo -e "${BLUE}[Gateway]${NC} Démarrage..."
(
    cd DotnetNiger.Gateway
    $DOTNET_CMD $VERBOSE --non-interactive 2>&1 | while IFS= read -r line; do
        echo -e "${BLUE}[Gateway]${NC} $line"
    done
) &
GATEWAY_PID=$!
echo $GATEWAY_PID >> "$PIDS_FILE"
sleep 3

# Lancer Identity
echo -e "${BLUE}[Identity]${NC} Démarrage..."
(
    cd DotnetNiger.Identity
    $DOTNET_CMD $VERBOSE --non-interactive 2>&1 | while IFS= read -r line; do
        echo -e "${GREEN}[Identity]${NC} $line"
    done
) &
IDENTITY_PID=$!
echo $IDENTITY_PID >> "$PIDS_FILE"
sleep 3

# Lancer Community
echo -e "${BLUE}[Community]${NC} Démarrage..."
(
    cd DotnetNiger.Community
    $DOTNET_CMD $VERBOSE --non-interactive 2>&1 | while IFS= read -r line; do
        echo -e "${YELLOW}[Community]${NC} $line"
    done
) &
COMMUNITY_PID=$!
echo $COMMUNITY_PID >> "$PIDS_FILE"

echo ""
echo -e "${GREEN}✓ Tous les services sont en cours de démarrage${NC}"
echo ""

# Attendre que tous les processus se terminent
wait
