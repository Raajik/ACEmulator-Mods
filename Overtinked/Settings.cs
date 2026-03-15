namespace Overtinked;

// Overtinked configuration. Controls max tinker attempts, difficulty scaling, imbue limits, and which Harmony patches run.
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
}
