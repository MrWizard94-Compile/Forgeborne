namespace Forgeborne.Core;

public enum SkillNodeType { Travel, Notable, Keystone }

/// <summary>
/// One passive skill node. Effects are applied to gameplay by
/// Simulation.ApplySkills; see that method for the mapping.
/// Effect keys used in this cluster: DamagePct, ShieldBonus, MoveSpeedPct,
/// GatherPct, CoreDamagePct, WorkstationRegenBonus.
/// </summary>
public sealed class SkillNode
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public SkillNodeType Type { get; set; }
    public List<string> Prerequisites { get; set; } = new();
    public Dictionary<string, double> Effects { get; set; } = new();
}

public static class SkillGraph
{
    /// <summary>First cluster only: "Forge Survivor", 6 nodes. NOT the full 1,500-node graph.</summary>
    public static readonly List<SkillNode> ForgeSurvivorCluster = new()
    {
        new() { Id = "S1", Name = "Sturdy Hands", Type = SkillNodeType.Travel, Effects = { ["DamagePct"] = 5 } },
        new() { Id = "S2", Name = "Iron Skin", Type = SkillNodeType.Travel, Effects = { ["ShieldBonus"] = 10 } },
        new() { Id = "N1", Name = "Forge Heart", Type = SkillNodeType.Notable, Prerequisites = { "S1", "S2" }, Effects = { ["DamagePct"] = 15, ["ShieldBonus"] = 10 } },
        new() { Id = "S3", Name = "Swift Feet", Type = SkillNodeType.Travel, Effects = { ["MoveSpeedPct"] = 5 } },
        new() { Id = "N2", Name = "Scavenger", Type = SkillNodeType.Notable, Prerequisites = { "S3" }, Effects = { ["GatherPct"] = 20 } },
        new() { Id = "K1", Name = "Corebreaker", Type = SkillNodeType.Keystone, Prerequisites = { "N1", "N2" }, Effects = { ["CoreDamagePct"] = 25, ["WorkstationRegenBonus"] = 5 } },
    };

    public static SkillNode? Find(string id) => ForgeSurvivorCluster.FirstOrDefault(n => n.Id == id);
}

/// <summary>Allocated nodes for one player. Allocation is prereq-gated and idempotent-rejecting.</summary>
public sealed class SkillBuild
{
    private readonly HashSet<string> _allocated = new();
    public IReadOnlyCollection<string> Allocated => _allocated;

    public bool Allocate(string id)
    {
        var node = SkillGraph.Find(id);
        if (node == null) return false;
        if (_allocated.Contains(id)) return false;
        if (node.Prerequisites.Any(p => !_allocated.Contains(p))) return false;
        _allocated.Add(id);
        return true;
    }

    public double TotalEffect(string key)
    {
        double total = 0;
        foreach (var id in _allocated)
        {
            var node = SkillGraph.Find(id);
            if (node != null && node.Effects.TryGetValue(key, out var v)) total += v;
        }
        return total;
    }

    /// <summary>Simple encoding: sorted ids joined by comma, Base64. Small clusters stay tiny; the blueprint's <100 bytes for 120 nodes is a later encoding optimisation, not claimed here.</summary>
    public string Encode() => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Join(",", _allocated.OrderBy(x => x))));

    public static SkillBuild Decode(string encoded)
    {
        var build = new SkillBuild();
        if (string.IsNullOrEmpty(encoded)) return build;
        var text = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        // Allocate in graph order so prerequisites are met regardless of encoded order.
        foreach (var node in SkillGraph.ForgeSurvivorCluster)
            if (text.Split(',').Contains(node.Id)) build.Allocate(node.Id);
        return build;
    }
}
