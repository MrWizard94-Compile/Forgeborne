namespace Forgeborne.Core;

/// <summary>Orbital Tactical doctrines. A doctrine is a standing
/// order for a whole squad, not a per-unit command — the
/// blueprint's "no individual micromanagement" rule. New doctrines
/// are added as enum values with their own Tick behaviour, never
/// as per-member orders.</summary>
public enum TacticalDoctrine
{
    /// <summary>Move to the rally point, then hold it and engage
    /// enemies in range. While moving, the squad does not fire:
    /// relocation is the order until the rally is reached.</summary>
    HoldGround = 0,
    /// <summary>Slice 4 Step 2: hunt the nearest living enemy, one
    /// voxel per tick, and fire at the nearest enemy in range
    /// whether or not the squad is at its rally — because this
    /// doctrine has no holding point. The rally a picker still
    /// requires to deploy is ignored after deployment: an
    /// Advance squad with no living enemy stands still, it
    /// does not march to its rally, and that is what keeps
    /// Advance from collapsing into "HoldGround at a moving
    /// rally". See MoveTowardNearestEnemy and FindTarget.</summary>
    Advance = 1,
}

/// <summary>Squad types. Vanguard is the general-purpose
/// 3-member squad; Recon (Slice 7 Step 1) is the second kind —
/// a fragile long-range screen, not a second generalist.
/// Heavy (Slice 11 Step 1) is the third kind — a close-range
/// anchor, not a tougher Vanguard in every dimension: it
/// trades range and firepower for survivability (see
/// SquadShapes). A flyer is a later step, not claimed
/// here.</summary>
public enum SquadKind
{
    Vanguard = 0,
    /// <summary>Slice 7 Step 1: 2 members, 20 HP each, 6 damage
    /// each, range 5. Reach Vanguard cannot make, paid for with
    /// less than half Vanguard's health and DPS — see
    /// SquadShapes and DECISIONS.md.</summary>
    Recon = 1,
    /// <summary>Slice 11 Step 1: 2 members, 50 HP each, 8 damage
    /// each, range 2. The anchor Vanguard cannot be: it survives
    /// a hit that kills Vanguard outright, but it fires only
    /// at range 2 and deals barely half Vanguard's damage —
    /// see SquadShapes and DECISIONS.md.</summary>
    Heavy = 2,
}

/// <summary>
/// One squad kind's shape: the numbers every squad rule reads.
/// Vanguard's shape is unchanged since 0.9.0 (3 members, 30 HP
/// each, 10 damage each, range 3); Recon's shape is decided in
/// Slice 7 Step 1 and Heavy's in Slice 11 Step 1, and both live
/// here, not as a second set of shared constants or a branch
/// in every rule. A kind's squad reads its shape from its Kind,
/// so movement, fire, snapshot, and load all agree about what
/// a kind is without duplicating its numbers. Unknown kinds
/// are rejected at the boundaries (deploy, picker selection,
/// load), so For throws for them rather than silently
/// returning a Vanguard shape.</summary>
public readonly record struct SquadShape(int Members, int MemberHealth, int MemberDamage, int EngagementRange);

public static class SquadShapes
{
    public static SquadShape For(SquadKind kind) => kind switch
    {
        SquadKind.Vanguard => new SquadShape(Members: 3, MemberHealth: 30, MemberDamage: 10, EngagementRange: 3),
        SquadKind.Recon => new SquadShape(Members: 2, MemberHealth: 20, MemberDamage: 6, EngagementRange: 5),
        SquadKind.Heavy => new SquadShape(Members: 2, MemberHealth: 50, MemberDamage: 8, EngagementRange: 2),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown squad kind has no shape."),
    };
}

