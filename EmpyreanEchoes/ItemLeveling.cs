namespace EmpyreanEchoes;

public static class ItemLeveling
{
    public static bool ApplyItemLevelProfile(WorldObject item, int tier, ItemLevelProfile profile)
    {
        if (item is null)
            return false;

        if (item.HasItemLevel)
            return false;

        long baseXp = profile.BaseXp;
        if (baseXp <= 0)
            baseXp = 1_000_000;

        double scale = profile.ScaleByTier;
        if (scale <= 0)
            scale = 1.0;

        long xpCost = (long)(baseXp * Math.Pow(scale, tier - 1));

        int minLevel = profile.MinLevel;
        int maxLevel = profile.MaxLevel;
        if (minLevel > maxLevel)
            (minLevel, maxLevel) = (maxLevel, minLevel);

        int maxItemLevel = minLevel == maxLevel ? minLevel : Random.Shared.Next(minLevel, maxLevel + 1);
        if (maxItemLevel <= 0)
            return false;

        item.ItemXpStyle = ItemXpStyle.ScalesWithLevel;
        item.ItemTotalXp = 0;
        item.ItemMaxLevel = maxItemLevel;
        item.ItemBaseXp = xpCost;

        return true;
    }
}

