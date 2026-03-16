namespace Overtinked;

// Buffed jewelry/imbue: replace default effect with configurable ranges (e.g. Hematite +25–50 HP).
public class BuffedImbueRule
{
    public uint[] Wcids { get; set; } = Array.Empty<uint>();
    public string? Name { get; set; }

    // Primary stat: MaxHealth, MaxStamina, MaxMana.
    public string PrimaryStat { get; set; } = "";
    public int PrimaryMin { get; set; }
    public int PrimaryMax { get; set; }

    // Optional secondary: e.g. damage rating from mana/stam (percent or flat).
    public string? SecondaryStat { get; set; }
    public double SecondaryValue { get; set; }

    // ACE ImbuedEffectType name so the item shows the correct imbue (e.g. WarriorVitality, WarriorVigor).
    public string? ImbuedEffectTypeName { get; set; }

    public bool Enabled { get; set; } = true;
}
