namespace Forgeborne.Core;

/// <summary>Original first-encounter layout, applied only by the Unity scene owner.
/// All gatherable resources, cover and platforms are authored into the canonical chunk.</summary>
public static class CourtyardEncounter
{
    public static void Populate(WorldZone world)
    {
        var chunk = world.Chunk;
        void Block(int x, int y, int z, BlockType type) => chunk.TrySet(x, y, z, type);
        // Clear the arrival space; no play begins inside a random surface tree.
        for (int x = 14; x <= 18; x++)
        for (int z = 13; z <= 30; z++)
        for (int y = 4; y <= 6; y++) Block(x, y, z, BlockType.Air);

        // Two readable salvage banks beside, rather than across, the first sightline.
        foreach (int bankX in new[] { 10, 20 })
        {
            for (int x = bankX; x < bankX + 3; x++)
            for (int z = 19; z <= 21; z++)
            for (int y = 4; y <= 5; y++) Block(x, y, z, BlockType.Stone);
            for (int z = 19; z <= 21; z++) Block(bankX + 1, 5, z, BlockType.EmberOre);
            Block(bankX, 5, 20, BlockType.EmberOre);
            Block(bankX + 2, 5, 20, BlockType.EmberOre);
        }
        for (int x = 6; x <= 8; x++)
        for (int z = 15; z <= 16; z++) Block(x, 4, z, BlockType.Wood);

        // Broken industrial walls make cover and approach lanes readable at both perspectives.
        for (int z = 8; z <= 26; z++)
        for (int y = 4; y <= (z % 5 == 0 ? 7 : 6); y++)
        {
            if (z < 13 || z > 17) Block(3, y, z, BlockType.Wall);
            if (z < 18 || z > 22) Block(28, y, z, BlockType.Wall);
        }
        for (int x = 5; x <= 11; x++)
        for (int y = 4; y <= 5; y++) Block(x, y, 25, BlockType.Wall);
        for (int x = 20; x <= 25; x++)
        for (int y = 4; y <= 5; y++) Block(x, y, 11, BlockType.Wall);

        // The rear relay platform has a clear south entrance and a raised machinery base.
        for (int x = 26; x <= 30; x++)
        for (int z = 26; z <= 30; z++)
            if (x >= 27 && x <= 29 && z >= 27 && z <= 29) Block(x, 4, z, BlockType.Wall);
        foreach (int x in new[] { 26, 30 })
        foreach (int z in new[] { 26, 30 })
        for (int y = 4; y <= 9; y++) Block(x, y, z, BlockType.Wall);
        for (int x = 26; x <= 30; x++) Block(x, 9, 30, BlockType.Wall);
        // The logical objective cell must always remain open to the canonical aim trace.
        Block(world.CoreX, world.CoreY, world.CoreZ, BlockType.Air);
    }

    public static void PlaceInitialWave(WorldZone world)
    {
        // Wardens approach through the player's opening view, not invisibly from behind.
        for (int i = 0; i < world.Enemies.Count; i++)
        {
            var enemy = world.Enemies[i];
            enemy.X = 6 + i % 3 * 10;
            enemy.Y = 5;
            enemy.Z = 28 - i / 3;
        }
    }
}
