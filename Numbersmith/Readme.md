## Numbersmith

Numbersmith lets you tweak how combat and progression feel on your server without touching C#.

Instead of hard‑coding numbers, Numbersmith reads simple math formulas from JSON and applies them to different parts of the game (damage, crits, healing, XP, and more). Formulas are evaluated with [AngouriMath](https://am.angouri.org/docs/AngouriMath/MathS.html), so you can write expressions like `x * 1.1` or `x + 10` directly in your config.

### How it works (high level)

- **Each balance tweak is a “patch”** (for example `WeaponCriticalChance`, `PlayerTakeDamage`, `LevelCost`).
- Patches are configured in JSON (usually in this mod’s `Settings.json`).
- For each patch you can:
  - **Turn it on or off** with the `Enabled` flag.
  - **Change the behavior** with a `Formula` string that uses the variables documented in that patch.

When the server runs, the mod reads your formulas, evaluates them, and uses the result instead of the default ACE logic for that piece of the game.

### Basic configuration example

Here is a minimal example of enabling the weapon critical chance patch and making crits slightly more common:

```csharp
{
  "PatchType": "WeaponCriticalChance",
  "Enabled": true,
  "Formula": "x + 0.01 * r"
}
```

- `PatchType` selects which patch to apply.  
- `Enabled: true` means this patch is active.  
- `Formula` is the math expression:
  - `x` is the base value the game would normally use.
  - `r` can be an additional variable exposed by the patch (for example, rating or skill).  

The exact variables available for each patch are defined in the corresponding C# file under this mod’s `Patches` folder.

### Where to change things

- Look for this mod’s `Settings.json` (or your global BaseMod settings file) and find the **Numbersmith** section.
- Each entry in that section corresponds to one of the patches listed below.
- Adjust the `Enabled` and `Formula` values, then restart your server (or hot‑reload if enabled) to apply your changes.

If you want the technical details of the math engine, see the AngouriMath [docs](https://am.angouri.org/docs/namespaces.html) and [overview article](https://habr.com/en/articles/546926/).

### Available patches

Each of these can be controlled independently in your settings:

- Armor
- CripplingBlowImbue
- CriticalStrikeImbue
- ElementalRendingImbue
- GrantExperience
- HealingDifficulty
- LevelCost
- MeleeArmorRending
- MeleeAttributeDamage
- MissileArmorRending
- MissileAttributeDamage
- NetherRating
- PlayerAccuracy
- PlayerPower
- PlayerTakeDamage
- PlayerTakeDamageOverTime
- SkillChance
- WeaponCriticalChance
- WeaponMagicCritFrequency

