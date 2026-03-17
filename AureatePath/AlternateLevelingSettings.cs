namespace EasyEnlightenment;

public class AlternateLevelingSettings
{
    public bool Enabled { get; set; } = true;

    public int LevelPropertyStart { get; set; } = 20_800;
    public int SpentPropertyStart { get; set; } = 20_800;

    public bool PreferStandard { get; set; } = true;
    public bool OffsetByLastStandard { get; set; } = true;

    public LevelCost Attribute { get; set; } = new(10_000_000, 2, 5_000_000, growthType: GrowthType.Polynomial);
    public LevelCost Vital { get; set; } = new(1_000_000_000, 1_000_000_000, 0, growthType: GrowthType.Linear);
    public LevelCost Trained { get; set; } = new();
    public LevelCost Specialized { get; set; } = new(100, 1.5);
}

