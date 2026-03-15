namespace Overtinked;

// Overtinked mod entry point. Registers the mod with the manager and applies Harmony patches via PatchClass.
// Based on the Tinkering sample by aquafir (ACE.BaseMod).
public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(Overtinked), new PatchClass(this));
}
