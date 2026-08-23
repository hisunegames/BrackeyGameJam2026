#!/usr/bin/env bash
set -e

if [[ "$OSTYPE" == "darwin"* ]]; then
    SEARCH_GLOB="/Applications/Unity/Hub/Editor/*/Unity.app/Contents/Tools/UnityYAMLMerge"
else
    SEARCH_GLOB="$HOME/Unity/Hub/Editor/*/Editor/Data/Tools/UnityYAMLMerge"
fi

UNITY_MERGE=$(ls -t $SEARCH_GLOB 2>/dev/null | head -n 1)

if [ -z "$UNITY_MERGE" ]; then
    echo "UnityYAMLMerge not found. Check your Unity Hub install location."
    exit 1
fi

git config --local merge.tool unityyamlmerge
git config --local mergetool.unityyamlmerge.trustExitCode false
git config --local mergetool.unityyamlmerge.cmd "'$UNITY_MERGE' merge -p \"\$BASE\" \"\$REMOTE\" \"\$LOCAL\" \"\$MERGED\""

echo "Configured UnityYAMLMerge using: $UNITY_MERGE"