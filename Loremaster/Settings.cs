namespace Loremaster;

public class Settings
{
    // ─────────────────────────────────────────────────────────────────────────
    // QUEST POINT SYSTEM (inherited from QuestBonus)
    // ─────────────────────────────────────────────────────────────────────────

    // XP multiplier per Quest Point (as a percentage).
    // Formula: 1 + (QP * BonusPerQuestPoint / 100)
    // Default (0.5): 100 QP → 1.5x, 200 QP → 2.0x, 50 QP → 1.25x
    public float BonusPerQuestPoint { get; set; } = 0.5f;

    // QP awarded for any quest not listed in QuestBonuses.
    // Set to 0 to only reward explicitly listed quests.
    public float DefaultPoints { get; set; } = 1;

    // Per-quest QP overrides. Keys are case-sensitive internal quest names (see Quests.txt).
    // Value 0 = tracked but awards no QP. Unlisted quests use DefaultPoints.
    public Dictionary<string, float> QuestBonuses { get; set; } = new()
    {
        ["PathwardenComplete"]        = 1,
        ["PathwardenFound1111"]       = 1,
        ["StipendsCollectedInAMonth"] = 1,
        ["StipendTimer_08"]           = 1,
        ["StipendTimer_Monthly"]      = 1,
    };

    // ─────────────────────────────────────────────────────────────────────────
    // ACCOUNT-WIDE QUEST TRACKING
    // ─────────────────────────────────────────────────────────────────────────

    // When true, QP and the XP multiplier are calculated from unique quests solved
    // across ALL characters on the account, not just the logged-in character.
    public bool UseAccountWideQuests { get; set; } = true;

    // ─────────────────────────────────────────────────────────────────────────
    // ONE-TIME COMPLETION BONUS XP
    // ─────────────────────────────────────────────────────────────────────────

    // Grant an XP bonus on each quest solve (first and repeat). Stacks with the ongoing multiplier.
    // Amount = DefaultCompletionBonusXpMultiplier × (XP needed for player's next level).
    public bool EnableCompletionBonusXp { get; set; } = true;

    // Multiplier applied to (XP needed for next level) for the completion bonus.
    // 0.30 = 30% of current level-up cost. Set to 0 to rely solely on CompletionBonusXpOverrides.
    public float DefaultCompletionBonusXpMultiplier { get; set; } = 0.30f;

    // Per-quest multiplier overrides for the first-solve XP bonus (case-sensitive, see Quests.txt).
    // Same unit as DefaultCompletionBonusXpMultiplier. Set to 0.0 to suppress for a specific quest.
    public Dictionary<string, float> CompletionBonusXpOverrides { get; set; } = new()
    {
        ["PathwardenComplete"]        = 1.0f,
        ["PathwardenFound1111"]       = 1.0f,
        ["StipendsCollectedInAMonth"] = 1.0f,
        ["StipendTimer_08"]           = 1.0f,
        ["StipendTimer_Monthly"]      = 1.0f,
    };

    // ─────────────────────────────────────────────────────────────────────────
    // REPEAT SOLVE BONUS LOOT
    // ─────────────────────────────────────────────────────────────────────────

    // Award one weighted-random item on every repeat solve (2nd+). First solves are not affected.
    // Loot tables are configured in RepeatSolveLoot.json in the mod folder.
    public bool EnableRepeatSolveLoot { get; set; } = true;

    // ─────────────────────────────────────────────────────────────────────────
    // MILESTONE BROADCASTS
    // ─────────────────────────────────────────────────────────────────────────

    // Broadcast a server-wide message when an account hits a milestone unique-quest count.
    public bool EnableMilestoneBroadcasts { get; set; } = true;

    // Account-wide unique quest counts that trigger a broadcast. Add or remove freely.
    public List<int> MilestoneThresholds { get; set; } = new()
    {
        25, 50, 100, 250, 500, 750, 1000,
        1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000,
        5500, 6000, 6500, 7000, 7500, 8000, 8500, 9000, 9500, 10000
    };

    // Formula for bonus QP per milestone: (MilestoneBonusQPPercent / 100) * MilestoneBonusQPBase.
    // Default 10% of 100 = 10 QP per milestone.
    public float MilestoneBonusQPPercent { get; set; } = 10f;
    public float MilestoneBonusQPBase { get; set; } = 100f;

    // Per-threshold overrides. When a threshold is present here, this QP is used instead of the formula.
    public Dictionary<int, float> MilestoneBonusQPOverrides { get; set; } = new();

    // Broadcast message format.
    // {0} = character name, {1} = ordinal milestone (e.g. "50th"), {2} = bonus QP awarded.
    public string MilestoneBroadcastFormat { get; set; } =
        "[Loremaster] {0} has just completed their {1} unique quest and earned {2} bonus quest points!";

    // ─────────────────────────────────────────────────────────────────────────
    // QUEST COOLDOWN REDUCTION
    // ─────────────────────────────────────────────────────────────────────────

    // When true, quest repeat cooldowns are reduced by the same percentage as the player's XP bonus.
    // E.g. 25% XP bonus → wait 75% of the normal repeat time.
    public bool EnableQuestCooldownReduction { get; set; } = true;

    // Cap on cooldown reduction (0–1). Null = uncapped. E.g. 0.9 = at most 90% reduction.
    public float? QuestCooldownReductionCap { get; set; } = 0.95f;

    // Quest names that act as one-time flags (e.g. portal eligibility). Once completed, the player
    // is always considered eligible; cooldown reduction does not apply. Case-sensitive (see Quests.txt).
    public List<string> PermanentFlagQuests { get; set; } = new()
    {
        "AcademyTokenGiven",
    };
}
