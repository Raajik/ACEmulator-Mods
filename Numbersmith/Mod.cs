namespace Numbersmith;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(Numbersmith), new PatchClass(this));
}