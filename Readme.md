# BetterCrafter
Mod for the game "Starsand" which just came out on steam in early access.
## What does it do?
Automatically adds needed ingredients to the crafting queue if you have the needed items for it. No need to manually craft all smaller peaces of it anymore.

## How does it work?
This is a C# Mod using Harmony and BepInEx loader, so you need that installed in your games root folder.
You can then start the game with mods loaded by passing the following command line arguments:
`--doorstop-enable true --doorstop-target BepInEx\core\BepInEx.Preloader.dll`

# No precompiled version yet so you need to do it yourself if you really want to
But note that its still uglyish code which needs more cleanup and is very likely prone to bugs. Also as the game is early access it is likely that game updates will break the mod at some point.