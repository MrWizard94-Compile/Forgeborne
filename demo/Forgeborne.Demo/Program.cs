using System.Diagnostics;
using Forgeborne.Core;

// Aim the crosshair at the first live enemy's centre, as a
// player would: Slice 8 Step 2 fire only hits what the Look
// ray reaches, so a demo that fired blind would prove nothing
// about aimed fire. The core rule stays aim-only; this helper
// just points the demo's crosshair.
void AimAtFirstEnemy(Simulation sim)
{
    if (sim.World.Enemies.Count == 0) return;
    var e = sim.World.Enemies[0];
    float dx = e.X + 0.5f - sim.Fps.X, dy = e.Y + 0.5f - (sim.Fps.Y + Simulation.EyeHeight), dz = e.Z + 0.5f - sim.Fps.Z;
    float yaw = (float)(Math.Atan2(dx, dz) * 180 / Math.PI);
    float pitch = (float)(Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)) * 180 / Math.PI);
    sim.ApplyLook(yaw - sim.Look.Yaw, pitch - sim.Look.Pitch);
}

// Headless demo: runs the full Slice 1 loop without Unity.
// Usage: dotnet run --project demo/Forgeborne.Demo
var sim = new Simulation(42);
sim.World.SpawnWave(3);
Console.WriteLine("Forgeborne Slice 1 headless demo — gather, build, fight, objective");
for (int tick = 0; tick < 60; tick++)
{
    sim.Tick(0.1f, tick < 20 ? 1f : 0f, 0f, false);
    if (tick == 5) { sim.ApplyLook(0f, -45f); Console.WriteLine($"Tick {tick}: gather -> {sim.GatherAhead()} (gathered {sim.BlocksGathered})"); }
    if (tick == 10) { sim.World.Player.Inventory.Add(BlockType.Stone, 10); Console.WriteLine($"Tick {tick}: build -> {sim.BuildWallAhead()} (built {sim.BlocksBuilt})"); }
    if (tick >= 20 && tick % 5 == 0) { AimAtFirstEnemy(sim); sim.FireAimed(); }
    if (tick % 20 == 0)
    {
        var hud = sim.Hud();
        Console.WriteLine($"Tick {tick}: HP {hud.Health} Shields {hud.Shields} Ammo {hud.AmmoMag}/{hud.AmmoReserve} Enemies {hud.EnemyCount} Objective {hud.ObjectivePercentCompleted}% Dist {hud.ObjectiveDistanceMetres}m");
    }
}
var sw = Stopwatch.StartNew();
var mesh = MeshBuilder.BuildFromChunk(sim.World.Chunk);
sw.Stop();
Console.WriteLine($"Mesher: {mesh.QuadCount} quads, {mesh.Vertices.Count} vertices in {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Done: gathered {sim.BlocksGathered}, built {sim.BlocksBuilt}, killed {sim.EnemiesKilled}, player dead: {sim.IsPlayerDead}, objective complete: {sim.IsObjectiveComplete}");
Console.WriteLine($"Save JSON length: {SaveSystem.SaveZone(sim.World).Length} chars");

