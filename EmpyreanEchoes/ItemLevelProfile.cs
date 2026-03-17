namespace EmpyreanEchoes;

public class ItemLevelProfile
{
    public long BaseXp { get; set; }

    public double ScaleByTier { get; set; }

    public int MinLevel { get; set; }

    public int MaxLevel { get; set; }

    public ItemLevelProfile(long baseXp, double scaleByTier, int minLevel, int maxLevel)
    {
        BaseXp = baseXp;
        ScaleByTier = scaleByTier;
        MinLevel = minLevel;
        MaxLevel = maxLevel;
    }
}

