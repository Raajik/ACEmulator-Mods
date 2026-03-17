namespace Swiftmend;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(Swiftmend), new PatchClass(this));
}

