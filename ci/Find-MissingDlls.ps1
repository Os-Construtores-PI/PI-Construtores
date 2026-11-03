$dir = 'C:\Program Files\Blender Foundation\Blender ' + $env:BLENDER_SERIES

function Get-PeImports([string]$path) {
    $b = [IO.File]::ReadAllBytes($path)
    $pe = [BitConverter]::ToInt32($b, 0x3C)
    $sections = [BitConverter]::ToUInt16($b, $pe + 6)
    $optSize = [BitConverter]::ToUInt16($b, $pe + 20)
    $opt = $pe + 24
    $dirs = if ([BitConverter]::ToUInt16($b, $opt) -eq 0x20b) { $opt + 112 } else { $opt + 96 }
    $importRva = [BitConverter]::ToUInt32($b, $dirs + 8)
    if ($importRva -eq 0) { return }
    $table = $opt + $optSize
    $toOffset = {
        param($rva)
        for ($i = 0; $i -lt $sections; $i++) {
            $s = $table + 40 * $i
            $va = [BitConverter]::ToUInt32($b, $s + 12)
            $size = [Math]::Max([BitConverter]::ToUInt32($b, $s + 8), [BitConverter]::ToUInt32($b, $s + 16))
            if ($rva -ge $va -and $rva -lt $va + $size) {
                return [int]($rva - $va + [BitConverter]::ToUInt32($b, $s + 20))
            }
        }
        return -1
    }
    $d = & $toOffset $importRva
    while ($d -ge 0) {
        $nameRva = [BitConverter]::ToUInt32($b, $d + 12)
        if ($nameRva -eq 0) { break }
        $n = & $toOffset $nameRva
        $end = $n
        while ($b[$end] -ne 0) { $end++ }
        [Text.Encoding]::ASCII.GetString($b, $n, $end - $n).ToLower()
        $d += 20
    }
}

'top-level folders: ' + ((Get-ChildItem $dir -Directory | ForEach-Object Name) -join ', ')

$all = Get-ChildItem $dir -Recurse -File
$have = @{}
foreach ($f in $all) { $have[$f.Name.ToLower()] = $true }

$files = $all | Where-Object { $_.Extension -in '.dll', '.exe', '.pyd' }
$missing = @{}
foreach ($f in $files) {
    foreach ($name in Get-PeImports $f.FullName) {
        if ($name -like 'api-ms-*' -or $name -like 'ext-ms-*') { continue }
        if ($have.ContainsKey($name) -or (Test-Path (Join-Path 'C:\Windows\System32' $name))) { continue }
        if (-not $missing.ContainsKey($name)) { $missing[$name] = @() }
        $missing[$name] += $f.Name
    }
}

foreach ($k in ($missing.Keys | Sort-Object)) {
    'MISSING: ' + $k + '  <- ' + (($missing[$k] | Select-Object -First 3) -join ', ')
}
'checked ' + $files.Count + ' files, ' + $missing.Count + ' missing'
exit 0
