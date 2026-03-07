#!/bin/bash
#
# DotnetNiger - Démarrer tous les services (Gateway, Identity, Community)
# Lance les 3 services dans des processus séparés
#
# Usage: ./start-all-services.sh
#

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
DARKGRAY='\033[0;90m'
NC='\033[0m' # No Color

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo -e "${CYAN}🚀 DotnetNiger - Démarrage de tous les services${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

# Configuration des services
declare -A services=(
    [Gateway]="DotnetNiger.Gateway|5000"
    [Identity]="DotnetNiger.Identity|5001"
    [Community]="DotnetNiger.Community|5002"
)

echo -e "${YELLOW}📋 Services à démarrer:${NC}"
for service in "${!services[@]}"; do
    IFS='|' read -r path port <<< "${services[$service]}"
    echo -e "   • ${GREEN}$service${NC}: Service sur port $port"
    echo -e "     ${DARKGRAY}Path: $SCRIPT_DIR/$path${NC}"
done

echo ""
read -p "Continuer ? (O/N): " response
if [[ ! "$response" =~ ^[Oo]$ ]]; then
    echo -e "${RED}❌ Annulation.${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}🔄 Arrêt des processus DotnetNiger existants...${NC}"

# Arrêter les processus existants
if pgrep -f "dotnet run" > /dev/null; then
    pkill -f "dotnet run" || true
    sleep 2
fi

echo ""
echo -e "${GREEN}✅ Démarrage des services...${NC}"
echo ""

# Lancer les services en background
for service in "${!services[@]}"; do
    IFS='|' read -r path port <<< "${services[$service]}"
    service_path="$SCRIPT_DIR/$path"
    
    echo -e "${GREEN}🟢 Démarrage $service (port $port)...${NC}"
    
    (
        cd "$service_path"
        echo "=== DotnetNiger - $service ===" 
        echo "Port: $port"
        echo "URL: http://localhost:$port"
        echo ""
        dotnet run
    ) &
    
    echo -e "   ${GREEN}✅ Lancé (PID: $!)${NC}"
    sleep 2
done

echo ""
echo -e "${CYAN}================================================${NC}"
echo -e "${GREEN}✅ Tous les services sont en cours de démarrage!${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

echo -e "${CYAN}📍 Points d'accès:${NC}"
for service in "${!services[@]}"; do
    IFS='|' read -r path port <<< "${services[$service]}"
    echo -e "   • ${YELLOW}$service${NC}: http://localhost:$port"
done

echo ""
echo -e "${CYAN}🔗 Routes Gateway:${NC}"
echo -e "   ${GRAY}Auth:      /auth/login, /auth/register${NC}"
echo -e "   ${GRAY}Users:     /users/me, /users/me/change-password${NC}"
echo -e "   ${GRAY}Admin:     /admin/users, /admin/api-keys${NC}"
echo -e "   ${GRAY}Health:    /health, /diagnostics/ping${NC}"

echo ""
echo -e "${CYAN}📝 Logs:${NC}"
echo -e "   ${GRAY}• Les services s'exécutent dans le background${NC}"
echo -e "   ${GRAY}• Pour arrêter: Tapez 'killall dotnet' ou 'Ctrl+C'${NC}"

echo ""
echo -e "${CYAN}🧪 Tester:${NC}"
echo -e "   ${GRAY}curl http://localhost:5000/health${NC}"
echo ""

# Afficher les processus en cours
echo -e "${CYAN}📊 Processus en cours:${NC}"
sleep 3
pgrep -f "dotnet run" | while read pid; do
    cmd=$(cat /proc/$pid/cmdline 2>/dev/null | tr '\0' ' ')
    echo -e "   ${GREEN}✅ PID $pid: $cmd${NC}"
done

echo ""
echo -e "${YELLOW}Appuyez sur Ctrl+C pour arrêter tous les services${NC}"

# Attendre (keep script running)
wait
