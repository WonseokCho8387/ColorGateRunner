# Regenerates the Android app icon art from the Theme01 Victory emblem: a
# night-violet radial background and the emblem on a transparent foreground
# (adaptive icon layers, 432 px), plus a flattened 512 px legacy icon.
#
# Usage: powershell -File Tools/Generate-AppIcon.ps1
# Then:  Tools > Color Gate Runner > Apply App Icons (Editor) to assign them.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$emblemPath = Join-Path $root 'ColorGateRunner\Assets\Game\Art\UI\Theme01\VictoryEmblem.png'
$outFolder = Join-Path $root 'ColorGateRunner\Assets\Game\Art\AppIcon'
New-Item -ItemType Directory -Force $outFolder | Out-Null

$layer = 432
# Android launchers show the inner 72/108 of the adaptive layers; the emblem
# fits the 66/108 safe zone (264 px of 432).
$emblemSize = 264
$visible = [int](72 * $layer / 108)

function New-Canvas([int]$size) {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = 'AntiAlias'
    $graphics.InterpolationMode = 'HighQualityBicubic'
    $graphics.PixelOffsetMode = 'HighQuality'
    return @($bitmap, $graphics)
}

# Background: near-black night with a violet glow behind the emblem.
$bg, $g = New-Canvas $layer
$full = New-Object System.Drawing.Rectangle 0, 0, $layer, $layer
$g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 8, 8, 28))), $full)
$glow = New-Object System.Drawing.Drawing2D.GraphicsPath
$glow.AddEllipse(-60, -40, $layer + 120, $layer + 120)
$brush = New-Object System.Drawing.Drawing2D.PathGradientBrush $glow
$brush.CenterColor = [System.Drawing.Color]::FromArgb(255, 70, 24, 120)
$brush.SurroundColors = @([System.Drawing.Color]::FromArgb(255, 8, 8, 28))
$g.FillRectangle($brush, $full)
$g.Dispose()

# Foreground: the emblem centred on transparency.
$fg, $g = New-Canvas $layer
$g.Clear([System.Drawing.Color]::Transparent)
$emblem = [System.Drawing.Image]::FromFile($emblemPath)
$offset = [int](($layer - $emblemSize) / 2)
$g.DrawImage($emblem, (New-Object System.Drawing.Rectangle $offset, $offset, $emblemSize, $emblemSize))
$emblem.Dispose()
$g.Dispose()

# Legacy: the visible adaptive area at 512 px, drawn from the source emblem
# so it stays sharp.
$legacy, $g = New-Canvas 512
$cut = [int](($layer - $visible) / 2)
$g.DrawImage($bg, (New-Object System.Drawing.Rectangle 0, 0, 512, 512), $cut, $cut, $visible, $visible, [System.Drawing.GraphicsUnit]::Pixel)
$legacyEmblem = [int](512 * $emblemSize / $visible)
$legacyOffset = [int]((512 - $legacyEmblem) / 2)
$emblem = [System.Drawing.Image]::FromFile($emblemPath)
$g.DrawImage($emblem, (New-Object System.Drawing.Rectangle $legacyOffset, $legacyOffset, $legacyEmblem, $legacyEmblem))
$emblem.Dispose()
$g.Dispose()

$bg.Save((Join-Path $outFolder 'AppIconBackground.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$fg.Save((Join-Path $outFolder 'AppIconForeground.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$legacy.Save((Join-Path $outFolder 'AppIconLegacy.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$bg.Dispose(); $fg.Dispose(); $legacy.Dispose()
Write-Output "App icon art written to $outFolder"
