namespace LeyLineLedger;

public class Mod : BasicMod
{
    public Mod() : base() => Setup("LeyLineLedger", new PatchClass(this));
}
