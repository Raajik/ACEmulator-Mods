namespace Aptitude;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(Aptitude), new PatchClass(this));
}
