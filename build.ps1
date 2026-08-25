[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [switch]$SkipPck
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Write-GodotPck {
    param(
        [Parameter(Mandatory = $true)][string]$OutputPath,
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$ResPrefix
    )

    $utf8 = [System.Text.UTF8Encoding]::new($false)
    $sourceRootFull = [System.IO.Path]::GetFullPath($SourceRoot)

    $files = Get-ChildItem -Path $sourceRootFull -File -Recurse | Sort-Object FullName
    if ($files.Count -eq 0) {
        throw "No files found under '$SourceRoot' to pack."
    }

    $entries = foreach ($file in $files) {
        $rel = $file.FullName.Substring($sourceRootFull.Length).TrimStart('\', '/')
        $resPath = ($ResPrefix.TrimEnd('/') + '/' + ($rel -replace '\\', '/'))
        [pscustomobject]@{
            ResPath   = $resPath
            LocalPath = $file.FullName
            Bytes     = [System.IO.File]::ReadAllBytes($file.FullName)
        }
    }

    # v2 header: magic(4) + version(4) + major(4) + minor(4) + patch(4)
    #            + reserved(16) + file_count(4) = 40 bytes,
    # then one record per file (path_len(4) + path + offset(8) + size(8) + md5(16)),
    # then the raw file data.
    $headerSize = 40
    foreach ($entry in $entries) {
        $headerSize += 4 + $utf8.GetByteCount($entry.ResPath) + 8 + 8 + 16
    }

    $offset = $headerSize
    $md5 = [System.Security.Cryptography.MD5]::Create()
    foreach ($entry in $entries) {
        $entry | Add-Member -NotePropertyName Offset -NotePropertyValue $offset
        $entry | Add-Member -NotePropertyName Md5 -NotePropertyValue ($md5.ComputeHash($entry.Bytes))
        $offset += $entry.Bytes.Length
    }

    $fs = [System.IO.File]::Create($OutputPath)
    try {
        $bw = [System.IO.BinaryWriter]::new($fs, $utf8, $false)
        try {
            $bw.Write([uint32]0x43504447)   # "GDPC"
            $bw.Write([uint32]2)            # pack format version
            $bw.Write([uint32]4)            # engine major
            $bw.Write([uint32]5)            # engine minor
            $bw.Write([uint32]1)            # engine patch
            $bw.Write([byte[]](New-Object byte[] 16))  # reserved
            $bw.Write([uint32]$entries.Count)

            foreach ($entry in $entries) {
                $pathBytes = $utf8.GetBytes($entry.ResPath)
                $bw.Write([uint32]$pathBytes.Length)
                $bw.Write($pathBytes)
                $bw.Write([uint64]$entry.Offset)
                $bw.Write([uint64]$entry.Bytes.Length)
                $bw.Write($entry.Md5)
            }

            foreach ($entry in $entries) {
                $bw.Write($entry.Bytes)
            }
        }
        finally {
            $bw.Dispose()
        }
    }
    finally {
        $fs.Dispose()
    }
}

Set-Location $Root

Write-Host "Building Forefinger ($Configuration)..."
dotnet build .\Forefinger.csproj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

$localProps = Join-Path $Root "local.props"
if (-not (Test-Path $localProps)) {
    throw "Missing local.props. Copy local.props.template to local.props and set Sts2Dir."
}
$localPropsText = Get-Content $localProps -Raw
$sts2Dir = [regex]::Match($localPropsText, '<Sts2Dir>(.*?)</Sts2Dir>').Groups[1].Value.Trim()
if ([string]::IsNullOrWhiteSpace($sts2Dir)) {
    throw "Could not read Sts2Dir from local.props."
}
$modOutputDir = Join-Path $sts2Dir "mods\Forefinger"

if (-not $SkipPck) {
    $pckOut = Join-Path $Root "Forefinger.pck"
    Write-Host "Packing localization into $pckOut ..."
    Write-GodotPck -OutputPath $pckOut -SourceRoot (Join-Path $Root "resources\Forefinger") -ResPrefix "res://Forefinger"
    Write-Host "PCK packed: $((Get-Item $pckOut).Length) bytes."

    New-Item -ItemType Directory -Force -Path $modOutputDir | Out-Null
    Copy-Item -Path $pckOut -Destination (Join-Path $modOutputDir "Forefinger.pck") -Force
    Write-Host "Deployed PCK to $modOutputDir."
}

Write-Host "Done."
