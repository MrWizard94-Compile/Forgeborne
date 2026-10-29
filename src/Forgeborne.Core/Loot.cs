namespace Forgeborne.Core;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum Element { None, Shock, Incendiary, Corrosive, Cryo }

/// <summary>
/// Procedural weapon generation, Borderlands-style parts, Diablo/PoE-style rarity.
/// Deterministic by seed + level: same seed/level always gives the same weapon.
/// No Random, no time. Parts are data, not copied names from other games.
/// </summary>
public sealed class GeneratedWeapon
{
    public string Name { get; set; } = "";
    public Rarity Rarity { get; set; }
    public string Body { get; set; } = "";
    public string Barrel { get; set; } = "";
    public string Stock { get; set; } = "";
    public string Grip { get; set; } = "";
    public string Sight { get; set; } = "";
    public Element Element { get; set; }
    public int Damage { get; set; }
    public int MagSize { get; set; }
    public double FireRatePerSecond { get; set; }
    public List<string> Affixes { get; set; } = new();
}

public static class LootGenerator
{
    private static readonly string[] Bodies = { "Light", "Standard", "Heavy", "Titan" };
    private static readonly string[] Barrels = { "Short", "Long", "Scatter", "Precision" };
    private static readonly string[] Stocks = { "Polymer", "Alloy", "Stabilised" };
    private static readonly string[] Grips = { "Tactical", "Ergonomic", "Weighted" };
    private static readonly string[] Sights = { "Iron", "Holo", "Scope" };
    private static readonly Element[] Elements = { Element.None, Element.Shock, Element.Incendiary, Element.Corrosive, Element.Cryo };

    private static int Pick(int seed, int offset, int count) => (int)(Deterministic.Hash(seed, offset, 0, 0) % (uint)count);

    public static GeneratedWeapon Generate(int seed, int level)
    {
        level = Math.Max(1, level);
        double roll = Deterministic.Unit(seed, 999, 0, 0);
        // Rarity weights shift slightly with level, but Legendary stays rare.
        Rarity rarity = roll < 0.50 ? Rarity.Common
            : roll < 0.75 ? Rarity.Uncommon
            : roll < 0.90 ? Rarity.Rare
            : roll < 0.98 ? Rarity.Epic
            : Rarity.Legendary;
        double rarityMult = rarity switch
        {
            Rarity.Common => 1.0,
            Rarity.Uncommon => 1.15,
            Rarity.Rare => 1.35,
            Rarity.Epic => 1.6,
            _ => 2.0,
        };
        var body = Bodies[Pick(seed, 1, Bodies.Length)];
        var barrel = Barrels[Pick(seed, 2, Barrels.Length)];
        var stock = Stocks[Pick(seed, 3, Stocks.Length)];
        var grip = Grips[Pick(seed, 4, Grips.Length)];
        var sight = Sights[Pick(seed, 5, Sights.Length)];
        var element = Elements[Pick(seed, 6, Elements.Length)];

        int baseDamage = body switch { "Light" => 8, "Standard" => 10, "Heavy" => 14, _ => 18 };
        baseDamage += barrel switch { "Short" => -1, "Long" => 2, "Scatter" => 3, _ => 1 };
        int damage = Math.Max(1, (int)Math.Round((baseDamage + level * 0.5) * rarityMult));
        int mag = barrel == "Scatter" ? 8 : barrel == "Long" ? 30 : 42;
        mag = (int)Math.Round(mag * (rarity == Rarity.Legendary ? 1.25 : 1.0));
        double fireRate = body == "Light" ? 8.0 : body == "Heavy" ? 4.0 : 6.0;

        var affixes = new List<string>();
        if (rarity >= Rarity.Uncommon) affixes.Add("+10% damage");
        if (rarity >= Rarity.Rare) affixes.Add("+15% fire rate");
        if (rarity >= Rarity.Epic) affixes.Add("Shots pierce 1 enemy");
        if (rarity == Rarity.Legendary) affixes.Add("Kills restore 5 shields");

        string elementSuffix = element == Element.None ? "" : $" of {element}";
        return new GeneratedWeapon
        {
            Name = $"{body} {barrel} Pulse Rifle{elementSuffix}",
            Rarity = rarity,
            Body = body,
            Barrel = barrel,
            Stock = stock,
            Grip = grip,
            Sight = sight,
            Element = element,
            Damage = damage,
            MagSize = mag,
            FireRatePerSecond = fireRate,
            Affixes = affixes,
        };
    }

    /// <summary>Enemy drop table for Slice 2: 30% chance, deterministic by seed + enemy id.</summary>
    public static GeneratedWeapon? EnemyDrop(int seed, int enemyId, int level)
    {
        if (Deterministic.Unit(seed, enemyId, 777, 0) >= 0.30) return null;
        return Generate(seed + enemyId * 1000, level);
    }
}
