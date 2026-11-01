using Forgeborne.Core;

internal static class CourtyardEncounterTests
{
    public static void Run(Action<bool, string> check)
    {
        var world = new WorldZone(42);
        CourtyardEncounter.Populate(world);
        var identical = new WorldZone(42);
        CourtyardEncounter.Populate(identical);
        check(world.Chunk.ToBytes().SequenceEqual(identical.Chunk.ToBytes()), "courtyard layout is deterministic");
        byte[] first = world.Chunk.ToBytes();
        CourtyardEncounter.Populate(world);
        check(first.SequenceEqual(world.Chunk.ToBytes()), "courtyard authored layout is idempotent");
        check(world.Chunk.Get(16, 4, 16) == BlockType.Air && world.Chunk.Get(16, 5, 16) == BlockType.Air,
            "courtyard arrival volume is clear");
        check(world.Chunk.Get(16, 3, 16) != BlockType.Air, "courtyard player starts on canonical ground");
        check(world.Chunk.Get(world.CoreX, world.CoreY, world.CoreZ) == BlockType.Air,
            "courtyard objective cell stays available to the aim trace");
        int stone = 0, ore = 0;
        for (int x = 10; x <= 12; x++)
        for (int y = 4; y <= 5; y++)
        for (int z = 19; z <= 21; z++)
        {
            var type = world.Chunk.Get(x, y, z);
            if (type == BlockType.Stone) stone++;
            if (type == BlockType.EmberOre) ore++;
        }
        check(stone >= 12 && ore >= 5, "courtyard first bank funds a wall and workstation with genuine gatherable resources");
        world.SpawnWave(3);
        CourtyardEncounter.PlaceInitialWave(world);
        check(world.Enemies.Count == 3 && world.Enemies.All(e => e.Z > world.Player.Z),
            "courtyard first wave enters the player's opening forward view");
        check(world.Enemies.All(e => VoxelChunk.InBounds(e.X, e.Y, e.Z) && world.Chunk.Get(e.X, e.Y, e.Z) == BlockType.Air),
            "courtyard wardens spawn in open canonical cells");
        string save = SaveSystem.SaveZone(world);
        var loaded = SaveSystem.LoadZone(save);
        check(world.Chunk.ToBytes().SequenceEqual(loaded.Chunk.ToBytes()) && loaded.Enemies.Count == 3,
            "courtyard authored cells and wave survive existing world saves");

        var mesh = MeshBuilder.BuildFromChunk(world.Chunk, world.PowerState);
        check(mesh.BlockTypes.Count == mesh.Vertices.Count && mesh.Colors.Count == mesh.Vertices.Count,
            "courtyard mesh provides one block identity and power tint for every vertex");
        foreach (var type in new[] { BlockType.Stone, BlockType.EmberOre, BlockType.Wood, BlockType.Wall })
            check(mesh.BlockTypes.Contains(type), $"courtyard mesh preserves {type} identity for the real material consumer");
    }
}
