# BrackeyGameJam2026

Brackeys Game Jam 2026

## Setup (after cloning)

1. Install Git LFS if you haven't: `git lfs install`
2. Run `scripts/setup-git.sh` (Mac/Linux) or `scripts/setup-git.ps1` (Windows) to configure Unity's smart merge tool. For Windows in Powershell enter `.\scripts\setup-git.ps1`
3. Before editing a scene or prefab, lock it: `git lfs lock Assets/Scenes/YourScene.unity`

### Check out **https://github.com/TomDuchene/unity-git-locks** to learn more about how to use Git LFS locks with Unity as a GUI tool to prevent merge conflicts on scenes and prefabs.

For merge conflicts `git mergetool --tool=unityyamlmerge`

Push large files to Git LFS: `git lfs push origin main`
Fetch large files from Git LFS: `git lfs fetch origin main --all`
