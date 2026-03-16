namespace Swarmed;

[HarmonyPatch]
public static class CallForHelp
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Creature), nameof(Creature.Die), new Type[] { typeof(DamageHistoryInfo), typeof(DamageHistoryInfo) })]
    public static void PostDie(DamageHistoryInfo lastDamager, DamageHistoryInfo topDamager, ref Creature __instance)
    {
        if (__instance == null)
            return;

        Settings? settings = PatchClass.CurrentSettings;
        if (settings == null)
            return;

        bool isDungeon = __instance.CurrentLandblock?.IsDungeon ?? false;
        bool landscape = !isDungeon;

        bool enabled = landscape ? settings.LandscapeEnabled : settings.DungeonEnabled;
        float chance = landscape ? settings.LandscapeChance : settings.DungeonChance;
        int spawnMin = landscape ? settings.LandscapeSpawnMin : settings.DungeonSpawnMin;
        int spawnMax = landscape ? settings.LandscapeSpawnMax : settings.DungeonSpawnMax;

        if (!enabled || chance <= 0)
            return;

        if (ThreadSafeRandom.Next(0f, 1f) > chance)
            return;

        if (lastDamager == null || !lastDamager.IsPlayer || lastDamager.TryGetPetOwnerOrAttacker() is not Player)
            return;

        if (__instance.CurrentLandblock == null)
            return;

        uint wcid = __instance.WeenieClassId;
        string creatureName = __instance.Name ?? "creature";
        var deathLocation = __instance.Location;
        uint originalMaxHealth = __instance.Health?.MaxValue ?? 1;
        float originalScale = __instance.ObjScale ?? 1f;

        Weenie? weenie = DatabaseManager.World.GetCachedWeenie(wcid);
        if (weenie == null)
            return;

        if (settings.CallForHelpMessages == null || settings.CallForHelpMessages.Count == 0)
            return;

        string messageFormat = settings.CallForHelpMessages[ThreadSafeRandom.Next(0, settings.CallForHelpMessages.Count)];
        string message = string.Format(messageFormat, creatureName);
        __instance.EnqueueBroadcast(new GameMessageSystemChat(message, ChatMessageType.Combat), WorldObject.LocalBroadcastRange, ChatMessageType.Combat);

        int count = Math.Clamp(ThreadSafeRandom.Next(spawnMin, spawnMax + 1), spawnMin, spawnMax);
        float healthMin = Math.Clamp(settings.ReinforcementHealthMin, 0.01f, 1f);
        float healthMax = Math.Clamp(settings.ReinforcementHealthMax, 0.01f, 1f);
        if (healthMin > healthMax)
            (healthMin, healthMax) = (healthMax, healthMin);
        float scaleMin = Math.Clamp(settings.ReinforcementScaleMin, 0.01f, 1f);
        float scaleMax = Math.Clamp(settings.ReinforcementScaleMax, 0.01f, 1f);
        if (scaleMin > scaleMax)
            (scaleMin, scaleMax) = (scaleMax, scaleMin);

        for (int i = 0; i < count; i++)
        {
            ObjectGuid guid = GuidManager.NewDynamicGuid();
            WorldObject? wo = WorldObjectFactory.CreateWorldObject(weenie, guid);
            if (wo is not Creature creature)
            {
                GuidManager.RecycleDynamicGuid(guid);
                continue;
            }

            creature.Location = new Position(deathLocation);
            float healthMult = (float)ThreadSafeRandom.Next(healthMin, healthMax);
            uint newMaxHealth = (uint)Math.Max(1, originalMaxHealth * healthMult);
            creature.Health.Ranks = newMaxHealth;
            creature.Health.Current = newMaxHealth;
            float scaleMult = (float)ThreadSafeRandom.Next(scaleMin, scaleMax);
            creature.ObjScale = originalScale * scaleMult;
            LandblockManager.AddObject(creature);
        }
    }
}