// Slice 2 scenario: the same Simulation composed with every Slice 2
// system at once — generated loot equipped, full Forge Survivor
// skill cluster applied, a Kinetic Mill built, an Orbital squad
// deployed — so the demo proves composition, not just that each
// system works in isolation. Numbers below are the evidence.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 2 scenario — loot, skills, Kinetic mill, Orbital squad, save with squads");
var loot = LootGenerator.Generate(0, 1);
var skillBuild = new SkillBuild();
skillBuild.Allocate("S1"); skillBuild.Allocate("S2"); skillBuild.Allocate("N1");
skillBuild.Allocate("S3"); skillBuild.Allocate("N2"); skillBuild.Allocate("K1");
var sim2 = new Simulation(42, loot, skillBuild);
Console.WriteLine($"Loot: {loot.Name} ({loot.Rarity}, {loot.Damage} dmg, {loot.MagSize} mag)");
Console.WriteLine($"Skills: full Forge Survivor cluster — effective damage {sim2.EffectiveDamage}, core damage {sim2.EffectiveCoreDamage}, shields cap {sim2.World.Player.MaxShields}, speed {sim2.Fps.EffectiveSpeed} m/s");
sim2.World.SpawnWave(3);
// Slice 3 Step 2: the demo squad deploys through the paid path
// (10 EmberOre), like a picker Confirm — the free direct path is
// headless-only and no longer represents a deploy in play.
sim2.World.Player.Inventory.Add(BlockType.EmberOre, 10);
var squad = sim2.DeployOrbitalSquadPaid(sim2.World.Player.X, sim2.World.Player.Y, sim2.World.Player.Z);
Console.WriteLine($"Orbital squad deployed: {(squad == null ? "REJECTED" : "Vanguard HoldGround at player position")}");
sim2.World.Player.Inventory.Add(BlockType.Stone, 10);
sim2.World.Player.Inventory.Add(BlockType.Wood, 5);
sim2.ApplyLook(-18.4f, -53f);
Console.WriteLine($"Kinetic mill build -> {sim2.BuildKineticMillAhead()} (built {sim2.BlocksBuilt})");
for (int tick = 0; tick < 20; tick++)
{
    sim2.Tick(0.1f, 0f, 0f, false);
    if (tick % 5 == 0) { AimAtFirstEnemy(sim2); sim2.FireAimed(); }
    if (tick % 5 == 4)
    {
        var snap = OrbitalView.Snapshot(sim2.World);
        Console.WriteLine($"Slice 2 tick {tick}: enemies {sim2.World.Enemies.Count}, squad members {(snap.Count == 0 ? 0 : snap[0].AliveMembers)}, squad HP {(snap.Count == 0 ? 0 : snap[0].Health)}, kinetic gathered {sim2.World.KineticBlocksGathered}");
    }
}
var save2 = SaveSystem.SaveZone(sim2.World);
var loaded2 = SaveSystem.LoadZone(save2);
Console.WriteLine($"Slice 2 done: gathered {sim2.BlocksGathered}, built {sim2.BlocksBuilt}, killed {sim2.EnemiesKilled}, kinetic gathered {sim2.World.KineticBlocksGathered}, player dead: {sim2.IsPlayerDead}, squads saved+loaded: {loaded2.Squads.Count}, save JSON length: {save2.Length} chars");

// Slice 3 Step 1 scenario: save-format completion. A skilled,
// loot-equipped Simulation damages its core, then saves with the
// full format and loads into a fresh Simulation — proving the
// core health, the skill build, the shields above the base cap,
// and the equipped weapon (with its in-progress mag) all survive.
// The FPS controller is teleported near the core as the headless
// stand-in for walking there; FireAimed at the core then does real damage.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 3 Step 1 scenario — core health, skill build, and weapon survive a save/load round-trip");
var loot3 = LootGenerator.Generate(0, 1);
var skillBuild3 = new SkillBuild();
skillBuild3.Allocate("S1"); skillBuild3.Allocate("S2"); skillBuild3.Allocate("N1");
skillBuild3.Allocate("S3"); skillBuild3.Allocate("N2"); skillBuild3.Allocate("K1");
var sim3 = new Simulation(42, loot3, skillBuild3);
sim3.World.Player.Restore(100, 100);
sim3.Fps.Teleport(24, 5, 24);
sim3.ApplyLook(45f, -10f);
sim3.FireAimed();
sim3.FireAimed();
Console.WriteLine($"Before save: core {sim3.World.CoreHealth} HP, weapon {sim3.Weapon.Name} {sim3.Weapon.Mag}/{sim3.Weapon.Reserve}, effective damage {sim3.EffectiveDamage}, shields {sim3.World.Player.Shields}/{sim3.World.Player.MaxShields}");
var save3 = sim3.SaveToJson();
var sim4 = new Simulation(99);
Console.WriteLine($"Load: {sim4.LoadFromJson(save3)}");
Console.WriteLine($"After load: core {sim4.World.CoreHealth} HP, weapon {sim4.Weapon.Name} {sim4.Weapon.Mag}/{sim4.Weapon.Reserve}, effective damage {sim4.EffectiveDamage}, shields {sim4.World.Player.Shields}/{sim4.World.Player.MaxShields}, save JSON length: {save3.Length} chars");

