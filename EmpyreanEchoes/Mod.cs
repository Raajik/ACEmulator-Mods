namespace EmpyreanEchoes;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(EmpyreanEchoes), new PatchClass(this));
}