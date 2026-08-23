$unityMerge = Get-ChildItem "C:\Program Files\Unity\Hub\Editor\*\Editor\Data\Tools\UnityYAMLMerge.exe" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $unityMerge) {
    Write-Error "UnityYAMLMerge.exe not found under Program Files. Check your Unity Hub install location."
    exit 1
}

$unityMergeForward = $unityMerge -replace '\\', '/'

git config --local merge.tool unityyamlmerge
git config --local mergetool.unityyamlmerge.trustExitCode false
git config --local mergetool.unityyamlmerge.cmd "'$unityMergeForward' merge -p `"`$BASE`" `"`$REMOTE`" `"`$LOCAL`" `"`$MERGED`""

Write-Host "Configured UnityYAMLMerge using: $unityMergeForward"