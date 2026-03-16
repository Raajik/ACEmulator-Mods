namespace Overtinked;

// Standard imbue salvage WCIDs (both normal and quest-reward pairs). Used for failure -> Workmanship when EnableDefaultImbueFailureWorkmanship.
public static class ImbueSalvageWcids
{
    // Retail imbue salvages: Armor Rending, Crippling Blow, Critical Strike, element rendings, defense imbues, etc.
    private static readonly HashSet<uint> DefaultWcids = new()
    {
        21079, 29582,  // Sunstone (Armor Rending)
        21049, 29575,  // Fire Opal (Crippling Blow)
        21040, 29573,  // Black Opal (Critical Strike)
        21037, 29571,  // Aquamarine (Cold Rending)
        21039,         // Black Garnet (Pierce Rending)
        21048, 29574,  // Emerald (Acid Rending)
        21056,         // Jet (Electric Rending)
        21069,         // Red Garnet (Fire Rending)
        21054, 29577,  // Imperial Topaz (Slash Rending)
        21086, 30260,  // White Sapphire (Bludgeon Rending)
        21066,         // Peridot (Melee Defense)
        21088,         // Yellow Topaz (Missile Defense)
        21089,         // Zircon (Magic Defense)
    };

    public static HashSet<uint> Build(Settings s)
    {
        var set = new HashSet<uint>(DefaultWcids);
        if (s.BleedImbue?.SalvageWcids != null)
            foreach (uint w in s.BleedImbue.SalvageWcids)
                set.Add(w);
        if (s.CleavingImbue?.SalvageWcids != null)
            foreach (uint w in s.CleavingImbue.SalvageWcids)
                set.Add(w);
        if (s.NetherRendingImbue?.SalvageWcids != null)
            foreach (uint w in s.NetherRendingImbue.SalvageWcids)
                set.Add(w);
        return set;
    }
}
