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
    /// and how many of them are powered (face-adjacent to a Dynamo,
    /// per WorldZone.IsPowered). A Dynamo on its own counts as
    /// neither, and a wall or workstation next to a Dynamo counts
    /// as neither — only mills are consumers in this loop.</summary>
    public int PoweredMills { get; set; }
    public int TotalMills { get; set; }
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
            if (world.Chunk.Get(x, y, z) != BlockType.KineticMill) continue;
            hud.TotalMills++;
            if (world.IsPowered(x, y, z)) hud.PoweredMills++;
        }
        return hud;
    }
}
