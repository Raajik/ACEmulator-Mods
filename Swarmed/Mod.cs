namespace Swarmed;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(Swarmed), new PatchClass(this));
}
