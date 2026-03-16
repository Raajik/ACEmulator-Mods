using System.Reflection.Emit;
using NativeStackable = ACE.Server.WorldObjects.Stackable;

namespace QOL;

[HarmonyPatchCategory(nameof(Features.Stackable))]
internal static class Stackable
{
    static StackableSettings Cfg => S.Settings.Stackable;

    // On creation, inject stack properties into weenie types that ACE doesn't natively stack.
    // Sets MaxStackSize and initialises per-unit weight/value so merges keep burden accurate.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldObjectFactory), nameof(WorldObjectFactory.CreateWorldObject),
        new Type[] { typeof(Weenie), typeof(ObjectGuid) })]
    public static void PostCreateWorldObject(ref WorldObject __result)
    {
        if (__result == null) return;
        if (__result is NativeStackable) return;
        if (!Cfg.IsEnabledType(__result.WeenieType)) return;
        if ((__result.MaxStackSize ?? 0) > 1) return; // already stackable

        // Record per-unit weight and value before MaxStackSize is raised,
        // mirroring what Stackable.SetEphemeralValues does for native stacks.
        __result.StackUnitEncumbrance ??= (ushort?)(__result.EncumbranceVal ?? 0);
        __result.StackUnitValue       ??= (ushort?)(__result.Value           ?? 0);
        __result.StackSize            ??= 1;
        __result.MaxStackSize           = Cfg.MaxStackSize;
    }

    // When ACE calls WorldObject.SetStackSize on a non-native stackable, recalculate
    // total weight and value — normally handled by Stackable.SetStackSize (override),
    // which doesn't run for objects that aren't actual Stackable subclass instances.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldObject), "SetStackSize", new Type[] { typeof(int?) })]
    public static void PostSetStackSize(WorldObject __instance, int? value)
    {
        if (__instance is NativeStackable) return;
        if ((__instance.MaxStackSize ?? 0) <= 1) return;

        __instance.EncumbranceVal = (__instance.StackUnitEncumbrance ?? 0) * (value ?? 1);
        __instance.Value          = (__instance.StackUnitValue       ?? 0) * (value ?? 1);
    }

    // Relaxes the `item is Stackable` class guard in HandleActionStackableMerge so that any
    // WorldObject with MaxStackSize > 1 can participate in the normal drag-to-merge UI.
    //
    // The method contains: ldloc <item>; isinst Stackable; ...check result...
    // Each `isinst Stackable` is replaced with a call to IsEffectivelyStackable, which
    // preserves the same null/non-null return semantics used by the downstream checks.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Player), "HandleActionStackableMerge",
        new Type[] { typeof(uint), typeof(uint), typeof(int) })]
    public static IEnumerable<CodeInstruction> TranspileHandleActionStackableMerge(
        IEnumerable<CodeInstruction> instructions)
    {
        var nativeType = typeof(NativeStackable);
        var helper = typeof(Stackable).GetMethod(
            nameof(IsEffectivelyStackable), BindingFlags.Static | BindingFlags.NonPublic);

        foreach (var instr in instructions)
        {
            if (instr.opcode == OpCodes.Isinst && instr.operand is Type t && t == nativeType)
                yield return new CodeInstruction(OpCodes.Call, helper);
            else
                yield return instr;
        }
    }

    // Returns the input object if it qualifies as stackable (native Stackable subclass or
    // MaxStackSize > 1), or null if it does not. Mirrors isinst return semantics so that
    // the surrounding ldnull/cgt.un or brtrue/brfalse instructions work unchanged.
    static object? IsEffectivelyStackable(object obj) =>
        obj is NativeStackable ? obj :
        obj is WorldObject wo && (wo.MaxStackSize ?? 0) > 1 ? obj :
        null;
}

public class StackableSettings
{
    // Maximum items per stack for non-native stackable types (max 65535).
    public ushort MaxStackSize { get; set; } = 100;

    [JsonPropertyName("// StackableTypes")]
    public string StackableTypesDoc { get; } = "Per-type toggles for extra stackable item categories. Set a value to true to make that WeenieType stackable, or false to leave it unmodified.";

    // When true, generic quest letters, lore pages, and other Book-type items can stack.
    public bool EnableBooks { get; set; } = true;

    // When true, all Key-type items (keys, keyrings, etc.) can stack.
    public bool EnableKeys { get; set; } = true;

    // When true, Generic-type collectibles (mob heads, trophies, misc quest items) can stack.
    public bool EnableGeneric { get; set; } = true;

    // When true, Lockpick-type items can stack.
    public bool EnableLockpicks { get; set; } = true;

    // When true, Scroll-type items (learnable spells, skill scrolls) can stack.
    public bool EnableScrolls { get; set; } = true;

    // Backing list to surface the effective types in Settings.json for reference.
    // See: https://github.com/ACEmulator/ACE/blob/master/Source/ACE.Entity/Enum/WeenieType.cs
    public List<WeenieType> StackableTypes { get; set; } =
    [
        WeenieType.Book,      // quest letters, scrolls
        WeenieType.Key,       // keys of all kinds
        WeenieType.Generic,   // mob heads, trophies, misc collectibles
        WeenieType.Lockpick,  // lockpicks
        WeenieType.Scroll,    // learnable scroll items
    ];

    // Returns true when a given WeenieType should be treated as stackable according to the
    // per-type toggles above. Falls back to StackableTypes for any types without a toggle.
    public bool IsEnabledType(WeenieType weenieType) => weenieType switch
    {
        WeenieType.Book     => EnableBooks,
        WeenieType.Key      => EnableKeys,
        WeenieType.Generic  => EnableGeneric,
        WeenieType.Lockpick => EnableLockpicks,
        WeenieType.Scroll   => EnableScrolls,
        _                   => StackableTypes.Contains(weenieType),
    };
}
