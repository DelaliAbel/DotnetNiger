# Script PowerShell pour lancer tous les services DotnetNiger
# Usage: 
#   .\run.ps1           # Mode watch (défaut)
#   .\run.ps1 -Run      # Mode run simple
#   .\run.ps1 -Build    # Build uniquement
#   .\run.ps1 -Clean    # Nettoyage
#   .\run.ps1 -Dev      # Mode développement (watch + logs détaillés)

param(
    [switch]$Clean,
    [switch]$Run,
    [switch]$Build,
    [switch]$Dev
)

$ErrorActionPreference = "Stop"

# Ports configurés
$GATEWAY_PORT = 5000
$IDENTITY_PORT = 5075
$COMMUNITY_PORT = 5269

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host "   DotnetNiger - Gestion des services" -ForegroundColor Blue
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host ""

# Mode Clean uniquement
if ($Clean -and -not $Run -and -not $Build -and -not $Dev) {
    Write-Host "🧹 Nettoyage des projets..." -ForegroundColor Yellow
    dotnet clean DotnetNiger.Gateway/DotnetNiger.Gateway.csproj
    dotnet clean DotnetNiger.Identity/DotnetNiger.Identity.csproj
    dotnet clean DotnetNiger.Community/DotnetNiger.Community.csproj
    Write-Host "✓ Nettoyage terminé" -ForegroundColor Green
    exit 0
}

# Mode Build uniquement
if ($Build) {
    Write-Host "🔨 Build des projets..." -ForegroundColor Yellow
    dotnet build DotnetNiger.Gateway/DotnetNiger.Gateway.csproj
    dotnet build DotnetNiger.Identity/DotnetNiger.Identity.csproj
    dotnet build DotnetNiger.Community/DotnetNiger.Community.csproj
    Write-Host "✓ Build terminé" -ForegroundColor Green
    exit 0
}

# Clean si demandé avant run/watch
if ($Clean) {
    Write-Host "🧹 Nettoyage des projets..." -ForegroundColor Yellow
    dotnet clean DotnetNiger.Gateway/DotnetNiger.Gateway.csproj --verbosity quiet
    dotnet clean DotnetNiger.Identity/DotnetNiger.Identity.csproj --verbosity quiet
    dotnet clean DotnetNiger.Community/DotnetNiger.Community.csproj --verbosity quiet
    Write-Host "✓ Nettoyage terminé" -ForegroundColor Green
    Write-Host ""
}

# Déterminer le mode
$mode = if ($Run) { "run" } elseif ($Dev) { "watch" } else { "watch" }
$modeLabel = if ($Run) { "Run" } elseif ($Dev) { "Dev (Watch + Logs)" } else { "Watch" }

# Clear screen
Clear-Host

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host "   Démarrage des services - Mode: $modeLabel" -ForegroundColor Blue
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host ""
Write-Host "🚀 Gateway    : " -NoNewline -ForegroundColor Green
Write-Host "http://localhost:$GATEWAY_PORT"
Write-Host "🔐 Identity   : " -NoNewline -ForegroundColor Green
Write-Host "http://localhost:$IDENTITY_PORT"
Write-Host "👥 Community  : " -NoNewline -ForegroundColor Green
Write-Host "http://localhost:$COMMUNITY_PORT"
Write-Host ""
Write-Host "💡 Appuyez sur Ctrl+C pour arrêter tous les services" -ForegroundColor Yellow
Write-Host ""

# Variable pour stocker les jobs
$jobs = @()

# Fonction pour arrêter les services
function Stop-Services {
    Write-Host ""
    Write-Host "🛑 Arrêt des services..." -ForegroundColor Red
    
    # Arrêter tous les jobs
    Get-Job -ErrorAction SilentlyContinue | Stop-Job -ErrorAction SilentlyContinue
    Get-Job -ErrorAction SilentlyContinue | Remove-Job -ErrorAction SilentlyContinue
    
    # Tuer tous les processus dotnet watch
    Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -like "*dotnet*watch*" } |
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
    
    Write-Host "✓ Services arrêtés" -ForegroundColor Green
}

# Capturer Ctrl+C
$null = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    Stop-Services
}

try {
    # Construire la commande selon le mode
    $dotnetCmd = if ($mode -eq "run") { "run" } else { "watch run" }
    $verbosity = if ($Dev) { "--verbosity detailed" } else { "" }
    
    # Lancer Gateway
    Write-Host "[Gateway] Démarrage..." -ForegroundColor Blue
    $gatewayJob = Start-Job -ScriptBlock {
        Set-Location $using:PWD
        Set-Location DotnetNiger.Gateway
        $cmd = "dotnet $using:dotnetCmd $using:verbosity --non-interactive"
        Invoke-Expression $cmd 2>&1 | ForEach-Object {
            Write-Output "[Gateway] $_"
        }
    }
    $jobs += $gatewayJob
    Start-Sleep -Seconds 3
    
    # Lancer Identity
    Write-Host "[Identity] Démarrage..." -ForegroundColor Green
    $identityJob = Start-Job -ScriptBlock {
        Set-Location $using:PWD
        Set-Location DotnetNiger.Identity
        $cmd = "dotnet $using:dotnetCmd $using:verbosity --non-interactive"
        Invoke-Expression $cmd 2>&1 | ForEach-Object {
            Write-Output "[Identity] $_"
        }
    }
    $jobs += $identityJob
    Start-Sleep -Seconds 3
    
    # Lancer Community
    Write-Host "[Community] Démarrage..." -ForegroundColor Yellow
    $communityJob = Start-Job -ScriptBlock {
        Set-Location $using:PWD
        Set-Location DotnetNiger.Community
        $cmd = "dotnet $using:dotnetCmd $using:verbosity --non-interactive"
        Invoke-Expression $cmd 2>&1 | ForEach-Object {
            Write-Output "[Community] $_"
        }
    }
    $jobs += $communityJob
    
    Write-Host ""
    Write-Host "✓ Tous les services sont en cours de démarrage" -ForegroundColor Green
    Write-Host ""
    
    # Afficher les logs en continu
    while ($true) {
        foreach ($job in $jobs) {
            $output = Receive-Job -Job $job -ErrorAction SilentlyContinue
            if ($output) {
                $output | ForEach-Object {
                    if ($_ -match "\[Gateway\]") {
                        Write-Host $_ -ForegroundColor Blue
                    }
                    elseif ($_ -match "\[Identity\]") {
                        Write-Host $_ -ForegroundColor Green
                    }
                    elseif ($_ -match "\[Community\]") {
                        Write-Host $_ -ForegroundColor Yellow
                    }
                    else {
                        Write-Host $_
                    }
                }
            }
        }
        
        # Vérifier si tous les jobs sont encore actifs
        $runningJobs = $jobs | Where-Object { $_.State -eq "Running" }
        if ($runningJobs.Count -eq 0) {
            Write-Host "⚠️  Tous les services se sont arrêtés" -ForegroundColor Yellow
            break
        }
        
        Start-Sleep -Milliseconds 100
    }
}
catch {
    Write-Host "❌ Erreur: $_" -ForegroundColor Red
}
finally {
    Stop-Services
}
