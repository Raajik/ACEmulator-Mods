using ACE.Server.Entity;

namespace LeyLineLedger;

[HarmonyPatch]
internal class DeathPenalty
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.GetNumCoinsDropped))]
    public static void PostGetNumCoinsDropped(ref int __result)
    {
        //Cap coins dropped
        if (PatchClass.Settings.MaxCoinsDropped < 0)
            return;

        __result = Math.Min(__result, PatchClass.Settings.MaxCoinsDropped);
    }
}
