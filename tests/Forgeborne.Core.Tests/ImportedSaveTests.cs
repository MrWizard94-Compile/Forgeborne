using System.Text.Json.Nodes;
using Forgeborne.Core;

internal static class ImportedSaveTests
{
    public static void Run(Action<bool, string> check)
    {
        var current = new Simulation(42);
        current.AdvanceFrame(0.137f, 0.3f, 0.2f, true);
        string original = current.SaveToJson();
        var world = current.World;
        foreach (string collection in new[] { "Inventory", "Enemies", "Squads" })
        {
            var broken = JsonNode.Parse(original)!;
            broken[collection] = null;
            check(!current.LoadFromJson(broken.ToJsonString()) && current.SaveToJson() == original
                && ReferenceEquals(current.World, world), "import rejects null " + collection + " without changing the encounter");
        }
        var malformed = JsonNode.Parse(original)!;
        malformed["Enemies"]!.AsArray().Add((JsonNode?)null);
        malformed["Squads"]!.AsArray().Add((JsonNode?)null);
        check(current.LoadFromJson(malformed.ToJsonString()) && current.SaveToJson() == original,
            "import skips null enemy/squad entries under the existing malformed-entry contract");
        foreach (string field in new[] { "PlayerX", "PlayerY", "PlayerZ" })
        {
            var broken = JsonNode.Parse(original)!;
            broken.AsObject().Remove("Runtime");
            broken[field] = int.MinValue;
            check(!current.LoadFromJson(broken.ToJsonString()) && current.SaveToJson() == original,
                "import rejects unsafe legacy " + field + " before distance arithmetic");
        }
        var enemyPosition = JsonNode.Parse(original)!;
        enemyPosition["Enemies"]!.AsArray().Add(new JsonArray(int.MaxValue, 5, 16, 30));
        check(!current.LoadFromJson(enemyPosition.ToJsonString()) && current.SaveToJson() == original,
            "import rejects unsafe enemy coordinates without changing state");
        var longName = JsonNode.Parse(original)!;
        longName["WeaponName"] = new string('x', 129);
        check(!current.LoadFromJson(longName.ToJsonString()) && current.SaveToJson() == original,
            "import rejects oversized weapon display content without changing state");
        var health = JsonNode.Parse(original)!;
        health["Enemies"]!.AsArray().Add(new JsonArray(16, 5, 19, int.MinValue));
        check(current.LoadFromJson(health.ToJsonString()) && current.World.Enemies[^1].IsDead,
            "import clamps extreme negative enemy health without arithmetic overflow");
    }
}
