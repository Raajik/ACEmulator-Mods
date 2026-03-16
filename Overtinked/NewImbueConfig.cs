namespace Overtinked;

// Config for a single custom imbue type (Bleed, Cleaving, Nether Rending).
public class NewImbueConfig
{
    // Salvage WCIDs that apply this imbue (e.g. Serpentine for Bleed). Include both IDs for dual-WCID items.
    public uint[] SalvageWcids { get; set; } = Array.Empty<uint>();

    public string? Name { get; set; }
    public bool Enabled { get; set; } = true;
}

// Bleed-specific: stacking DoT. Stored here; combat tick logic can use these values.
public class BleedImbueConfig : NewImbueConfig
{
    // Damage per tick (e.g. 1).
    public int DamagePerTick { get; set; } = 1;

    // Max stacks (e.g. 10 or 20).
    public int MaxStacks { get; set; } = 10;

    // Tick interval in seconds (1 preferred; 3–5 if needed for performance).
    public float TickIntervalSeconds { get; set; } = 1f;
}
