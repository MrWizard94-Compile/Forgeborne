using System.Text.Json;
using System.Text.Json.Serialization;

namespace Forgeborne.Core;

public sealed class SaveData
{
    public int Seed { get; set; }
    public int Tick { get; set; }
    public string ChunkBase64 { get; set; } = "";
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public int PlayerZ { get; set; }
    public int PlayerHealth { get; set; }
    public int PlayerShields { get; set; }
    public Dictionary<string, int> Inventory { get; set; } = new();
    public List<int[]> Enemies { get; set; } = new();
    /// <summary>One entry per squad: x, y, z, rallyX, rallyY, rallyZ,
    /// health, doctrine (int), kind (int). Missing in old saves,
    /// which then load with no squads, as before.</summary>
    public List<int[]> Squads { get; set; } = new();
    // Slice 3 save-format completion: all nullable, declared last so
    // existing field order is unchanged. A missing field means an old
    // save, never an error: CoreHealth defaults to 100 (a fresh core,
    // as before), and a missing skill build or weapon means "keep
    // whatever the loading Simulation already has" (see LoadSave).
    // Null fields are omitted when writing, so a world-only save
    // stays close to its old shape plus CoreHealth.
    /// <summary>Core objective health. Null in old saves: the core loads at 100, as before. A present 0 means the core was destroyed, not a missing field.</summary>
    public int? CoreHealth { get; set; }
    /// <summary>Applied skill build, as SkillBuild.Encode(). Null in old saves: no build is restored.</summary>
    public string? SkillBuild { get; set; }
    /// <summary>Equipped weapon state, all five fields together or none: name, damage, mag capacity, current mag, reserve. Any missing field skips the weapon restore, never fails the load.</summary>
    public string? WeaponName { get; set; }
    public int? WeaponDamage { get; set; }
    public int? WeaponMagCapacity { get; set; }
    public int? WeaponMag { get; set; }
    public int? WeaponReserve { get; set; }
}

/// <summary>A loaded save: the world, plus the Simulation-level state
/// the save carries (skill build, equipped weapon), if any. Skills
/// is null when the save carries no build or a malformed one;
/// Weapon is null when the save carries no complete weapon.
/// LoadZone returns only the world, for callers that need nothing
/// else; Simulation.LoadFromJson uses the full result.</summary>
public sealed class LoadedSave
{
    public required WorldZone World { get; set; }
    public SkillBuild? Skills { get; set; }
    public Weapon? Weapon { get; set; }
}

public static class SaveSystem
{
    private static readonly JsonSerializerOptions SaveOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Save a world only: core health is saved, but no skill build or weapon (a world has neither). Simulation.SaveToJson is the full save.</summary>
    public static string SaveZone(WorldZone world) => SaveZone(world, null, null);

    public static string SaveZone(WorldZone world, SkillBuild? skills, Weapon? weapon)
    {
        var data = new SaveData
        {
            Seed = world.Seed,
            Tick = world.Tick,
            ChunkBase64 = Convert.ToBase64String(world.Chunk.ToBytes()),
            PlayerX = world.Player.X,
            PlayerY = world.Player.Y,
            PlayerZ = world.Player.Z,
            PlayerHealth = world.Player.Health,
            PlayerShields = world.Player.Shields,
            Inventory = world.Player.Inventory.Items.ToDictionary(k => k.Key.ToString(), v => v.Value),
            Enemies = world.Enemies.Select(e => new[] { e.X, e.Y, e.Z, e.Health }).ToList(),
            Squads = world.Squads.Select(s => new[] { s.X, s.Y, s.Z, s.RallyX, s.RallyY, s.RallyZ, s.Health, (int)s.Doctrine, (int)s.Kind }).ToList(),
            CoreHealth = world.CoreHealth,
            SkillBuild = skills?.Encode(),
            WeaponName = weapon?.Name,
            WeaponDamage = weapon?.Damage,
            WeaponMagCapacity = weapon?.MagCapacity,
            WeaponMag = weapon?.Mag,
            WeaponReserve = weapon?.Reserve,
        };
        return JsonSerializer.Serialize(data, SaveOptions);
    }

