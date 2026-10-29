namespace Forgeborne.Core;

public enum GatherResult { Success, OutOfBounds, Air, NotGatherable }
public enum BuildResult { Success, OutOfBounds, NotAir, InsufficientResources, Floating, InvalidType }

public sealed class Enemy
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Health { get; private set; } = 30;
    public const int Damage = 10;
    public bool IsDead => Health <= 0;

    public Enemy(int x, int y, int z) { X = x; Y = y; Z = z; }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        Health = Math.Max(0, Health - amount);
    }

    /// <summary>One tick: move one voxel toward the player on the axis with the largest gap, if that cell is free. No diagonal moves.</summary>
    public void TickToward(Player player, VoxelChunk chunk)
    {
        if (IsDead) return;
        int dx = player.X - X, dy = player.Y - Y, dz = player.Z - Z;
        if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 1) return; // adjacent: attack phase handles damage, do not move into player
        int nx = X, ny = Y, nz = Z;
        if (Math.Abs(dx) >= Math.Abs(dy) && Math.Abs(dx) >= Math.Abs(dz)) nx += Math.Sign(dx);
        else if (Math.Abs(dy) >= Math.Abs(dz)) ny += Math.Sign(dy);
        else nz += Math.Sign(dz);
        if (VoxelChunk.InBounds(nx, ny, nz) && !BlockInfo.IsSolid(chunk.Get(nx, ny, nz)))
        {
            X = nx; Y = ny; Z = nz;
        }
    }

    public bool IsAdjacentTo(Player player) =>
        Math.Abs(player.X - X) + Math.Abs(player.Y - Y) + Math.Abs(player.Z - Z) == 1;
}

/// <summary>
/// Slice 1 zone: one chunk, one player, enemies, gather / build rules.
/// All rules are deterministic and synchronous. No time, no Random.
/// </summary>
public sealed class WorldZone
{
    public int Seed { get; }
    public VoxelChunk Chunk { get; }
    public Player Player { get; }
    public List<Enemy> Enemies { get; } = new();
    public int Tick { get; private set; }
    public void RestoreTick(int tick) { Tick = Math.Max(0, tick); }

    public WorldZone(int seed)
    {
        Seed = seed;
        Chunk = VoxelChunk.Generate(seed);
        // Player starts in open air above the ground, near the centre.
        Player = new Player(VoxelChunk.Size / 2, 5, VoxelChunk.Size / 2);
    }

    public GatherResult Gather(int x, int y, int z)
    {
        if (!VoxelChunk.InBounds(x, y, z)) return GatherResult.OutOfBounds;
        var block = Chunk.Get(x, y, z);
        if (block == BlockType.Air) return GatherResult.Air;
        if (!BlockInfo.IsGatherable(block)) return GatherResult.NotGatherable;
        var overflow = Player.Inventory.Add(BlockInfo.GatherYield(block), 1);
        if (overflow > 0) return GatherResult.NotGatherable; // inventory full: do not consume the block
        Chunk.TrySet(x, y, z, BlockType.Air);
        return GatherResult.Success;
    }

    private static readonly (int, int, int)[] Neighbours =
        { (1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1) };

    public BuildResult Build(int x, int y, int z, BlockType type)
    {
        if (type is not (BlockType.Wall or BlockType.Workstation)) return BuildResult.InvalidType;
        if (!VoxelChunk.InBounds(x, y, z)) return BuildResult.OutOfBounds;
        if (Chunk.Get(x, y, z) != BlockType.Air) return BuildResult.NotAir;
        // Support rule: at least one neighbouring cell must be solid, or y == 0 ground (bedrock layer is solid anyway, but keep the rule explicit).
        bool supported = false;
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (VoxelChunk.InBounds(nx, ny, nz) && BlockInfo.IsSolid(Chunk.Get(nx, ny, nz))) { supported = true; break; }
        }
        if (!supported) return BuildResult.Floating;
        if (type == BlockType.Wall)
        {
            if (!Player.Inventory.Remove(BlockType.Stone, 2)) return BuildResult.InsufficientResources;
        }
        else // Workstation: 10 Stone + 5 EmberOre
        {
            if (!Player.Inventory.Has(BlockType.Stone, 10) || !Player.Inventory.Has(BlockType.EmberOre, 5))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.Stone, 10);
            Player.Inventory.Remove(BlockType.EmberOre, 5);
        }
        Chunk.TrySet(x, y, z, type);
        return BuildResult.Success;
    }

    /// <summary>Spawn a pressure wave: N enemies at the zone edge, in open air.</summary>
    public void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int x = i % 2 == 0 ? 1 : VoxelChunk.Size - 2;
            Enemies.Add(new Enemy(x, 5, 2 + i * 2 < VoxelChunk.Size ? 2 + i * 2 : VoxelChunk.Size - 2));
        }
    }

    /// <summary>One world tick: enemies move, then adjacent enemies attack. Dead enemies are removed.</summary>
    public void Advance()
    {
        Tick++;
        foreach (var e in Enemies)
        {
            e.TickToward(Player, Chunk);
            if (e.IsAdjacentTo(Player)) Player.TakeDamage(Enemy.Damage);
        }
        Enemies.RemoveAll(e => e.IsDead);
    }

    public int AttackEnemy(Enemy enemy, int damage)
    {
        enemy.TakeDamage(damage);
        if (enemy.IsDead) Enemies.Remove(enemy);
        return enemy.Health;
    }
}