// Slice 3 Step 3 scenario: Slice 3 composed in one run — the Step 1
// persistence (damaged core, skilled build, loot weapon) plus the
// Step 2 economy (paid squads, cap, picker refusal), through a
// save/load round-trip. This is the composition proof the scope
// asked for: persistence and economy working together, not only
// in their own steps' tests.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 3 Step 3 scenario — damaged core, skilled build, paid capped squads, save/load round-trip");
var lootS3 = LootGenerator.Generate(0, 1);
var skillBuildS3 = new SkillBuild();
skillBuildS3.Allocate("S1"); skillBuildS3.Allocate("S2"); skillBuildS3.Allocate("N1");
skillBuildS3.Allocate("S3"); skillBuildS3.Allocate("N2"); skillBuildS3.Allocate("K1");
var simS3 = new Simulation(42, lootS3, skillBuildS3);
simS3.World.Player.Restore(100, 100);
simS3.Fps.Teleport(24, 5, 24);
simS3.ApplyLook(45f, -10f);
simS3.FireAimed();
simS3.FireAimed();
simS3.World.Player.Inventory.Add(BlockType.EmberOre, 40);
var s3Squad1 = simS3.DeployOrbitalSquadPaid(18, 5, 16);
var s3Squad2 = simS3.DeployOrbitalSquadPaid(19, 5, 16);
var s3Squad3 = simS3.DeployOrbitalSquadPaid(20, 5, 16);
Console.WriteLine($"Before save: core {simS3.World.CoreHealth} HP, weapon {simS3.Weapon.Name} {simS3.Weapon.Mag}/{simS3.Weapon.Reserve}, effective damage {simS3.EffectiveDamage}, shields {simS3.World.Player.Shields}/{simS3.World.Player.MaxShields}");
Console.WriteLine($"Paid squads deployed: {(s3Squad1 == null ? 0 : 1) + (s3Squad2 == null ? 0 : 1) + (s3Squad3 == null ? 0 : 1)} (ore left {simS3.World.Player.Inventory.Get(BlockType.EmberOre)}), squad HP {(simS3.World.Squads.Count == 0 ? 0 : simS3.World.Squads[0].Health)}");
// The 4th deploy goes through the picker, as in play: at the cap
// it is refused, the picker stays open, and the ore is untouched.
simS3.BeginOrbitalPick();
simS3.SetOrbitalRally(21, 5, 16);
var s3Fourth = simS3.ConfirmOrbitalDeploy();
Console.WriteLine($"4th picker deploy: {(s3Fourth == null ? "REFUSED at cap" : "DEPLOYED (unexpected)")}, picker still open: {simS3.OrbitalCommand.IsPicking}, ore left {simS3.World.Player.Inventory.Get(BlockType.EmberOre)}");
var saveS3 = simS3.SaveToJson();
var simS3Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS3Loaded.LoadFromJson(saveS3)}");
var s3Snap = OrbitalView.Snapshot(simS3Loaded.World);
Console.WriteLine($"After load: core {simS3Loaded.World.CoreHealth} HP, weapon {simS3Loaded.Weapon.Name} {simS3Loaded.Weapon.Mag}/{simS3Loaded.Weapon.Reserve}, effective damage {simS3Loaded.EffectiveDamage}, shields {simS3Loaded.World.Player.Shields}/{simS3Loaded.World.Player.MaxShields}, squads loaded {s3Snap.Count} (HP {(s3Snap.Count == 0 ? 0 : s3Snap[0].Health)}, members {(s3Snap.Count == 0 ? 0 : s3Snap[0].AliveMembers)}), ore {simS3Loaded.World.Player.Inventory.Get(BlockType.EmberOre)}, can deploy more: {simS3Loaded.CanDeployOrbitalSquad()}, save JSON length: {saveS3.Length} chars");

