namespace Forgeborne.Core;

/// <summary>Canonical existing construction prices, shared by preview and payment.</summary>
public readonly record struct BuildCost(int Stone, int EmberOre, int Wood)
{
    public static bool TryGet(BlockType type, out BuildCost cost)
    {
        cost = type switch
        {
            BlockType.Wall => new(2, 0, 0),
            BlockType.Workstation => new(10, 5, 0),
            BlockType.KineticMill => new(10, 0, 5),
            BlockType.Dynamo => new(0, 5, 0),
            BlockType.Transformer => new(0, 5, 0),
            BlockType.Cable => new(0, 1, 0),
            BlockType.Alternator => new(10, 0, 0),
            BlockType.ShockPylon => new(5, 10, 0),
            BlockType.Conduit => new(5, 5, 0),
            _ => default,
        };
        return cost != default;
    }

    public bool CanAfford(Inventory inventory) => inventory.Has(BlockType.Stone, Stone)
        && inventory.Has(BlockType.EmberOre, EmberOre) && inventory.Has(BlockType.Wood, Wood);

    internal bool TrySpend(Inventory inventory)
    {
        if (!CanAfford(inventory)) return false;
        inventory.Remove(BlockType.Stone, Stone);
        inventory.Remove(BlockType.EmberOre, EmberOre);
        inventory.Remove(BlockType.Wood, Wood);
        return true;
    }
}

/// <summary>Read-only prediction; placement always revalidates the live world before payment.</summary>
public readonly record struct BuildPreview(BlockType Type, bool HasCell, int X, int Y, int Z,
    BuildResult Result, BuildCost Cost);
