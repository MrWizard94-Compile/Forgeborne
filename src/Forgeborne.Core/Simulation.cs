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

    public void Tick(float dt, float forward, float strafe, bool jump)
    {
        Fps.Update(dt, forward, strafe, jump, World.Chunk);
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

    public GatherResult GatherAhead()
    {
        // Gather the ground block directly in front of the FPS player.
        int x = (int)Math.Floor(Fps.X);
        int y = (int)Math.Floor(Fps.Y) - 1;
        int z = (int)Math.Floor(Fps.Z) + 1;
        var blockBefore = World.Chunk.Get(x, y, z);
        var r = World.Gather(x, y, z);
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

    public BuildResult BuildWallAhead()
    {
        // Build a wall at feet level, front-right of the player, supported by the ground below.
        // Front-centre is used by GatherAhead in tests, so front-right avoids building on a just-gathered cell.
        int x = (int)Math.Floor(Fps.X) + 1;
        int y = (int)Math.Floor(Fps.Y);
        int z = (int)Math.Floor(Fps.Z) + 1;
        var r = World.Build(x, y, z, BlockType.Wall);
        if (r == BuildResult.Success) BlocksBuilt++;
        return r;
    }

    /// <summary>
    /// Orbital Tactical command from the headless loop: deploy a
    /// Vanguard squad under the HoldGround doctrine. The squad
    /// spawns at the player's grid position and moves to the rally
    /// on subsequent ticks; see WorldZone.DeploySquad for the
    /// rejection rules. Returns null if the rally is rejected.
    /// This direct path remains for headless use and the demo;
    /// the Unity adapter now goes through OrbitalCommand instead,
    /// so a deploy in play is always a picked rally, not a key
    /// that fires a squad at the player's feet.
    /// </summary>
    public TacticalSquad? DeployOrbitalSquad(int rallyX, int rallyY, int rallyZ) =>
        World.DeploySquad(rallyX, rallyY, rallyZ);

    /// <summary>
    /// Doctrine menu / rally picker state (0.15.0): the selected
    /// doctrine/kind and the pending rally, if any. The Unity
    /// adapter binds O / crosshair / Enter / Esc to this object;
    /// it holds no rendering and no second copy of squad rules.
    /// </summary>
    public OrbitalCommand OrbitalCommand { get; } = new();

    /// <summary>Open the rally picker (clears any stale pending rally).</summary>
    public void BeginOrbitalPick() => OrbitalCommand.BeginPick();
    /// <summary>Update the pending rally while picking; false outside picking or out of bounds.</summary>
    public bool SetOrbitalRally(int x, int y, int z) => OrbitalCommand.SetRally(x, y, z);
    /// <summary>Cancel the pick: no squad, pending rally dropped.</summary>
    public void CancelOrbitalPick() => OrbitalCommand.CancelPick();
    /// <summary>Deploy the picker's selected squad at its pending rally; null if not picking, no rally, or rejected.</summary>
    public TacticalSquad? ConfirmOrbitalDeploy() => OrbitalCommand.Confirm(World);

    public BuildResult BuildKineticMillAhead()
    {
        // Front-left, mirroring BuildWallAhead's front-right, so the
        // two builders do not collide in tests or in play. Supported
        // by the ground below, like a wall.
        int x = (int)Math.Floor(Fps.X) - 1;
        int y = (int)Math.Floor(Fps.Y);
        int z = (int)Math.Floor(Fps.Z) + 1;
        var r = World.Build(x, y, z, BlockType.KineticMill);
        if (r == BuildResult.Success) BlocksBuilt++;
        return r;
    }

    public bool FireAtNearestEnemy()
    {
        if (!Weapon.Fire()) return false;
        Enemy? nearest = null;
        double best = double.MaxValue;
        foreach (var e in World.Enemies)
        {
            double d = Math.Abs(e.X - Fps.X) + Math.Abs(e.Y - Fps.Y) + Math.Abs(e.Z - Fps.Z);
            if (d < best) { best = d; nearest = e; }
        }
        if (nearest == null) return true; // shot fired, nothing hit
        World.AttackEnemy(nearest, EffectiveDamage);
        if (nearest.IsDead) EnemiesKilled++;
        return true;
    }

    /// <summary>Fire at the core if within 10 metres. Returns true if a shot was fired.</summary>
    public bool FireAtCore()
    {
        if (!Weapon.Fire()) return false;
        double dx = World.CoreX - Fps.X, dy = World.CoreY - Fps.Y, dz = World.CoreZ - Fps.Z;
        double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (dist <= 10) World.DamageCore(EffectiveCoreDamage);
        return true;
    }

    public HudState Hud() => HudState.FromWorld(World, Weapon);
    public bool IsObjectiveComplete => World.CoreHealth <= 0;
    public bool IsPlayerDead => World.Player.IsDead;
}