// Slice 4 Step 3 scenario: Slice 4 composed in one run — the Step 1
// enemy pursuit and the Step 2 Advance doctrine together, not two
// separate proofs. Both squads deploy through the paid picker
// (10 EmberOre each, as in play), both weakened to 1 member so
// their 10-damage fire does not kill an enemy outright and hide
// the engagement, and the player moves far away so every enemy
// step is a squad pursuit, not player chasing. Enemy A (east)
// pursues the HoldGround squad; enemy B (west) pursues the
// squads from the other side; the Advance squad hunts enemy A,
// then turns to enemy B after A dies. After 4 ticks the game
// saves and loads, and 2 more ticks prove both doctrines keep
// their behaviour from the saved positions and health.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 4 Step 3 scenario — pursued HoldGround squad, hunting Advance squad, save/load round-trip");
var simS4 = new Simulation(42);
simS4.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS4.BeginOrbitalPick();
simS4.SetOrbitalRally(20, 5, 16);
var s4Hold = simS4.ConfirmOrbitalDeploy();
simS4.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
simS4.BeginOrbitalPick();
simS4.SetOrbitalRally(24, 5, 16);
var s4Adv = simS4.ConfirmOrbitalDeploy();
Console.WriteLine($"Paid squads deployed: HoldGround {(s4Hold == null ? "REJECTED" : "ok")}, Advance {(s4Adv == null ? "REJECTED" : "ok")}, ore left {simS4.World.Player.Inventory.Get(BlockType.EmberOre)}");
s4Hold!.TakeDamage(60); // 90 -> 30, 1 member
s4Adv!.TakeDamage(60); // 90 -> 30, 1 member
simS4.World.Player.X = 28; simS4.World.Player.Z = 28;
simS4.World.Enemies.Add(new Enemy(22, 5, 16));
simS4.World.Enemies.Add(new Enemy(10, 5, 16));
void PrintS4(string label, Simulation s)
{
    var snap = OrbitalView.Snapshot(s.World);
    var enemies = string.Join(", ", s.World.Enemies.Select(e => $"({e.X},5,{e.Z}) {e.Health} HP"));
    Console.WriteLine($"{label}: HoldGround ({(snap.Count > 0 ? snap[0].X : -1)},5,{(snap.Count > 0 ? snap[0].Z : -1)}) {snap[0].Health} HP, Advance ({(snap.Count > 1 ? snap[1].X : -1)},5,{(snap.Count > 1 ? snap[1].Z : -1)}) {snap[1].Health} HP, enemies [{enemies}]");
}
PrintS4("Slice 4 start", simS4);
for (int tick = 1; tick <= 4; tick++)
{
    simS4.World.Advance();
    PrintS4($"Slice 4 tick {tick}", simS4);
}
var saveS4 = simS4.SaveToJson();
var simS4Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS4Loaded.LoadFromJson(saveS4)}");
PrintS4("Slice 4 after load", simS4Loaded);
for (int tick = 5; tick <= 6; tick++)
{
    simS4Loaded.World.Advance();
    PrintS4($"Slice 4 tick {tick} (loaded)", simS4Loaded);
}
var s4LoadedSnap = OrbitalView.Snapshot(simS4Loaded.World);
Console.WriteLine($"Slice 4 done: HoldGround doctrine {s4LoadedSnap[0].Doctrine} at rally: {s4LoadedSnap[0].AtRally}, Advance doctrine {s4LoadedSnap[1].Doctrine}, enemies left {simS4Loaded.World.Enemies.Count}, save JSON length: {saveS4.Length} chars");

// Slice 5 Step 2 scenario: Slice 5 composed in one run — the Step 1
// electrical loop running inside the gather -> build -> survive
// loop, not in a pocket test alone. The powered pair is built
// through the real Simulation builders (mill front-left, Dynamo
// front-centre, face-adjacent with no move between builds), its
// neighbours are seeded as one fresh stone pocket so the rate
// comparison is controlled, and an unpowered mill pocket runs
// alongside it as the kinetic baseline. After 5 ticks the game
// saves and loads, fresh stone is seeded around the loaded
// powered mill (its first pocket is spent, as it should be),
// and 5 more ticks prove the loaded loop still runs at the
// powered rate while the kinetic baseline still runs at its own.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 5 Step 2 scenario — powered mill vs kinetic mill, save/load round-trip");
var simS5 = new Simulation(42);
simS5.Tick(0.1f, 0, 0, false);
simS5.World.Player.Inventory.Add(BlockType.Stone, 10);
simS5.World.Player.Inventory.Add(BlockType.Wood, 5);
simS5.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simS5.ApplyLook(-18.4f, -53f);
var s5Mill = simS5.BuildKineticMillAhead();
simS5.ApplyLook(18.4f, 8f);
var s5Dyn = simS5.BuildDynamoAhead();
Console.WriteLine($"Mill build -> {s5Mill}, Dynamo build -> {s5Dyn}, ore left {simS5.World.Player.Inventory.Get(BlockType.EmberOre)}");
Console.WriteLine($"Powered pair: mill at (15,4,17) = {simS5.World.Chunk.Get(15, 4, 17)}, Dynamo at (16,4,17) = {simS5.World.Chunk.Get(16, 4, 17)}, mill powered: {simS5.World.IsPowered(15, 4, 17)}");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS5.World.Chunk.TrySet(x, y, z, BlockType.Stone);
simS5.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
foreach (var (x, y, z) in new[] { (9, 20, 10), (10, 21, 10), (10, 19, 10), (10, 20, 11), (10, 20, 9) })
    simS5.World.Chunk.TrySet(x, y, z, BlockType.Stone);
