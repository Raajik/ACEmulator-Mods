
namespace AethericWeaver;

// AethericWeaver mod entry point; wires PatchClass into ACE.BaseMod
public class Mod : BasicMod
{
    public static Mod Instance { get; private set; }

    public Mod() : base()
    {
        Instance = this;
        Setup(nameof(AethericWeaver), new PatchClass(this));
    }
}