using System.Text.Json;

namespace Forgeborne.Core;

/// <summary>
/// Headless simulation loop for Slice 1: one world, one FPS controller,
/// one weapon, one objective core. This is the build-around for
/// "no playable Unity build in this environment": the full loop
/// (move, gather, build, fight, survive, objective) runs and is
/// tested here, and Unity later only renders it and feeds it input.
/// </summary>
public sealed class Simulation
{
    /// <summary>
    /// Current world. Replaced in place by RestoreWorld so holders of
    /// this Simulation (e.g. Unity's GameManager and MeshRenderer) keep
    /// working without re-wiring; the Fps object itself is never
    /// replaced, only teleported to the restored player's position.
    /// </summary>
    public WorldZone World { get; private set; }
    public FpsController Fps { get; }
    /// <summary>Equipped weapon. Default is the Pulse Rifle; pass a GeneratedWeapon to play with loot stats instead of the fixed weapon.</summary>
    public Weapon Weapon { get; private set; }
    public int EnemiesKilled { get; private set; }
    public int BlocksGathered { get; private set; }
    public int BlocksBuilt { get; private set; }
    /// <summary>Currently applied skill build, or null if none. Re-applying replaces it; it never stacks with a previous build.</summary>
    public SkillBuild? AppliedSkills { get; private set; }
    /// <summary>Weapon damage after skill DamagePct, rounded away from zero so a small bonus is visible on a 10-damage weapon (10.5 -> 11, not 10).</summary>
    public int EffectiveDamage => (int)Math.Round(Weapon.Damage * (1 + (AppliedSkills?.TotalEffect("DamagePct") ?? 0) / 100.0), MidpointRounding.AwayFromZero);
    /// <summary>Core damage after skill DamagePct plus the Corebreaker CoreDamagePct bonus.</summary>
    public int EffectiveCoreDamage => (int)Math.Round(Weapon.Damage * (1 + ((AppliedSkills?.TotalEffect("DamagePct") ?? 0) + (AppliedSkills?.TotalEffect("CoreDamagePct") ?? 0)) / 100.0), MidpointRounding.AwayFromZero);
    // Fractional gather bonus carried across gathers: +20% means every
    // 5th successful gather yields one extra unit. Failed gathers do
    // not advance it. Reset on ApplySkills so a respec cannot carry
    // a near-complete bonus from an old build.
    private double _gatherBonusCarry;

    public Simulation(int seed, GeneratedWeapon? equipped = null, SkillBuild? skills = null)
    {
        World = new WorldZone(seed);
        Fps = new FpsController(World.Player.X, 4f, World.Player.Z);
        // Sync the grid player to the FPS controller's starting position.
        World.Player.X = (int)Math.Floor(Fps.X);
        World.Player.Z = (int)Math.Floor(Fps.Z);
        Weapon = equipped == null ? new Weapon() : Weapon.FromGenerated(equipped);
        if (skills != null) ApplySkills(skills);
    }

    /// <summary>
    /// Apply a skill build to gameplay, replacing any previous build:
    /// DamagePct scales weapon damage (enemy and core), CoreDamagePct
    /// adds on top for the core only, ShieldBonus raises the shield
    /// cap, MoveSpeedPct scales FPS speed, WorkstationRegenBonus adds
    /// to workstation shield regen per tick, GatherPct accrues a
    /// fractional bonus unit per gather. Returns false for null,
    /// leaving the current build untouched.
    /// </summary>
    public bool ApplySkills(SkillBuild? build)
    {
        if (build == null) return false;
        AppliedSkills = build;
        World.Player.SetMaxShields(85 + (int)build.TotalEffect("ShieldBonus"));
        Fps.SpeedMultiplier = 1f + (float)(build.TotalEffect("MoveSpeedPct") / 100.0);
        World.WorkstationRegenPerTick = 5 + (int)build.TotalEffect("WorkstationRegenBonus");
        _gatherBonusCarry = 0;
        return true;
    }

