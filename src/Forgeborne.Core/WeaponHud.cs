namespace Forgeborne.Core;

public sealed class Weapon
{
    public string Name { get; } = "Pulse Rifle";
    public int MagCapacity { get; } = 42;
    public int Mag { get; private set; } = 42;
    public int Reserve { get; private set; } = 120;
    public int Damage { get; } = 10;

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
    /// <summary>Mini-map: top-down solid / enemy presence, 32x32, for Slice 1 placeholder rendering.</summary>
    public bool[,] MiniMapSolid { get; set; } = new bool[32, 32];

    public static HudState FromWorld(WorldZone world, Weapon weapon)
    {
        var hud = new HudState
        {
            Health = world.Player.Health,
            Shields = world.Player.Shields,
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
        return hud;
    }
}
