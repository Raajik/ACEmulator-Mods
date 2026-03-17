namespace Swarmed;

[HarmonyPatch]
public static class CallForHelp
{
    // Custom PropertyFloat slots for Swarmed reinforcement XP logic
    const int SwarmedReinforcementXpBonusPropertyId = 40110;
    const int SwarmedPlayerXpBonusPropertyId = 40111;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Creature), nameof(Creature.Die), new Type[] { typeof(DamageHistoryInfo), typeof(DamageHistoryInfo) })]
    public static void PostDie(DamageHistoryInfo lastDamager, DamageHistoryInfo topDamager, ref Creature __instance)
    {
        if (__instance == null)
            return;

        Settings? settings = PatchClass.CurrentSettings;
        if (settings == null)
            return;

        var reinforcementBonusProp = (PropertyFloat)SwarmedReinforcementXpBonusPropertyId;
        var playerBonusProp = (PropertyFloat)SwarmedPlayerXpBonusPropertyId;

        double? reinforcementBonus = __instance.GetProperty(reinforcementBonusProp);

        Player? killerPlayer = null;
        if (lastDamager != null && lastDamager.IsPlayer)
            killerPlayer = lastDamager.TryGetPetOwnerOrAttacker() as Player;

        if (reinforcementBonus.HasValue && killerPlayer != null)
        {
            double bonusValue = reinforcementBonus.Value;
            if (bonusValue > 0)
            {
                killerPlayer.SetProperty(playerBonusProp, bonusValue);
            }

            return;
        }

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

        int maxCallerHealth = settings.MaxCallerHealth;
        if (maxCallerHealth > 0 && originalMaxHealth > maxCallerHealth)
            return;

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

        float xpBonusMin = Math.Clamp(settings.ReinforcementXpBonusMin, 0f, 10f);
        float xpBonusMax = Math.Clamp(settings.ReinforcementXpBonusMax, 0f, 10f);
        if (xpBonusMin > xpBonusMax)
            (xpBonusMin, xpBonusMax) = (xpBonusMax, xpBonusMin);

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

            if (xpBonusMax > 0f)
            {
                float bonus = (float)ThreadSafeRandom.Next(xpBonusMin, xpBonusMax);
                if (bonus > 0f)
                    creature.SetProperty(reinforcementBonusProp, bonus);
            }

            LandblockManager.AddObject(creature);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.GrantXP), new Type[] { typeof(long), typeof(XpType), typeof(ShareType) })]
    public static void PreGrantXP(ref long amount, XpType xpType, ShareType shareType, ref Player __instance)
    {
        if (xpType != XpType.Kill)
            return;

        var playerBonusProp = (PropertyFloat)SwarmedPlayerXpBonusPropertyId;
        double? bonus = __instance.GetProperty(playerBonusProp);
        if (!bonus.HasValue || bonus.Value <= 0)
            return;

        long extra = (long)(amount * bonus.Value);
        if (extra <= 0)
        {
            __instance.RemoveProperty(playerBonusProp);
            return;
        }

        amount += extra;
        __instance.RemoveProperty(playerBonusProp);
    }
}
