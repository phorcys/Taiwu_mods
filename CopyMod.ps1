
param ($Mod, $ModsDir)

function CheckDir ([System.IO.DirectoryInfo]$dir) {
    $res = $true;
    foreach ($file in Get-ChildItem $dir.FullName) {
        if ($file -is [System.IO.FileInfo]) {
            if ((".dll", ".cs") -contains $file.Extension) {
                $res = $false;
                break;
            }
        }
        else {
            if (!(CheckDir $file)) {
                $res = $false;
                break;
            }
        }
    }
    return $res;
}

$path = Join-Path $PSScriptRoot $Mod;
$targetDir = Join-Path $ModsDir $Mod;

if (Test-Path $targetDir) {
    Remove-Item "$targetDir\*" -Recurse -Force -Exclude *.xml, *.dll
    foreach ($file in Get-ChildItem $path) {
        if ($file -is [System.IO.FileInfo]) {
            if ((".dll", ".cs") -notcontains $file.Extension) {
                Write-Output ("copy file " + $file.Name)
                Copy-Item -Force $file.FullName (Join-Path $targetDir $file.Name)
            }
        }
        else {
            if (CheckDir $file) {
                Write-Output ("copy folder " + $file.Name)
                Copy-Item -Force -Recurse $file.FullName $targetDir
            }
        }
    }
}