    /// <summary>
    /// Replace the current world with a loaded one, in place: this
    /// Simulation object (and its Fps object) survives, so Unity
    /// adapters holding it need no re-wiring. The FPS controller is
    /// teleported to the loaded player's grid position (fractional
    /// feet position is not saved, so it restarts at the cell corner).
    /// Session counters reset, because they describe the pre-load
    /// session; the equipped weapon and skill build are kept by this
    /// world-only path, because a WorldZone carries neither —
    /// LoadFromJson (which uses LoadSave) is the path that restores
    /// them from a full save. An applied skill build is re-applied
    /// to the new world, so caps, speed, and regen stay coherent
    /// instead of silently reverting to base values. Returns false
    /// for null, leaving everything untouched.
    /// </summary>
    public bool RestoreWorld(WorldZone? loaded)
    {
        if (loaded == null) return false;
        World = loaded;
        Fps.Teleport(loaded.Player.X, loaded.Player.Y, loaded.Player.Z);
        EnemiesKilled = 0;
        BlocksGathered = 0;
        BlocksBuilt = 0;
        if (AppliedSkills != null) ApplySkills(AppliedSkills);
        else _gatherBonusCarry = 0;
        return true;
    }

    /// <summary>Full save: world, applied skill build, and equipped weapon, via SaveSystem.SaveZone. World-only callers should keep using SaveSystem.SaveZone directly.</summary>
    public string SaveToJson() => SaveSystem.SaveZone(World, AppliedSkills, Weapon);

