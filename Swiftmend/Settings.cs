namespace Swiftmend;

public class Settings
{
    public bool Enabled { get; set; } = true;

    public double HotDurationSeconds { get; set; } = 30.0;

    public double HotTickSeconds { get; set; } = 3.0;

    public double BaseSkillPercentPerTick { get; set; } = 0.03;

    public double SpecializedMultiplier { get; set; } = 2.0;

    public bool EnableHealthKits { get; set; } = true;

    public bool EnableStaminaKits { get; set; } = true;

    public bool EnableManaKits { get; set; } = true;
}

