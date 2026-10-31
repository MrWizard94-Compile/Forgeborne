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
    /// <summary>Slice 11 Step 2: guard the world objective core
    /// (WorldZone.CoreX/Y/Z), not the rally the picker still
    /// requires to deploy. The guard post is the core cell
    /// itself; the squad marches to it one voxel per tick,
    /// chases a living enemy within DefendCoreChaseRadius of
    /// the core, and otherwise returns to the core — it never
    /// marches to its rally and never hunts an enemy far from
    /// the core. It fires whenever an enemy is in its kind's
    /// engagement range (Advance's fire rule, not HoldGround's
    /// hold-only rule), so a chasing or returning guard still
    /// shoots what it passes in range, but it does not chase
    /// that target: movement stays core-bound. See
    /// MoveForDefendCore and DECISIONS.md.</summary>
    DefendCore = 2,
    /// <summary>Slice 15 Step 1: walk a closed loop around the
    /// rally and repeat it, one voxel per tick through the
    /// shared step rule, firing in passing at any enemy in
    /// its kind's engagement range without ever chasing one:
    /// movement stays route-bound, so a patrolling squad is
    /// Advance's opposite on engagement — Advance walks to
    /// its prey, Patrol walks its route and shoots what the
    /// route brings into range. The route is derived from
    /// the rally alone, not stored and not saved: waypoint 0
    /// is the rally itself, then east, south, west, and north
    /// at PatrolLoopRadius, then back to the rally, in that
    /// order, forever. A squad deploys marching to waypoint 0
    /// like HoldGround, then keeps walking instead of holding.
    /// At a waypoint it turns and continues in the same tick
    /// (no dwell); a refused step first tries a
    /// distance-reducing sidestep, and only then skips
    /// that waypoint for this circuit instead of holding
    /// at a wall forever, and a full circuit of refusals
    /// reads Stuck. See MoveForPatrol and DECISIONS.md.</summary>
    Patrol = 3,
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
    /// <summary>Slice 20 Step 1: the rally is mutable only
    /// through TacticalSquad.ReRally, which WorldZone
    /// calls after its stuck-Patrol gate — never by a
    /// direct setter, so no caller can move a squad's
    /// loop without passing that gate. The save array
    /// already carries the rally, so a re-rally persists
    /// on the next save with no new save field.</summary>
    public int RallyX { get; private set; }
    public int RallyY { get; private set; }
    public int RallyZ { get; private set; }
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
    /// <summary>Slice 11 Step 2 Defend Core chase radius, in
    /// Manhattan distance from the core cell: a living enemy
    /// at most this far from the core is an attacker the guard
    /// chases; one farther away is ignored, however near it is
    /// to the squad. 8 is a first-pass value, not a balance
    /// claim: it is larger than every kind's engagement range
    /// (Vanguard 3, Recon 5, Heavy 2), so a guard can intercept
    /// before an attacker reaches the core, and far smaller
    /// than the spawn-to-core distance (24), so the perimeter
    /// is local to the core, not the whole map.</summary>
    public const int DefendCoreChaseRadius = 8;

    /// <summary>Slice 11 Step 2: one voxel per tick toward the
    /// core cell itself, through the shared MoveTowardPosition
    /// rule — same axis choice, tie-break, and solid /
    /// enemy-occupied refusals as HoldGround and Advance, so
    /// a guard whose post is blocked holds short of it rather
    /// than stepping through a block or onto an enemy. The
    /// rally is not consulted: a Defend Core squad's rally is
    /// a deployment formality the picker still requires, not
    /// a place the squad ever goes. Returns true if the squad
    /// moved.</summary>
    public bool MoveTowardCore(VoxelChunk chunk, IReadOnlyList<Enemy> enemies,
        int coreX, int coreY, int coreZ) =>
        MoveTowardPosition(coreX, coreY, coreZ, chunk, enemies);

    /// <summary>Slice 11 Step 2: the nearest living enemy within
    /// DefendCoreChaseRadius of the core, or null when no
    /// attacker is that close. Distance is enemy-to-core, not
    /// enemy-to-squad: an enemy standing next to a far-flung
    /// guard is not an attacker if the core is far from it.
    /// Ties go to the first enemy in list order, the same rule
    /// as FindTarget, so chase, fire, and display never
    /// disagree about which attacker is nearest.</summary>
    public Enemy? NearestCoreAttacker(IReadOnlyList<Enemy> enemies,
        int coreX, int coreY, int coreZ)
    {
        Enemy? nearest = null;
        int best = int.MaxValue;
        foreach (var e in enemies)
        {
            if (e.IsDead) continue;
            int toCore = Math.Abs(e.X - coreX) + Math.Abs(e.Y - coreY) + Math.Abs(e.Z - coreZ);
            if (toCore > DefendCoreChaseRadius) continue;
            int d = Math.Abs(e.X - X) + Math.Abs(e.Y - Y) + Math.Abs(e.Z - Z);
            if (d < best) { best = d; nearest = e; }
        }
        return nearest;
    }

    /// <summary>Slice 11 Step 2: one Defend Core tick's single
    /// move, decided once so the squad can never take two
    /// steps in one tick. With a core attacker it steps toward
    /// that attacker (the chase); without one it steps toward
    /// the core (the return, or standing at post when already
    /// there). A refused step — solid block, enemy-occupied
    /// cell, out of bounds — ends the tick's movement: the
    /// squad does not then step toward the other target
    /// instead, because that would be a second, different
    /// order executed in the same tick. Returns true if the
    /// squad moved.</summary>
    public bool MoveForDefendCore(VoxelChunk chunk, IReadOnlyList<Enemy> enemies,
        int coreX, int coreY, int coreZ)
    {
        var attacker = NearestCoreAttacker(enemies, coreX, coreY, coreZ);
        return attacker != null
            ? MoveTowardPosition(attacker.X, attacker.Y, attacker.Z, chunk, enemies)
            : MoveTowardPosition(coreX, coreY, coreZ, chunk, enemies);
    }

    /// <summary>Slice 15 Step 1: the patrol loop's radius, in
    /// voxels, from the rally: waypoints sit 3 east, south,
    /// west, and north of the rally, on the rally's own Y
    /// plane, so a patrol is a flat loop at rally height, not
    /// a climb. 3 is a first-pass value, not a balance claim:
    /// it is larger than every kind's engagement range
    /// except Recon's 5 (so a patrolling Recon can shoot
    /// across its own loop, while a Vanguard or Heavy
    /// shoots what the loop brings close), and small
    /// enough that a full circuit is 24 steps or fewer, so
    /// a test can watch it repeat. The radius is measured
    /// from the rally, never from the spawn, so the loop is
    /// re-derived from the rally on every tick.</summary>
    public const int PatrolLoopRadius = 3;
    /// <summary>Slice 15 Step 1: waypoints in the loop, in
    /// walking order: the rally itself, then east, south,
    /// west, and north of it. Five, not four, because the
    /// rally is a waypoint the squad returns to between
    /// circuits: without it the loop would orbit the rally
    /// without ever visiting it, and a patrol that never
    /// visits its rally is a loop around a point, not a
    /// route from it.</summary>
    public const int PatrolWaypointCount = 5;

    /// <summary>Slice 15 Step 1: the patrol waypoint the squad
    /// is currently walking toward, as an index into the
    /// loop (0 = rally, 1 = east, 2 = south, 3 = west,
    /// 4 = north, then back to 0). This is live route
    /// progress, not a saved field: it starts at 0 on
    /// deployment and resets to 0 on load, so a loaded
    /// patrol marches to its rally first and then resumes
    /// the loop from there, whatever circuit it was on
    /// before the save. Old saves load unchanged because
    /// the save array already carries doctrine, kind,
    /// position, and rally, and Patrol is an appended
    /// enum value the loader already accepts once defined
    /// — no save field is added, moved, or reinterpreted
    /// here. See MoveForPatrol and DECISIONS.md.</summary>
    public int PatrolWaypointIndex { get; private set; }

    /// <summary>Slice 18 Step 1: consecutive Patrol ticks
    /// that ended in a refused step with no sidestep
    /// move, counting the skip that followed each one.
    /// Live route progress, like PatrolWaypointIndex:
    /// it resets to 0 on any successful Patrol step
    /// (direct or sidestep) and starts at 0 on load,
    /// because a loaded squad is a new object and no
    /// save field carries it — a loaded patrol is
    /// therefore never stuck on arrival, whatever it
    /// was before the save. It is not a health, fire,
    /// or economy value, and no other doctrine reads
    /// it: HoldGround, Advance, and Defend Core have
    /// no skip rule and no stuck reading.</summary>
    public int PatrolConsecutiveRefusals { get; private set; }

    /// <summary>Slice 18 Step 1: true only for a Patrol
    /// squad that has refused a step, with no sidestep
    /// move, on every waypoint of a full circuit —
    /// PatrolConsecutiveRefusals at or above
    /// PatrolWaypointCount. Fewer refusals, even four
    /// in a row, are not stuck: a patrol that moved
    /// anywhere in its circuit is still patrolling
    /// the rest of its loop, and a single blocked
    /// waypoint is a skip, not a stuck squad. The
    /// threshold is the waypoint count itself, not a
    /// new tuned number, so a full circuit is exactly
    /// the proof that every waypoint was tried and
    /// refused in turn.</summary>
    public bool IsPatrolStuck => Doctrine == TacticalDoctrine.Patrol
        && PatrolConsecutiveRefusals >= PatrolWaypointCount;

    /// <summary>Slice 20 Step 1: move a squad's patrol loop
    /// to a new rally, without moving the squad itself.
    /// Internal, called only by
    /// WorldZone.TryReRallyStuckPatrol after that method
    /// has checked the squad is a live, stuck Patrol and
    /// the new rally is in bounds and different from the
    /// old one — this method performs no gate of its
    /// own, so a caller that reached it has already
    /// passed the gate, and a caller that must not
    /// re-rally must never call it directly. The new
    /// rally re-derives the whole loop through
    /// PatrolTarget (rally, then east, south, west,
    /// and north at PatrolLoopRadius, unchanged), so
    /// the radius and waypoint count never move with
    /// a re-rally, only the loop's centre does. The
    /// waypoint index and the refusal counter both
    /// reset to 0: the loop is new, so its progress
    /// is new too, and the snapshot reads Patrolling
    /// at once, not Stuck — honestly, because the
    /// squad is no longer on the failed loop, not
    /// because the block vanished. If the new loop
    /// is itself fully blocked, the counter climbs
    /// again and the squad reads Stuck after a full
    /// circuit on the new loop, proved in tests, so
    /// a rescue that cannot fail again is not smuggled
    /// in here. Position, health, doctrine, and kind
    /// are untouched: a re-rally is not a teleport,
    /// not a heal, and not a redeploy, and it costs
    /// no EmberOre and frees no cap slot — see
    /// TryReRallyStuckPatrol and DECISIONS.md.</summary>
    internal void ReRally(int rallyX, int rallyY, int rallyZ)
    {
        RallyX = rallyX; RallyY = rallyY; RallyZ = rallyZ;
        PatrolWaypointIndex = 0;
        PatrolConsecutiveRefusals = 0;
    }

    /// <summary>Slice 15 Step 1: the cell of the current
    /// patrol waypoint, derived from the rally and
    /// PatrolWaypointIndex on every call, never stored.
    /// An out-of-bounds or solid waypoint is still a
    /// waypoint: MoveForPatrol sidesteps and then skips
    /// it when its step is refused (Slice 18 Step 1),
    /// rather than the derivation pretending such a
    /// waypoint cannot exist.</summary>
    public (int X, int Y, int Z) PatrolTarget => PatrolWaypointIndex switch
    {
        0 => (RallyX, RallyY, RallyZ),
        1 => (RallyX + PatrolLoopRadius, RallyY, RallyZ),
        2 => (RallyX, RallyY, RallyZ + PatrolLoopRadius),
        3 => (RallyX - PatrolLoopRadius, RallyY, RallyZ),
        _ => (RallyX, RallyY, RallyZ - PatrolLoopRadius),
    };

    /// <summary>Slice 15 Step 1: true while the squad stands
    /// on its current patrol waypoint. Standing on a
    /// waypoint is a turn, not a hold: MoveForPatrol
    /// advances past it in the same tick.</summary>
    public bool AtPatrolTarget
    {
        get
        {
            var t = PatrolTarget;
            return X == t.X && Y == t.Y && Z == t.Z;
        }
    }

    /// <summary>Slice 15 Step 1, decided further in Slice
    /// 18 Step 1: one Patrol tick's single move, decided
    /// once so the squad can never take two steps in
    /// one tick. Standing on a waypoint advances the
    /// index first (turn and continue, no dwell tick),
    /// then the squad steps toward the next waypoint
    /// through the shared MoveTowardPosition rule —
    /// same axis choice, tie-break, and solid /
    /// enemy-occupied refusals as every other
    /// doctrine, so a patrol step never walks through
    /// a block or onto an enemy to stay on route.
    /// Slice 18 Step 1 changes only what happens when
    /// that direct step is refused, and only in this
    /// method, never in the shared rule: first a
    /// distance-reducing sidestep is tried — the
    /// remaining gap axes, largest gap first with
    /// the shared X, then Y, then Z tie-break, each
    /// step still one voxel, still refused into a
    /// solid block, out of bounds, or onto a living
    /// enemy, and still reducing the Manhattan
    /// distance to the waypoint by exactly one, so a
    /// sidestep can never wander away from the route
    /// or loop around a wall. A sidestep move keeps
    /// the waypoint index, so the next tick retries
    /// the same waypoint from the sidestepped cell.
    /// Only if every gap axis is refused does the
    /// step skip that waypoint for this circuit
    /// (index advances, no move that tick, refusal
    /// counter increments) instead of holding at a
    /// wall forever: the skipped waypoint is tried
    /// again on the next circuit. A single-axis
    /// block therefore still skips, exactly as Slice
    /// 15 proved, because there is no second gap axis
    /// to sidestep onto; a diagonal block sidesteps
    /// instead of skipping. A squad whose every
    /// waypoint is blocked cycles its index without
    /// moving, which is still not a hold — it is
    /// waiting on no single waypoint — but after a
    /// full circuit of such refusals it reads Stuck
    /// (see IsPatrolStuck), not Patrolling, so a
    /// player watching an enclosed patrol is not
    /// told a dead route is being walked. No
    /// pathfinder is added here: a sidestep is one
    /// voxel on a gap axis, not a planned route, a
    /// route cache, or a search over several cells.
    /// The squad never chases: enemies are handled
    /// by FindTarget's fire-in-range rule, not by
    /// movement, so a patrol that shoots in passing
    /// is still walking its route on the next tick,
    /// and a stuck patrol fires exactly as a moving
    /// patrol fires — the stuck reading changes the
    /// display word, never the fire rule. Returns
    /// true if the squad moved.</summary>
    public bool MoveForPatrol(VoxelChunk chunk, IReadOnlyList<Enemy> enemies)
    {
        if (AtPatrolTarget) PatrolWaypointIndex = (PatrolWaypointIndex + 1) % PatrolWaypointCount;
        var target = PatrolTarget;
        bool moved = MoveTowardPosition(target.X, target.Y, target.Z, chunk, enemies);
        if (moved)
        {
            PatrolConsecutiveRefusals = 0;
            return true;
        }
        if (X == target.X && Y == target.Y && Z == target.Z) return false;
        // Direct step refused: try a sidestep on the
        // remaining gap axes, largest gap first, shared
        // tie-break. The primary axis is already refused
        // under this chunk and enemy list, so it is not
        // retried. A successful sidestep is still one
        // voxel, still distance-reducing, and keeps the
        // waypoint: the route is resumed, not skipped.
        int dx = target.X - X, dy = target.Y - Y, dz = target.Z - Z;
        var axes = new (int Axis, int Abs, int Sign)[]
        {
            (0, Math.Abs(dx), Math.Sign(dx)),
            (1, Math.Abs(dy), Math.Sign(dy)),
            (2, Math.Abs(dz), Math.Sign(dz)),
        };
        // Bubble-sort three entries: largest gap first,
        // ties X, then Y, then Z — the shared rule's
        // tie-break, applied to the sidestep order too.
        for (int i = 0; i < axes.Length; i++)
            for (int j = i + 1; j < axes.Length; j++)
                if (axes[j].Abs > axes[i].Abs
                    || (axes[j].Abs == axes[i].Abs && axes[j].Axis < axes[i].Axis))
                    (axes[i], axes[j]) = (axes[j], axes[i]);
        foreach (var a in axes)
        {
            if (a.Abs == 0) continue;
            if (a.Axis == axes[0].Axis) continue;
            if (TryStepOnAxis(a.Axis, a.Sign, chunk, enemies))
            {
                PatrolConsecutiveRefusals = 0;
                return true;
            }
        }
        // Every gap axis refused (solid, enemy-occupied,
        // or out of bounds): skip this waypoint for
        // this circuit, as Slice 15 decided, and count
        // the refusal toward the stuck reading.
        PatrolConsecutiveRefusals++;
        PatrolWaypointIndex = (PatrolWaypointIndex + 1) % PatrolWaypointCount;
        return false;
    }

    /// <summary>Slice 18 Step 1: one voxel on a single
    /// named axis, under the shared rule's refusals
    /// (out of bounds, solid block, living enemy's
    /// cell) and nothing else. Patrol's sidestep only:
    /// no other doctrine calls it, and the shared
    /// MoveTowardPosition rule is untouched, so no
    /// other doctrine can gain a sidestep by
    /// accident.</summary>
    private bool TryStepOnAxis(int axis, int sign,
        VoxelChunk chunk, IReadOnlyList<Enemy> enemies)
    {
        int nx = X, ny = Y, nz = Z;
        if (axis == 0) nx += sign;
        else if (axis == 1) ny += sign;
        else nz += sign;
        if (!VoxelChunk.InBounds(nx, ny, nz)) return false;
        if (BlockInfo.IsSolid(chunk.Get(nx, ny, nz))) return false;
        foreach (var e in enemies)
            if (!e.IsDead && e.X == nx && e.Y == ny && e.Z == nz) return false;
        X = nx; Y = ny; Z = nz;
        return true;
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

    /// <summary>The single squad movement rule, shared by all
    /// doctrines: one voxel on the largest-gap axis (ties X, then
    /// Y, then Z), refused out of bounds, into a solid block, or
    /// onto a living enemy's cell. A call already at its target
    /// moves nowhere and returns false (Slice 11 Step 2: a
    /// Defend Core guard at its post must report no move, not
    /// a phantom step to its own cell). Callers choose the target;
    /// this method owns only the step.</summary>
    private bool MoveTowardPosition(int targetX, int targetY, int targetZ,
        VoxelChunk chunk, IReadOnlyList<Enemy> enemies)
    {
        if (targetX == X && targetY == Y && targetZ == Z) return false;
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
    /// silence it. Defend Core fires whenever in range too
    /// (Slice 11 Step 2): a chasing or returning guard that
    /// passes an enemy in range shoots it, but — movement is
    /// a separate decision — it does not chase a target that
    /// is not a core attacker; only MoveForDefendCore decides
    /// where the squad walks. Patrol fires whenever in range
    /// too (Slice 15 Step 1), by the same branch: it shoots
    /// in passing while walking its route, and — movement is
    /// a separate decision — it never chases; only
    /// MoveForPatrol decides where the squad walks, so a
    /// firing patrol is still on route on the next tick.
    /// Returns null if dead, or nothing is in range (or, for
    /// HoldGround, the squad is still moving).
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
/// to refund through. Slice 20 Step 1 adds no refund and
/// no price change: a stuck patrol's re-rally (see
/// WorldZone.TryReRallyStuckPatrol) costs 0 EmberOre
/// because it moves a squad already paid for, it does
/// not deploy a new one, and it frees no cap slot —
/// a rescue priced like a deploy would charge twice
/// for one presence. Both values are first-pass values for
/// determinism and testability, not final balance (see the
/// Slice 3 scope's no-balance-pass rule). Enforcement lives in
/// WorldZone.TryDeployPaidSquad, which checks cap and
/// affordability BEFORE charging, so a refused deploy charges
/// nothing and deploys nothing — no partial charge, no
/// half-deployed squad. The picker (OrbitalCommand.Confirm)
/// deploys through that paid path, so the play path is priced;
/// Since Slice 18 Step 2 there is no public free spawn
/// primitive at all: the former WorldZone.DeploySquad was
/// removed, and headless tests use the internal test seam
/// WorldZone.SpawnSquad (friend assembly only). The demo
/// already deploys through the paid path and never used the
/// free path. A free deploy is never the Unity play path.
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
    /// checked here, matching WorldZone.SpawnSquad: a rally inside
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
/// Advance is documented to behave). A Defend Core squad is
/// Guarding at its post, Hunting a core attacker, or Marching
/// back to its post. A Patrol squad is Patrolling on its
/// loop, firing in passing or not, or Stuck when a full
/// circuit of its waypoints has been refused in turn
/// (Slice 18 Step 1): Stuck is a Patrol reading only,
/// never a word for another doctrine, and it changes
/// the display word, never movement or fire. The word
/// is doctrine truth, not rally proximity.
/// </summary>
public enum SquadActivity
{
    Marching = 0,
    Holding = 1,
    Hunting = 2,
    Idle = 3,
    /// <summary>Slice 11 Step 2: a Defend Core squad standing at
    /// its guard post (the core cell) with no core attacker to
    /// chase. A Defend Core squad away from the core with no
    /// attacker reads Marching (the return march, not a rally
    /// march), and one chasing a core attacker reads Hunting,
    /// so the word still says what the squad is doing, not
    /// merely which doctrine it holds.</summary>
    Guarding = 4,
    /// <summary>Slice 15 Step 1: a Patrol squad walking its
    /// loop, whatever waypoint it is on and whether or not
    /// an enemy is in range. It is its own word, not a
    /// reuse of Marching, because Marching already means
    /// two different things (a HoldGround squad marching to
    /// its rally, a Defend Core squad marching back to its
    /// post) and a patrol is neither: it is not going to a
    /// place it will hold, and it is not returning to a
    /// post it left. A patrolling squad firing in passing
    /// still reads Patrolling, not Hunting, because it
    /// chases nothing — Hunting would claim a prey the
    /// squad will never walk toward. The marker
    /// distinction from Slice 13 Step 2 is by kind, not by
    /// doctrine, so no new marker shape is added here:
    /// the text line carries the doctrine word, the marker
    /// carries the kind shape.</summary>
    Patrolling = 5,
    /// <summary>Slice 18 Step 1: a Patrol squad whose
    /// every waypoint has been refused in turn, for a
    /// full circuit of consecutive refusals, with no
    /// sidestep move in between. Appended only, so no
    /// earlier activity value moves. The Unity text
    /// line already renders Activity.ToString(), so
    /// this word reaches the scene with no adapter
    /// edit and no marker change: the marker still
    /// carries the kind shape, the text line now
    /// carries the stuck truth instead of Patrolling.
    /// A stuck patrol still fires in passing, exactly
    /// as a moving patrol does — Stuck is a display
    /// word, not a hold, not a fire rule, and not a
    /// new doctrine.</summary>
    Stuck = 6,
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
            else if (s.Doctrine == TacticalDoctrine.DefendCore)
            {
                // Defend Core activity is post-and-attacker
                // truth, not rally truth: chasing a core
                // attacker reads Hunting with that attacker as
                // the target; at the core with no attacker it
                // reads Guarding; away from the core with no
                // attacker it reads Marching, the return march.
                // An enemy far from the core is not a prey
                // here, even if it stands next to the guard —
                // NearestCoreAttacker applies the chase
                // radius, so display and movement agree.
                prey = s.NearestCoreAttacker(world.Enemies, world.CoreX, world.CoreY, world.CoreZ);
                bool atCore = s.X == world.CoreX && s.Y == world.CoreY && s.Z == world.CoreZ;
                activity = prey != null ? SquadActivity.Hunting
                    : atCore ? SquadActivity.Guarding : SquadActivity.Marching;
            }
            else if (s.Doctrine == TacticalDoctrine.Patrol)
            {
                // Patrol activity is route truth, not prey
                // truth: a patrolling squad reads Patrolling
                // on every waypoint, firing in passing or
                // not, and never Hunting, because it chases
                // nothing — prey stays null, so the snapshot
                // cannot claim a target the movement will
                // never walk toward. Slice 18 Step 1 adds
                // the one exception that keeps route truth
                // honest: a squad that has refused every
                // waypoint in a full circuit reads Stuck,
                // not Patrolling, because a dead route
                // must not be named as a walked one. A
                // stuck patrol still fires in passing, so
                // Stuck changes the word, never the fire
                // rule. Display and movement agree because
                // both read the same doctrine and the
                // same live refusal counter: MoveForPatrol
                // walks the loop and counts refusals, this
                // branch names the loop or its stuck state.
                activity = s.IsPatrolStuck ? SquadActivity.Stuck : SquadActivity.Patrolling;
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
