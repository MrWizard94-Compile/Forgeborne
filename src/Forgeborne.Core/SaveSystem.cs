using System.Text.Json;

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
}

public static class SaveSystem
{
    public static string SaveZone(WorldZone world)
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
        };
        return JsonSerializer.Serialize(data);
    }

    public static WorldZone LoadZone(string json)
    {
        var data = JsonSerializer.Deserialize<SaveData>(json)
            ?? throw new InvalidOperationException("Save data is null.");
        var world = new WorldZone(data.Seed);
        world.Chunk.FromBytes(Convert.FromBase64String(data.ChunkBase64));
        world.Player.X = data.PlayerX;
        world.Player.Y = data.PlayerY;
        world.Player.Z = data.PlayerZ;
        // Restore health/shields through the damage/heal path in reverse:
        // start at 100/85, then apply the difference as damage/heal.
        // This keeps Player's invariants (0..100, 0..85, dead at 0) intact.
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
        return world;
    }
}
