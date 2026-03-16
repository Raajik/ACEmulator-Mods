namespace Loremaster;

using ACE.Common;
using ACE.Database;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;
using HarmonyLib;

// Patches QuestManager.GetNextSolveTime so the effective repeat cooldown is reduced by the
// player's Loremaster XP bonus percentage (with optional cap).
[HarmonyPatch]
internal static class QuestCooldownReductionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(QuestManager), nameof(QuestManager.GetNextSolveTime), new[] { typeof(string) })]
    public static void GetNextSolveTime_Postfix(string questFormat, QuestManager __instance, ref TimeSpan __result)
    {
        if (__result == TimeSpan.MinValue) return;
        if (PatchClass.Settings is null || !PatchClass.Settings.EnableQuestCooldownReduction) return;

        if (__instance.Creature is not Player player)
            return;

        var reduction = player.GetQuestCooldownReduction();
        if (reduction <= 0) return;

        var questName = QuestManager.GetQuestName(questFormat);
        var quest = DatabaseManager.World.GetCachedQuest(questName);
        if (quest is null) return;

        var playerQuest = __instance.GetQuest(questName);
        if (playerQuest is null) return;

        var currentTime = (uint)Time.GetUnixTime();
        double minDeltaSeconds = quest.MinDelta;
        if (QuestManager.CanScaleQuestMinDelta(quest))
            minDeltaSeconds *= PropertyManager.GetDouble("quest_mindelta_rate", 1).Item;

        var effectiveMinDelta = minDeltaSeconds * (1.0 - reduction);
        var nextSolveTime = playerQuest.LastTimeCompleted + (uint)effectiveMinDelta;

        if (currentTime >= nextSolveTime)
            __result = TimeSpan.MinValue;
        else
            __result = TimeSpan.FromSeconds(nextSolveTime - currentTime);
    }
}
