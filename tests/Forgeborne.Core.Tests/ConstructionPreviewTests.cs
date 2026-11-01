using Forgeborne.Core;

internal static class ConstructionPreviewTests
{
    public static void Run(Action<bool, string> check)
    {
        var s = new Simulation(42);
        CourtyardEncounter.Populate(s.World);
        s.Fps.Teleport(16.5f, 4, 16.5f);
        s.AdvanceFrame(0, 0, 0, false);
        float pitch = (float)(Math.Atan2(-2.1, 2) * 180 / Math.PI);
        s.ApplyLook(90, pitch);
        string before = s.SaveToJson();
        var preview = s.InspectBuildAhead(BlockType.Wall);
        check(preview.HasCell && preview.Result == BuildResult.InsufficientResources
            && preview.Cost.Stone == 2 && preview.Y == 4,
            "build preview identifies the aimed supported cell and real wall cost before funding");
        check(s.SaveToJson() == before, "build preview changes no resources, rules, clocks or saved state");
        s.World.Player.Inventory.Add(BlockType.Stone, 20);
        s.World.Player.Inventory.Add(BlockType.EmberOre, 20);
        s.World.Player.Inventory.Add(BlockType.Wood, 20);
        before = s.SaveToJson();
        preview = s.InspectBuildAhead(BlockType.Workstation);
        check(preview.Result == BuildResult.Success && preview.Cost == new BuildCost(10, 5, 0)
            && s.SaveToJson() == before, "workstation preview is read-only and uses its existing two-resource recipe");
        var enemy = new Enemy(preview.X, preview.Y, preview.Z);
        s.World.Enemies.Add(enemy);
        before = s.SaveToJson();
        check(s.BuildAhead(BlockType.Workstation) == BuildResult.Occupied && s.SaveToJson() == before,
            "placement revalidates a previously clear preview when a unit enters the cell");
        s.World.Enemies.Clear();
        preview = s.InspectBuildAhead(BlockType.Workstation);
        s.World.Player.Inventory.Remove(BlockType.EmberOre, 20);
        before = s.SaveToJson();
        check(preview.Result == BuildResult.Success && s.BuildAhead(BlockType.Workstation) == BuildResult.InsufficientResources
            && s.SaveToJson() == before, "placement does not trust stale preview affordability or partially charge stone");
        s.World.Player.Inventory.Add(BlockType.EmberOre, 5);
        preview = s.InspectBuildAhead(BlockType.Workstation);
        check(s.BuildAhead(BlockType.Workstation) == BuildResult.Success
            && s.World.Chunk.Get(preview.X, preview.Y, preview.Z) == BlockType.Workstation
            && s.World.Player.Inventory.Get(BlockType.Stone) == 10
            && s.World.Player.Inventory.Get(BlockType.EmberOre) == 0 && s.BlocksBuilt == 1,
            "generic construction places the exact previewed cell and spends the unchanged recipe once");
        s.ApplyLook(0, 89 - s.Look.Pitch);
        before = s.SaveToJson();
        preview = s.InspectBuildAhead(BlockType.Wall);
        check(!preview.HasCell && preview.Result == BuildResult.Floating && s.SaveToJson() == before,
            "skyward construction preview hides its cell and changes nothing");
        check(s.InspectBuildAhead(BlockType.Stone).Result == BuildResult.InvalidType
            && s.BuildAhead(BlockType.Stone) == BuildResult.InvalidType && s.SaveToJson() == before,
            "generic construction rejects unsupported block types without mutation");
        foreach (var type in Enum.GetValues<BlockType>())
        {
            if (!BuildCost.TryGet(type, out var cost)) continue;
            var world = new WorldZone(42);
            world.Player.Inventory.Add(BlockType.Stone, 999);
            world.Player.Inventory.Add(BlockType.EmberOre, 999);
            world.Player.Inventory.Add(BlockType.Wood, 999);
            world.Chunk.TrySet(10, 4, 10, BlockType.Air);
            world.Chunk.TrySet(10, 3, 10, BlockType.Stone);
            string snapshot = SaveSystem.SaveZone(world);
            check(world.InspectBuild(10, 4, 10, type) == BuildResult.Success && SaveSystem.SaveZone(world) == snapshot
                && world.Build(10, 4, 10, type) == BuildResult.Success
                && world.Player.Inventory.Get(BlockType.Stone) == 999 - cost.Stone
                && world.Player.Inventory.Get(BlockType.EmberOre) == 999 - cost.EmberOre
                && world.Player.Inventory.Get(BlockType.Wood) == 999 - cost.Wood,
                "build preview/payment agree without double spending for " + type);
        }
    }
}