Console.WriteLine($"Unpowered mill pocket: mill at (10,20,10) powered: {simS5.World.IsPowered(10, 20, 10)}");
int stoneBefore = simS5.World.Player.Inventory.Get(BlockType.Stone);
for (int i = 0; i < 5; i++) simS5.World.Advance();
Console.WriteLine($"After 5 ticks: powered gathered {simS5.World.PoweredBlocksGathered}, kinetic gathered {simS5.World.KineticBlocksGathered}, stone {stoneBefore} -> {simS5.World.Player.Inventory.Get(BlockType.Stone)}, Dynamo still at (16,4,17): {simS5.World.Chunk.Get(16, 4, 17)}");
var saveS5 = simS5.SaveToJson();
var simS5Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS5Loaded.LoadFromJson(saveS5)}");
Console.WriteLine($"After load: mill {simS5Loaded.World.Chunk.Get(15, 4, 17)}, Dynamo {simS5Loaded.World.Chunk.Get(16, 4, 17)}, mill powered: {simS5Loaded.World.IsPowered(15, 4, 17)}, session counters reset: powered {simS5Loaded.World.PoweredBlocksGathered}, kinetic {simS5Loaded.World.KineticBlocksGathered}");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS5Loaded.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS5Loaded.World.Advance();
Console.WriteLine($"Slice 5 done: loaded powered gathered {simS5Loaded.World.PoweredBlocksGathered}, loaded kinetic gathered {simS5Loaded.World.KineticBlocksGathered}, stone now {simS5Loaded.World.Player.Inventory.Get(BlockType.Stone)}, save JSON length: {saveS5.Length} chars");

