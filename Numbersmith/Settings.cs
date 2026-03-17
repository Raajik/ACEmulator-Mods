using Numbersmith.Patches;

namespace Numbersmith;

public class Settings
{
    // Maximum character level Numbersmith uses when overriding ACE's XP curve.
    // This does NOT change any other mods' caps; it only affects Numbersmith's level/XP patches.
    public uint MaxLevel { get; set; } = 275;

    // List of formula-driven patches Numbersmith can apply.
    // Each entry controls a single patch type, its Enabled flag, and its Formula string.
    public List<AngouriPatchSettings> Formulas { get; set; } = new()
    {
        // Default formulas for each patch. Variable names are documented in each patch class.
        new (nameof(ArmorMod)),
        new (nameof(CripplingBlowImbueMod)),
        new (nameof(CriticalStrikeImbueMod)),
        new (nameof(ElementalRendingImbue)),
        new (nameof(GrantExperience), false),
        new (nameof(HealingDifficulty)),
        new (nameof(LevelCost), false),
        new (nameof(MagicWeaponCriticalChance)),
        new (nameof(MeleeArmorRending)),
        new (nameof(MeleeAttributeDamage)),
        new (nameof(MissileArmorRending)),
        new (nameof(MissileAttributeDamage), false),
        new (nameof(NetherRating), false),
        new (nameof(PlayerAccuracyMod)),
        new (nameof(PlayerPowerMod)),
        new (nameof(WeaponCriticalChance)),
        new (nameof(SkillChance)),
        new (nameof(PlayerTakeDamage)),
        new (nameof(PlayerTakeDamageOverTime)),
    };

    // When true, Numbersmith logs a summary of which patches were loaded and which formulas are active.
    public bool Verbose { get; set; } = false;
}