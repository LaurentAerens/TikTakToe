param(
    [switch]$SkipUp,
    [switch]$KeepData
)

$ErrorActionPreference = 'Stop'

if ($KeepData) {
    Write-Host 'Stopping dev containers without removing the persisted Postgres volume...'
    docker compose down --remove-orphans
}
else {
    Write-Host 'Stopping dev containers and removing the persisted Postgres volume...'
    docker compose down --remove-orphans --volumes
}

if ($SkipUp) {
    Write-Host 'Skipped startup because -SkipUp was supplied.'
    exit 0
}

if ($KeepData) {
    Write-Host 'Starting the dev stack while preserving the existing database volume...'
    $env:FEATURES__RESETDATABASEONSTARTUP = 'false'
}
else {
    Write-Host 'Starting the dev stack with a fresh database...'
    $env:FEATURES__RESETDATABASEONSTARTUP = 'true'
}

docker compose --profile dev up --build --force-recreate
