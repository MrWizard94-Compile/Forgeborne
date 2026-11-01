using Forgeborne.Core;

internal static class FrameCadenceTests
{
    public static void Run(Action<bool, string> check)
    {
        Simulation RunAt(int fps, bool combat, bool walking)
        {
            var sim = new Simulation(42);
            for (int x = 15; x <= 17; x++)
            for (int z = 15; z <= 30; z++)
            for (int y = 4; y <= 7; y++) sim.World.Chunk.TrySet(x, y, z, BlockType.Air);
            if (combat) sim.World.Enemies.Add(new Enemy(16, 4, 17));
            for (int frame = 0; frame < fps * 2; frame++)
                sim.AdvanceFrame(1f / fps, walking ? 1 : 0, 0, false);
            return sim;
        }

        var walking30 = RunAt(30, false, true);
        foreach (int fps in new[] { 30, 60, 144, 240 })
        {
            var walking = RunAt(fps, false, true);
            check(walking.World.Tick == 4, $"cadence {fps} FPS: two seconds is four world ticks");
            check(Math.Abs(walking.Fps.Z - walking30.Fps.Z) < 0.0001f,
                $"cadence {fps} FPS: equal-time movement agrees with 30 FPS");
            check(Math.Abs(walking.Fps.Z - 28) < 0.01f,
                $"cadence {fps} FPS: two seconds at six metres/second moves twelve metres");
            var combat = RunAt(fps, true, false);
            check(combat.World.Tick == 4 && combat.World.Player.Shields == 45 && combat.World.Player.Health == 100,
                $"cadence {fps} FPS: adjacent enemy deals four attacks, not one per frame");
            var factory = new Simulation(42);
            factory.World.Player.Inventory.Add(BlockType.Stone, 100);
            factory.World.Player.Inventory.Add(BlockType.Wood, 100);
            factory.World.Player.Inventory.Add(BlockType.EmberOre, 100);
            factory.World.Chunk.TrySet(10, 3, 10, BlockType.Air);
            if (factory.World.Build(10, 3, 10, BlockType.KineticMill) != BuildResult.Success)
                throw new InvalidOperationException("Cadence automation fixture could not build its mill.");
            for (int frame = 0; frame < fps * 5; frame++) factory.AdvanceFrame(1f / fps, 0, 0, false);
            check(factory.World.Tick == 10 && factory.World.KineticBlocksGathered == 2,
                $"cadence {fps} FPS: five seconds produces two mill activations");
        }

        var hitch = new Simulation(42);
        hitch.AdvanceFrame(10, 0, 0, false);
        check(hitch.World.Tick == 0, "cadence long hitch is bounded to 250 ms");
        hitch.AdvanceFrame(0.25f, 0, 0, false);
        check(hitch.World.Tick == 1, "cadence capped hitch retains the next half-second boundary");
        foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity })
        {
            bool rejected = false;
            try { hitch.AdvanceFrame(invalid, 0, 0, false); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            check(rejected, "cadence rejects invalid elapsed time before mutating the world");
        }

        var jump = new Simulation(42);
        jump.AdvanceFrame(0.1f, 0, 0, false);
        check(jump.Fps.IsGrounded, "cadence jump fixture is grounded");
        jump.AdvanceFrame(1f / 240f, 0, 0, true);
        jump.AdvanceFrame(1f / 240f, 0, 0, false);
        check(jump.Fps.Y > 4 && jump.Fps.VelocityY > 0, "cadence buffers a jump edge across sub-step render frames");

        var loaded = new Simulation(42);
        string oldSave = loaded.SaveToJson();
        loaded.AdvanceFrame(0.25f, 0, 0, false);
        check(loaded.LoadFromJson(oldSave), "cadence existing saves still load into the same simulation");
        loaded.AdvanceFrame(0.25f, 0, 0, false);
        check(loaded.World.Tick == 0, "cadence load does not inherit the previous world's fractional clock");
        loaded.AdvanceFrame(0.25f, 0, 0, false);
        check(loaded.World.Tick == 1, "cadence restored world advances on its own clock boundary");

        var dead = new Simulation(42);
        dead.World.Player.TakeDamage(1000);
        dead.AdvanceFrame(0.25f, 1, 0, true);
        check(dead.World.Tick == 0 && dead.Fps.Z == 16, "cadence death stops movement and progression");
        var won = new Simulation(42);
        won.World.DamageCore(100);
        won.AdvanceFrame(0.25f, 1, 0, false);
        check(won.World.Tick == 0 && won.Fps.Z == 16, "cadence victory stops encounter progression");

        var explicitTick = new Simulation(42);
        explicitTick.Tick(0.01f, 0, 0, false);
        check(explicitTick.World.Tick == 1, "cadence explicit call-based Tick contract remains intact");
    }
}
