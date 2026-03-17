namespace EmpyreanEchoes;

public partial class Settings
{
    // Mutator configuration
    public string MutatorPath => Path.Combine(Mod.Instance.ModPath, "Mutators");

    public List<MutatorSettings> Mutators { get; set; } =
        new()
        {
            new MutatorSettings(Mutation.SampleMutator.ToString())
            {
                Odds = nameof(OddsGroup.Always),
                Events = MutationEvent.Containers | MutationEvent.EmoteGive
            },
            // Growth-style leveling from treasure containers.
            new MutatorSettings(Mutation.GrowthItem.ToString())
            {
                Odds = nameof(OddsGroup.Always),
                TreasureTargets = nameof(TargetGroup.Weapon),
                Events = MutationEvent.Containers
            },
            // LootGrowthItem: generalized loot leveling + pre-imbues for eligible equipables.
            new MutatorSettings(Mutation.LootGrowthItem.ToString())
            {
                Odds = nameof(OddsGroup.Always),
                TreasureTargets = nameof(TargetGroup.Equipables),
                Events = MutationEvent.Containers
            },
        };

    // GrowthItem mutator settings
    public Dictionary<WeenieType, string> GrowthAugments { get; set; } = new()
    {
        [WeenieType.MeleeWeapon] = nameof(AugmentGroup.Weapon),
        [WeenieType.Caster] = nameof(AugmentGroup.Weapon),
        [WeenieType.MissileLauncher] = nameof(AugmentGroup.Weapon),
    };

    public Dictionary<WeenieType, Dictionary<int, Augment>> GrowthFixedLevelAugments { get; set; } = new()
    {
        [WeenieType.MeleeWeapon] = new Dictionary<int, Augment>()
        {
            [1] = Augment.RendAll,
        },
        [WeenieType.Caster] = new Dictionary<int, Augment>()
        {
            [1] = Augment.RendAll,
        },
        [WeenieType.MissileLauncher] = new Dictionary<int, Augment>()
        {
            [1] = Augment.RendAll,
        }
    };

    public Dictionary<int, IntRange> GrowthTierLevelRange { get; set; } =
        Enumerable.Range(1, 8).ToDictionary(x => x, x => Range.Int(x * 2 - 1, x * 2 + 3));

    public long GrowthXpBase { get; set; } = 1_000_000;

    public double GrowthXpScaleByTier { get; set; } = 1.2;

    // LootGrowth mutator settings
    public bool EnableLootItemLeveling { get; set; } = true;

    public bool EnableLootItemPreImbue { get; set; } = true;

    public long LootItemXpBase { get; set; } = 1_000_000;

    public double LootItemXpScale { get; set; } = 1.2;

    public int LootItemMaxLevelMin { get; set; } = 2;

    public int LootItemMaxLevelMax { get; set; } = 6;

    public double LootItemPreImbueChance { get; set; } = 0.02;

    public WeenieType[] LootItemLevelingEligibleWeenieTypes { get; set; } =
    {
        WeenieType.MeleeWeapon,
        WeenieType.MissileLauncher,
        WeenieType.Caster,
        WeenieType.Clothing,
    };

    // Set mutator settings
    public Dictionary<TreasureItemType_Orig, EquipmentSetGroup> ItemTypeEquipmentSets { get; set; } = new()
    {
        [TreasureItemType_Orig.Armor] = EquipmentSetGroup.Armor,
        [TreasureItemType_Orig.Clothing] = EquipmentSetGroup.Armor,
        [TreasureItemType_Orig.Cloak] = EquipmentSetGroup.Cloak,
        [TreasureItemType_Orig.Jewelry] = EquipmentSetGroup.Cloak,
    };

    // Slayer mutator settings
    public string Slayers { get; set; } = nameof(CreatureTypeGroup.Popular);

    public Dictionary<int, float> SlayerPower { get; set; } = new()
    {
        [0] = .5f,
        [1] = 1f,
        [2] = 1.5f,
        [3] = 2f,
        [4] = 2.5f,
        [5] = 3f,
        [6] = 3.5f,
        [7] = 4f,
        [8] = 4.5f,
    };

    // ProcOnHit / proc settings
    public string ProcOnSpells { get; set; } = nameof(SpellGroup.CloakOnly);
}

