# Setup-LogixDb.ps1
$InstallDir = "C:\Program Files\LogixDb"
$ServiceName = "LogixDb"
$ServiceAccount = "NT SERVICE\LogixDb"

function Write-Step($msg) { Write-Host "[SETUP] $msg" -ForegroundColor Cyan }

# 1. Elevate and Prepare
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Please run this script as Administrator."
    exit
}

# 2. Stop Service
if (Get-Service $ServiceName -ErrorAction SilentlyContinue) {
    Write-Step "Stopping existing service..."
    Stop-Service $ServiceName -Force -ErrorAction SilentlyContinue
}

# 3. Deploy Files
Write-Step "Deploying files to $InstallDir..."
if (-not (Test-Path $InstallDir)) { New-Item -Path $InstallDir -ItemType Directory -Force }

# Copy all files from current directory to InstallDir (excluding the script itself)
Get-ChildItem -Exclude "Setup-LogixDb.ps1" | Copy-Item -Destination $InstallDir -Recurse -Force

# 4. Seed FTAC Permissions
$sqlScript = @"
IF EXISTS (SELECT name FROM master.sys.databases WHERE name = N'AssetCentre')
BEGIN
    IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = '$ServiceAccount')
        CREATE LOGIN [$ServiceAccount] FROM WINDOWS;
    
    USE [AssetCentre];
    IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '$ServiceAccount')
        CREATE USER [$ServiceAccount] FOR LOGIN [$ServiceAccount];
    
    GRANT SELECT ON [dbo].[arch_AssetVersions] TO [$ServiceAccount];
    GRANT EXECUTE ON [dbo].[arch_ReadFileChunkStart] TO [$ServiceAccount];
    GRANT EXECUTE ON [dbo].[arch_ReadFileChunk] TO [$ServiceAccount];
END
"@

Write-Step "Checking for FTAC database and seeding permissions..."
try {
    # Attempt to run SQL if SqlServer module is available, otherwise log the requirement
    if (Get-Module -ListAvailable SqlServer) {
        Invoke-Sqlcmd -Query $sqlScript -ErrorAction SilentlyContinue
    } else {
        Write-Host "SqlServer module not found. Skipping automatic SQL seeding." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Could not automatically seed SQL permissions. Manual setup may be required." -ForegroundColor Yellow
}

# 5. Register Service and Path
Write-Step "Configuring Windows Service..."
$exePath = Join-Path $InstallDir "LogixDb.Service.exe"
if (Get-Service $ServiceName -ErrorAction SilentlyContinue) {
    sc.exe config $ServiceName binPath= "`"$exePath`"" obj= "$ServiceAccount" start= auto
} else {
    New-Service -Name $ServiceName -BinaryPathName "`"$exePath`"" -DisplayName "LogixDb" -Description "LogixDb Ingestion and FTAC Monitor" -StartupType Automatic -Account $ServiceAccount
}

# Add to PATH
$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
if ($currentPath -notlike "*$InstallDir*") {
    [Environment]::SetEnvironmentVariable("Path", $currentPath.TrimEnd(';') + ";$InstallDir", "Machine")
}

# 6. Start
Write-Step "Starting service..."
Start-Service $ServiceName

Write-Host "Setup Complete! LogixDb CLI is now available on your PATH." -ForegroundColor Green