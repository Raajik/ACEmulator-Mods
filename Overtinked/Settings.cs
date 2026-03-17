namespace Overtinked;

// Overtinked configuration. Controls max tinker attempts, difficulty scaling, imbue limits, salvage rules, and failure behavior.
public class Settings
{
    // Max number of tinkers allowed per item (recipe check uses MaxTries - 1).
    public int MaxTries { get; set; } = 3;

    // Difficulty step between each extra tinker tier.
    public float Scale { get; set; } = .5f;

    // Max imbue effects allowed on an item (checked via bit count of ImbuedEffect).
    public int MaxImbueEffects { get; set; } = 2;

    // Harmony patch category for RecipeManager patches; keep distinct from other tinkering mods.
    public const string RecipeManagerCategory = "OvertinkedRecipeManagerPatch";

    // When true, patches RecipeManager.UseObjectOnTarget (and related) for tinkering flow.
    public bool EnableRecipeManagerPatch { get; set; } = true;

    // Per-salvage tinker rules (random or fixed). Include both WCIDs for quest-reward pairs.
    public List<SalvageTinkerRule> SalvageRules { get; set; } = new();

    // When true, failed tinkers apply the opposite effect instead of destroying the item.
    public bool EnableFailureRedesign { get; set; } = false;

    // When true, failed imbue tinkers (standard + custom) add +1 Workmanship to the item instead of destroying it, capped at 10.
    public bool EnableDefaultImbueFailureWorkmanship { get; set; } = false;

    // Bleed imbue (e.g. Serpentine 21075). Apply SalvageWcids with both IDs for dual-WCID items.
    public BleedImbueConfig BleedImbue { get; set; } = new();

    // Cleaving imbue (e.g. Tiger Eye 21081).
    public NewImbueConfig CleavingImbue { get; set; } = new();

    // Nether Rending imbue (e.g. Onyx 21064).
    public NewImbueConfig NetherRendingImbue { get; set; } = new();

    // Buffed jewelry imbues (Hematite HP, Malachite Stam + melee, Lavender Jade mana + damage from mana, etc.).
    public List<BuffedImbueRule> BuffedImbueRules { get; set; } = new();

    // When true, send the player a short message when a custom salvage/imbue is applied.
    public bool ShowPlayerSalvageMessage { get; set; } = false;

    // When true, quest items are given workmanship on creation so they can be tinkered like standard lootgen items.
    public bool EnableQuestItemWorkmanship { get; set; } = true;

    // When true, quest items roll initial tinkering-style effects (imbues/slayers/salvage-like boosts) on creation.
    public bool EnableQuestItemInitialEffects { get; set; } = true;

    // When true, quest items are initialized with item XP and can level up similarly to growth/rare items.
    public bool EnableQuestItemLeveling { get; set; } = true;

    // Minimum and maximum workmanship rolled for quest items when EnableQuestItemWorkmanship is true.
    public int QuestItemWorkmanshipMin { get; set; } = 4;
    public int QuestItemWorkmanshipMax { get; set; } = 8;

    // Base XP per quest-item level and global XP scale; mirrors CHANGEExpansion growth-style settings.
    public long QuestItemXpBase { get; set; } = 1_000_000;
    public double QuestItemXpScale { get; set; } = 1.2;

    // Global min/max quest-item level range when EnableQuestItemLeveling is true.
    public int QuestItemMaxLevelMin { get; set; } = 3;
    public int QuestItemMaxLevelMax { get; set; } = 8;

    // Global knobs for initial quest-item effects; favors imbues, then slayers, then salvage-like boosts.
    public QuestItemEffectSettings QuestItemEffects { get; set; } = new();
}

// High-level quest-item effect configuration. Keeps v1 simple with global weights and flags.
public class QuestItemEffectSettings
{
    // Allow standard imbues (ImbuedEffectType flags) to roll on quest items.
    public bool AllowStandardImbues { get; set; } = true;

    // Allow Overtinked custom imbues (Bleed/Cleaving/Nether Rending) to roll where appropriate.
    public bool AllowCustomImbues { get; set; } = true;

    // Allow slayer mods (SlayerCreatureType / SlayerDamageBonus) to roll on quest items.
    public bool AllowSlayer { get; set; } = true;

    // Allow small numeric boosts using SalvageEffectApplier (Damage, ArmorLevel, ArmorModVsX, etc.).
    public bool AllowSalvageLikeBoosts { get; set; } = true;

    // Relative weights when picking initial effects; higher values increase likelihood.
    public int ImbueWeight { get; set; } = 3;
    public int SlayerWeight { get; set; } = 2;
    public int SalvageWeight { get; set; } = 1;
}