// Slice 6 Step 4 scenario: Slice 6 composed in one run — the core
// presentation pieces of steps 1-3, in scope order, through a
// save/load round-trip. Pitch first: the Look starts facing +Z,
// turns 90 yaw / 45 pitch, then is driven 100 further up and
// clamps at 89, so the forward vector is almost straight up.
// Then squads, one at a time so their readings stay readable:
// a HoldGround squad marches to its rally and then holds, and
// an Advance squad hunts a living enemy and then idles once
// its prey is dead — the rally the Advance squad was deployed
// with is ignored throughout, which is the documented Advance
// behaviour, not a bug. Then the powered pair, built through
// the real Simulation builders (mill front-left, Dynamo
// front-centre, no move between builds), with its neighbours
// seeded as stone so the powered rate is controlled. After a
// save/load round-trip the pair, the squads, and their derived
// readings all survive — while the Look does not, by design:
// it is session input state, and a fresh Simulation starts
// facing +Z. What this demo cannot show, by construction, is
// any adapter rendering: no marker, key press, or HUD text
// line appears in a headless run, so those claims stay in
// their Windows checklists, not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 6 Step 4 scenario — pitch, squad activity words, powered count, save/load round-trip");
var simS6 = new Simulation(42);
var f0 = simS6.Look.Forward();
Console.WriteLine($"Look start: yaw {simS6.Look.Yaw}, pitch {simS6.Look.Pitch}, forward ({f0.X:F3}, {f0.Y:F3}, {f0.Z:F3})");
simS6.ApplyLook(90f, 45f);
var f1 = simS6.Look.Forward();
Console.WriteLine($"Look after 90 yaw / 45 pitch: forward ({f1.X:F3}, {f1.Y:F3}, {f1.Z:F3})");
simS6.ApplyLook(0f, 100f);
var f2 = simS6.Look.Forward();
Console.WriteLine($"Look after +100 pitch more: pitch {simS6.Look.Pitch} (clamped at 89), forward ({f2.X:F3}, {f2.Y:F3}, {f2.Z:F3})");
simS6.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS6.BeginOrbitalPick();
simS6.SetOrbitalRally(20, 5, 16);
var s6Hold = simS6.ConfirmOrbitalDeploy();
void PrintS6(string label, Simulation s)
{
    var snap = OrbitalView.Snapshot(s.World);
    foreach (var sq in snap)
        Console.WriteLine($"{label}: {sq.Doctrine} at ({sq.X},5,{sq.Z}) rally ({sq.RallyX},5,{sq.RallyZ}) reads {sq.Activity}{(sq.HasHuntingTarget ? $" hunting ({sq.HuntingTargetX},5,{sq.HuntingTargetZ})" : "")} HP {sq.Health}");
}
PrintS6("Slice 6 HoldGround deployed", simS6);
for (int tick = 1; tick <= 4; tick++) simS6.World.Advance();
PrintS6("Slice 6 HoldGround after 4 ticks", simS6);
simS6.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
simS6.BeginOrbitalPick();
simS6.SetOrbitalRally(24, 5, 16);
var s6Adv = simS6.ConfirmOrbitalDeploy();
simS6.World.Enemies.Add(new Enemy(16, 5, 20));
Console.WriteLine($"Paid squads deployed: HoldGround {(s6Hold == null ? "REJECTED" : "ok")}, Advance {(s6Adv == null ? "REJECTED" : "ok")}, ore left {simS6.World.Player.Inventory.Get(BlockType.EmberOre)}");
PrintS6("Slice 6 Advance with live prey", simS6);
simS6.World.Advance();
PrintS6("Slice 6 Advance after prey dies", simS6);
Console.WriteLine($"Enemies left: {simS6.World.Enemies.Count}");
simS6.World.Player.Inventory.Add(BlockType.Stone, 10);
simS6.World.Player.Inventory.Add(BlockType.Wood, 5);
simS6.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simS6.ApplyLook(-108.4f, -142f);
var s6Mill = simS6.BuildKineticMillAhead();
simS6.ApplyLook(18.4f, 8f);
var s6Dyn = simS6.BuildDynamoAhead();
Console.WriteLine($"Mill build -> {s6Mill}, Dynamo build -> {s6Dyn}, ore left {simS6.World.Player.Inventory.Get(BlockType.EmberOre)}");
var s6Hud = simS6.Hud();
Console.WriteLine($"Power indicator: {s6Hud.PoweredMills}/{s6Hud.TotalMills} MILLS POWERED (mill powered: {simS6.World.IsPowered(15, 4, 17)})");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS6.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS6.World.Advance();
Console.WriteLine($"After 5 ticks: powered gathered {simS6.World.PoweredBlocksGathered}, stone {simS6.World.Player.Inventory.Get(BlockType.Stone)}");
var saveS6 = simS6.SaveToJson();
var simS6Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS6Loaded.LoadFromJson(saveS6)}");
var s6LoadedHud = simS6Loaded.Hud();
Console.WriteLine($"After load: mill {simS6Loaded.World.Chunk.Get(15, 4, 17)}, Dynamo {simS6Loaded.World.Chunk.Get(16, 4, 17)}, power {s6LoadedHud.PoweredMills}/{s6LoadedHud.TotalMills}, session counters reset: powered {simS6Loaded.World.PoweredBlocksGathered}");
PrintS6("Slice 6 after load", simS6Loaded);
var fLoaded = simS6Loaded.Look.Forward();
Console.WriteLine($"Loaded Look (not saved, fresh view): yaw {simS6Loaded.Look.Yaw}, pitch {simS6Loaded.Look.Pitch}, forward ({fLoaded.X:F3}, {fLoaded.Y:F3}, {fLoaded.Z:F3})");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS6Loaded.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS6Loaded.World.Advance();
Console.WriteLine($"Slice 6 done: loaded powered gathered {simS6Loaded.World.PoweredBlocksGathered}, save JSON length: {saveS6.Length} chars");

