namespace Forgeborne.Core;

public sealed class Weapon
{
    public string Name { get; }
    public int MagCapacity { get; }
    public int Mag { get; private set; }
    public int Reserve { get; private set; }
    public int Damage { get; }

    /// <summary>Default Slice 1 weapon: the Pulse Rifle (42 mag / 120 reserve, 10 damage).</summary>
    public Weapon() : this("Pulse Rifle", 10, 42, 120) { }

    public Weapon(string name, int damage, int magCapacity, int reserve = 120)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Pulse Rifle" : name;
        Damage = Math.Max(1, damage);
        MagCapacity = Math.Max(1, magCapacity);
        Mag = MagCapacity;
        Reserve = Math.Max(0, reserve);
    }

    /// <summary>Build a live weapon from a procedurally generated one. No duplicate damage/mag logic: Simulation fires this Weapon, so a generated weapon's stats are the stats that actually hit.</summary>
    public static Weapon FromGenerated(GeneratedWeapon generated)
    {
        ArgumentNullException.ThrowIfNull(generated);
        return new Weapon(generated.Name, generated.Damage, generated.MagSize);
    }

    /// <summary>Restore a live weapon from saved state (Slice 3 save-format completion): the same clamps as the constructor, plus the in-progress mag, so a half-empty mag survives a load instead of refilling for free. Used only by SaveSystem.</summary>
    public static Weapon FromSaved(string name, int damage, int magCapacity, int mag, int reserve)
    {
        var weapon = new Weapon(name, damage, magCapacity, reserve);
        weapon.Mag = Math.Clamp(mag, 0, weapon.MagCapacity);
        return weapon;
    }

    public bool Fire()
    {
        if (Mag <= 0) return false;
        Mag--;
        return true;
    }

    public void Reload()
    {
        int need = MagCapacity - Mag;
        int take = Math.Min(need, Reserve);
        Mag += take;
        Reserve -= take;
    }
}

/// <summary>HUD snapshot for the reference image layout: health, shields, objective, ammo, mini-map data.</summary>
public sealed class HudState
{
    public int Health { get; set; }
    public int Shields { get; set; }
    public string ObjectiveName { get; set; } = "DESTROY CORE";
    public int ObjectiveDistanceMetres { get; set; }
    public int ObjectivePercentCompleted { get; set; }
    public int AmmoMag { get; set; }
    public int AmmoReserve { get; set; }
    public string WeaponName { get; set; } = "PULSE RIFLE";
    public int EnemyCount { get; set; }
    /// <summary>Slice 6 Step 3 power indicator, derived from placement
    /// on every snapshot, never saved: how many Kinetic Mills exist,
    /// and how many of them are powered (receiving their rated
    /// voltage, per WorldZone.IsPowered). Slice 9 Step 1 meaning,
    /// stated so the count cannot lie: a mill fed HV counts as
    /// NOT powered, because it runs at kinetic cadence in this
    /// step, not at the every-tick cadence the count describes.
    /// A Dynamo on its own counts as neither, and a wall or
    /// workstation next to a Dynamo counts as neither — only
    /// mills are consumers in this loop. No voltage split is
    /// added in this step: the count stays one number, and its
    /// meaning is the rated-voltage rule above.</summary>
    public int PoweredMills { get; set; }
    public int TotalMills { get; set; }
    /// <summary>Slice 10 Step 1 power indicator, second consumer:
    /// how many Workstations exist, and how many are powered
    /// (delivered their rated LV, per IsWorkstationPowered).
    /// An HV-fed workstation counts as NOT powered, because it
    /// is disabled that tick, not boosted — counting it as
    /// powered would be a display lie, the same lie the mill
    /// count avoids. An unpowered workstation counts as NOT
    /// powered, even though it still regens at base rate.</summary>
    public int PoweredWorkstations { get; set; }
    public int TotalWorkstations { get; set; }
    /// <summary>Slice 12 Step 2 power indicator, third consumer:
    /// how many Shock Pylons exist, and how many are powered
    /// (delivered their rated HV, per IsPylonPowered). An
    /// LV-fed pylon counts as NOT powered, because it is
    /// disabled that tick, not firing — counting it as powered
    /// would be the same display lie the mill and workstation
    /// counts avoid. An unpowered pylon counts as NOT powered,
    /// even though it still stands as a solid block.</summary>
    public int PoweredPylons { get; set; }
    public int TotalPylons { get; set; }
    /// <summary>Slice 12 Step 1 live-power display, derived from
    /// placement on every snapshot, never saved: how many live
    /// electrical structures exist that no consumer count already
    /// shows (LiveNetworks: cable components carrying a voltage
    /// that is not None, and standalone transformer groups with a
    /// non-None output, each feeding no consumer), and how many
    /// lone sources (LoneSources: Dynamo / Alternator / Conduit
    /// cells with no cable, transformer, or consumer adjacent;
    /// a Conduit counts since Slice 14 Step 1). See
    /// WorldZone.LivePowerCounts for the decided rules: networks
    /// not cells, no voltage split (LV and HV count alike), and
    /// this count is not a warning — an HV network counted here
    /// is live, not safe. Removing a source drops the count on
    /// the next snapshot, like the consumer counts.</summary>
    public int LiveNetworks { get; set; }
    public int LoneSources { get; set; }
    /// <summary>Slice 13 Step 1 warning readout, derived from
    /// placement and delivered voltage on every snapshot, never
    /// saved, in the pattern of PoweredMills / TotalMills — but
    /// split per outcome, not folded into one number, because a
    /// mill about to be destroyed and a workstation about to be
    /// disabled are not the same loss, and one number would hide
    /// which machine the player built. WarnedMills counts mills
    /// where IsMismatched is true (delivered HV: destroyed on
    /// the next tick if the wiring stays); WarnedWorkstations
    /// counts workstations where IsWorkstationMismatched is true
    /// (delivered HV: disabled that tick, recoverable on
    /// rewiring); WarnedPylons counts pylons where
    /// IsPylonMismatched is true (delivered LV: disabled that
    /// tick, recoverable on rewiring). A powered, unpowered, or
    /// conflicted (None) machine never warns: it loses nothing
    /// on the next tick, so warning for it would cry wolf every
    /// snapshot, and None is neither powered nor mismatched by
    /// the existing predicates. Rewiring clears a warning on the
    /// next snapshot; destroying a mill removes its warning
    /// because the mill is gone. This is a readout only: no
    /// delivery, precedence, conflict, rating, cost, or phase
    /// rule moves, and a warning postpones nothing — a warned
    /// mill still dies on the next tick if it stays wired.</summary>
    public int WarnedMills { get; set; }
    public int WarnedWorkstations { get; set; }
    public int WarnedPylons { get; set; }
    /// <summary>Mini-map: top-down solid / enemy presence, 32x32, for Slice 1 placeholder rendering.</summary>
    public bool[,] MiniMapSolid { get; set; } = new bool[32, 32];

