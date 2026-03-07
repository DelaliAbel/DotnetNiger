#!/bin/bash

# DotnetNiger launcher tout-en-un (mode interactif + commandes directes)
# Usage:
#   ./run.sh              # Menu interactif
#   ./run.sh watch        # Lance les 3 services avec dotnet watch run
#   ./run.sh run          # Lance les 3 services avec dotnet run
#   ./run.sh dev          # Lance en watch + logs detailles
#   ./run.sh build        # Build des 3 projets
#   ./run.sh clean        # Clean des 3 projets
#   ./run.sh init-db      # Initialisation base partagee SQLite
#   ./run.sh stop         # Stop des processus dotnet run/watch
#   ./run.sh status       # Etat des services

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
DARKGRAY='\033[0;90m'
NC='\033[0m'

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PIDS_FILE="/tmp/dotnetniger_pids.txt"

GATEWAY_PORT=5000
IDENTITY_PORT=5075
COMMUNITY_PORT=5269

score=0
DOTNET_BIN=""

check_dotnet() {
    if [ -n "$DOTNET_BIN" ]; then
        return
    fi

    if command -v dotnet >/dev/null 2>&1; then
        DOTNET_BIN="dotnet"
    elif command -v dotnet.exe >/dev/null 2>&1; then
        # Git Bash on Windows may expose dotnet only as dotnet.exe.
        DOTNET_BIN="dotnet.exe"
    fi

    if [ -z "$DOTNET_BIN" ]; then
        echo -e "${RED}Erreur: .NET SDK non trouve.${NC}"
        exit 1
    fi
}

pause() {
    echo ""
    read -r -p "Appuyez sur Entree pour continuer..." _
}

add_score() {
    score=$((score + $1))
    echo -e "${CYAN}Score +$1 | Total: $score XP${NC}"
}

show_banner() {
    clear
    echo -e "${BLUE}==============================================${NC}"
    echo -e "${BLUE}      DotnetNiger Command Center       ${NC}"
    echo -e "${BLUE}==============================================${NC}"
    echo -e "${GRAY}Missions: build, init DB, start, monitor, stop${NC}"
    echo -e "${CYAN}Votre XP: $score${NC}"
    echo ""
}

confirm_action() {
    local prompt="$1"
    read -r -p "$prompt (O/N): " response
    [[ "$response" =~ ^[Oo]$ ]]
}

build_all() {
    check_dotnet
    echo -e "${YELLOW}Mission Build: compilation des 3 projets...${NC}"
    "$DOTNET_BIN" build "$SCRIPT_DIR/DotnetNiger.Gateway/DotnetNiger.Gateway.csproj"
    "$DOTNET_BIN" build "$SCRIPT_DIR/DotnetNiger.Identity/DotnetNiger.Identity.csproj"
    "$DOTNET_BIN" build "$SCRIPT_DIR/DotnetNiger.Community/DotnetNiger.Community.csproj"
    echo -e "${GREEN}Build termine.${NC}"
    add_score 20
}

clean_all() {
    check_dotnet
    echo -e "${YELLOW}Mission Clean: nettoyage des 3 projets...${NC}"
    "$DOTNET_BIN" clean "$SCRIPT_DIR/DotnetNiger.Gateway/DotnetNiger.Gateway.csproj"
    "$DOTNET_BIN" clean "$SCRIPT_DIR/DotnetNiger.Identity/DotnetNiger.Identity.csproj"
    "$DOTNET_BIN" clean "$SCRIPT_DIR/DotnetNiger.Community/DotnetNiger.Community.csproj"
    echo -e "${GREEN}Nettoyage termine.${NC}"
    add_score 10
}