/// <summary>
/// One Orbital Tactical squad, in pure C# core. Real-time, not
/// turn-based: it moves one voxel per world tick toward its rally
/// point and, once holding, fires once per tick at the nearest
/// enemy in range. It is commanded only by doctrine + rally point.
/// </summary>
public sealed class TacticalSquad
{
    /// <summary>The kind's shape, read from Kind — never a
    /// second copy of the numbers. Vanguard: 3 x 30 HP, 10
    /// damage, range 3. Recon: 2 x 20 HP, 6 damage, range 5.
    /// Heavy: 2 x 50 HP, 8 damage, range 2.</summary>
    public SquadShape Shape => SquadShapes.For(Kind);
    /// <summary>Health per member, from the kind's shape.</summary>
    public int MemberHealth => Shape.MemberHealth;
    /// <summary>Damage each living member deals per engagement tick, from the kind's shape.</summary>
    public int MemberDamage => Shape.MemberDamage;
    /// <summary>Engagement range in Manhattan distance, from the kind's shape.</summary>
    public int EngagementRange => Shape.EngagementRange;
    /// <summary>Starting members, from the kind's shape.</summary>
    public int StartingMembers => Shape.Members;
    /// <summary>Full-strength health: members x HP per member (Vanguard 90, Recon 40, Heavy 100).</summary>
    public int MaxHealth => Shape.Members * Shape.MemberHealth;

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public int RallyX { get; }
    public int RallyY { get; }
    public int RallyZ { get; }
    public TacticalDoctrine Doctrine { get; }
    public SquadKind Kind { get; }
    public int Health { get; private set; }
    public bool IsDead => Health <= 0;

    /// <summary>
    /// Living members, from total health in steps of one
    /// member's HP: e.g. Vanguard 61-90 = 3, 31-60 = 2, 1-30 = 1,
    /// 0 = 0; Recon 21-40 = 2, 1-20 = 1, 0 = 0; Heavy 51-100 = 2,
    /// 1-50 = 1, 0 = 0. A wounded member
    /// keeps firing until its HP are gone, so firepower drops
    /// in steps, not per hit.
    /// </summary>
    public int AliveMembers => Health <= 0 ? 0 : (Health + MemberHealth - 1) / MemberHealth;
    public int DamagePerTick => AliveMembers * MemberDamage;
    public bool AtRally => X == RallyX && Y == RallyY && Z == RallyZ;

    public TacticalSquad(int spawnX, int spawnY, int spawnZ, int rallyX, int rallyY, int rallyZ,
        TacticalDoctrine doctrine = TacticalDoctrine.HoldGround, SquadKind kind = SquadKind.Vanguard)
        : this(spawnX, spawnY, spawnZ, rallyX, rallyY, rallyZ,
            SquadShapes.For(kind).Members * SquadShapes.For(kind).MemberHealth, doctrine, kind)
    {
    }

    /// <summary>
    /// Load path: same squad, but with restored health (e.g. a wounded
    /// Vanguard squad at 30 HP still has 1 living member). Health is
    /// clamped to 0..the kind's own maximum (Vanguard 90, Recon 40,
    /// Heavy 100), so a corrupt save cannot create a super-squad
    /// and a kind is never silently trimmed to another kind's
    /// health on load; a 0-health squad is dead and is skipped
    /// by the loader. Kind is assigned before the clamp because
    /// the clamp is per-kind.
    /// </summary>
    public TacticalSquad(int spawnX, int spawnY, int spawnZ, int rallyX, int rallyY, int rallyZ,
        int health, TacticalDoctrine doctrine, SquadKind kind)
    {
        X = spawnX; Y = spawnY; Z = spawnZ;
        RallyX = rallyX; RallyY = rallyY; RallyZ = rallyZ;
        Doctrine = doctrine; Kind = kind;
        Health = Math.Clamp(health, 0, MaxHealth);
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
        return MoveTowardPosition(RallyX, RallyY, RallyZ, chunk, enemies);
    }

    /// <summary>
    /// Advance movement: one voxel per tick toward the NEAREST
    /// living enemy, re-derived every call (Manhattan distance,
    /// ties go to the first enemy in list order — the same rule
    /// as FindTarget, so movement and fire never disagree about
    /// which enemy is nearest). With no living enemy the squad
    /// does not move at all: it does not fall back to its rally,
    /// because a rally fallback would make Advance a HoldGround
    /// squad whenever the battlefield is empty. The step itself
    /// is the shared MoveTowardPosition rule, so Advance and
    /// HoldGround cannot drift apart on axis choice, tie-break,
    /// or blocking — only the target differs. A step onto the
    /// target enemy's cell is refused (enemy-occupied), so an
    /// advancing squad stops adjacent to its prey and fires
    /// from there instead of overlapping it. Returns true if
    /// the squad moved.
    /// </summary>
    public bool MoveTowardNearestEnemy(VoxelChunk chunk, IReadOnlyList<Enemy> enemies)
    {
        Enemy? nearest = null;
        int best = int.MaxValue;
        foreach (var e in enemies)
        {
            if (e.IsDead) continue;
            int d = Math.Abs(e.X - X) + Math.Abs(e.Y - Y) + Math.Abs(e.Z - Z);
            if (d < best) { best = d; nearest = e; }
        }
        if (nearest == null) return false;
        return MoveTowardPosition(nearest.X, nearest.Y, nearest.Z, chunk, enemies);
    }

