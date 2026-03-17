namespace EmpyreanEchoes;

public partial class Settings
{
    public bool Verbose { get; set; } = false;

    public List<Feature> Features { get; set; } = new()
    {
        //Feature.FakePropertyCache,
        Feature.CreatureEx,
        Feature.MutatorHooks,
    };

    public List<CreatureExType> CreatureFeatures { get; set; } = new()
    {
        CreatureExType.Horde,
    };

    #region FakePropertyCache
    public PropertyBonusSettings BonusCaps { get; set; } = new();

    #endregion

    #region ProcRateOverride
    public double CloakProcRate { get; set; } = .05; //5%
    public float AetheriaProcRate { get; set; } = .05f;
    #endregion

    public string LootProfilePath { get; } = Path.Combine(ModManager.ModPath, "LootProfiles");//Path.Combine(Mod.ModPath, "LootProfiles");
    public bool LootProfileUseUsername { get; set; } = true;

    public float HardcoreSecondsBetweenDeathAllowed { get; set; } = 60;
    public int HardcoreStartingLives { get; set; } = 5;

    public double CreatureChance { get; set; } = 0;

    public SpellSettings SpellSettings { get; set; } = new();

    #region Pools (e.g., Odds / Targets / Sets / Spell IDs / Species)
    //For convenience.  People can make their own
    public Dictionary<string, Odds> Odds { get; set; } = Enum.GetValues<OddsGroup>().ToDictionary(x => x.ToString(), x => x.OddsOf());
    public Dictionary<string, TreasureItemType_Orig[]> TargetGroups { get; set; } = new()
    {
        [nameof(TargetGroup.Accessories)] = TargetGroup.Accessories.SetOf(),
        [nameof(TargetGroup.ArmorClothing)] = TargetGroup.ArmorClothing.SetOf(),
        [nameof(TargetGroup.Equipables)] = TargetGroup.Equipables.SetOf(),
        [nameof(TargetGroup.Weapon)] = TargetGroup.Weapon.SetOf(),
        [nameof(TargetGroup.Wearables)] = TargetGroup.Wearables.SetOf(),
    };
    public Dictionary<string, WeenieType[]> WeenieTypeGroups { get; set; } = new()
    {
        [nameof(WeenieTypeGroup.Container)] = WeenieTypeGroup.Container.SetOf(),
    };
    //Full pools defined in enum helpers or it can be done explicitly like TargetGroups
    public Dictionary<string, CreatureType[]> CreatureTypeGroups { get; set; } = Enum.GetValues<CreatureTypeGroup>().ToDictionary(x => x.ToString(), x => x.SetOf());
    public Dictionary<string, EquipmentSet[]> EquipmentSetGroups { get; set; } = Enum.GetValues<EquipmentSetGroup>().ToDictionary(x => x.ToString(), x => x.SetOf());
    public Dictionary<string, SpellId[]> SpellGroups { get; set; } = Enum.GetValues<SpellGroup>().ToDictionary(x => x.ToString(), x => x.SetOf());
    public Dictionary<string, Augment[]> AugmentGroups { get; set; } = Enum.GetValues<AugmentGroup>().ToDictionary(x => x.ToString(), x => x.SetOf());
    #endregion
}
