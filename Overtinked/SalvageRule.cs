namespace Overtinked;

// Single salvage tinker rule: applies to all WCIDs in Wcids (use both IDs for quest-reward pairs).
// EffectKind matches retail behavior; use MinValue/MaxValue for random, or FixedValue for static.
public class SalvageTinkerRule
{
    // Both normal and quest-reward WCIDs so behavior is consistent (e.g. 20993 and 29581 for Steel).
    public uint[] Wcids { get; set; } = Array.Empty<uint>();

    // Display name for logs/config (e.g. "Iron", "Salvaged Steel").
    public string? Name { get; set; }

    // Kind of effect: Damage, DamageVariance, WeaponSpeed, ArmorLevel, DamageMod, WeaponDefense, WeaponOffense,
    // ArmorModVsAcid, ArmorModVsSlash, ArmorModVsBludgeon, ArmorModVsFire, ArmorModVsCold, ArmorModVsElectric, etc.
    public string EffectKind { get; set; } = "";

    // Random range (inclusive). Ignored if FixedValue is set.
    public int MinValue { get; set; }
    public int MaxValue { get; set; }

    // If set, use this value instead of rolling MinValue..MaxValue.
    public int? FixedValue { get; set; }

    public bool Enabled { get; set; } = true;
}
