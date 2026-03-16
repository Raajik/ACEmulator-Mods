namespace Overtinked;

// Custom imbue flags for weapons (Bleed, Cleaving, Nether Rending). Stored by item Guid; combat code can check these.
[Flags]
public enum OvertinkedImbueFlags : byte
{
    None = 0,
    Bleed = 1,
    Cleaving = 2,
    NetherRending = 4,
}

// Tracks which items have which custom imbues. Key = target.Guid.Full.
public static class OvertinkedImbueStore
{
    private static readonly Dictionary<uint, OvertinkedImbueFlags> _flags = new();

    public static void Add(uint objectId, OvertinkedImbueFlags flags)
    {
        lock (_flags)
        {
            _flags.TryGetValue(objectId, out var existing);
            _flags[objectId] = existing | flags;
        }
    }

    public static OvertinkedImbueFlags Get(uint objectId)
    {
        lock (_flags)
            return _flags.TryGetValue(objectId, out var f) ? f : OvertinkedImbueFlags.None;
    }

    public static void Remove(uint objectId)
    {
        lock (_flags)
            _flags.Remove(objectId);
    }
}
