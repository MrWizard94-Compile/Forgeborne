using Forgeborne.Core;

internal static class ActionFeedbackTests
{
    public static void Run(Action<bool, string> check)
    {
        var simulation = new Simulation(42);
        simulation.AdvanceFrame(0, 0, 0, false);
        CourtyardEncounter.Populate(simulation.World);
        simulation.World.Enemies.Clear();
        var enemy = new Enemy(16, 5, 19);
        simulation.World.Enemies.Add(enemy);
        string beforeInspection = simulation.SaveToJson();
        var preview = simulation.InspectAim();
        check(preview.Kind == AimTargetKind.Enemy && ReferenceEquals(preview.Enemy, enemy),
            "feedback preview identifies the actual canonical actor");
        check(simulation.SaveToJson() == beforeInspection,
            "feedback preview consumes no ammo, progression, motion or time");
        int tick = simulation.World.Tick;
        check(simulation.FireAimed(out var hit) && hit.Fired && hit.Target == AimTargetKind.Enemy
            && hit.X == 16 && hit.Y == 5 && hit.Z == 19 && hit.DamageApplied == 10 && !hit.Destroyed,
            "feedback shot reports actual actor cell and applied damage");
        check(enemy.Health == 20 && simulation.Weapon.Mag == 41 && simulation.World.Tick == tick,
            "feedback overload preserves the existing ammo/damage/clock rule");

        simulation.World.Chunk.TrySet(16, 5, 18, BlockType.Wall);
        check(simulation.InspectAim().Kind == AimTargetKind.Solid
            && simulation.FireAimed(out var blocked) && blocked.Target == AimTargetKind.Solid
            && blocked.DamageApplied == 0 && !blocked.Destroyed && enemy.Health == 20,
            "feedback cannot report damage through an occluding voxel");
        simulation.World.Chunk.TrySet(16, 5, 18, BlockType.Air);
        enemy.TakeDamage(15);
        check(simulation.FireAimed(out var kill) && kill.DamageApplied == 5 && kill.Destroyed
            && simulation.EnemiesKilled == 1 && !simulation.World.Enemies.Contains(enemy),
            "feedback overkill reports clamped health loss and canonical removal");

        simulation.ApplyLook(0, 89);
        int ammo = simulation.Weapon.Mag;
        check(simulation.FireAimed(out var miss) && miss.Target == AimTargetKind.None
            && miss.DamageApplied == 0 && !miss.Destroyed && simulation.Weapon.Mag == ammo - 1,
            "feedback miss still costs its real round without claiming a hit");
        while (simulation.Weapon.Mag > 0) simulation.FireAimed();
        string empty = simulation.SaveToJson();
        check(!simulation.FireAimed(out var dry) && !dry.Fired && dry.DamageApplied == 0
            && simulation.SaveToJson() == empty, "feedback dry fire leaves canonical state unchanged");

        simulation.Weapon.Reload();
        // Stand on the relay platform; the former test position was inside the
        // eastern cover wall and correctly produced an occluded solid hit.
        simulation.Fps.Teleport(28.5f, 5, 27.5f);
        simulation.AdvanceFrame(0, 0, 0, false);
        float dx = 28.5f - simulation.Fps.X, dz = 28.5f - simulation.Fps.Z;
        float dy = 5.5f - simulation.Fps.Y - Simulation.EyeHeight;
        float yaw = (float)(Math.Atan2(dx, dz) * 180 / Math.PI);
        float pitch = (float)(Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)) * 180 / Math.PI);
        simulation.ApplyLook(yaw - simulation.Look.Yaw, pitch - simulation.Look.Pitch);
        check(simulation.InspectAim().Kind == AimTargetKind.Core, "feedback relay fixture has a clear canonical sightline");
        simulation.World.DamageCore(97);
        check(simulation.FireAimed(out var relay) && relay.Target == AimTargetKind.Core
            && relay.DamageApplied == 3 && relay.Destroyed && simulation.IsObjectiveComplete,
            "feedback relay damage reports actual clamped destruction");
    }
}
