namespace Forgeborne.Core;

/// <summary>
/// One voxel chunk, 32 x 32 x 32, matching the blueprint's chunk size.
/// Indexing: index = (y * Size + z) * Size + x, x = east, y = up, z = north.
/// This class is pure logic: no UnityEngine dependency, so it can be
/// tested headlessly and later driven by Unity Entities / DOTS.
/// </summary>
public sealed class VoxelChunk
{
    public const int Size = 32;
    public const int Volume = Size * Size * Size;

    private readonly byte[] _blocks = new byte[Volume];

    public VoxelChunk() { }

    public static int Index(int x, int y, int z) => (y * Size + z) * Size + x;
    public static bool InBounds(int x, int y, int z) =>
        x >= 0 && x < Size && y >= 0 && y < Size && z >= 0 && z < Size;

    public BlockType Get(int x, int y, int z) => InBounds(x, y, z) ? (BlockType)_blocks[Index(x, y, z)] : BlockType.Air;
    public bool TrySet(int x, int y, int z, BlockType type)
    {
        if (!InBounds(x, y, z)) return false;
        _blocks[Index(x, y, z)] = (byte)type;
        return true;
    }

    public int CountSolid()
    {
        int n = 0;
        foreach (var b in _blocks) if (b != 0) n++;
        return n;
    }

    public byte[] ToBytes() => (byte[])_blocks.Clone();
    public void FromBytes(byte[] data)
    {
        if (data.Length != Volume) throw new ArgumentException($"Chunk data must be {Volume} bytes, got {data.Length}.");
        Array.Copy(data, _blocks, Volume);
    }

    /// <summary>
    /// Deterministic Slice 1 zone generation.
    /// y = 0: Bedrock (ungatherable). y = 1..3: Stone ground.
    /// EmberOre: ~4% of stone in y = 1..3, deterministic by seed.
    /// Wood: a few surface trees at deterministic positions on y = 4.
    /// Everything above y = 4 is Air. No randomness beyond the seed.
    /// </summary>
    public static VoxelChunk Generate(int seed)
    {
        var chunk = new VoxelChunk();
        for (int x = 0; x < Size; x++)
        for (int z = 0; z < Size; z++)
        {
            chunk.TrySet(x, 0, z, BlockType.Bedrock);
            for (int y = 1; y <= 3; y++)
            {
                var type = Deterministic.Unit(seed, x, y, z) < 0.04
                    ? BlockType.EmberOre
                    : BlockType.Stone;
                chunk.TrySet(x, y, z, type);
            }
            // Trees: deterministic, sparse, on the ground surface.
            if (Deterministic.Unit(seed + 1000, x, 0, z) < 0.02)
                chunk.TrySet(x, 4, z, BlockType.Wood);
        }
        return chunk;
    }
}
