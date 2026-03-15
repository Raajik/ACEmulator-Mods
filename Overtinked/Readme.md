# Overtinked

Mod that extends tinkering limits. Based on the **Tinkering** sample.

## Behavior

- **Max tinkers**: Recipe requirement `NumTimesTinkered` is checked against `Settings.MaxTries` (default 3) instead of the server default. `RecipeManager.TinkeringDifficulty` is extended with extra steps so each tinker tier has a difficulty value.
- **Max imbues**: Requirement `ImbuedEffect` is checked against `Settings.MaxImbueEffects` (default 2) using the bit count of the effect flags.
- **Imbue application**: When a recipe applies an imbue by mutation dataId, the mod sets `target.ImbuedEffect` and handles tinker logging (same dataId map as the sample).

## Configuration

Edit `Settings.json` in the mod output folder (e.g. `C:\ACE\Mods\Overtinked\`):

- `MaxTries`: Max number of tinkers per item.
- `Scale`: Difficulty step between each added tinker tier.
- `MaxImbueEffects`: Max imbue effects per item.
- `EnableRecipeManagerPatch`: If true, patches `RecipeManager.UseObjectOnTarget` so the full tinkering flow uses Overtinked's logic.

## Files

- **Mod.cs** — Entry point; registers the mod and patch class.
- **PatchClass.cs** — Extends difficulty list, prefixes `VerifyRequirements` and `TryMutate`.
- **RecipeManagerEx.cs** — Prefixes `UseObjectOnTarget` (when enabled) for the craft flow.
- **ComparisonHelpers.cs** — CompareType extensions for requirement checks and player messages.
- **Settings.cs** — Config model and Harmony category name.

Do not enable both Overtinked and the Tinkering sample at once; they patch the same methods.

## Credits

Based on the **Tinkering** sample from [ACE.BaseMod](https://github.com/aquafir/ACE.BaseMod) by **aquafir**.
