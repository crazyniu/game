# One-click launcher for Perch Sort (Windows). Same behaviour as run.sh.
#   .\run.ps1                 open all levels, top up to at least 50,000 coins, and test mode: the star chest,
#                             daily reward and lucky draw never run out
#   .\run.ps1 --level=12      start directly on level 12
#   .\run.ps1 --lab=1         open trial level 1 of 3 (the Dive talent); trial levels save nothing
#   .\run.ps1 --reset         delete saved progress, then apply the launcher defaults
#   .\run.ps1 --update        pull the latest main first (only if the checkout has no local changes)
# The original save is backed up once as profile.before-launcher-access.json. No Python needed.
# Godot is found via $env:GODOT, godot(.exe) on PATH, or common install folders. Extra engine flags: $env:GODOT_FLAGS.
# If script execution is blocked, use run.bat instead.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$project = Join-Path $root 'PerchSort'

function Find-Godot {
    if ($env:GODOT) { return $env:GODOT }
    foreach ($name in 'godot', 'godot.exe', 'Godot_v4.7.2-stable_win64.exe', 'Godot_v4.7.2-stable_win64_console.exe') {
        $cmd = Get-Command $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($cmd) { return $cmd.Source }
    }
    $dirs = @(
        (Join-Path $env:USERPROFILE 'scoop\apps\godot\current'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Godot'),
        (Join-Path $env:LOCALAPPDATA 'Microsoft\WinGet\Links'),
        'C:\Godot', 'C:\Program Files\Godot', (Join-Path $env:USERPROFILE 'Godot'),
        (Join-Path $env:USERPROFILE 'Downloads')
    )
    foreach ($dir in $dirs) {
        if (-not (Test-Path $dir)) { continue }
        # Prefer the console build so engine output is visible; fall back to the GUI build.
        $found = Get-ChildItem $dir -Filter 'Godot*.exe' -ErrorAction SilentlyContinue |
            Sort-Object { $_.Name -notmatch 'console' }, Name | Select-Object -First 1
        if ($found) { return $found.FullName }
    }
    [Console]::Error.WriteLine('Godot 4.7.2 not found. Put godot.exe on PATH or set $env:GODOT = "C:\path\to\godot.exe".')
    exit 1
}

function Get-SaveDir { Join-Path $env:APPDATA 'Godot\app_userdata\Perch Sort' }

function Invoke-Git {
    $out = & git -C $root @args 2>$null
    if ($LASTEXITCODE -ne 0) { return $null }
    return $out
}

$gameArgs = New-Object System.Collections.Generic.List[string]
$boostProfile = $true
foreach ($arg in $args) {
    switch -Wildcard ($arg) {
        '--reset' {
            Remove-Item -LiteralPath (Join-Path (Get-SaveDir) 'profile.json') -Force -ErrorAction SilentlyContinue
            Write-Host 'Saved progress deleted.'
        }
        '--update' {
            if (Invoke-Git status --porcelain) {
                [Console]::Error.WriteLine('Local changes present; skipping update.')
            } else {
                & git -C $root pull --ff-only origin main
                if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
            }
        }
        { $_ -in '-h', '--help' } {
            Get-Content (Join-Path $root 'run.ps1') -TotalCount 9 | Select-Object -Skip 1 | ForEach-Object { $_ -replace '^# ?', '' }
            exit 0
        }
        { $_ -like '--capture=*' -or $_ -like '--frames=*' -or $_ -like '--preview=*' -or $_ -like '--profile=*' -or $_ -in '--smoke', '--autoplay' } {
            $boostProfile = $false
            $gameArgs.Add($arg)
        }
        default { $gameArgs.Add($arg) }
    }
}

$godot = Find-Godot
$version = (& $godot --version 2>$null | Select-Object -First 1)
if (-not ($version -like '4.7.*')) {
    $shown = if ($version) { $version } else { 'unknown' }
    [Console]::Error.WriteLine("Warning: expected Godot 4.7.x, found $shown.")
}

# A plain game launch does not import new or changed art, so import whenever the project changed
# since the last import (new commit, local status, or file modification time). Without Git, always import.
$stampFile = Join-Path $project '.godot\run_import_stamp'
$stamp = $null
$head = Invoke-Git rev-parse HEAD
if ($head) {
    $status = Invoke-Git status --porcelain -- PerchSort
    $text = ((@($head) + @($status)) -join "`n")
    $sha = [System.Security.Cryptography.SHA1]::Create()
    $stamp = ([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($text)))) -replace '-', ''
}
$needImport = $true
if ($stamp -and (Test-Path $stampFile)) {
    $needImport = ((Get-Content $stampFile -Raw).Trim() -ne $stamp)
    if (-not $needImport) {
        $stampTime = (Get-Item $stampFile).LastWriteTime
        $godotDir = Join-Path $project '.godot'
        $newer = Get-ChildItem $project -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { -not $_.FullName.StartsWith($godotDir, [StringComparison]::OrdinalIgnoreCase) -and $_.LastWriteTime -gt $stampTime } |
            Select-Object -First 1
        $needImport = [bool]$newer
    }
}
if ($needImport) {
    Write-Host 'Importing assets (the first time takes about a minute)...'
    try { & $godot --headless --path $project --import *> $null } catch { }
    if ($stamp) {
        New-Item -ItemType Directory -Force -Path (Split-Path $stampFile) | Out-Null
        Set-Content -LiteralPath $stampFile -Value $stamp -NoNewline
    }
}

