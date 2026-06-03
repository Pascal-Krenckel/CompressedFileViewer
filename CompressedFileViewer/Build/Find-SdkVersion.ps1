$kitsRoot = (Get-ItemProperty `
    -Path "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots" `
    -Name "KitsRoot10").KitsRoot10

$binRoot = Join-Path $kitsRoot "bin"

$version = Get-ChildItem $binRoot -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
    Sort-Object Name -Descending |
    Select-Object -First 1

if ($version) {
    Write-Output $version.Name
    exit 0
}

exit 1