namespace EmpyreanEchoes.Mutators;
internal class GrowthItem : Mutator
{
    public override bool TryMutateLoot(HashSet<Mutation> mutations, TreasureDeath profile, TreasureRoll roll, WorldObject item)
    {
        if (item.HasItemLevel)
            return false;

        //Only the unmutated?
        if (mutations.Count > 0)
            return false;

        var profileSettings = new ItemLevelProfile(
            S.Settings.GrowthXpBase,
            S.Settings.GrowthXpScaleByTier,
            S.Settings.GrowthTierLevelRange.TryGetValue(profile.Tier, out var range) ? range.Min : 1,
            S.Settings.GrowthTierLevelRange.TryGetValue(profile.Tier, out range) ? range.Max : 1);

        if (!ItemLeveling.ApplyItemLevelProfile(item, profile.Tier, profileSettings))
            return false;

        //Store item tier
        item.SetProperty(FakeBool.GrowthItem, true);
        item.SetProperty(FakeInt.GrowthTier, profile.Tier);
        item.SetProperty(FakeInt.OriginalItemType, (int)roll.ItemType);

        return true;
    }
}