    public static HudState FromWorld(WorldZone world, Weapon weapon)
    {
        var hud = new HudState
        {
            Health = world.Player.Health,
            Shields = world.Player.Shields,
            WeaponName = weapon.Name.ToUpperInvariant(),
            AmmoMag = weapon.Mag,
            AmmoReserve = weapon.Reserve,
            EnemyCount = world.Enemies.Count,
            ObjectivePercentCompleted = 100 - world.CoreHealth,
        };
        double dx = world.CoreX - world.Player.X;
        double dy = world.CoreY - world.Player.Y;
        double dz = world.CoreZ - world.Player.Z;
        hud.ObjectiveDistanceMetres = (int)Math.Round(Math.Sqrt(dx * dx + dy * dy + dz * dz));
        for (int x = 0; x < 32; x++)
        for (int z = 0; z < 32; z++)
        {
            bool solid = false;
            for (int y = 0; y < 32; y++) if (BlockInfo.IsSolid(world.Chunk.Get(x, y, z))) { solid = true; break; }
            hud.MiniMapSolid[x, z] = solid;
        }
        // Power indicator (Slice 6 Step 3): count mills, then ask
        // IsPowered per mill only — the block-type check first keeps
        // the 32^3 scan cheap, since mills are rare and IsPowered
        // checks six neighbours. Derived, like the activity word:
        // removing a Dynamo drops the count on the next snapshot.
        for (int x = 0; x < 32; x++)
        for (int y = 0; y < 32; y++)
        for (int z = 0; z < 32; z++)
        {
            var block = world.Chunk.Get(x, y, z);
            if (block == BlockType.KineticMill)
            {
                hud.TotalMills++;
                if (world.IsPowered(x, y, z)) hud.PoweredMills++;
                if (world.IsMismatched(x, y, z)) hud.WarnedMills++;
            }
            else if (block == BlockType.Workstation)
            {
                hud.TotalWorkstations++;
                if (world.IsWorkstationPowered(x, y, z)) hud.PoweredWorkstations++;
                if (world.IsWorkstationMismatched(x, y, z)) hud.WarnedWorkstations++;
            }
            else if (block == BlockType.ShockPylon)
            {
                hud.TotalPylons++;
                if (world.IsPylonPowered(x, y, z)) hud.PoweredPylons++;
                if (world.IsPylonMismatched(x, y, z)) hud.WarnedPylons++;
            }
        }
        // Live-power display (Slice 12 Step 1): one counting
        // walk over the whole zone, reusing the delivery walk —
        // no per-consumer counting here, so a network feeding a
        // mill is not counted twice (see LivePowerCounts).
        (hud.LiveNetworks, hud.LoneSources) = world.LivePowerCounts();
        return hud;
    }
}