// Slice 7 Step 2 scenario: Slice 7 composed in one run — the Step 1
// second kind together with Vanguard, not separately. Both squads
// deploy through the paid picker (10 EmberOre each, as in play):
// a Vanguard HoldGround squad on the z=16 lane and a Recon
// Advance squad on the z=20 lane, each with an enemy approaching
// from the east at (27,5,z). The player is parked far away at
// (28,5,4) after deployment, so every enemy step is a squad
// pursuit, not player chasing. The two lanes close at the same
// rate, which makes the range contrast readable in one run: the
// Recon fires first, at exactly range 5 on tick 3 — a distance
// Vanguard's range 3 cannot reach — while the Vanguard, still
// marching that tick, cannot fire at all; on tick 4 the Vanguard
// reaches its rally and kills its enemy at exactly range 3,
// and the Recon fires its lane enemy down to 6 HP. On tick 5
// the Recon closes to its prey and kills it, so both squads
// finish intact at full health. A third picker deploy is then
// refused for affordability (20 ore spent, 0 left, picker stays
// open, nothing charged), the game saves and loads with both
// kinds, doctrines, positions, and health intact, and an old
// Vanguard-era save (Squads field stripped, as in the tests)
// still loads with no squads instead of crashing.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 7 Step 2 scenario — Vanguard + Recon together, paid picker, range contrast, save/load round-trip");
var simS7 = new Simulation(42);
simS7.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS7.BeginOrbitalPick();
simS7.SetOrbitalRally(20, 5, 16);
var s7Van = simS7.ConfirmOrbitalDeploy();
// The second squad spawns at the player's feet, so the player
// walks to the second lane first: the Recon then starts on
// z=20, not stacked on the Vanguard's z=16 lane.
simS7.World.Player.Z = 20;
simS7.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
simS7.OrbitalCommand.SelectKind(SquadKind.Recon);
simS7.BeginOrbitalPick();
simS7.SetOrbitalRally(24, 5, 20);
var s7Rec = simS7.ConfirmOrbitalDeploy();
Console.WriteLine($"Paid squads deployed: Vanguard HoldGround {(s7Van == null ? "REJECTED" : $"{s7Van.AliveMembers} members, {s7Van.Health} HP, {s7Van.DamagePerTick} dmg, range {s7Van.EngagementRange}")}, Recon Advance {(s7Rec == null ? "REJECTED" : $"{s7Rec.AliveMembers} members, {s7Rec.Health} HP, {s7Rec.DamagePerTick} dmg, range {s7Rec.EngagementRange}")}, price {SquadEconomy.CostForKind(SquadKind.Vanguard)}/{SquadEconomy.CostForKind(SquadKind.Recon)} EmberOre each, ore left {simS7.World.Player.Inventory.Get(BlockType.EmberOre)}");
simS7.BeginOrbitalPick();
simS7.SetOrbitalRally(20, 5, 20);
var s7Third = simS7.ConfirmOrbitalDeploy();
Console.WriteLine($"3rd picker deploy: {(s7Third == null ? "REFUSED unaffordable" : "DEPLOYED (unexpected)")}, picker still open: {simS7.OrbitalCommand.IsPicking}, ore left {simS7.World.Player.Inventory.Get(BlockType.EmberOre)}");
simS7.OrbitalCommand.CancelPick();
simS7.World.Player.X = 28; simS7.World.Player.Z = 4;
simS7.World.Enemies.Add(new Enemy(27, 5, 16));
simS7.World.Enemies.Add(new Enemy(27, 5, 20));
void PrintS7(string label, Simulation s)
{
    var snap = OrbitalView.Snapshot(s.World);
    foreach (var sq in snap)
        Console.WriteLine($"{label}: {sq.Kind} {sq.Doctrine} at ({sq.X},5,{sq.Z}) rally ({sq.RallyX},5,{sq.RallyZ}) reads {sq.Activity}{(sq.HasHuntingTarget ? $" hunting ({sq.HuntingTargetX},5,{sq.HuntingTargetZ})" : "")} {sq.AliveMembers} members, {sq.Health} HP, {sq.DamagePerTick} dmg, range {(sq.Kind == SquadKind.Recon ? 5 : 3)}");
    var enemies = string.Join(", ", s.World.Enemies.Select(e => $"({e.X},5,{e.Z}) {e.Health} HP"));
    Console.WriteLine($"{label}: enemies [{enemies}]");
}
PrintS7("Slice 7 start", simS7);
for (int tick = 1; tick <= 5; tick++)
{
    simS7.World.Advance();
    PrintS7($"Slice 7 tick {tick}", simS7);
}
var saveS7 = simS7.SaveToJson();
var simS7Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS7Loaded.LoadFromJson(saveS7)}");
PrintS7("Slice 7 after load", simS7Loaded);
simS7Loaded.World.Advance();
PrintS7("Slice 7 loaded tick 1 (no enemies: Holding / Idle)", simS7Loaded);
var worldSaveS7 = SaveSystem.SaveZone(simS7.World);
var oldSaveS7 = worldSaveS7[..worldSaveS7.LastIndexOf(",\"Squads\":", StringComparison.Ordinal)] + "}";
var oldLoadedS7 = SaveSystem.LoadZone(oldSaveS7);
Console.WriteLine($"Old save (Squads field stripped) loads: {oldLoadedS7.Squads.Count} squads, no crash, ore {simS7Loaded.World.Player.Inventory.Get(BlockType.EmberOre)}, can deploy more: {simS7Loaded.CanDeployOrbitalSquad()}");
Console.WriteLine($"Slice 7 done: Vanguard {simS7Loaded.World.Squads[0].Kind} {simS7Loaded.World.Squads[0].Health} HP, Recon {simS7Loaded.World.Squads[1].Kind} {simS7Loaded.World.Squads[1].Health} HP, enemies left {simS7Loaded.World.Enemies.Count}, save JSON length: {saveS7.Length} chars");

