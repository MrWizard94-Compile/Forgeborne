using Forgeborne.Core;

internal static class ConstructionSafetyTests
{
    public static void Run(Action<bool, string> check)
    {
        var simulation = new Simulation(42);
        CourtyardEncounter.Populate(simulation.World);
        simulation.AdvanceFrame(0, 0, 0, false);
        simulation.World.Player.Inventory.Add(BlockType.Stone, 20);
        simulation.ApplyLook(0, -89);
        var aim = simulation.InspectAim(false);
        check(aim.Kind == AimTargetKind.Solid && aim.PrevY == 4
            && aim.PrevX == 16 && aim.PrevZ == 16,
            "construction safety fixture aims at the suit's own floor cell");
        string before = simulation.SaveToJson();
        check(simulation.BuildWallAhead() == BuildResult.Occupied,
            "construction refuses a wall overlapping the suit's feet");
        check(simulation.SaveToJson() == before,
            "construction body refusal spends nothing and changes no canonical state");

        void RefusesActorCell(WorldZone world, int x, int y, int z, string actor)
        {
            world.Player.Inventory.Add(BlockType.Stone, 20);
            world.Chunk.TrySet(x, y, z, BlockType.Air);
            world.Chunk.TrySet(x, y - 1, z, BlockType.Stone);
            string snapshot = SaveSystem.SaveZone(world);
            check(world.Build(x, y, z, BlockType.Wall) == BuildResult.Occupied,
                "construction refuses occupied " + actor + " cell");
            check(SaveSystem.SaveZone(world) == snapshot,
                "construction " + actor + " refusal preserves exact world/resources");
        }

        var enemyWorld = new WorldZone(42);
        enemyWorld.Enemies.Add(new Enemy(10, 4, 10));
        RefusesActorCell(enemyWorld, 10, 4, 10, "warden");
        var squadWorld = new WorldZone(42);
        squadWorld.Squads.Add(new TacticalSquad(10, 4, 10, 12, 4, 12));
        RefusesActorCell(squadWorld, 10, 4, 10, "squad");
        var coreWorld = new WorldZone(42);
        RefusesActorCell(coreWorld, coreWorld.CoreX, coreWorld.CoreY, coreWorld.CoreZ, "live relay");
        var playerWorld = new WorldZone(42);
        RefusesActorCell(playerWorld, playerWorld.Player.X, playerWorld.Player.Y, playerWorld.Player.Z, "player");

        simulation.Fps.Teleport(16.9f, 4, 16.5f);
        simulation.AdvanceFrame(0, 0, 0, false);
        simulation.World.Chunk.TrySet(18, 5, 16, BlockType.Wall);
        simulation.ApplyLook(90 - simulation.Look.Yaw, -simulation.Look.Pitch);
        aim = simulation.InspectAim(false);
        check(aim.PrevX == 17 && aim.PrevY == 5 && aim.PrevZ == 16,
            "construction fractional-head fixture targets the cell beyond the suit's centre");
        before = simulation.SaveToJson();
        check(simulation.BuildWallAhead() == BuildResult.Occupied && simulation.SaveToJson() == before,
            "construction rejects fractional head/shoulder overlap without spending");
        check(simulation.Fps.OverlapsCell(17, 5, 16) && !simulation.Fps.OverlapsCell(16, 3, 16)
            && !simulation.Fps.OverlapsCell(16, 6, 16) && !simulation.Fps.OverlapsCell(16, 4, 17),
            "construction body footprint includes shoulders/head but excludes floor, ceiling and clear neighbour");

        enemyWorld.Enemies[0].TakeDamage(30);
        check(enemyWorld.Build(10, 4, 10, BlockType.Wall) == BuildResult.Success
            && enemyWorld.Player.Inventory.Get(BlockType.Stone) == 18,
            "construction permits a defeated warden cell with the unchanged two-stone wall cost");
        coreWorld.DamageCore(100);
        check(coreWorld.Build(coreWorld.CoreX, coreWorld.CoreY, coreWorld.CoreZ, BlockType.Wall) == BuildResult.Success,
            "construction does not reserve a disabled relay forever");
    }
}
