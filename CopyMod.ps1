
param ($Mod, $ModsDir)

function CheckDir ([System.IO.DirectoryInfo]$dir) {
    $res = $true;
    foreach ($file in dir $dir.FullName) {
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
    foreach ($file in dir $path) {
        if ($file -is [System.IO.FileInfo]) {
            if ((".dll", ".cs") -notcontains $file.Extension) {
                copy -Force $file.FullName (Join-Path $targetDir $file.Name)
            }
        }
        else {
            if (CheckDir $file) {
                copy -Force -Recurse $file.FullName $targetDir
            }
        }
    }
}
