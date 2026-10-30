namespace Forgeborne.Core;

/// <summary>Block types in Slice 1. Values are stable for save files — do not reorder. New types are appended only.</summary>
public enum BlockType : byte
{
    Air = 0,
    Stone = 1,
    EmberOre = 2,
    Wood = 3,
    Wall = 4,
    Workstation = 5,
    Bedrock = 6,
    /// <summary>Slice 2 automation tier 1 (Kinetic): a crank-driven mill that excavates one adjacent gatherable block per activation. Appended in 0.8.0; earlier values unchanged, so old saves still load.</summary>
    KineticMill = 7,
    /// <summary>Slice 5 automation tier 2 (Electrical, first loop only): the single power source. A Dynamo generates power for face-adjacent Kinetic Mills only — no cables, no network propagation, no voltage tiers yet. Appended in 0.25.0; earlier values unchanged, so old saves still load.</summary>
    Dynamo = 8,
    /// <summary>Slice 9 Step 1 (Electrical tier 2, ladder + conversion): converts the voltage it is fed to the other level of the two-level ladder (LV to HV, HV to LV — see VoltageRules.Flipped), face-adjacency only in this step; cables arrive in Step 2. Appended in 0.40.0; earlier values unchanged, so old saves still load.</summary>
    Transformer = 9,
    /// <summary>Slice 9 Step 2 (Electrical tier 2, distribution): carries a voltage beyond face-adjacency over a face-adjacent network of Cable cells, with no loss and no length limit in this step. Only Cable cells conduct: a wall, workstation, or mill between cables breaks the network. Appended in 0.41.0; earlier values unchanged, so old saves still load.</summary>
    Cable = 10,
    /// <summary>Slice 10 Step 2 (Electrical widening, second source): a native HV generator. Unlike the Dynamo (LV) it cannot safely feed an LV-rated consumer directly — a mill fed HV directly is destroyed, a workstation fed HV directly is disabled — and safe use runs through one transformer step-down. Appended in 0.46.0; earlier values unchanged, so old saves still load.</summary>
    Alternator = 11,
}

public static class BlockInfo
{
    public static bool IsSolid(BlockType t) => t != BlockType.Air;
    public static bool IsGatherable(BlockType t) => t is BlockType.Stone or BlockType.EmberOre or BlockType.Wood;
    /// <summary>Resource yielded when a gatherable block is gathered. One block = one unit, Slice 1 rule.</summary>
    public static BlockType GatherYield(BlockType t) => t; // Stone->Stone, EmberOre->EmberOre, Wood->Wood
}

/// <summary>Deterministic hash: same seed + coordinates always give the same value. No global Random, no time.</summary>
public static class Deterministic
{
    public static uint Hash(int seed, int x, int y, int z)
    {
        uint h = (uint)seed;
        h ^= (uint)x * 374761393u;
        h ^= (uint)y * 668265263u;
        h ^= (uint)z * 2147483647u;
        h *= 2654435761u;
        h ^= h >> 13;
        h *= 1274126177u;
        h ^= h >> 16;
        return h;
    }
    public static double Unit(int seed, int x, int y, int z) => (Hash(seed, x, y, z) % 10000) / 10000.0;
}
