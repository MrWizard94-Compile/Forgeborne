namespace Forgeborne.Core;

/// <summary>Orbital Tactical doctrines. Prototype scope: HoldGround only.
/// A doctrine is a standing order for a whole squad, not a per-unit
/// command — the blueprint's "no individual micromanagement" rule.
/// New doctrines are added as enum values with their own Tick
/// behaviour, never as per-member orders.</summary>
public enum TacticalDoctrine
{
    /// <summary>Move to the rally point, then hold it and engage
    /// enemies in range. While moving, the squad does not fire:
    /// relocation is the order until the rally is reached.</summary>
    HoldGround = 0,
}

/// <summary>Squad types. Prototype scope: Vanguard only — a
/// general-purpose 3-member squad. Further types (recon, heavy,
/// flyer) are later steps, not claimed here.</summary>
public enum SquadKind
{
    Vanguard = 0,
}

/// <summary>
/// One Orbital Tactical squad, in pure C# core. Real-time, not
/// turn-based: it moves one voxel per world tick toward its rally
/// point and, once holding, fires once per tick at the nearest
/// enemy in range. It is commanded only by doctrine + rally point.
/// </summary>
public sealed class TacticalSquad
{
    /// <summary>Health per member. A squad starts at 3 members x 30 = 90.</summary>
    public const int MemberHealth = 30;
    /// <summary>Damage each living member deals per engagement tick.</summary>
    public const int MemberDamage = 10;
    /// <summary>Engagement range in Manhattan distance, from the squad to the enemy.</summary>
    public const int EngagementRange = 3;
    /// <summary>Starting members for a Vanguard squad.</summary>
    public const int StartingMembers = 3;

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public int RallyX { get; }
    public int RallyY { get; }
    public int RallyZ { get; }
    public TacticalDoctrine Doctrine { get; }
    public SquadKind Kind { get; }
    public int Health { get; private set; } = StartingMembers * MemberHealth;
    public bool IsDead => Health <= 0;

    /// <summary>
    /// Living members, from total health: 61-90 = 3, 31-60 = 2,
    /// 1-30 = 1, 0 = 0. A wounded member keeps firing until its
    /// 30 HP are gone, so firepower drops in steps, not per hit.
    /// </summary>
    public int AliveMembers => Health <= 0 ? 0 : (Health + MemberHealth - 1) / MemberHealth;
    public int DamagePerTick => AliveMembers * MemberDamage;
    public bool AtRally => X == RallyX && Y == RallyY && Z == RallyZ;

    public TacticalSquad(int spawnX, int spawnY, int spawnZ, int rallyX, int rallyY, int rallyZ,
        TacticalDoctrine doctrine = TacticalDoctrine.HoldGround, SquadKind kind = SquadKind.Vanguard)
        : this(spawnX, spawnY, spawnZ, rallyX, rallyY, rallyZ, StartingMembers * MemberHealth, doctrine, kind)
    {
    }

    /// <summary>
    /// Load path: same squad, but with restored health (e.g. a wounded
    /// squad at 30 HP still has 1 living member). Health is clamped
    /// to 0..90, so a corrupt save cannot create a super-squad; a
    /// 0-health squad is dead and is skipped by the loader.
    /// </summary>
    public TacticalSquad(int spawnX, int spawnY, int spawnZ, int rallyX, int rallyY, int rallyZ,
        int health, TacticalDoctrine doctrine, SquadKind kind)
    {
        X = spawnX; Y = spawnY; Z = spawnZ;
        RallyX = rallyX; RallyY = rallyY; RallyZ = rallyZ;
        Health = Math.Clamp(health, 0, StartingMembers * MemberHealth);
        Doctrine = doctrine; Kind = kind;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        Health = Math.Max(0, Health - amount);
    }

    /// <summary>
    /// HoldGround movement: one voxel per tick toward the rally, on
    /// the axis with the largest remaining gap (ties: X, then Y,
    /// then Z — the same tie-break as Enemy movement, so the path
    /// is deterministic). The step is refused — the squad holds
    /// its position that tick — if it would leave the chunk, enter
    /// a solid block, or enter a cell occupied by an enemy.
    /// Returns true if the squad moved.
    /// </summary>
    public bool MoveTowardRally(VoxelChunk chunk, IReadOnlyList<Enemy> enemies)
    {
        if (AtRally) return false;
        int dx = RallyX - X, dy = RallyY - Y, dz = RallyZ - Z;
        int nx = X, ny = Y, nz = Z;
        if (Math.Abs(dx) >= Math.Abs(dy) && Math.Abs(dx) >= Math.Abs(dz)) nx += Math.Sign(dx);
        else if (Math.Abs(dy) >= Math.Abs(dz)) ny += Math.Sign(dy);
        else nz += Math.Sign(dz);
        if (!VoxelChunk.InBounds(nx, ny, nz)) return false;
        if (BlockInfo.IsSolid(chunk.Get(nx, ny, nz))) return false;
        foreach (var e in enemies)
            if (!e.IsDead && e.X == nx && e.Y == ny && e.Z == nz) return false;
        X = nx; Y = ny; Z = nz;
        return true;
    }

    /// <summary>
    /// HoldGround engagement: only once holding the rally, the
    /// nearest living enemy within EngagementRange (Manhattan).
    /// Ties go to the first enemy in list order. Returns null if
    /// the squad is still moving, dead, or nothing is in range.
    /// </summary>
    public Enemy? FindTarget(IReadOnlyList<Enemy> enemies)
    {
        if (IsDead || !AtRally) return null;
        Enemy? nearest = null;
        int best = int.MaxValue;
        foreach (var e in enemies)
        {
            if (e.IsDead) continue;
            int d = Math.Abs(e.X - X) + Math.Abs(e.Y - Y) + Math.Abs(e.Z - Z);
            if (d <= EngagementRange && d < best) { best = d; nearest = e; }
        }
        return nearest;
    }
}
