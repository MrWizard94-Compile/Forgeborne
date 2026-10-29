namespace Forgeborne.Core;

/// <summary>Block types in Slice 1. Values are stable for save files — do not reorder.</summary>
public enum BlockType : byte
{
    Air = 0,
    Stone = 1,
    EmberOre = 2,
    Wood = 3,
    Wall = 4,
    Workstation = 5,
    Bedrock = 6,
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
