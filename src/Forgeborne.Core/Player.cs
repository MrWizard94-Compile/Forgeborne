namespace Forgeborne.Core;

public sealed class Inventory
{
    public const int MaxStack = 999;
    private readonly Dictionary<BlockType, int> _items = new();

    public int Get(BlockType type) => _items.TryGetValue(type, out var n) ? n : 0;
    public IReadOnlyDictionary<BlockType, int> Items => _items;

    /// <summary>Adds up to MaxStack. Returns the overflow that did NOT fit.</summary>
    public int Add(BlockType type, int amount)
    {
        if (amount <= 0) return 0;
        var current = Get(type);
        var room = MaxStack - current;
        var added = Math.Min(room, amount);
        if (added > 0) _items[type] = current + added;
        return amount - added;
    }

    public bool Remove(BlockType type, int amount)
    {
        if (amount <= 0) return true;
        if (Get(type) < amount) return false;
        _items[type] = Get(type) - amount;
        if (_items[type] == 0) _items.Remove(type);
        return true;
    }

    public bool Has(BlockType type, int amount) => Get(type) >= amount;
}

public sealed class Player
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Health { get; private set; } = 100;
    public int Shields { get; private set; } = 85;
    public Inventory Inventory { get; } = new();
    public bool IsDead => Health <= 0;

    public Player(int x, int y, int z) { X = x; Y = y; Z = z; }

    /// <summary>Damage hits shields first, then health. Reference HUD: 100 HP, Shields 85.</summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        var absorbed = Math.Min(Shields, amount);
        Shields -= absorbed;
        Health = Math.Max(0, Health - (amount - absorbed));
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;
        Health = Math.Min(100, Health + amount);
    }

    /// <summary>Restore health/shields from a save file, clamped to valid ranges. Used only by SaveSystem.</summary>
    public void Restore(int health, int shields)
    {
        Health = Math.Clamp(health, 0, 100);
        Shields = Math.Clamp(shields, 0, 85);
    }

    public bool MoveTo(int x, int y, int z, VoxelChunk chunk)
    {
        if (!VoxelChunk.InBounds(x, y, z)) return false;
        if (BlockInfo.IsSolid(chunk.Get(x, y, z))) return false;
        X = x; Y = y; Z = z;
        return true;
    }
}