init_shared_db() {
    check_dotnet
    local identity_path="$SCRIPT_DIR/DotnetNiger.Identity"
    local community_path="$SCRIPT_DIR/DotnetNiger.Community"
    local db_path="$SCRIPT_DIR/DotnetNiger.db"

    echo -e "${CYAN}Mission DB: initialisation de DotnetNiger.db${NC}"
    echo -e "${GRAY}.NET SDK: $($DOTNET_BIN --version)${NC}"

    echo -e "${YELLOW}Etape 1/2: migrations Identity${NC}"
    pushd "$identity_path" >/dev/null
    if "$DOTNET_BIN" ef database update; then
        echo -e "${GREEN}Identity migrations appliquees.${NC}"
    else
        echo -e "${YELLOW}Identity migrations: avertissement.${NC}"
    fi
    popd >/dev/null

    echo -e "${YELLOW}Etape 2/2: migrations Community${NC}"
    pushd "$community_path" >/dev/null
    if "$DOTNET_BIN" ef database update; then
        echo -e "${GREEN}Community migrations appliquees.${NC}"
    else
        echo -e "${YELLOW}Community migrations: avertissement.${NC}"
    fi
    popd >/dev/null

    if [ -f "$db_path" ]; then
        local db_size
        db_size=$(du -h "$db_path" | cut -f1)
        echo -e "${GREEN}DB creee: $db_path (${db_size})${NC}"
        add_score 25
    else
        echo -e "${YELLOW}DB introuvable a: $db_path${NC}"
    fi
}

stop_services() {
    echo -e "${RED}Arret des services DotnetNiger...${NC}"

    if [ -f "$PIDS_FILE" ]; then
        while read -r pid; do
            if ps -p "$pid" >/dev/null 2>&1; then
                # Stop child process tree first, then parent.
                pkill -P "$pid" 2>/dev/null || true
                kill "$pid" 2>/dev/null || true
            fi
        done < "$PIDS_FILE"
        rm -f "$PIDS_FILE"
    fi

    # Fallback: target only DotnetNiger services to avoid killing unrelated dotnet apps.
    pkill -f "DotnetNiger.Gateway.*dotnet" 2>/dev/null || true
    pkill -f "DotnetNiger.Identity.*dotnet" 2>/dev/null || true
    pkill -f "DotnetNiger.Community.*dotnet" 2>/dev/null || true

    echo -e "${GREEN}Services arretes.${NC}"
}

has_running_services() {
    if [ -f "$PIDS_FILE" ]; then
        while read -r pid; do
            if ps -p "$pid" >/dev/null 2>&1; then
                return 0
            fi
        done < "$PIDS_FILE"
    fi

    pgrep -f "DotnetNiger\.(Gateway|Identity|Community)" >/dev/null 2>&1
}

show_status() {
    echo -e "${CYAN}Etat des processus dotnet run/watch:${NC}"

    if [ -f "$PIDS_FILE" ]; then
        echo -e "${GRAY}PIDs suivis (${PIDS_FILE}):${NC}"
        while read -r pid; do
            if ps -p "$pid" >/dev/null 2>&1; then
                local tracked_cmd
                tracked_cmd=$(ps -p "$pid" -o command= 2>/dev/null || true)
                echo -e "${GREEN}PID $pid${NC} ${DARKGRAY}$tracked_cmd${NC}"
            fi
        done < "$PIDS_FILE"
    fi

    if pgrep -f "DotnetNiger\.(Gateway|Identity|Community)" >/dev/null; then
        pgrep -f "DotnetNiger\.(Gateway|Identity|Community)" | while read -r pid; do
            local cmd
            cmd=$(ps -p "$pid" -o command= 2>/dev/null || true)
            echo -e "${GREEN}PID $pid${NC} ${DARKGRAY}$cmd${NC}"
        done
    elif [ ! -f "$PIDS_FILE" ]; then
        echo -e "${YELLOW}Aucun service DotnetNiger detecte.${NC}"
    fi

    echo ""
    echo -e "${CYAN}Endpoints cibles:${NC}"
    echo -e "${GRAY}Gateway   : http://localhost:${GATEWAY_PORT}${NC}"
    echo -e "${GRAY}Identity  : http://localhost:${IDENTITY_PORT}${NC}"
    echo -e "${GRAY}Community : http://localhost:${COMMUNITY_PORT}${NC}"
}

