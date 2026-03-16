namespace Overtinked;

// Applies bleed DoT when a weapon with Bleed imbue hits. Uses configurable tick interval and max stacks (Dirty Fighting-style).
[HarmonyPatchCategory(Settings.RecipeManagerCategory)]
public static class BleedImbueCombat
{
    private static readonly object _lock = new();
    private static readonly Dictionary<(uint attackerId, uint targetId), BleedEntry> _entries = new();

    private sealed class BleedEntry
    {
        public int Stacks;
        public bool ChainScheduled;
        public WeakReference<Player>? AttackerRef;
        public WeakReference<Creature>? TargetRef;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.DamageTarget), new Type[] { typeof(Creature), typeof(WorldObject) })]
    public static void PostDamageTarget(Creature target, WorldObject damageSource, ref Player __instance, ref DamageEvent __result)
    {
        if (__result is null || !__result.HasDamage || target is null || damageSource is null)
            return;

        Settings? s = PatchClass.CurrentSettings;
        if (s?.BleedImbue?.Enabled != true)
            return;

        OvertinkedImbueFlags flags = OvertinkedImbueStore.Get(damageSource.Guid.Full);
        if ((flags & OvertinkedImbueFlags.Bleed) == 0)
            return;

        int maxStacks = Math.Max(1, s.BleedImbue.MaxStacks);
        float interval = Math.Max(0.25f, s.BleedImbue.TickIntervalSeconds);
        int damagePerTick = Math.Max(0, s.BleedImbue.DamagePerTick);

        lock (_lock)
        {
            uint aid = __instance.Guid.Full;
            uint tid = target.Guid.Full;
            var key = (aid, tid);
            if (!_entries.TryGetValue(key, out BleedEntry? entry))
            {
                entry = new BleedEntry
                {
                    AttackerRef = new WeakReference<Player>(__instance),
                    TargetRef = new WeakReference<Creature>(target)
                };
                _entries[key] = entry;
            }

            entry.Stacks = Math.Min(entry.Stacks + 1, maxStacks);

            if (!entry.ChainScheduled)
            {
                entry.ChainScheduled = true;
                ScheduleBleedTick((WorldObject)__instance, key, interval, damagePerTick);
            }
        }
    }

    private static void ScheduleBleedTick(WorldObject actor, (uint attackerId, uint targetId) key, float intervalSeconds, int damagePerTick)
    {
        var chain = new ActionChain();
        chain.AddDelaySeconds(intervalSeconds);
        chain.AddAction(actor, () => ApplyBleedTick(key, intervalSeconds, damagePerTick));
        chain.EnqueueChain();
    }

    private static void ApplyBleedTick((uint attackerId, uint targetId) key, float intervalSeconds, int damagePerTick)
    {
        Player? attacker = null;
        Creature? target = null;
        int stacks = 0;
        bool remove = false;

        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out BleedEntry? entry))
                return;
            entry.ChainScheduled = false;
            stacks = entry.Stacks;
            entry.AttackerRef?.TryGetTarget(out attacker);
            entry.TargetRef?.TryGetTarget(out target);
            if (target is null || target.IsDead)
                remove = true;
        }

        if (remove)
        {
            lock (_lock)
                _entries.Remove(key);
            return;
        }

        int damage = damagePerTick * Math.Max(1, stacks);
        if (damage > 0 && target != null)
            target.TakeDamage(attacker, DamageType.Health, (float)damage, false);

        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out BleedEntry? entry))
                return;
            if (entry.TargetRef?.TryGetTarget(out target) != true || target is null || target.IsDead)
            {
                _entries.Remove(key);
                return;
            }
            if (entry.Stacks > 0 && !entry.ChainScheduled)
            {
                entry.ChainScheduled = true;
                WorldObject? actor = attacker ?? target;
                if (actor != null)
                    ScheduleBleedTick(actor, key, intervalSeconds, damagePerTick);
            }
        }
    }
}