    /// <summary>
    /// Load a full save JSON into this Simulation via RestoreWorld.
    /// Fail-closed: null, empty, or malformed JSON returns false and
    /// leaves the current world untouched (the new world is fully
    /// parsed before any swap happens). A save that carries a skill
    /// build replaces the current build; a save that carries a weapon
    /// replaces the current weapon. A save that carries neither (an
    /// old save) keeps the current build and weapon, as RestoreWorld
    /// did before — and its core loads at 100 HP, as before, because
    /// the old format had no core field. A malformed skill string or
    /// a partial weapon in an otherwise valid save is skipped, never
    /// fatal, and leaves the current build / weapon in place.
    /// </summary>
    public bool LoadFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            var loaded = SaveSystem.LoadSave(json);
            if (!RestoreWorld(loaded.World)) return false;
            if (loaded.Skills != null) ApplySkills(loaded.Skills);
            if (loaded.Weapon != null) Weapon = loaded.Weapon;
            return true;
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or ArgumentException)
        {
            return false;
        }
    }

    /// <summary>Swap to a generated weapon (e.g. an enemy drop). Fresh weapon: full mag, 120 reserve. Returns false for null, leaving the current weapon untouched.</summary>
    public bool Equip(GeneratedWeapon? generated)
    {
        if (generated == null) return false;
        Weapon = Weapon.FromGenerated(generated);
        return true;
    }

    /// <summary>
    /// Slice 8 Step 1 — yaw-rotated movement: the adapter passes raw
    /// W/S (forward) and A/D (strafe) scalars, and this method rotates
    /// them by Look.Yaw before they reach the FPS controller. Yaw only,
    /// horizontal only: the forward used is (sin yaw, cos yaw) in XZ
    /// and the right used is (cos yaw, -sin yaw), so at yaw 0 the
    /// result is exactly the Slice 1 behaviour (W +Z, D +X), at yaw 90
    /// W walks +X and D walks -Z (the player's right when facing +X),
    /// and pitch never moves the player vertically — looking up does
    /// not make W climb, looking down does not make W dive. The two
    /// scalars are combined at full value each (no diagonal
    /// normalisation), preserving the Slice 1 diagonal-speed quirk;
    /// see DECISIONS.md 0.36.0. Gather, build, and fire read the
    /// same Look direction from Slice 8 Step 2 (see GatherAhead,
    /// BuildAimed, FireAimed).
    /// </summary>
    public void Tick(float dt, float forward, float strafe, bool jump)
    {
        float yawRad = Look.Yaw * (float)Math.PI / 180f;
        float sin = (float)Math.Sin(yawRad), cos = (float)Math.Cos(yawRad);
        float worldX = forward * sin + strafe * cos;
        float worldZ = forward * cos - strafe * sin;
        Fps.UpdateWorld(dt, worldX, worldZ, jump, World.Chunk);
        World.Player.X = (int)Math.Floor(Fps.X);
        World.Player.Y = (int)Math.Floor(Fps.Y);
        World.Player.Z = (int)Math.Floor(Fps.Z);
        int before = World.Enemies.Count;
        World.Advance();
        // Note: World.Advance removes dead enemies; kills are counted
        // by the caller via Attack, not here, to avoid double-counting
        // enemies that die from other causes in future slices.
        _ = before;
        // Damage from adjacent enemies is applied in World.Advance via
        // the grid player, which is synced from the FPS controller above.
        // Sync FPS health display back through HudState in callers.
    }

    /// <summary>
    /// Slice 8 Step 2 — aim constants, decided in this step
    /// (rationale in DECISIONS.md). EyeHeight 1.6 m is not a new
    /// invention: the Unity adapter already renders the camera
    /// at Fps.Y + 1.6, so core and adapter now agree on where
    /// the eye is instead of core borrowing an adapter height
    /// silently. AimReach 10 m is one reach for all three
    /// actions, a first-pass value for determinism and
    /// testability, not a feel claim — and it matches the old
    /// FireAtCore 10-metre bubble, so fire keeps its old range
    /// while gaining aim. An unbounded ray was refused: it
    /// would let a key press strip a distant hillside or hit
    /// an enemy anywhere in the chunk.
    /// </summary>
    public const float EyeHeight = 1.6f;
    public const float AimReach = 10f;

    /// <summary>Trace the player's aim ray (eye point, Look
    /// forward, AimReach) through the shared AimRay rule.</summary>
    private AimHit TraceAim(bool includeActors) =>
        AimRay.Trace(World.Chunk, World.Enemies, World.CoreX, World.CoreY, World.CoreZ,
            Fps.X, Fps.Y + EyeHeight, Fps.Z, Look.Forward(), AimReach, includeActors);

    /// <summary>
    /// Slice 8 Step 2 — gather by aim, replacing the fixed +Z
    /// ground cell: the first solid along the aim ray is the
    /// gather target, through the existing World.Gather rules
    /// (bedrock, walls, workstations, and mills are never
    /// gathered; a full inventory consumes nothing; the skill
    /// gather bonus accrues only on successful gathers).
    /// Looking down at the ground gathers ground, looking
    /// level gathers what is ahead at eye height, and looking
    /// up gathers nothing (Air) — there is no ground under a
    /// skyward ray within reach, and no fallback to +Z. A ray
    /// that leaves the chunk returns OutOfBounds. Actors do
    /// not block a gather: the ray passes through enemy and
    /// core cells to the first solid behind them.
    /// </summary>
    public GatherResult GatherAhead()
    {
        var hit = TraceAim(includeActors: false);
        if (hit.Kind != AimTargetKind.Solid)
            return hit.LeftBounds ? GatherResult.OutOfBounds : GatherResult.Air;
        var blockBefore = World.Chunk.Get(hit.X, hit.Y, hit.Z);
        var r = World.Gather(hit.X, hit.Y, hit.Z);
        if (r == GatherResult.Success)
        {
            BlocksGathered++;
            // Skill gather bonus: accrue the fractional bonus; a whole
            // unit is granted only when the carry reaches 1, so +20%
            // is exactly one bonus unit per 5 successful gathers.
            _gatherBonusCarry += (AppliedSkills?.TotalEffect("GatherPct") ?? 0) / 100.0;
            if (_gatherBonusCarry >= 1)
            {
                _gatherBonusCarry -= 1;
                World.Player.Inventory.Add(blockBefore, 1);
            }
        }
        return r;
    }

    /// <summary>
    /// Slice 8 Step 2 — build by aim, shared by all three
    /// builders: the build cell is the last air cell before
    /// the first solid along the aim ray (face-adjacent to
    /// that solid, because the ray steps face-by-face), through
    /// the existing World.Build air / support / cost rules.
    /// The old front-right / front-left / front-centre offsets
    /// retire in this step: the three builders are now
    /// distinguished by block type, not by offset, because
    /// keeping offsets under an aim rule would mean the aim
    /// decides one cell and the offset silently moves it to
    /// another. A ray with no solid within reach has no aimed
    /// surface, so the build fails Floating (and spends
    /// nothing, since World.Build is never reached); a ray
    /// that leaves the chunk fails OutOfBounds. The
    /// Dynamo-mill face-adjacency power rule is unchanged:
    /// a powered pair is still a mill face-adjacent to a
    /// Dynamo, however the two blocks were placed.
    /// </summary>
    private BuildResult BuildAimed(BlockType type)
    {
        var hit = TraceAim(includeActors: false);
        if (hit.Kind != AimTargetKind.Solid)
            return hit.LeftBounds ? BuildResult.OutOfBounds : BuildResult.Floating;
        var r = World.Build(hit.PrevX, hit.PrevY, hit.PrevZ, type);
        if (r == BuildResult.Success) BlocksBuilt++;
        return r;
    }

    public BuildResult BuildWallAhead() => BuildAimed(BlockType.Wall);

    /// <summary>Slice 10 Step 1: a Workstation by aim (see
    /// BuildAimed), costing 10 Stone + 5 EmberOre as in Slice 1.
    /// This closes the workstation's missing aimed builder: before
    /// this step a workstation could only be placed by direct
    /// World.Build, with no adapter key and no aim rule, while
    /// every other machine had one. No new block type, no new
    /// cost — the same workstation, now placeable by aim.</summary>
    public BuildResult BuildWorkstationAhead() => BuildAimed(BlockType.Workstation);

    /// <summary>
    /// Orbital Tactical command from the headless loop: deploy a
    /// Vanguard squad under the HoldGround doctrine. The squad
    /// spawns at the player's grid position and moves to the rally
    /// on subsequent ticks; see WorldZone.DeploySquad for the
    /// rejection rules. Returns null if the rally is rejected.
    /// This direct path remains for headless use and the demo
    /// ONLY: it is the free spawn primitive and ignores the squad
    /// economy. A deploy in play is always a picked rally through
    /// ConfirmOrbitalDeploy (paid), or this paid headless path.
    /// </summary>
    public TacticalSquad? DeployOrbitalSquad(int rallyX, int rallyY, int rallyZ) =>
        World.DeploySquad(rallyX, rallyY, rallyZ);

    /// <summary>True when a paid deploy would succeed right now: under the live-squad cap and able to pay 10 EmberOre.</summary>
    public bool CanDeployOrbitalSquad() => SquadEconomy.CanDeploy(World);

    /// <summary>
    /// Paid headless deploy: same squad as DeployOrbitalSquad,
    /// but through the squad economy — refused (null, no charge,
    /// no squad) when unaffordable or at the 3-live-squad cap.
    /// This is the headless equivalent of a picker Confirm.
    /// </summary>
    public TacticalSquad? DeployOrbitalSquadPaid(int rallyX, int rallyY, int rallyZ) =>
        World.TryDeployPaidSquad(rallyX, rallyY, rallyZ);

    /// <summary>
    /// Doctrine menu / rally picker state (0.15.0): the selected
    /// doctrine/kind and the pending rally, if any. The Unity
    /// adapter binds O / crosshair / Enter / Esc to this object;
    /// it holds no rendering and no second copy of squad rules.
    /// </summary>
    /// <summary>
    /// FPS view direction (Slice 6 Step 1, movement rotated by it in
    /// Slice 8 Step 1, gather / build / fire aimed by it in
    /// Slice 8 Step 2): yaw + clamped pitch, with a derived forward
    /// vector. The Unity adapter feeds mouse deltas here and renders
    /// the camera from it; it holds no yaw of its own any more.
    /// Session input state, not saved: a fresh Simulation starts at
    /// yaw 0, pitch 0, and RestoreWorld does not reset it.
    /// </summary>
    public Look Look { get; } = new();

    /// <summary>Feed already-scaled mouse deltas into Look: positive
    /// yawDelta turns toward +X, positive pitchDelta looks up.</summary>
    public void ApplyLook(float yawDeltaDegrees, float pitchDeltaDegrees) =>
        Look.Add(yawDeltaDegrees, pitchDeltaDegrees);

    public OrbitalCommand OrbitalCommand { get; } = new();

    /// <summary>Open the rally picker (clears any stale pending rally).</summary>
    public void BeginOrbitalPick() => OrbitalCommand.BeginPick();
    /// <summary>Update the pending rally while picking; false outside picking or out of bounds.</summary>
    public bool SetOrbitalRally(int x, int y, int z) => OrbitalCommand.SetRally(x, y, z);
    /// <summary>Cancel the pick: no squad, pending rally dropped.</summary>
    public void CancelOrbitalPick() => OrbitalCommand.CancelPick();
    /// <summary>Deploy the picker's selected squad at its pending rally, paying the squad economy price; null if not picking, no rally, unaffordable, or over-cap — in which cases the picker stays open, with no squad and no charge.</summary>
    public TacticalSquad? ConfirmOrbitalDeploy() => OrbitalCommand.Confirm(World);

    /// <summary>Slice 8 Step 2: a Kinetic Mill by aim (see
    /// BuildAimed). A powered pair is still built by aiming
    /// at the ground left-front for the mill and front for
    /// the Dynamo, with no move between builds.</summary>
    public BuildResult BuildKineticMillAhead() => BuildAimed(BlockType.KineticMill);

    /// <summary>Slice 8 Step 2: a Dynamo by aim (see BuildAimed),
    /// costing 5 EmberOre as in Slice 5 Step 1.</summary>
    public BuildResult BuildDynamoAhead() => BuildAimed(BlockType.Dynamo);

    /// <summary>Slice 9 Step 1: a Transformer by aim (see
    /// BuildAimed), costing 5 EmberOre like a Dynamo. Placed by
    /// aim like every other block since Slice 8 — no revived
    /// front-left / front-centre offset, and no placement rule
    /// of its own: the aim decides one cell and the builder
    /// builds there.</summary>
    public BuildResult BuildTransformerAhead() => BuildAimed(BlockType.Transformer);

    /// <summary>Slice 9 Step 2: a Cable by aim (see
    /// BuildAimed), costing 1 EmberOre per cell. Placed one
    /// cell at a time by aim like every other block since
    /// Slice 8 — no drag-to-lay or multi-cell placement mode
    /// rides along with the cable rule, per the scope.</summary>
    public BuildResult BuildCableAhead() => BuildAimed(BlockType.Cable);

    /// <summary>
    /// Slice 8 Step 2 — fire by aim, replacing BOTH the old
    /// FireAtNearestEnemy (Manhattan-nearest, aim-blind) and
    /// FireAtCore (10-metre bubble, aim-blind) with one aim
    /// rule: the first enemy cell or core cell the aim ray
    /// reaches within AimReach is hit; a solid block before
    /// either stops the shot, so fire cannot hit through a
    /// wall; a shot with nothing aimed still consumes ammo
    /// and hits nothing, as before — a fired shot is a fired
    /// shot, and making misses free would be an economy
    /// change, not an aim fix. Weapon damage, magazine, and
    /// reload rules do not change: aim changes what is hit,
    /// not how hard the weapon hits. Squad fire rules are
    /// untouched. Returns false only when the weapon cannot
    /// fire (empty magazine); returns true when a shot was
    /// fired, hit or not. The old methods are removed, not
    /// kept as aliases: keeping a method named "nearest"
    /// beside an aim rule would leave the aim-blind rule
    /// alive under its old name.
    /// </summary>
    public bool FireAimed()
    {
        if (!Weapon.Fire()) return false;
        var hit = TraceAim(includeActors: true);
        if (hit.Kind == AimTargetKind.Enemy && hit.Enemy != null)
        {
            World.AttackEnemy(hit.Enemy, EffectiveDamage);
            if (hit.Enemy.IsDead) EnemiesKilled++;
        }
        else if (hit.Kind == AimTargetKind.Core)
        {
            World.DamageCore(EffectiveCoreDamage);
        }
        return true; // shot fired; a solid, a miss, or no target still costs the round
    }

    public HudState Hud() => HudState.FromWorld(World, Weapon);
    public bool IsObjectiveComplete => World.CoreHealth <= 0;
    public bool IsPlayerDead => World.Player.IsDead;
}