    /// <summary>The single squad movement rule, shared by both
    /// doctrines: one voxel on the largest-gap axis (ties X, then
    /// Y, then Z), refused out of bounds, into a solid block, or
    /// onto a living enemy's cell. Callers choose the target;
    /// this method owns only the step.</summary>
    private bool MoveTowardPosition(int targetX, int targetY, int targetZ,
        VoxelChunk chunk, IReadOnlyList<Enemy> enemies)
    {
        int dx = targetX - X, dy = targetY - Y, dz = targetZ - Z;
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
    /// Squad engagement: the nearest living enemy within
    /// EngagementRange (Manhattan), ties to the first enemy in
    /// list order. HoldGround fires only once holding its rally;
    /// Advance fires regardless of its rally, because it has no
    /// holding point — an advancing squad that is in range but
    /// not at its rally is engaging, not relocating, so the
    /// HoldGround "do not fire while moving" rule must not
    /// silence it. Returns null if dead, or nothing is in range
    /// (or, for HoldGround, the squad is still moving).
    /// </summary>
    public Enemy? FindTarget(IReadOnlyList<Enemy> enemies)
    {
        if (IsDead) return null;
        if (Doctrine == TacticalDoctrine.HoldGround && !AtRally) return null;
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

/// <summary>
/// The Slice 3 squad economy, in pure C# core: what a deployment
/// costs and how many squads a commander may field at once.
/// Pricing basis (decided 0.18.0, rationale in DECISIONS.md):
/// gathered resources, not a new currency. The price is EmberOre
/// only — 10 ore, twice a workstation's 5 — because ore is the
/// already-valuable resource (~4% of ground cells, so 10 ore is
/// deliberate gathering, not incidental pickup) and pricing in
/// Stone would make every squad compete with walls and mills for
/// the gather -&gt; build loop's core material. The cap limits
/// concurrent LIVE squads to 3 (9 members at full strength); at
/// the cap a deploy is refused, not an oldest-squad recall,
/// because recall would silently destroy a paid investment and
/// its position. Deployment is a sunk cost: no refund exists,
/// on death or otherwise, because there is no recall mechanic
/// to refund through. Both values are first-pass values for
/// determinism and testability, not final balance (see the
/// Slice 3 scope's no-balance-pass rule). Enforcement lives in
/// WorldZone.TryDeployPaidSquad, which checks cap and
/// affordability BEFORE charging, so a refused deploy charges
/// nothing and deploys nothing — no partial charge, no
/// half-deployed squad. The picker (OrbitalCommand.Confirm)
/// deploys through that paid path, so the play path is priced;
/// WorldZone.DeploySquad remains the free spawn primitive for
/// headless tests and the demo only, never the Unity play path.
/// </summary>
public static class SquadEconomy
{
    /// <summary>Maximum concurrent live squads. Dead squads do not count.</summary>
    public const int MaxConcurrentSquads = 3;
    /// <summary>Deployment price, in EmberOre, for one squad of
    /// any kind. Slice 7 Step 1 keeps one price for both kinds
    /// then existing, and Slice 11 Step 1 keeps it for Heavy
    /// too (see CostForKind): the kinds trade role for role,
    /// not price for power, so a price difference would invent
    /// an economy distinction no balance evidence supports yet.</summary>
    public const int EmberOreCost = 10;

    /// <summary>Deployment price for a kind, decided in Slice 7
    /// Step 1 and kept in Slice 11 Step 1: 10 EmberOre for
    /// Vanguard, for Recon, and for Heavy alike, so the picker
    /// prompt's COST 10 line stays true for all three kinds
    /// and the cap question stays separate from price.
    /// Unknown kinds have no price; callers must reject them
    /// before charging, as TryDeployPaidSquad does.</summary>
    public static int CostForKind(SquadKind kind) => kind switch
    {
        SquadKind.Vanguard => EmberOreCost,
        SquadKind.Recon => EmberOreCost,
        SquadKind.Heavy => EmberOreCost,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown squad kind has no price."),
    };

    /// <summary>Live squads in the world (dead squads, not yet removed by Advance, excluded).</summary>
    public static int LiveSquadCount(WorldZone world)
    {
        ArgumentNullException.ThrowIfNull(world);
        int n = 0;
        foreach (var s in world.Squads) if (!s.IsDead) n++;
        return n;
    }

    public static bool CanAfford(WorldZone world)
    {
        ArgumentNullException.ThrowIfNull(world);
        return world.Player.Inventory.Has(BlockType.EmberOre, EmberOreCost);
    }

    /// <summary>True only when under the cap AND able to pay.</summary>
    public static bool CanDeploy(WorldZone world) =>
        LiveSquadCount(world) < MaxConcurrentSquads && CanAfford(world);
}

/// <summary>
/// The doctrine menu / rally picker state machine, in pure C# core.
/// This is the menu, not a rendering: the Unity adapter only binds
/// keys and a crosshair raycast to it (see docs/orbital-command.md),
/// and every rule below is headless-tested. Flow: BeginPick opens
/// the picker (clearing any stale pending rally, so Enter cannot
/// deploy yesterday's point by accident), SetRally updates the
/// pending rally while picking, Confirm deploys one squad with the
/// selected doctrine/kind at that rally, Cancel abandons the pick.
/// Confirm without a pick or without a rally, and SetRally outside
/// picking or out of bounds, fail closed and change nothing.
/// Confirm deploys through the paid economy path
/// (WorldZone.TryDeployPaidSquad): a deploy the player cannot
/// afford, or that exceeds the 3-live-squad cap, fails closed
/// like any failed Confirm — picker stays open, no squad, no
/// charge — so the player can gather more ore or wait for a
/// slot and re-confirm instead of losing the menu.
/// </summary>
public sealed class OrbitalCommand
{
    public TacticalDoctrine Doctrine { get; private set; } = TacticalDoctrine.HoldGround;
    public SquadKind Kind { get; private set; } = SquadKind.Vanguard;
    public bool IsPicking { get; private set; }
    public bool HasRally { get; private set; }
    public int RallyX { get; private set; }
    public int RallyY { get; private set; }
    public int RallyZ { get; private set; }

    /// <summary>Select a doctrine. Unknown values are rejected and leave the selection unchanged.</summary>
    public bool SelectDoctrine(TacticalDoctrine doctrine)
    {
        if (!Enum.IsDefined(doctrine)) return false;
        Doctrine = doctrine;
        return true;
    }

    /// <summary>Select a squad kind. Unknown values are rejected and leave the selection unchanged.</summary>
    public bool SelectKind(SquadKind kind)
    {
        if (!Enum.IsDefined(kind)) return false;
        Kind = kind;
        return true;
    }

    /// <summary>Open the picker. Clears any pending rally from an earlier pick.</summary>
    public void BeginPick()
    {
        IsPicking = true;
        HasRally = false;
    }

    /// <summary>
    /// Update the pending rally while picking. Only bounds are
    /// checked here, matching WorldZone.DeploySquad: a rally inside
    /// a solid block is accepted and the squad then holds short of
    /// it, so a bad rally is visible, not silent.
    /// </summary>
    public bool SetRally(int x, int y, int z)
    {
        if (!IsPicking) return false;
        if (!VoxelChunk.InBounds(x, y, z)) return false;
        RallyX = x; RallyY = y; RallyZ = z;
        HasRally = true;
        return true;
    }

    /// <summary>Abandon the pick: no squad is deployed, the pending rally is dropped.</summary>
    public void CancelPick()
    {
        IsPicking = false;
        HasRally = false;
    }

    /// <summary>
    /// Deploy the selected squad at the pending rally, paying the
    /// squad economy price. On success the picker closes and the
    /// rally is cleared; on failure (including unaffordable or
    /// over-cap) the picker stays open for a re-pick, with no
    /// squad and no charge. Returns null on failure.
    /// </summary>
    public TacticalSquad? Confirm(WorldZone world)
    {
        if (!IsPicking || !HasRally) return null;
        var squad = world.TryDeployPaidSquad(RallyX, RallyY, RallyZ, Doctrine, Kind);
        if (squad == null) return null;
        IsPicking = false;
        HasRally = false;
        return squad;
    }
}

/// <summary>
/// What a squad is doing, for display only — derived in the
/// snapshot, never stored on the squad and never saved. This
/// replaces the old rally-relative display word: a HoldGround
/// squad marching to its rally is Marching and at its rally is
/// Holding, while an Advance squad is Hunting whenever a living
/// enemy exists (moving toward it or standing adjacent firing —
/// the rally it ignores says nothing about that) and Idle when
/// none does (standing still, ignoring its rally, exactly as
/// Advance is documented to behave). The word is doctrine
/// truth, not rally proximity.
/// </summary>
public enum SquadActivity
{
    Marching = 0,
    Holding = 1,
    Hunting = 2,
    Idle = 3,
}

/// <summary>
/// Snapshot of one squad, copied by value at snapshot time.
/// The Unity view renders this, never the live squad objects,
/// so rendering cannot mutate core state and a snapshot taken
/// before a tick still shows the pre-tick position afterwards.
/// </summary>
public sealed class SquadSnapshot
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public int RallyX { get; init; }
    public int RallyY { get; init; }
    public int RallyZ { get; init; }
    public int Health { get; init; }
    public int AliveMembers { get; init; }
    public int DamagePerTick { get; init; }
    public bool AtRally { get; init; }
    public TacticalDoctrine Doctrine { get; init; }
    public SquadKind Kind { get; init; }
    /// <summary>True while the squad is still marching to its rally; false once it is holding and firing.</summary>
    public bool IsMoving => !AtRally;
    /// <summary>Display activity, derived in OrbitalView.Snapshot (see SquadActivity).</summary>
    public SquadActivity Activity { get; init; }
    /// <summary>True only for a Hunting squad: the prey cell below is meaningful.</summary>
    public bool HasHuntingTarget { get; init; }
    /// <summary>Cell of the nearest living enemy, for a Hunting squad only; otherwise null.
/// Re-derived on every snapshot from the same nearest-enemy rule Advance movement and fire
/// already use (Manhattan, first in list order on ties), so movement, fire, and display can
/// never disagree about the prey — and, like targeting, it is derived, not persisted.</summary>
    public int? HuntingTargetX { get; init; }
    public int? HuntingTargetY { get; init; }
    public int? HuntingTargetZ { get; init; }
}

/// <summary>
/// The Orbital view: a copied, read-only snapshot of every live
/// squad in a world, in squad list order. This is the single
/// source the Unity adapter renders; it contains no rendering
/// logic and no second copy of squad rules. Dead squads are
/// already removed by WorldZone.Advance, so they never appear.
/// </summary>
public static class OrbitalView
{
    public static IReadOnlyList<SquadSnapshot> Snapshot(WorldZone world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var list = new List<SquadSnapshot>(world.Squads.Count);
        foreach (var s in world.Squads)
        {
            if (s.IsDead) continue;
            // Activity is derived here, in core, not in the adapter,
            // so the text display and the 3D markers read the same
            // word from the same snapshot. HoldGround is rally truth
            // (marching / holding); Advance is prey truth (hunting /
            // idle). The hunting target is the nearest living enemy
            // by the same Manhattan, first-on-ties rule
            // MoveTowardNearestEnemy and FindTarget use — with NO
            // range limit, because hunting is not engagement: a
            // squad 10 cells from its prey is hunting it, not idle,
            // even though it cannot fire yet. An unknown doctrine
            // value falls back to the HoldGround reading rather
            // than inventing a word for a doctrine that does not
            // exist. Nothing here is saved: the next snapshot
            // re-derives activity and target from the live world.
            SquadActivity activity;
            Enemy? prey = null;
            if (s.Doctrine == TacticalDoctrine.Advance)
            {
                int best = int.MaxValue;
                foreach (var e in world.Enemies)
                {
                    if (e.IsDead) continue;
                    int d = Math.Abs(e.X - s.X) + Math.Abs(e.Y - s.Y) + Math.Abs(e.Z - s.Z);
                    if (d < best) { best = d; prey = e; }
                }
                activity = prey == null ? SquadActivity.Idle : SquadActivity.Hunting;
            }
            else
            {
                activity = s.AtRally ? SquadActivity.Holding : SquadActivity.Marching;
            }
            list.Add(new SquadSnapshot
            {
                X = s.X, Y = s.Y, Z = s.Z,
                RallyX = s.RallyX, RallyY = s.RallyY, RallyZ = s.RallyZ,
                Health = s.Health, AliveMembers = s.AliveMembers,
                DamagePerTick = s.DamagePerTick, AtRally = s.AtRally,
                Doctrine = s.Doctrine, Kind = s.Kind,
                Activity = activity,
                HasHuntingTarget = prey != null,
                HuntingTargetX = prey?.X, HuntingTargetY = prey?.Y, HuntingTargetZ = prey?.Z,
            });
        }
        return list;
    }
}
