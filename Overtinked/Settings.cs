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
}
