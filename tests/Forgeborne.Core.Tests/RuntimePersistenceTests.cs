using System.Text.Json.Nodes;
using Forgeborne.Core;

internal static class RuntimePersistenceTests
{
    public static void Run(Action<bool, string> check)
    {
        var skills = new SkillBuild();
        foreach (var node in SkillGraph.ForgeSurvivorCluster) skills.Allocate(node.Id);
        var source = new Simulation(42, skills: skills);
        CourtyardEncounter.Populate(source.World);
        source.AdvanceFrame(0.137f, 0.7f, -0.2f, false);

        void AimAt(float x, float y, float z)
        {
            float dx = x - source.Fps.X, dy = y - source.Fps.Y - Simulation.EyeHeight, dz = z - source.Fps.Z;
            float yaw = (float)(Math.Atan2(dx, dz) * 180 / Math.PI);
            float pitch = (float)(Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)) * 180 / Math.PI);
            source.ApplyLook(yaw - source.Look.Yaw, pitch - source.Look.Pitch);
        }

        AimAt(12.5f, 4.5f, 19.5f);
        check(source.GatherAhead() == GatherResult.Success, "runtime save fixture gathers a real courtyard resource by aim");
        source.World.Player.Inventory.Add(BlockType.Stone, 10);
        check(source.BuildWallAhead() == BuildResult.Success, "runtime save fixture builds a real supported wall by aim");
        var enemy = new Enemy(16, 5, 18);
        source.World.Enemies.Add(enemy);
        AimAt(16.5f, 5.5f, 18.5f);
        for (int shot = 0; shot < 4 && !enemy.IsDead; shot++) source.FireAimed();
        check(enemy.IsDead && source.EnemiesKilled == 1, "runtime save fixture earns a kill through the production fire path");
        source.ApplyLook(3.375f, 1.125f);
        // Less than one movement step: the jump edge and fractional clock must survive the save.
        source.AdvanceFrame(1f / 240f, 0.7f, -0.2f, true);
        string snapshot = source.SaveToJson();
        var nodeSnapshot = JsonNode.Parse(snapshot)!;
        bool rejectedWriter = false;
        try { SaveSystem.SaveZone(source.World, source.AppliedSkills, source.Weapon, new RuntimeSaveState { Version = 99 }); }
        catch (ArgumentException) { rejectedWriter = true; }
        check(rejectedWriter, "runtime save writer refuses a snapshot its consumer cannot restore");
        check(nodeSnapshot["Runtime"] != null && nodeSnapshot["Runtime"]!["Version"]!.GetValue<int>() == 1,
            "runtime save writes a versioned interactive snapshot");
        check(nodeSnapshot["Runtime"]!["GatherBonusCarry"]!.GetValue<double>() > 0,
            "runtime save captures earned fractional gathering progress");

        var restored = new Simulation(9);
        var sameFps = restored.Fps;
        var sameLook = restored.Look;
        check(restored.LoadFromJson(snapshot), "runtime save loads a complete interactive snapshot");
        check(ReferenceEquals(sameFps, restored.Fps) && ReferenceEquals(sameLook, restored.Look),
            "runtime save retains the existing controller/look objects");
        check(restored.Fps.X == source.Fps.X && restored.Fps.Y == source.Fps.Y && restored.Fps.Z == source.Fps.Z,
            "runtime save preserves exact fractional FPS position");
        check(restored.Fps.VelocityY == source.Fps.VelocityY && restored.Fps.IsGrounded == source.Fps.IsGrounded,
            "runtime save preserves vertical motion and grounding");
        check(restored.Look.Yaw == source.Look.Yaw && restored.Look.Pitch == source.Look.Pitch,
            "runtime save preserves exact camera look");
        check(restored.BlocksGathered == 1 && restored.BlocksBuilt == 1 && restored.EnemiesKilled == 1,
            "runtime save preserves interactive run counters");
        check(restored.SaveToJson() == snapshot, "runtime save round-trips all canonical and runtime fields exactly");
        for (int frame = 0; frame < 240; frame++)
        {
            source.AdvanceFrame(1f / 144f, 0.7f, -0.2f, false);
            restored.AdvanceFrame(1f / 144f, 0.7f, -0.2f, false);
        }
        check(source.SaveToJson() == restored.SaveToJson(),
            "runtime save continuation matches uninterrupted physics, queued jump and world timing");

        string beforeBad = restored.SaveToJson();
        var worldIdentity = restored.World;
        foreach (string field in new[] { "Version", "X", "FrameRemainder", "QueuedJump" })
        {
            var broken = JsonNode.Parse(snapshot)!;
            broken["Runtime"]!.AsObject().Remove(field);
            check(!restored.LoadFromJson(broken.ToJsonString()) && restored.SaveToJson() == beforeBad
                && ReferenceEquals(worldIdentity, restored.World), $"runtime save rejects missing {field} without a partial world swap");
        }
        foreach (var mutation in new[] { ("Version", 99), ("MovementPhase", 60), ("BlocksBuilt", -1), ("Pitch", 90), ("X", 100) })
        {
            var broken = JsonNode.Parse(snapshot)!;
            broken["Runtime"]![mutation.Item1] = mutation.Item2;
            check(!restored.LoadFromJson(broken.ToJsonString()) && restored.SaveToJson() == beforeBad,
                $"runtime save rejects invalid {mutation.Item1} without changing the encounter");
        }
        var remainder = JsonNode.Parse(snapshot)!;
        remainder["Runtime"]!["FrameRemainder"] = 1;
        check(!restored.LoadFromJson(remainder.ToJsonString()) && restored.SaveToJson() == beforeBad,
            "runtime save rejects an out-of-range fractional clock");

        string legacy = SaveSystem.SaveZone(source.World, source.AppliedSkills, source.Weapon);
        restored.ApplyLook(20, 2);
        float legacyYaw = restored.Look.Yaw;
        check(restored.LoadFromJson(legacy), "runtime save retains legacy world/full save loading");
        check(restored.Fps.X == restored.World.Player.X && restored.Fps.Y == restored.World.Player.Y
            && restored.Fps.Z == restored.World.Player.Z && restored.Look.Yaw == legacyYaw,
            "runtime legacy load keeps its documented grid-position/current-look contract");
        check(restored.BlocksGathered == 0 && restored.BlocksBuilt == 0 && restored.EnemiesKilled == 0,
            "runtime legacy load retains its documented session-counter reset");
    }
}