    public static WorldZone LoadZone(string json) => LoadSave(json).World;

    public static LoadedSave LoadSave(string json)
    {
        var data = JsonSerializer.Deserialize<SaveData>(json)
            ?? throw new InvalidOperationException("Save data is null.");
        var world = new WorldZone(data.Seed);
        world.Chunk.FromBytes(Convert.FromBase64String(data.ChunkBase64));
        // Parse the skill build BEFORE restoring the player, so the
        // raised shield cap is in place before shields are restored:
        // without this, a skilled save with 100 shields would clamp
        // to the base 85 cap and the build would restore the cap but
        // not the shields it was protecting. A malformed build string
        // is skipped, never fatal, matching the 0.10.0 squad rule.
        SkillBuild? skills = null;
        if (data.SkillBuild != null)
        {
            try { skills = SkillBuild.Decode(data.SkillBuild); }
            catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException) { skills = null; }
        }
        if (skills != null)
        {
            world.Player.SetMaxShields(85 + (int)skills.TotalEffect("ShieldBonus"));
            world.WorkstationRegenPerTick = 5 + (int)skills.TotalEffect("WorkstationRegenBonus");
        }
        world.Player.X = data.PlayerX;
        world.Player.Y = data.PlayerY;
        world.Player.Z = data.PlayerZ;
        // Restore health/shields through the damage/heal path in reverse:
        // start at 100/85, then apply the difference as damage/heal.
        // This keeps Player's invariants (0..100, 0..cap, dead at 0) intact.
        // Damage hits shields first, so restoring an arbitrary pair needs
        // a direct path — see Player.Restore in Player.cs.
        world.Player.Restore(data.PlayerHealth, data.PlayerShields);
        foreach (var (k, v) in data.Inventory)
            if (Enum.TryParse<BlockType>(k, out var type))
                world.Player.Inventory.Add(type, v);
        world.Enemies.Clear();
        foreach (var e in data.Enemies)
        {
            if (e.Length == 3) world.Enemies.Add(new Enemy(e[0], e[1], e[2]));
            else if (e.Length == 4)
            {
                var enemy = new Enemy(e[0], e[1], e[2]);
                enemy.TakeDamage(30 - e[3]);
                world.Enemies.Add(enemy);
            }
        }
        world.RestoreTick(data.Tick);
        world.Squads.Clear();
        foreach (var s in data.Squads)
        {
            // Skip malformed entries rather than failing the whole load:
            // wrong length, out-of-bounds position/rally, unknown
            // doctrine/kind, or a dead squad (removed on next tick anyway).
            if (s.Length != 9) continue;
            if (!VoxelChunk.InBounds(s[0], s[1], s[2]) || !VoxelChunk.InBounds(s[3], s[4], s[5])) continue;
            if (!Enum.IsDefined(typeof(TacticalDoctrine), s[7]) || !Enum.IsDefined(typeof(SquadKind), s[8])) continue;
            if (s[6] <= 0) continue;
            world.Squads.Add(new TacticalSquad(s[0], s[1], s[2], s[3], s[4], s[5], s[6],
                (TacticalDoctrine)s[7], (SquadKind)s[8]));
        }
        // Core health: a present value (including 0, destroyed) is
        // restored as-is, clamped; a missing value is an old save
        // and keeps the fresh 100-HP core, as before.
        if (data.CoreHealth.HasValue) world.RestoreCoreHealth(data.CoreHealth.Value);
        // Weapon: all five fields or none. A partial weapon is skipped,
        // never fatal, and the caller keeps its current weapon.
        Weapon? weapon = null;
        if (data.WeaponName != null && data.WeaponDamage.HasValue && data.WeaponMagCapacity.HasValue
            && data.WeaponMag.HasValue && data.WeaponReserve.HasValue)
        {
            weapon = Weapon.FromSaved(data.WeaponName, data.WeaponDamage.Value, data.WeaponMagCapacity.Value,
                data.WeaponMag.Value, data.WeaponReserve.Value);
        }
        return new LoadedSave { World = world, Skills = skills, Weapon = weapon };
    }
}
