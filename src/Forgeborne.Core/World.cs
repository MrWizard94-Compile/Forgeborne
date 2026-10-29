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
    /// <summary>Orbital Tactical squads deployed in this zone. Empty by default, so zones without squads behave exactly as before. Session-only in this prototype: squads are NOT yet saved (see CHANGELOG 0.9.0).</summary>
    public List<TacticalSquad> Squads { get; } = new();
    public int Tick { get; private set; }
    /// <summary>Shields restored per tick near a workstation. Base 5; skill WorkstationRegenBonus raises it via Simulation.ApplySkills.</summary>
    public int WorkstationRegenPerTick { get; set; } = 5;
    /// <summary>Blocks gathered by Kinetic Mills this session (not saved: a loaded world starts at 0, like Simulation's session counters).</summary>
    public int KineticBlocksGathered { get; private set; }
    /// <summary>Kinetic automation interval: mills act once every 5 ticks, so automation is real-time pressure relief, not instant or turn-based.</summary>
    public const int KineticIntervalTicks = 5;
    // Slice 1 objective core, from the reference image: DESTROY CORE.
    public int CoreX { get; } = 28;
    public int CoreY { get; } = 5;
    public int CoreZ { get; } = 28;
    public int CoreHealth { get; private set; } = 100;
    public void DamageCore(int amount) { if (amount > 0) CoreHealth = Math.Max(0, CoreHealth - amount); }
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
        if (type is not (BlockType.Wall or BlockType.Workstation or BlockType.KineticMill)) return BuildResult.InvalidType;
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
        else if (type == BlockType.KineticMill)
        {
            // Kinetic Mill: 10 Stone (frame) + 5 Wood (crank / gears).
            // Distinct from the workstation's 10 Stone + 5 EmberOre, so
            // the two machines compete for stone but not for each other.
            if (!Player.Inventory.Has(BlockType.Stone, 10) || !Player.Inventory.Has(BlockType.Wood, 5))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.Stone, 10);
            Player.Inventory.Remove(BlockType.Wood, 5);
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

    /// <summary>Workstation effect: if a workstation block is within Manhattan distance 3 of the player, restore 5 shields per tick, capped at 85. This is the workstation's real, distinct effect — a wall only blocks movement, a workstation repairs shields, so building one and defending it is a real decision.</summary>
    public bool IsNearWorkstation()
    {
        for (int dx = -3; dx <= 3; dx++)
            for (int dy = -3; dy <= 3; dy++)
                for (int dz = -3; dz <= 3; dz++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) > 3) continue;
                    int x = Player.X + dx, y = Player.Y + dy, z = Player.Z + dz;
                    if (VoxelChunk.InBounds(x, y, z) && Chunk.Get(x, y, z) == BlockType.Workstation) return true;
                }
        return false;
    }

    /// <summary>
    /// Deploy an Orbital Tactical squad: it spawns at the player's
    /// position and follows its doctrine toward the rally point.
    /// The rally must be in bounds; a rally inside a solid block is
    /// accepted (the squad then holds short of it, blocked) rather
    /// than rejected, so a bad rally is visible, not silent. An
    /// unknown doctrine is rejected. Returns null on rejection.
    /// </summary>
    public TacticalSquad? DeploySquad(int rallyX, int rallyY, int rallyZ,
        TacticalDoctrine doctrine = TacticalDoctrine.HoldGround, SquadKind kind = SquadKind.Vanguard)
    {
        if (!VoxelChunk.InBounds(rallyX, rallyY, rallyZ)) return null;
        if (!Enum.IsDefined(doctrine) || !Enum.IsDefined(kind)) return null;
        var squad = new TacticalSquad(Player.X, Player.Y, Player.Z, rallyX, rallyY, rallyZ, doctrine, kind);
        Squads.Add(squad);
        return squad;
    }

    /// <summary>One world tick: enemies move, then adjacent enemies attack, then the workstation effect applies, then squads move / engage / take adjacent-enemy damage (in that order, so a squad that kills its attacker that tick is not hit by it), then — every 5th tick — Kinetic Mills act. Dead enemies and squads are removed.</summary>
    public void Advance()
    {
        Tick++;
        foreach (var e in Enemies)
        {
            e.TickToward(Player, Chunk);
            if (e.IsAdjacentTo(Player)) Player.TakeDamage(Enemy.Damage);
        }
        Enemies.RemoveAll(e => e.IsDead);
        if (IsNearWorkstation()) Player.RestoreShields(WorkstationRegenPerTick);
        RunSquads();
        if (Tick % KineticIntervalTicks == 0) RunKineticAutomation();
    }

    /// <summary>
    /// Orbital squad phase, in squad list order: move toward the
    /// rally, then (only if holding) fire at the nearest enemy in
    /// range, then take 10 damage per enemy still adjacent to the
    /// squad. A squad at 0 health is removed at the end of the phase.
    /// </summary>
    private void RunSquads()
    {
        foreach (var squad in Squads.ToList())
        {
            if (squad.IsDead) continue;
            squad.MoveTowardRally(Chunk, Enemies);
            var target = squad.FindTarget(Enemies);
            if (target != null) AttackEnemy(target, squad.DamagePerTick);
            int adjacent = 0;
            foreach (var e in Enemies)
                if (!e.IsDead && Math.Abs(e.X - squad.X) + Math.Abs(e.Y - squad.Y) + Math.Abs(e.Z - squad.Z) == 1) adjacent++;
            if (adjacent > 0) squad.TakeDamage(adjacent * Enemy.Damage);
        }
        Squads.RemoveAll(s => s.IsDead);
    }

    /// <summary>
    /// Slice 2 automation tier 1 (Kinetic only): each Kinetic Mill
    /// excavates at most one adjacent gatherable block per activation,
    /// in fixed neighbour order, into the player's inventory. It reuses
    /// <see cref="Gather(int, int, int)"/>, so the same rules apply:
    /// bedrock, walls, workstations, and other mills are never gathered,
    /// and a full inventory consumes nothing. Mills are scanned in
    /// chunk-index order, so two mills reaching for the same block
    /// resolve deterministically — the first in scan order wins.
    /// No separate machine inventory yet: tier 1 output goes to the
    /// player's inventory, to be revisited when digital storage
    /// (tier 3) arrives.
    /// </summary>
    private void RunKineticAutomation()
    {
        for (int y = 0; y < VoxelChunk.Size; y++)
        for (int z = 0; z < VoxelChunk.Size; z++)
        for (int x = 0; x < VoxelChunk.Size; x++)
        {
            if (Chunk.Get(x, y, z) != BlockType.KineticMill) continue;
            foreach (var (dx, dy, dz) in Neighbours)
            {
                if (Gather(x + dx, y + dy, z + dz) == GatherResult.Success)
                {
                    KineticBlocksGathered++;
                    break;
                }
            }
        }
    }

    public int AttackEnemy(Enemy enemy, int damage)
    {
        enemy.TakeDamage(damage);
        if (enemy.IsDead) Enemies.Remove(enemy);
        return enemy.Health;
    }
}
