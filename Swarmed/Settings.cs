namespace Swarmed;

// Configuration for call-for-help reinforcement spawns (landscape vs dungeon, chance, count, health, messages).
public class Settings
{
    public bool LandscapeEnabled { get; set; } = true;
    public float LandscapeChance { get; set; } = 0.10f;
    public int LandscapeSpawnMin { get; set; } = 1;
    public int LandscapeSpawnMax { get; set; } = 5;

    public bool DungeonEnabled { get; set; } = false;
    public float DungeonChance { get; set; } = 0.15f;
    public int DungeonSpawnMin { get; set; } = 1;
    public int DungeonSpawnMax { get; set; } = 3;

    public float ReinforcementHealthMin { get; set; } = 0.3f;
    public float ReinforcementHealthMax { get; set; } = 0.7f;

    public float ReinforcementScaleMin { get; set; } = 0.3f;
    public float ReinforcementScaleMax { get; set; } = 0.8f;

    public int MaxCallerHealth { get; set; } = 3000;

    public float ReinforcementXpBonusMin { get; set; } = 0.75f;
    public float ReinforcementXpBonusMax { get; set; } = 2.0f;

    // Format string: {0} = creature name. One chosen at random when the event triggers.
    public List<string> CallForHelpMessages { get; set; } = new()
    {
        "The {0} cries out for help!",
        "The {0} howls for its kin!",
        "The {0} shrieks as it falls!",
        "The {0} calls to its allies!",
        "The {0} wails in desperation!",
        "The {0} roars for reinforcement!",
        "The {0} screeches a warning!",
        "The {0} bellows in rage!",
        "The {0} keens for aid!",
        "The {0} summons its brethren!",
        "The {0} yowls for backup!",
        "The {0} bleats for help!",
        "Upon seeing the {0} fall, reinforcements arrive swiftly!",
        "The fall of the {0} draws more of its kind!",
        "Reinforcements rush in as the {0} goes down!",
        "More {0} pour in to avenge their fallen!",
        "The death of the {0} has been noticed—allies close in!",
        "As the {0} falls, its kin answer the call!",
        "Reinforcements surge forward at the {0}'s demise!",
        "The {0}'s fall signals others to join the fray!",
        "New foes emerge where the {0} fell!",
        "The {0} falls—and its brethren rise to take its place!",
        "Word of the {0}'s defeat spreads; reinforcements converge!",
        "No sooner does the {0} fall than more appear!",
        "The {0} dies with a final cry—and the pack responds!",
        "Reinforcements stream in where the {0} fell!",
        "With the {0} down, its allies rush to the fight!",
        "The {0} falls; from nearby, more close in!",
        "Its death cry echoes—more {0} answer!",
        "The fall of one {0} brings others running!",
    };
}