if ($boostProfile) {
    $saveDir = Get-SaveDir
    New-Item -ItemType Directory -Force -Path $saveDir | Out-Null
    $profilePath = Join-Path $saveDir 'profile.json'
    $backupPath = Join-Path $saveDir 'profile.before-launcher-access.json'
    $utf8 = New-Object System.Text.UTF8Encoding($false)

    if (Test-Path $profilePath) {
        $original = [IO.File]::ReadAllBytes($profilePath)
        $profile = $utf8.GetString($original) | ConvertFrom-Json
        if (-not (Test-Path $backupPath)) { [IO.File]::WriteAllBytes($backupPath, $original) }
    } else {
        $profile = [pscustomobject]@{}
    }

    function Set-Field($obj, $name, $value) {
        if ($obj.PSObject.Properties[$name]) { $obj.$name = $value }
        else { $obj | Add-Member -NotePropertyName $name -NotePropertyValue $value }
    }

    $levels = 100
    # Older saves number their levels differently: move the records as the game would, before opening the levels.
    $profileSource = [IO.File]::ReadAllText((Join-Path $project 'meta\profile.gd'), $utf8)
    $currentSchema = [int][regex]::Match($profileSource, 'const SCHEMA_VERSION := (\d+)').Groups[1].Value
    function Get-LevelTable($name) {
        $body = [regex]::Match($profileSource, $name + ' := \{(.*?)\}').Groups[1].Value
        $moves = @{}
        foreach ($m in [regex]::Matches($body, '(\d+): (\d+)')) { $moves[[int]$m.Groups[1].Value] = [int]$m.Groups[2].Value }
        return $moves
    }

    # No save yet counts as the current schema; a save without the field is schema 1.
    $schema = $currentSchema
    if (Test-Path $profilePath) {
        $schema = if ($profile.PSObject.Properties['schemaVersion']) { [int]$profile.schemaVersion } else { 1 }
    }
    $coins = if ($profile.PSObject.Properties['coins']) { [long]$profile.coins } else { 0 }
    Set-Field $profile 'coins' ([Math]::Max($coins, 50000))
    foreach ($step in @(@(2, 'LEVEL_RENUMBER'), @(3, 'LEVEL_EXPANSION'), @(4, 'LEVEL_DIFFICULTY'))) {
        if ($schema -ge $step[0]) { continue }
        $moves = Get-LevelTable $step[1]
        foreach ($field in 'bestStars', 'clearCounts', 'bestMoves') {
            $old = if ($profile.PSObject.Properties[$field] -and $null -ne $profile.$field) { @($profile.$field) } else { @() }
            $new = New-Object 'object[]' ($levels + 1)
            for ($i = 0; $i -le $levels; $i++) { $new[$i] = 0 }
            foreach ($source in $moves.Keys) {
                if ($source -lt $old.Count) { $new[$moves[$source]] = $old[$source] }
            }
            Set-Field $profile $field $new
        }
        $gifts = @()
        if ($profile.PSObject.Properties['levelGifts']) {
            foreach ($level in @($profile.levelGifts)) {
                if ($moves.ContainsKey([int]$level)) { $gifts += $moves[[int]$level] }
            }
        }
        Set-Field $profile 'levelGifts' $gifts
    }
    Set-Field $profile 'schemaVersion' $currentSchema
    Set-Field $profile 'highestUnlockedLevel' $levels
    # The game's existing unlock rule requires a star on each preceding level.
    foreach ($field in 'bestStars', 'clearCounts') {
        $entries = New-Object System.Collections.Generic.List[object]
        if ($profile.PSObject.Properties[$field] -and $null -ne $profile.$field) {
            foreach ($item in @($profile.$field)) { $entries.Add($item) }
        }
        while ($entries.Count -lt $levels + 1) { $entries.Add(0) }
        for ($level = 1; $level -lt $levels; $level++) {
            if ([long]$entries[$level] -lt 1) { $entries[$level] = 1 }
        }
        Set-Field $profile $field ($entries.ToArray())
    }

    $temporary = Join-Path $saveDir ('.profile-' + [Guid]::NewGuid().ToString('N') + '.tmp')
    [IO.File]::WriteAllText($temporary, ($profile | ConvertTo-Json -Depth 100 -Compress), $utf8)
    Move-Item -LiteralPath $temporary -Destination $profilePath -Force
}

$engineFlags = @()
if ($env:GODOT_FLAGS) { $engineFlags = $env:GODOT_FLAGS -split '\s+' | Where-Object { $_ } }
if ($boostProfile) { $gameArgs.Add('--unlimited') }
& $godot @engineFlags --path $project -- @($gameArgs)
exit $LASTEXITCODE