// Slice 8 Step 3 scenario: Slice 8 composed in one run —
// yaw-rotated movement and aim-based interaction together,
// not separately. The whole run faces +X: the player yaws
// 90 degrees, walks one tick (W now walks +X, not +Z),
// pitches down 45 degrees, gathers the +X ground cell
// (18,3,16) — one cell further east than the feet-level
// yaw-90 cell (17,3,16), because the eye is 0.6 m east
// after the walk and the ray descends as it travels —
// then builds a wall back into that same cell, pitches
// level, and fires the +X enemy dead in 3 shots. The
// game then saves and loads: the wall, the empty enemy
// list, and the world survive, while the view rule is
// stated, not smuggled — a fresh Simulation starts at
// yaw 0, so the loaded run's W walks +Z, not +X, and
// the fractional 0.6 m of the pre-save walk is not
// saved (the player restarts at the cell corner). The
// session counters reset on load, as in every slice:
// what survives is the world (wall present, enemy
// gone), not the pre-load counts. What this demo cannot
// show, by construction, is any adapter feel: no mouse
// turn, crosshair, or key press appears in a headless
// run, so those claims stay in their Windows checklist,
// not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 8 Step 3 scenario — yaw walk, aimed gather, aimed build, aimed fire, save/load round-trip");
var simS8 = new Simulation(42);
simS8.Tick(0.1f, 0, 0, false);
Console.WriteLine($"Slice 8 start: player feet ({simS8.Fps.X:F1}, {simS8.Fps.Y:F1}, {simS8.Fps.Z:F1}), Look yaw {simS8.Look.Yaw}, pitch {simS8.Look.Pitch}");
simS8.ApplyLook(90f, 0f);
simS8.Tick(0.1f, 1f, 0f, false);
Console.WriteLine($"Slice 8 yaw 90 walk: W walks +X, feet now ({simS8.Fps.X:F1}, {simS8.Fps.Y:F1}, {simS8.Fps.Z:F1})");
simS8.ApplyLook(0f, -45f);
var s8Gather = simS8.GatherAhead();
Console.WriteLine($"Slice 8 aimed gather -> {s8Gather} (gathered {simS8.BlocksGathered}), cell (18,3,16) now {simS8.World.Chunk.Get(18, 3, 16)}");
simS8.World.Player.Inventory.Add(BlockType.Stone, 10);
var s8Build = simS8.BuildWallAhead();
Console.WriteLine($"Slice 8 aimed build -> {s8Build} (built {simS8.BlocksBuilt}), cell (18,3,16) now {simS8.World.Chunk.Get(18, 3, 16)}, stone left {simS8.World.Player.Inventory.Get(BlockType.Stone)}");
simS8.World.Enemies.Add(new Enemy(22, 5, 16));
simS8.ApplyLook(0f, 45f);
for (int i = 0; i < 3; i++) simS8.FireAimed();
Console.WriteLine($"Slice 8 aimed fire x3: killed {simS8.EnemiesKilled}, enemies left {simS8.World.Enemies.Count}, ammo {simS8.Weapon.Mag}/{simS8.Weapon.Reserve}");
var saveS8 = simS8.SaveToJson();
var simS8Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS8Loaded.LoadFromJson(saveS8)}");
Console.WriteLine($"After load: wall at (18,3,16): {simS8Loaded.World.Chunk.Get(18, 3, 16)}, enemies left {simS8Loaded.World.Enemies.Count}, player feet ({simS8Loaded.Fps.X:F1}, {simS8Loaded.Fps.Y:F1}, {simS8Loaded.Fps.Z:F1}) (fractional walk not saved), Look yaw {simS8Loaded.Look.Yaw}, pitch {simS8Loaded.Look.Pitch} (fresh view, not the pre-save yaw 90), session counters reset: gathered {simS8Loaded.BlocksGathered}, built {simS8Loaded.BlocksBuilt}, killed {simS8Loaded.EnemiesKilled}");
simS8Loaded.Tick(0.1f, 1f, 0f, false);
Console.WriteLine($"Loaded walk: fresh-view W walks +Z, feet now ({simS8Loaded.Fps.X:F1}, {simS8Loaded.Fps.Y:F1}, {simS8Loaded.Fps.Z:F1})");
Console.WriteLine($"Slice 8 done: gathered {simS8.BlocksGathered}, built {simS8.BlocksBuilt}, killed {simS8.EnemiesKilled}, save JSON length: {saveS8.Length} chars");
