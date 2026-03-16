namespace Overtinked;

// Tracks buffed jewelry items that have a secondary effect (e.g. Damage Rating from 5% of max Stamina/Mana).
// Key = item Guid.Full. Value = (SecondaryStat name, percent e.g. 5 for 5%).
public static class BuffedJewelrySecondaryStore
{
    private static readonly Dictionary<uint, (string Stat, double Percent)> _store = new();

    public static void Add(uint objectId, string secondaryStat, double percent)
    {
        lock (_store)
            _store[objectId] = (secondaryStat, percent);
    }

    public static (string Stat, double Percent)? Get(uint objectId)
    {
        lock (_store)
            return _store.TryGetValue(objectId, out var v) ? v : null;
    }

    public static void Remove(uint objectId)
    {
        lock (_store)
            _store.Remove(objectId);
    }
}
