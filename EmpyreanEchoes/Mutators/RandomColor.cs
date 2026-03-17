namespace EmpyreanEchoes.Mutators;

internal class RandomColor : Mutator
{
    public override bool TryMutateEnterInventory(HashSet<Mutation> mutations, WorldObject item, Player player) => false;
}