start_services() {
    local mode="$1"
    local verbose="$2"
    local dotnet_cmd
    local mode_label

    check_dotnet

    # Avoid lock/build conflicts if previous instances are still running.
    if has_running_services; then
        echo -e "${YELLOW}Instances existantes detectees, arret avant redemarrage...${NC}"
        stop_services
        sleep 2
    fi

    if [ "$mode" = "run" ]; then
        dotnet_cmd="$DOTNET_BIN run"
        mode_label="Run"
    else
        dotnet_cmd="$DOTNET_BIN watch run"
        mode_label="Watch"
    fi

    clear
    echo -e "${BLUE}==============================================${NC}"
    echo -e "${BLUE}Demarrage des services - Mode: ${mode_label}${NC}"
    echo -e "${BLUE}==============================================${NC}"
    echo -e "${GREEN}Gateway   :${NC} http://localhost:${GATEWAY_PORT}"
    echo -e "${GREEN}Identity  :${NC} http://localhost:${IDENTITY_PORT}"
    echo -e "${GREEN}Community :${NC} http://localhost:${COMMUNITY_PORT}"
    echo -e "${YELLOW}Ctrl+C pour arreter tous les services${NC}"
    echo ""

    > "$PIDS_FILE"

    cleanup_runtime() {
        echo ""
        echo -e "${RED}Arret en cours...${NC}"
        stop_services
        exit 0
    }
    trap cleanup_runtime INT TERM EXIT

    echo -e "${BLUE}[Gateway]${NC} Demarrage..."
    (
        cd "$SCRIPT_DIR/DotnetNiger.Gateway"
        $dotnet_cmd $verbose --non-interactive 2>&1 | while IFS= read -r line; do
            echo -e "${BLUE}[Gateway]${NC} $line"
        done
    ) &
    echo "$!" >> "$PIDS_FILE"
    sleep 3

    echo -e "${GREEN}[Identity]${NC} Demarrage..."
    (
        cd "$SCRIPT_DIR/DotnetNiger.Identity"
        $dotnet_cmd $verbose --non-interactive 2>&1 | while IFS= read -r line; do
            echo -e "${GREEN}[Identity]${NC} $line"
        done
    ) &
    echo "$!" >> "$PIDS_FILE"
    sleep 3

    echo -e "${YELLOW}[Community]${NC} Demarrage..."
    (
        cd "$SCRIPT_DIR/DotnetNiger.Community"
        $dotnet_cmd $verbose --non-interactive 2>&1 | while IFS= read -r line; do
            echo -e "${YELLOW}[Community]${NC} $line"
        done
    ) &
    echo "$!" >> "$PIDS_FILE"

    echo ""
    echo -e "${GREEN}Tous les services sont lances.${NC}"
    add_score 30
    wait
}

interactive_menu() {
    while true; do
        show_banner
        echo -e "${YELLOW}Choisissez votre mission:${NC}"
        echo "1) Lancer services (watch)"
        echo "2) Lancer services (run)"
        echo "3) Lancer services (dev verbose)"
        echo "4) Build projets"
        echo "5) Clean projets"
        echo "6) Initialiser DB partagee"
        echo "7) Voir etat services"
        echo "8) Stop services"
        echo "9) Quitter"
        echo ""
        read -r -p "Votre choix [1-9]: " choice

        case "$choice" in
            1)
                if confirm_action "Lancer les services en mode watch"; then
                    start_services "watch" ""
                fi
                ;;
            2)
                if confirm_action "Lancer les services en mode run"; then
                    start_services "run" ""
                fi
                ;;
            3)
                if confirm_action "Lancer les services en mode dev verbose"; then
                    start_services "watch" "--verbosity detailed"
                fi
                ;;
            4)
                build_all
                pause
                ;;
            5)
                clean_all
                pause
                ;;
            6)
                if confirm_action "Initialiser la base partagee maintenant"; then
                    init_shared_db
                fi
                pause
                ;;
            7)
                show_status
                pause
                ;;
            8)
                if confirm_action "Arreter tous les services"; then
                    stop_services
                    add_score 5
                fi
                pause
                ;;
            9)
                echo -e "${CYAN}Session terminee. Score final: $score XP${NC}"
                exit 0
                ;;
            *)
                echo -e "${RED}Choix invalide.${NC}"
                pause
                ;;
        esac
    done
}

case "${1:-interactive}" in
    watch)
        start_services "watch" ""
        ;;
    run)
        start_services "run" ""
        ;;
    dev)
        start_services "watch" "--verbosity detailed"
        ;;
    build)
        build_all
        ;;
    clean)
        clean_all
        ;;
    init-db)
        init_shared_db
        ;;
    stop)
        stop_services
        ;;
    status)
        show_status
        ;;
    interactive)
        interactive_menu
        ;;
    *)
        echo -e "${RED}Commande inconnue: $1${NC}"
        echo "Usage: ./run.sh [watch|run|dev|build|clean|init-db|stop|status]"
        exit 1
        ;;
esac