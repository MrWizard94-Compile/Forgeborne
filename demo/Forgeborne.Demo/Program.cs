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

// Slice 9 Step 4 scenario: Slice 9 composed in one run —
// the ladder, the cable, and the destruction rule together,
// not separately. Four mills on one high plane (y=20, the
// same staged plane the Slice 9 Step 1-3 tests use), in four
// rows far enough apart that no row touches another:
// (A, z=10) Dynamo -> cable -> cable -> mill: rated LV
// through a cable run, powered every tick; (B, z=14)
// Dynamo -> transformer -> transformer -> mill: the
// round-trip conversion back to LV, powered every tick;
// (C, z=18) Dynamo -> transformer -> cable -> mill: the
// converted HV reaches the mill through a cable run, so
// the mill is destroyed on its first tick, with no
// salvage and no cascade — its cable, transformer, and
// Dynamo all survive; (D, z=22) a mill with no power at
// all, still excavating every 5th tick beside them. The
// scope's Step 4 wording said the second mill "runs at
// the converted voltage" — after Step 3 that wording
// cannot mean a mill surviving on HV, because HV now
// destroys a mill, so this scenario proves the converted
// voltage two honest ways instead: row B survives on the
// converted voltage converted back to LV, and row C is
// destroyed by the converted voltage itself. The layout
// is staged directly with TrySet, exactly as the Step 1-3
// tests stage theirs, not placed by aim in this run:
// aim placement of a transformer and of a cable was each
// proved in its own step's tests, and walking four aim
// placements here would prove walking, not wiring. Each
// mill gets a five-stone pocket (its five non-infrastructure
// neighbours), so the rates are readable: 5 ticks give
// the two powered mills 5 each, the kinetic mill 1, and
// the destroyed mill 0, with its pocket untouched. The
// game then saves and loads: the surviving blocks and
// the destroyed mill's absence both survive, power
// re-derives from placement with no stored voltage flag,
// and the session counters reset — then the loaded world
// re-seeds the two powered pockets (labelled as fresh
// seeding, not leftovers) and gathers the same 5 + 5 + 1
// again, proving the loaded wiring still works. What
// this demo cannot show, by construction, is any adapter
// feel or scene visibility: no U / I key press, no HUD
// voltage line, and no visible disappearance of the
// destroyed mill appear in a headless run, so those
// claims stay in their Windows checklist items (15, 16,
// 17), not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 9 Step 4 scenario — LV cable mill, round-trip LV mill, HV cable mill destroyed, kinetic mill, save/load round-trip");
var simS9 = new Simulation(42);
void SeedPocketS9(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
simS9.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
simS9.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
simS9.World.Chunk.TrySet(12, 20, 10, BlockType.Cable);
simS9.World.Chunk.TrySet(13, 20, 10, BlockType.Dynamo);
simS9.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
simS9.World.Chunk.TrySet(11, 20, 14, BlockType.Transformer);
simS9.World.Chunk.TrySet(12, 20, 14, BlockType.Transformer);
simS9.World.Chunk.TrySet(13, 20, 14, BlockType.Dynamo);
simS9.World.Chunk.TrySet(10, 20, 18, BlockType.KineticMill);
simS9.World.Chunk.TrySet(11, 20, 18, BlockType.Cable);
simS9.World.Chunk.TrySet(12, 20, 18, BlockType.Transformer);
simS9.World.Chunk.TrySet(13, 20, 18, BlockType.Dynamo);
simS9.World.Chunk.TrySet(10, 20, 22, BlockType.KineticMill);
SeedPocketS9(simS9.World, 10, 10);
SeedPocketS9(simS9.World, 10, 14);
SeedPocketS9(simS9.World, 10, 18);
SeedPocketS9(simS9.World, 10, 22);
Console.WriteLine($"Slice 9 voltages before any tick: cable mill {simS9.World.DeliveredVoltage(10, 20, 10)}, round-trip mill {simS9.World.DeliveredVoltage(10, 20, 14)}, HV cable mill {simS9.World.DeliveredVoltage(10, 20, 18)}, kinetic mill {simS9.World.DeliveredVoltage(10, 20, 22)}");
var s9HudBefore = simS9.Hud();
Console.WriteLine($"Slice 9 HUD before: {s9HudBefore.PoweredMills}/{s9HudBefore.TotalMills} MILLS POWERED (only the two LV mills count)");
simS9.World.Advance();
Console.WriteLine($"Slice 9 tick 1: HV mill cell now {simS9.World.Chunk.Get(10, 20, 18)} (destroyed), destroyed {simS9.World.DestroyedMills}, powered gathered {simS9.World.PoweredBlocksGathered}, kinetic gathered {simS9.World.KineticBlocksGathered}, stone {simS9.World.Player.Inventory.Get(BlockType.Stone)}, HV pocket stone at (9,20,18) still {simS9.World.Chunk.Get(9, 20, 18)}, HV cable {simS9.World.Chunk.Get(11, 20, 18)}, transformer {simS9.World.Chunk.Get(12, 20, 18)}, Dynamo {simS9.World.Chunk.Get(13, 20, 18)}");
var s9HudAfter1 = simS9.Hud();
Console.WriteLine($"Slice 9 HUD after tick 1: {s9HudAfter1.PoweredMills}/{s9HudAfter1.TotalMills} MILLS POWERED (total drops with the destroyed mill)");
for (int i = 0; i < 4; i++) simS9.World.Advance();
Console.WriteLine($"Slice 9 after 5 ticks: powered gathered {simS9.World.PoweredBlocksGathered} (5 + 5), kinetic gathered {simS9.World.KineticBlocksGathered}, destroyed {simS9.World.DestroyedMills}, stone {simS9.World.Player.Inventory.Get(BlockType.Stone)} (5 + 5 + 1, no refund for the destroyed mill)");
var saveS9 = simS9.SaveToJson();
var simS9Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS9Loaded.LoadFromJson(saveS9)}");
var s9LoadedHud = simS9Loaded.Hud();
Console.WriteLine($"Slice 9 after load: cable mill {simS9Loaded.World.Chunk.Get(10, 20, 10)}, round-trip mill {simS9Loaded.World.Chunk.Get(10, 20, 14)}, HV mill cell {simS9Loaded.World.Chunk.Get(10, 20, 18)} (still gone, no resurrection), kinetic mill {simS9Loaded.World.Chunk.Get(10, 20, 22)}, power {s9LoadedHud.PoweredMills}/{s9LoadedHud.TotalMills}, voltages re-derived: cable {simS9Loaded.World.DeliveredVoltage(10, 20, 10)}, round-trip {simS9Loaded.World.DeliveredVoltage(10, 20, 14)}, session counters reset: powered {simS9Loaded.World.PoweredBlocksGathered}, kinetic {simS9Loaded.World.KineticBlocksGathered}, destroyed {simS9Loaded.World.DestroyedMills}, stone {simS9Loaded.World.Player.Inventory.Get(BlockType.Stone)} (no refund on load)");
SeedPocketS9(simS9Loaded.World, 10, 10);
SeedPocketS9(simS9Loaded.World, 10, 14);
for (int i = 0; i < 5; i++) simS9Loaded.World.Advance();
Console.WriteLine($"Slice 9 loaded phase after 5 ticks: powered gathered {simS9Loaded.World.PoweredBlocksGathered} (5 + 5 again), kinetic gathered {simS9Loaded.World.KineticBlocksGathered}, destroyed {simS9Loaded.World.DestroyedMills} (no new destruction), stone {simS9Loaded.World.Player.Inventory.Get(BlockType.Stone)}");
Console.WriteLine($"Slice 9 done: powered {simS9.World.PoweredBlocksGathered} + loaded {simS9Loaded.World.PoweredBlocksGathered}, kinetic {simS9.World.KineticBlocksGathered} + loaded {simS9Loaded.World.KineticBlocksGathered}, destroyed {simS9.World.DestroyedMills}, save JSON length: {saveS9.Length} chars");

// Slice 10 Step 3 scenario: Slice 10 composed in one run —
// the second consumer and the second source together, not
// separately. Three mills on the y=20 plane, as in Slice 9,
// plus three workstations on the y=5 plane beside the
// player's spawn, because workstation regen is
// player-centred (Manhattan distance 3) while mill rates
// are machine-centred, so the two consumers cannot be
// proved side by side at one player position: the demo
// therefore walks the player between the three benches
// to read each bench's regen, and states that walk
// openly instead of pretending one position proves all
// three. Mill rows: (z=10) Alternator -> cable ->
// transformer -> cable -> mill, the cable step-down path
// Step 2 proved, running LV every tick; (z=14) a mill
// fed HV directly by an Alternator, destroyed on tick 1
// by the Step 1 mismatch rule, with no salvage and no
// cascade; (z=18) a mill with no power at all, still
// excavating every 5th tick beside them. Workstation
// benches: (z=16) Alternator -> transformer ->
// workstation, the face-adjacent step-down path Step 2
// proved, powered at base + 5; (z=22) a workstation with
// no source, still regenerating at base beside it;
// (z=28) a workstation fed HV directly by an Alternator,
// disabled (regen 0) but surviving, by the Step 1 rule
// that a bench trips offline where a mill breaks. Each
// mill gets a five-stone pocket, as in Slice 9. The
// layout is staged directly with TrySet, exactly as the
// Step 1-2 tests stage theirs: aim placement of a mill,
// a workstation, a transformer, a cable, and an
// Alternator was each proved in its own step's tests,
// and walking this layout in by aim would prove walking,
// not wiring. What this demo cannot show, by
// construction, is any adapter feel or scene visibility:
// no B / N key press, no powered-workstation indicator
// beyond the HUD counts, and no visible disappearance
// of the destroyed mill appear in a headless run, so
// those claims stay in their Windows checklist items
// (17 for destruction disappearance, 18 for the N key;
// the B key has no checklist item yet, the gap Step 2
// observed and this review carries), not in the numbers
// below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 10 Step 3 scenario — Alternator-fed mill + workstation, HV mill destroyed, HV workstation disabled, kinetic + unpowered baselines, save/load round-trip");
var simS10 = new Simulation(42);
void SeedPocketS10(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
simS10.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
simS10.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
simS10.World.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
simS10.World.Chunk.TrySet(13, 20, 10, BlockType.Cable);
simS10.World.Chunk.TrySet(14, 20, 10, BlockType.Alternator);
simS10.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
simS10.World.Chunk.TrySet(11, 20, 14, BlockType.Alternator);
simS10.World.Chunk.TrySet(10, 20, 18, BlockType.KineticMill);
SeedPocketS10(simS10.World, 10, 10);
SeedPocketS10(simS10.World, 10, 14);
SeedPocketS10(simS10.World, 10, 18);
simS10.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
simS10.World.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
simS10.World.Chunk.TrySet(19, 5, 16, BlockType.Alternator);
simS10.World.Chunk.TrySet(17, 5, 22, BlockType.Workstation);
simS10.World.Chunk.TrySet(17, 5, 28, BlockType.Workstation);
simS10.World.Chunk.TrySet(18, 5, 28, BlockType.Alternator);
simS10.World.Player.TakeDamage(20);
Console.WriteLine($"Slice 10 voltages before any tick: cable step-down mill {simS10.World.DeliveredVoltage(10, 20, 10)}, direct HV mill {simS10.World.DeliveredVoltage(10, 20, 14)}, kinetic mill {simS10.World.DeliveredVoltage(10, 20, 18)}, powered workstation {simS10.World.DeliveredVoltage(17, 5, 16)}, unpowered workstation {simS10.World.DeliveredVoltage(17, 5, 22)}, HV workstation {simS10.World.DeliveredVoltage(17, 5, 28)}");
var s10HudBefore = simS10.Hud();
Console.WriteLine($"Slice 10 HUD before: {s10HudBefore.PoweredMills}/{s10HudBefore.TotalMills} MILLS POWERED, {s10HudBefore.PoweredWorkstations}/{s10HudBefore.TotalWorkstations} WORKSTATIONS POWERED (only the LV mill and the LV workstation count)");
Console.WriteLine($"Slice 10 powered bench regen at spawn: {simS10.World.EffectiveWorkstationRegen()} (base 5 + powered 5), shields {simS10.World.Player.Shields}");
simS10.World.Advance();
Console.WriteLine($"Slice 10 tick 1: HV mill cell now {simS10.World.Chunk.Get(10, 20, 14)} (destroyed), destroyed {simS10.World.DestroyedMills}, powered gathered {simS10.World.PoweredBlocksGathered}, kinetic gathered {simS10.World.KineticBlocksGathered}, stone {simS10.World.Player.Inventory.Get(BlockType.Stone)}, HV pocket stone at (9,20,14) still {simS10.World.Chunk.Get(9, 20, 14)}, HV Alternator {simS10.World.Chunk.Get(11, 20, 14)}, HV workstation {simS10.World.Chunk.Get(17, 5, 28)} (survives, disabled), shields {simS10.World.Player.Shields} (65 + 10 powered regen)");
var s10HudAfter1 = simS10.Hud();
Console.WriteLine($"Slice 10 HUD after tick 1: {s10HudAfter1.PoweredMills}/{s10HudAfter1.TotalMills} MILLS POWERED (total drops with the destroyed mill), {s10HudAfter1.PoweredWorkstations}/{s10HudAfter1.TotalWorkstations} WORKSTATIONS POWERED (the disabled bench stays in the total, not the powered count)");
var s10WalkUnpowered = simS10.World.Player.MoveTo(16, 5, 22, simS10.World.Chunk);
Console.WriteLine($"Slice 10 walk to unpowered bench -> {s10WalkUnpowered}, effective regen there: {simS10.World.EffectiveWorkstationRegen()} (base 5, no source)");
var s10WalkMismatched = simS10.World.Player.MoveTo(16, 5, 28, simS10.World.Chunk);
Console.WriteLine($"Slice 10 walk to HV bench -> {s10WalkMismatched}, effective regen there: {simS10.World.EffectiveWorkstationRegen()} (0, disabled — not destroyed, not boosted)");
var s10WalkPowered = simS10.World.Player.MoveTo(16, 5, 16, simS10.World.Chunk);
Console.WriteLine($"Slice 10 walk back to powered bench -> {s10WalkPowered}, effective regen there: {simS10.World.EffectiveWorkstationRegen()} (base 5 + powered 5 again)");
for (int i = 0; i < 4; i++) simS10.World.Advance();
Console.WriteLine($"Slice 10 after 5 ticks: powered gathered {simS10.World.PoweredBlocksGathered} (5, pocket exhausted), kinetic gathered {simS10.World.KineticBlocksGathered}, destroyed {simS10.World.DestroyedMills}, stone {simS10.World.Player.Inventory.Get(BlockType.Stone)} (5 + 1, no refund for the destroyed mill), shields {simS10.World.Player.Shields} (capped at 85)");
var saveS10 = simS10.SaveToJson();
var simS10Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS10Loaded.LoadFromJson(saveS10)}");
var s10LoadedHud = simS10Loaded.Hud();
Console.WriteLine($"Slice 10 after load: cable step-down mill {simS10Loaded.World.Chunk.Get(10, 20, 10)}, HV mill cell {simS10Loaded.World.Chunk.Get(10, 20, 14)} (still gone, no resurrection), kinetic mill {simS10Loaded.World.Chunk.Get(10, 20, 18)}, powered workstation {simS10Loaded.World.Chunk.Get(17, 5, 16)}, unpowered workstation {simS10Loaded.World.Chunk.Get(17, 5, 22)}, HV workstation {simS10Loaded.World.Chunk.Get(17, 5, 28)} (still standing, disabled), power {s10LoadedHud.PoweredMills}/{s10LoadedHud.TotalMills} mills, {s10LoadedHud.PoweredWorkstations}/{s10LoadedHud.TotalWorkstations} workstations, voltages re-derived: mill {simS10Loaded.World.DeliveredVoltage(10, 20, 10)}, powered workstation {simS10Loaded.World.DeliveredVoltage(17, 5, 16)}, session counters reset: powered {simS10Loaded.World.PoweredBlocksGathered}, kinetic {simS10Loaded.World.KineticBlocksGathered}, destroyed {simS10Loaded.World.DestroyedMills}, stone {simS10Loaded.World.Player.Inventory.Get(BlockType.Stone)} (no refund on load), player back at powered bench ({simS10Loaded.World.Player.X},{simS10Loaded.World.Player.Y},{simS10Loaded.World.Player.Z}), effective regen {simS10Loaded.World.EffectiveWorkstationRegen()}");
simS10Loaded.World.Player.TakeDamage(20);
SeedPocketS10(simS10Loaded.World, 10, 10);
for (int i = 0; i < 5; i++) simS10Loaded.World.Advance();
Console.WriteLine($"Slice 10 loaded phase after 5 ticks: powered gathered {simS10Loaded.World.PoweredBlocksGathered} (5 again), kinetic gathered {simS10Loaded.World.KineticBlocksGathered}, destroyed {simS10Loaded.World.DestroyedMills} (no new destruction), stone {simS10Loaded.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS10Loaded.World.Player.Shields} (65 + 10 per tick, capped at 85)");
Console.WriteLine($"Slice 10 done: powered {simS10.World.PoweredBlocksGathered} + loaded {simS10Loaded.World.PoweredBlocksGathered}, kinetic {simS10.World.KineticBlocksGathered} + loaded {simS10Loaded.World.KineticBlocksGathered}, destroyed {simS10.World.DestroyedMills}, save JSON length: {saveS10.Length} chars");

// Slice 11 Step 3 scenario: Slice 11 composed in one run —
// the third kind under the new doctrine beside an existing
// kind under an existing doctrine, not two separate proofs.
// A Heavy Defend Core squad (2 members, 50 HP each, 8 damage
// each, range 2) deploys through the paid picker with a dummy
// rally (20,5,20) the doctrine never holds at, and a
// Vanguard HoldGround squad (3 members, 30 HP each, 10 damage
// each, range 3) deploys through the same picker to a real
// rally (20,5,8), each costing 10 EmberOre as in play. The
// Heavy passes through its dummy rally cell at tick 8 without
// holding there — the cell is on its core-ward path, and the
// doctrine, not the rally, decides where it stops. The
// player parks far away at (4,5,4) so every enemy step is a
// squad pursuit, not player chasing. The Vanguard lane runs
// the Slice 7 pattern: its enemy starts at (27,5,8), closes
// two cells a tick, and dies at exactly range 3 on tick 4,
// the tick the Vanguard reaches its rally. The Heavy marches
// to the core itself (28,5,28) over 24 ticks — one voxel per
// tick, 12 east + 12 south — reading Marching all the way
// and Guarding once at its post, never visiting its dummy
// rally. Only then is a core attacker staged, at (26,5,28),
// two cells from the core: the guard reads Hunting, steps
// toward the attacker but is refused its occupied cell,
// fires 16 per tick at range 2, takes one 10-damage adjacent
// hit, kills the attacker in two ticks, and reads Guarding
// again — chase, kill, and return-to-post in one run, with
// the attacker never drawing the guard away from the core.
// A third picker squad then proves the gate's dead-squad
// rule literally: a Vanguard HoldGround squad deployed at
// the player's feet, weakened to 10 HP, with an enemy
// adjacent, fires once for 10 and dies to the adjacent hit
// in the same tick. The game then saves and loads: the
// Heavy (Defend Core, at the core, 90 HP) and the Vanguard
// (HoldGround, at its rally, 90 HP) survive with kind and
// doctrine re-derived from the saved ints, the dead squad
// stays dead (2 squads loaded, not 3), the surviving enemy
// keeps its 20 HP, and one loaded tick proves both squads
// still read Guarding / Holding with no resurrection and
// no new save field. What this demo cannot show, by
// construction, is any adapter feel: no Alpha5 / Alpha6
// key press and no scene marker appear in a headless run,
// so those claims stay in their Windows checklist items,
// not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 11 Step 3 scenario — Heavy Defend Core + Vanguard HoldGround, core attacker killed, dead squad stays dead, save/load round-trip");
var simS11 = new Simulation(42);
simS11.World.Player.Inventory.Add(BlockType.EmberOre, 30);
simS11.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore);
simS11.OrbitalCommand.SelectKind(SquadKind.Heavy);
simS11.BeginOrbitalPick();
simS11.SetOrbitalRally(20, 5, 20);
var s11Heavy = simS11.ConfirmOrbitalDeploy();
simS11.World.Player.X = 16; simS11.World.Player.Z = 8;
simS11.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
simS11.OrbitalCommand.SelectKind(SquadKind.Vanguard);
simS11.BeginOrbitalPick();
simS11.SetOrbitalRally(20, 5, 8);
var s11Van = simS11.ConfirmOrbitalDeploy();
Console.WriteLine($"Paid squads deployed: Heavy DefendCore {(s11Heavy == null ? "REJECTED" : $"{s11Heavy.AliveMembers} members, {s11Heavy.Health} HP, {s11Heavy.DamagePerTick} dmg, range {s11Heavy.EngagementRange}, dummy rally (20,5,20)")}, Vanguard HoldGround {(s11Van == null ? "REJECTED" : $"{s11Van.AliveMembers} members, {s11Van.Health} HP, {s11Van.DamagePerTick} dmg, range {s11Van.EngagementRange}")}, ore left {simS11.World.Player.Inventory.Get(BlockType.EmberOre)}");
simS11.World.Player.X = 4; simS11.World.Player.Z = 4;
simS11.World.Enemies.Add(new Enemy(27, 5, 8));
void PrintS11(string label, Simulation s)
{
    var snap = OrbitalView.Snapshot(s.World);
    foreach (var sq in snap)
        Console.WriteLine($"{label}: {sq.Kind} {sq.Doctrine} at ({sq.X},5,{sq.Z}) rally ({sq.RallyX},5,{sq.RallyZ}) reads {sq.Activity}{(sq.HasHuntingTarget ? $" hunting ({sq.HuntingTargetX},5,{sq.HuntingTargetZ})" : "")} {sq.AliveMembers} members, {sq.Health} HP, {sq.DamagePerTick} dmg, range {(sq.Kind == SquadKind.Heavy ? 2 : 3)}");
    var enemies = string.Join(", ", s.World.Enemies.Select(e => $"({e.X},5,{e.Z}) {e.Health} HP"));
    Console.WriteLine($"{label}: enemies [{enemies}], core {s.World.CoreHealth} HP at (28,5,28)");
}
PrintS11("Slice 11 start", simS11);
for (int tick = 1; tick <= 24; tick++)
{
    simS11.World.Advance();
    if (tick % 4 == 0) PrintS11($"Slice 11 tick {tick}", simS11);
}
simS11.World.Enemies.Add(new Enemy(26, 5, 28));
Console.WriteLine($"Slice 11 core attacker staged at (26,5,28), 2 from the core, Heavy reads {OrbitalView.Snapshot(simS11.World)[0].Activity} hunting (26,5,28)");
for (int tick = 25; tick <= 27; tick++)
{
    simS11.World.Advance();
    PrintS11($"Slice 11 tick {tick} (core attacker)", simS11);
}
simS11.World.Player.X = 10; simS11.World.Player.Z = 10;
simS11.World.Player.Inventory.Add(BlockType.EmberOre, 10);
simS11.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
simS11.OrbitalCommand.SelectKind(SquadKind.Vanguard);
simS11.BeginOrbitalPick();
simS11.SetOrbitalRally(10, 5, 10);
var s11Doomed = simS11.ConfirmOrbitalDeploy();
s11Doomed!.TakeDamage(80);
simS11.World.Enemies.Add(new Enemy(11, 5, 10));
Console.WriteLine($"Slice 11 doomed squad deployed at player feet (10,5,10), weakened to {s11Doomed.Health} HP, enemy adjacent at (11,5,10), ore left {simS11.World.Player.Inventory.Get(BlockType.EmberOre)}");
simS11.World.Advance();
PrintS11("Slice 11 after doomed squad tick", simS11);
Console.WriteLine($"Slice 11 doomed squad alive: {simS11.World.Squads.Count == 3} (squads now {simS11.World.Squads.Count}, the dead squad is removed, not saved)");
var saveS11 = simS11.SaveToJson();
var simS11Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS11Loaded.LoadFromJson(saveS11)}");
PrintS11("Slice 11 after load", simS11Loaded);
Console.WriteLine($"Slice 11 loaded: squads {simS11Loaded.World.Squads.Count} (dead squad stays dead), can deploy more: {simS11Loaded.CanDeployOrbitalSquad()} (cap freed by the death), ore {simS11Loaded.World.Player.Inventory.Get(BlockType.EmberOre)}");
simS11Loaded.World.Advance();
PrintS11("Slice 11 loaded tick 1", simS11Loaded);
Console.WriteLine($"Slice 11 done: Heavy {simS11Loaded.World.Squads[0].Kind} {simS11Loaded.World.Squads[0].Doctrine} {simS11Loaded.World.Squads[0].Health} HP at core, Vanguard {simS11Loaded.World.Squads[1].Kind} {simS11Loaded.World.Squads[1].Doctrine} {simS11Loaded.World.Squads[1].Health} HP at rally, enemies left {simS11Loaded.World.Enemies.Count}, save JSON length: {saveS11.Length} chars");

// Slice 12 Step 3 scenario: Slice 12 composed in one run —
// the Step 1 display reading a live network with no consumer
// and a lone source, beside the Step 2 widening (an HV-rated
// Shock Pylon) running beside an existing LV consumer (a
// powered mill) and existing sources (Dynamo and Alternator),
// not three separate proofs. On the y=20 plane: a live
// network (Dynamo -> cable -> cable, free end, feeding
// nothing), an LV mill fed directly by a Dynamo with a
// five-stone pocket, and a lone Alternator with nothing
// adjacent to it. On the y=5 plane: a Shock Pylon fed HV
// directly by an Alternator, and a powered workstation fed
// LV through an Alternator -> transformer step-down, whose
// regen is player-centred (Manhattan distance 3) while the
// pylon's fire is machine-centred, so the demo parks the
// player far away for the pylon's three firing ticks and
// then walks to the bench to read its regen, and states
// that walk openly instead of pretending one position
// proves both. The enemy starts 5 from the pylon (its
// maximum range) and walks player-ward, so it stays in
// range while the pylon fires 10 per tick and dies on
// tick 3. The layout is staged directly with TrySet,
// exactly as the Step 1-2 tests stage theirs: aim
// placement of a cable, a mill, a pylon, a workstation,
// a transformer, a Dynamo, and an Alternator was each
// proved in its own step's tests, and walking this
// layout in by aim would prove walking, not wiring.
// What this demo cannot show, by construction, is any
// adapter feel or scene visibility: no P key press, no
// LivePowerText or PylonPowerText line, and no visible
// distinction between a live cable and a dead one appear
// in a headless run, so those claims stay in their
// Windows checklist items (22 for the live line, 23 for
// the P key), not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 12 Step 3 scenario — live network + lone source beside an HV pylon, an LV mill, and a powered workstation, save/load round-trip");
var simS12 = new Simulation(42);
void SeedPocketS12(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
simS12.World.Chunk.TrySet(14, 20, 10, BlockType.Dynamo);
simS12.World.Chunk.TrySet(13, 20, 10, BlockType.Cable);
simS12.World.Chunk.TrySet(12, 20, 10, BlockType.Cable);
simS12.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
simS12.World.Chunk.TrySet(11, 20, 14, BlockType.Dynamo);
SeedPocketS12(simS12.World, 10, 14);
simS12.World.Chunk.TrySet(10, 20, 18, BlockType.Alternator);
simS12.World.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
simS12.World.Chunk.TrySet(18, 5, 16, BlockType.Alternator);
simS12.World.Chunk.TrySet(17, 5, 28, BlockType.Workstation);
simS12.World.Chunk.TrySet(18, 5, 28, BlockType.Transformer);
simS12.World.Chunk.TrySet(19, 5, 28, BlockType.Alternator);
simS12.World.Player.TakeDamage(20);
simS12.World.Player.X = 10; simS12.World.Player.Z = 10;
simS12.World.Enemies.Add(new Enemy(17, 5, 21));
Console.WriteLine($"Slice 12 voltages before any tick: LV mill {simS12.World.DeliveredVoltage(10, 20, 14)}, HV pylon {simS12.World.DeliveredVoltage(17, 5, 16)}, LV workstation {simS12.World.DeliveredVoltage(17, 5, 28)}");
var s12HudBefore = simS12.Hud();
Console.WriteLine($"Slice 12 HUD before: {s12HudBefore.PoweredMills}/{s12HudBefore.TotalMills} MILLS POWERED, {s12HudBefore.PoweredWorkstations}/{s12HudBefore.TotalWorkstations} WORKSTATIONS POWERED, {s12HudBefore.PoweredPylons}/{s12HudBefore.TotalPylons} PYLONS POWERED, POWER: {s12HudBefore.LiveNetworks} LIVE NETWORKS, {s12HudBefore.LoneSources} LONE SOURCES (the live network and the lone source appear in no older count)");
Console.WriteLine($"Slice 12 shields before: {simS12.World.Player.Shields} (65 after 20 damage, no bench in range while parked at (10,5,10))");
for (int tick = 1; tick <= 3; tick++)
{
    simS12.World.Advance();
    Console.WriteLine($"Slice 12 tick {tick}: enemies {simS12.World.Enemies.Count}{(simS12.World.Enemies.Count > 0 ? $" at ({simS12.World.Enemies[0].X},5,{simS12.World.Enemies[0].Z}) {simS12.World.Enemies[0].Health} HP" : " (pylon kill, 10 per tick for 3 ticks)")}, powered gathered {simS12.World.PoweredBlocksGathered}, shields {simS12.World.Player.Shields} (no regen while parked far from the bench)");
}
var s12Walk = simS12.World.Player.MoveTo(16, 5, 28, simS12.World.Chunk);
Console.WriteLine($"Slice 12 walk to powered bench -> {s12Walk}, effective regen there: {simS12.World.EffectiveWorkstationRegen()} (base 5 + powered 5)");
for (int tick = 4; tick <= 5; tick++) simS12.World.Advance();
Console.WriteLine($"Slice 12 after 5 ticks: powered gathered {simS12.World.PoweredBlocksGathered} (pocket exhausted), destroyed {simS12.World.DestroyedMills}, stone {simS12.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS12.World.Player.Shields} (65 + 10 per bench tick, capped at 85)");
var saveS12 = simS12.SaveToJson();
var simS12Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS12Loaded.LoadFromJson(saveS12)}");
var s12LoadedHud = simS12Loaded.Hud();
Console.WriteLine($"Slice 12 after load: live network cable {simS12Loaded.World.Chunk.Get(12, 20, 10)}, lone Alternator {simS12Loaded.World.Chunk.Get(10, 20, 18)}, LV mill {simS12Loaded.World.Chunk.Get(10, 20, 14)}, pylon {simS12Loaded.World.Chunk.Get(17, 5, 16)}, workstation {simS12Loaded.World.Chunk.Get(17, 5, 28)}, power {s12LoadedHud.PoweredMills}/{s12LoadedHud.TotalMills} mills, {s12LoadedHud.PoweredWorkstations}/{s12LoadedHud.TotalWorkstations} workstations, {s12LoadedHud.PoweredPylons}/{s12LoadedHud.TotalPylons} pylons, POWER: {s12LoadedHud.LiveNetworks} LIVE NETWORKS, {s12LoadedHud.LoneSources} LONE SOURCES (counts re-derived, not saved), session counters reset: powered {simS12Loaded.World.PoweredBlocksGathered}, destroyed {simS12Loaded.World.DestroyedMills}, stone {simS12Loaded.World.Player.Inventory.Get(BlockType.Stone)}, player back at bench ({simS12Loaded.World.Player.X},{simS12Loaded.World.Player.Y},{simS12Loaded.World.Player.Z}), effective regen {simS12Loaded.World.EffectiveWorkstationRegen()}");
simS12Loaded.World.Player.TakeDamage(20);
SeedPocketS12(simS12Loaded.World, 10, 14);
for (int i = 0; i < 5; i++) simS12Loaded.World.Advance();
Console.WriteLine($"Slice 12 loaded phase after 5 ticks: powered gathered {simS12Loaded.World.PoweredBlocksGathered} (5 again), destroyed {simS12Loaded.World.DestroyedMills} (no new destruction), stone {simS12Loaded.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS12Loaded.World.Player.Shields} (65 + 10 per bench tick, capped at 85), enemies {simS12Loaded.World.Enemies.Count} (the pylon kill stays dead)");
Console.WriteLine($"Slice 12 done: powered {simS12.World.PoweredBlocksGathered} + loaded {simS12Loaded.World.PoweredBlocksGathered}, destroyed {simS12.World.DestroyedMills}, save JSON length: {saveS12.Length} chars");

// Slice 13 Step 4 scenario: Slice 13 composed in one run —
// the Step 1 warning reading a threatened mill, a threatened
// workstation, and a threatened pylon in the same Simulation,
// beside the Step 2 snapshot kinds (all three, paid, through
// the picker, HoldGround, deployed at the player and marching
// to their rallies) and the Step 3
// power-state query reading the same cells the warning reads
// — not three separate proofs. On the y=20 plane: a mill fed
// HV directly by an Alternator (threatened, destroyed on the
// first tick), a mill fed LV directly by a Dynamo with a
// five-stone pocket (powered, gathering), and a live cable
// run (Dynamo -> cable -> cable, free end, energised, feeding
// nothing). On the y=5 plane: a workstation fed HV directly
// by an Alternator (threatened, disabled, not destroyed), a
// workstation fed LV through an Alternator -> transformer
// step-down (powered), and a pylon fed LV directly by a Dynamo
// (threatened, disabled, not destroyed), with one enemy staged
// in the pylon's range so the disabled pylon's silence is a
// real non-fire, not an empty lane. The player spawns at
// (16,5,16), already beside the threatened workstation, so the
// first tick reads its disablement as regen 0 with no walk.
// After that tick the two surviving threatened machines are
// rewired — the workstation's Alternator swapped for a Dynamo,
// the pylon's Dynamo swapped for an Alternator — and their
// warnings clear on the next snapshot while the destroyed mill
// stays gone, which is the warning/tint agreement in action:
// a rewired machine loses its warning and its red tint
// together, a destroyed machine loses both because it is gone.
// The player then walks to the pylon lane while the rewired
// pylon proves its recovery by firing on the same enemy it
// could not fire at while threatened until the kill, and walks
// back to the rewired bench to read regen 10. The destroyed
// mill's Alternator becomes a lone source after the tick, so
// the live counts read 1 LIVE NETWORK and 1 LONE SOURCE after
// load where they read 1 and 0 before the tick — an orphaned
// source, not a new source. The layout is staged directly with TrySet,
// exactly as the Step 1-3 tests stage theirs: aim placement of
// a mill, a workstation, a pylon, a cable, a transformer, a
// Dynamo, and an Alternator was each proved in its own step's
// tests, and walking this layout in by aim would prove walking,
// not wiring. What this demo cannot show, by construction, is
// any adapter feel or scene visibility: no WarningText line,
// no per-kind marker shape, no mesh tint, and no refresh event
// appear in a headless run, so those claims stay in their
// Windows checklist items (24 for the warning line, 25 for the
// markers, 26 for the tint, 27 for the refresh), not in the
// numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 13 Step 4 scenario — warned mill, workstation, and pylon beside three squad kinds and the power-state query, save/load round-trip");
var simS13 = new Simulation(42);
simS13.World.Player.Inventory.Add(BlockType.EmberOre, 30);
simS13.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
simS13.OrbitalCommand.SelectKind(SquadKind.Vanguard);
simS13.BeginOrbitalPick();
simS13.SetOrbitalRally(5, 5, 5);
var s13Van = simS13.ConfirmOrbitalDeploy();
simS13.OrbitalCommand.SelectKind(SquadKind.Recon);
simS13.BeginOrbitalPick();
simS13.SetOrbitalRally(5, 5, 25);
var s13Rec = simS13.ConfirmOrbitalDeploy();
simS13.OrbitalCommand.SelectKind(SquadKind.Heavy);
simS13.BeginOrbitalPick();
simS13.SetOrbitalRally(25, 5, 5);
var s13Heavy = simS13.ConfirmOrbitalDeploy();
Console.WriteLine($"Paid squads deployed: Vanguard {(s13Van == null ? "REJECTED" : "HoldGround, rally (5,5,5)")}, Recon {(s13Rec == null ? "REJECTED" : "HoldGround, rally (5,5,25)")}, Heavy {(s13Heavy == null ? "REJECTED" : "HoldGround, rally (25,5,5)")}, ore left {simS13.World.Player.Inventory.Get(BlockType.EmberOre)} (squads deploy at the player, (16,5,16), and march to their rallies)");
void SeedPocketS13(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
simS13.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
simS13.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
simS13.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
simS13.World.Chunk.TrySet(11, 20, 14, BlockType.Dynamo);
SeedPocketS13(simS13.World, 10, 14);
simS13.World.Chunk.TrySet(14, 20, 18, BlockType.Dynamo);
simS13.World.Chunk.TrySet(13, 20, 18, BlockType.Cable);
simS13.World.Chunk.TrySet(12, 20, 18, BlockType.Cable);
simS13.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
simS13.World.Chunk.TrySet(18, 5, 16, BlockType.Alternator);
simS13.World.Chunk.TrySet(17, 5, 28, BlockType.Workstation);
simS13.World.Chunk.TrySet(18, 5, 28, BlockType.Transformer);
simS13.World.Chunk.TrySet(19, 5, 28, BlockType.Alternator);
simS13.World.Chunk.TrySet(10, 5, 16, BlockType.ShockPylon);
simS13.World.Chunk.TrySet(11, 5, 16, BlockType.Dynamo);
simS13.World.Player.TakeDamage(20);
simS13.World.Enemies.Add(new Enemy(10, 5, 19));
Console.WriteLine($"Slice 13 voltages before any tick: threatened mill {simS13.World.DeliveredVoltage(10, 20, 10)}, powered mill {simS13.World.DeliveredVoltage(10, 20, 14)}, cable {simS13.World.DeliveredVoltage(12, 20, 18)}, threatened workstation {simS13.World.DeliveredVoltage(17, 5, 16)}, powered workstation {simS13.World.DeliveredVoltage(17, 5, 28)}, threatened pylon {simS13.World.DeliveredVoltage(10, 5, 16)}");
var s13HudBefore = simS13.Hud();
Console.WriteLine($"Slice 13 HUD before: {s13HudBefore.PoweredMills}/{s13HudBefore.TotalMills} MILLS POWERED, {s13HudBefore.PoweredWorkstations}/{s13HudBefore.TotalWorkstations} WORKSTATIONS POWERED, {s13HudBefore.PoweredPylons}/{s13HudBefore.TotalPylons} PYLONS POWERED, WARNED: {s13HudBefore.WarnedMills} MILLS, {s13HudBefore.WarnedWorkstations} WORKSTATIONS, {s13HudBefore.WarnedPylons} PYLONS, POWER: {s13HudBefore.LiveNetworks} LIVE NETWORKS, {s13HudBefore.LoneSources} LONE SOURCES");
Console.WriteLine($"Slice 13 power states before: threatened mill {simS13.World.PowerState(10, 20, 10)}, powered mill {simS13.World.PowerState(10, 20, 14)}, cable {simS13.World.PowerState(12, 20, 18)}, threatened workstation {simS13.World.PowerState(17, 5, 16)}, powered workstation {simS13.World.PowerState(17, 5, 28)}, threatened pylon {simS13.World.PowerState(10, 5, 16)}, Dynamo {simS13.World.PowerState(11, 20, 14)}, transformer {simS13.World.PowerState(18, 5, 28)} (sources and transformers stay None by decision)");
foreach (var sq in OrbitalView.Snapshot(simS13.World))
    Console.WriteLine($"Slice 13 snapshot before: {sq.Kind} {sq.Doctrine} at ({sq.X},5,{sq.Z}) rally ({sq.RallyX},5,{sq.RallyZ}) reads {sq.Activity}");
Console.WriteLine($"Slice 13 shields before: {simS13.World.Player.Shields} (65 after 20 damage, parked beside the threatened workstation at (16,5,16)), enemy at (10,5,19) 30 HP, in the threatened pylon's range");
simS13.World.Advance();
var s13HudAfter1 = simS13.Hud();
Console.WriteLine($"Slice 13 tick 1: threatened mill cell now {simS13.World.Chunk.Get(10, 20, 10)} (destroyed), destroyed {simS13.World.DestroyedMills}, powered gathered {simS13.World.PoweredBlocksGathered}, WARNED now {s13HudAfter1.WarnedMills} MILLS, {s13HudAfter1.WarnedWorkstations} WORKSTATIONS, {s13HudAfter1.WarnedPylons} PYLONS (the mill's warning is gone because the mill is gone; the two disablements still warn), shields {simS13.World.Player.Shields} (regen 0 at the disabled bench), enemy at ({simS13.World.Enemies[0].X},5,{simS13.World.Enemies[0].Z}) {simS13.World.Enemies[0].Health} HP (the disabled pylon fired nothing)");
simS13.World.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
simS13.World.Chunk.TrySet(11, 5, 16, BlockType.Alternator);
var s13HudRewired = simS13.Hud();
Console.WriteLine($"Slice 13 rewired: workstation source Alternator -> Dynamo, pylon source Dynamo -> Alternator; WARNED now {s13HudRewired.WarnedMills} MILLS, {s13HudRewired.WarnedWorkstations} WORKSTATIONS, {s13HudRewired.WarnedPylons} PYLONS, power {s13HudRewired.PoweredWorkstations}/{s13HudRewired.TotalWorkstations} WORKSTATIONS, {s13HudRewired.PoweredPylons}/{s13HudRewired.TotalPylons} PYLONS, states workstation {simS13.World.PowerState(17, 5, 16)}, pylon {simS13.World.PowerState(10, 5, 16)} (warning and tint agree: both clear together)");
var s13WalkLane = simS13.World.Player.MoveTo(10, 5, 10, simS13.World.Chunk);
Console.WriteLine($"Slice 13 walk to pylon lane -> {s13WalkLane}, player at (10,5,10), enemy stays in the rewired pylon's range");
for (int tick = 2; tick <= 5; tick++)
{
    simS13.World.Advance();
    Console.WriteLine($"Slice 13 tick {tick}: enemies {simS13.World.Enemies.Count}{(simS13.World.Enemies.Count > 0 ? $" at ({simS13.World.Enemies[0].X},5,{simS13.World.Enemies[0].Z}) {simS13.World.Enemies[0].Health} HP" : " (rewired pylon kill)")} , powered gathered {simS13.World.PoweredBlocksGathered}, shields {simS13.World.Player.Shields} (no bench in range on the pylon lane)");
}
var s13WalkBench = simS13.World.Player.MoveTo(16, 5, 16, simS13.World.Chunk);
Console.WriteLine($"Slice 13 walk back to rewired bench -> {s13WalkBench}, effective regen there: {simS13.World.EffectiveWorkstationRegen()} (base 5 + powered 5)");
Console.WriteLine($"Slice 13 after 5 ticks: powered gathered {simS13.World.PoweredBlocksGathered} (pocket exhausted), destroyed {simS13.World.DestroyedMills}, stone {simS13.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS13.World.Player.Shields}, enemies {simS13.World.Enemies.Count}");
var saveS13 = simS13.SaveToJson();
var simS13Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS13Loaded.LoadFromJson(saveS13)}");
var s13LoadedHud = simS13Loaded.Hud();
Console.WriteLine($"Slice 13 after load: threatened mill cell {simS13Loaded.World.Chunk.Get(10, 20, 10)} (still gone, no resurrection), powered mill {simS13Loaded.World.Chunk.Get(10, 20, 14)}, cable {simS13Loaded.World.Chunk.Get(12, 20, 18)}, rewired workstation {simS13Loaded.World.Chunk.Get(17, 5, 16)}, rewired pylon {simS13Loaded.World.Chunk.Get(10, 5, 16)}, power {s13LoadedHud.PoweredMills}/{s13LoadedHud.TotalMills} mills, {s13LoadedHud.PoweredWorkstations}/{s13LoadedHud.TotalWorkstations} workstations, {s13LoadedHud.PoweredPylons}/{s13LoadedHud.TotalPylons} pylons, WARNED {s13LoadedHud.WarnedMills}/{s13LoadedHud.WarnedWorkstations}/{s13LoadedHud.WarnedPylons}, POWER: {s13LoadedHud.LiveNetworks} LIVE NETWORKS, {s13LoadedHud.LoneSources} LONE SOURCES, states mill {simS13Loaded.World.PowerState(10, 20, 14)}, workstation {simS13Loaded.World.PowerState(17, 5, 16)}, pylon {simS13Loaded.World.PowerState(10, 5, 16)}, cable {simS13Loaded.World.PowerState(12, 20, 18)} (all re-derived, not saved), session counters reset: powered {simS13Loaded.World.PoweredBlocksGathered}, destroyed {simS13Loaded.World.DestroyedMills}, stone {simS13Loaded.World.Player.Inventory.Get(BlockType.Stone)}, player back at bench ({simS13Loaded.World.Player.X},{simS13Loaded.World.Player.Y},{simS13Loaded.World.Player.Z}), effective regen {simS13Loaded.World.EffectiveWorkstationRegen()}");
foreach (var sq in OrbitalView.Snapshot(simS13Loaded.World))
    Console.WriteLine($"Slice 13 snapshot after load: {sq.Kind} {sq.Doctrine} at ({sq.X},5,{sq.Z}) reads {sq.Activity}");
simS13Loaded.World.Player.TakeDamage(20);
SeedPocketS13(simS13Loaded.World, 10, 14);
for (int i = 0; i < 5; i++) simS13Loaded.World.Advance();
Console.WriteLine($"Slice 13 loaded phase after 5 ticks: powered gathered {simS13Loaded.World.PoweredBlocksGathered} (5 again), destroyed {simS13Loaded.World.DestroyedMills} (no new destruction), stone {simS13Loaded.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS13Loaded.World.Player.Shields} (65 + 10 per bench tick, capped at 85), enemies {simS13Loaded.World.Enemies.Count} (the pylon kill stays dead)");
Console.WriteLine($"Slice 13 done: powered {simS13.World.PoweredBlocksGathered} + loaded {simS13Loaded.World.PoweredBlocksGathered}, destroyed {simS13.World.DestroyedMills}, save JSON length: {saveS13.Length} chars");

// Slice 14 Step 2 scenario: Slice 14 composed in one run —
// the third source beside the sources the reader already
// trusts, with a mismatch and an idle case beside them, so
// the demo proves a distinction, not a label. On the y=20
// plane: a mill fed HV through a Conduit -> cable -> cable
// run (destroyed on tick 1, the cable-only delivery in
// action); a mill fed LV directly by a Dynamo with a
// five-stone pocket (powered, the existing source at its
// voltage); and a mill standing directly beside a Conduit
// with no cable (reads None, warns nothing, survives, and
// falls back to kinetic cadence — the idle case that proves
// the Conduit delivers nothing face-adjacent). On the y=5
// plane: a Shock Pylon fed HV through a single Conduit ->
// cable run (powered, and it kills the staged enemy in three
// ticks of 10 damage — and, since Slice 17 Step 1 (0.74.0),
// its only cable cell reads Energised, because PowerState
// reads the component's voltage rather than the cell's own
// delivered voltage; before that step the cell read None
// while the network was live, the carried single-cable
// tint finding); and a workstation fed LV through a Conduit ->
// cable -> transformer step-down beside the player spawn,
// so the powered bench regen reads 10 from the first tick.
// The layout is staged directly with TrySet, exactly as the
// Step 1 tests stage theirs: aim placement of a Conduit was
// proved in Step 1's tests, and walking this layout in by
// aim would prove walking, not wiring. What this demo
// cannot show, by construction, is any adapter feel: no
// M-key build, no mesh tint, and no scene visibility appear
// in a headless run, so the M key stays in playtest-gate
// checklist item 28 (Windows-only) and the single-cable
// tint stays a review remainder, not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 14 Step 2 scenario — Conduit cable-fed mill destroyed, Dynamo mill powered, direct-Conduit mill kinetic, Conduit cable pylon firing, Conduit step-down bench, save/load round-trip");
var simS14 = new Simulation(42);
void SeedPocketS14(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
simS14.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
simS14.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
simS14.World.Chunk.TrySet(12, 20, 10, BlockType.Cable);
simS14.World.Chunk.TrySet(13, 20, 10, BlockType.Conduit);
simS14.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
simS14.World.Chunk.TrySet(11, 20, 14, BlockType.Dynamo);
SeedPocketS14(simS14.World, 10, 14);
simS14.World.Chunk.TrySet(10, 20, 18, BlockType.KineticMill);
simS14.World.Chunk.TrySet(11, 20, 18, BlockType.Conduit);
SeedPocketS14(simS14.World, 10, 18);
simS14.World.Chunk.TrySet(10, 5, 16, BlockType.ShockPylon);
simS14.World.Chunk.TrySet(11, 5, 16, BlockType.Cable);
simS14.World.Chunk.TrySet(12, 5, 16, BlockType.Conduit);
simS14.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
simS14.World.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
simS14.World.Chunk.TrySet(19, 5, 16, BlockType.Cable);
simS14.World.Chunk.TrySet(20, 5, 16, BlockType.Conduit);
simS14.World.Player.TakeDamage(20);
simS14.World.Enemies.Add(new Enemy(10, 5, 17));
Console.WriteLine($"Slice 14 voltages before any tick: Conduit cable mill {simS14.World.DeliveredVoltage(10, 20, 10)}, Dynamo mill {simS14.World.DeliveredVoltage(10, 20, 14)}, direct-Conduit mill {simS14.World.DeliveredVoltage(10, 20, 18)}, Conduit cable pylon {simS14.World.DeliveredVoltage(10, 5, 16)}, Conduit step-down workstation {simS14.World.DeliveredVoltage(17, 5, 16)}");
var s14HudBefore = simS14.Hud();
Console.WriteLine($"Slice 14 HUD before: {s14HudBefore.PoweredMills}/{s14HudBefore.TotalMills} MILLS POWERED, {s14HudBefore.PoweredWorkstations}/{s14HudBefore.TotalWorkstations} WORKSTATIONS POWERED, {s14HudBefore.PoweredPylons}/{s14HudBefore.TotalPylons} PYLONS POWERED, WARNED: {s14HudBefore.WarnedMills} MILLS, {s14HudBefore.WarnedWorkstations} WORKSTATIONS, {s14HudBefore.WarnedPylons} PYLONS, POWER: {s14HudBefore.LiveNetworks} LIVE NETWORKS, {s14HudBefore.LoneSources} LONE SOURCES (1 LONE since Slice 17 Step 2: the direct Conduit beside its mill now counts as lone, because it feeds no consumer directly — the Slice 12 reading, under which it counted neither, is superseded)");
Console.WriteLine($"Slice 14 power states before: Conduit cable mill {simS14.World.PowerState(10, 20, 10)}, its cables {simS14.World.PowerState(11, 20, 10)}/{simS14.World.PowerState(12, 20, 10)} (two-cable run: both Energised), Dynamo mill {simS14.World.PowerState(10, 20, 14)}, direct-Conduit mill {simS14.World.PowerState(10, 20, 18)}, pylon {simS14.World.PowerState(10, 5, 16)}, its single cable {simS14.World.PowerState(11, 5, 16)} (Energised since Slice 17 Step 1: component voltage, not cell delivery), step-down workstation {simS14.World.PowerState(17, 5, 16)}, its cable {simS14.World.PowerState(19, 5, 16)}, transformer {simS14.World.PowerState(18, 5, 16)} (sources and transformers stay None by decision)");
Console.WriteLine($"Slice 14 shields before: {simS14.World.Player.Shields} (65 after 20 damage, parked beside the powered step-down bench at (16,5,16), effective regen {simS14.World.EffectiveWorkstationRegen()}), enemy at (10,5,17) 30 HP, in the Conduit-fed pylon's range");
simS14.World.Advance();
var s14HudAfter1 = simS14.Hud();
Console.WriteLine($"Slice 14 tick 1: Conduit cable mill cell now {simS14.World.Chunk.Get(10, 20, 10)} (destroyed), destroyed {simS14.World.DestroyedMills}, powered gathered {simS14.World.PoweredBlocksGathered}, kinetic gathered {simS14.World.KineticBlocksGathered}, direct-Conduit mill still {simS14.World.Chunk.Get(10, 20, 18)} (survives: it was never fed), WARNED now {s14HudAfter1.WarnedMills} MILLS, {s14HudAfter1.WarnedWorkstations} WORKSTATIONS, {s14HudAfter1.WarnedPylons} PYLONS, POWER now {s14HudAfter1.LiveNetworks} LIVE NETWORKS, {s14HudAfter1.LoneSources} LONE SOURCES (the destroyed mill's cable run now feeds nothing, so it counts live; the direct Conduit still counts 1 lone, as before the tick, since Slice 17 Step 2), shields {simS14.World.Player.Shields} (regen 10 at the powered bench), enemy at ({simS14.World.Enemies[0].X},5,{simS14.World.Enemies[0].Z}) {simS14.World.Enemies[0].Health} HP (the Conduit-fed pylon fired 10; its single cable reads {simS14.World.PowerState(11, 5, 16)}, Energised since Slice 17 Step 1)");
for (int tick = 2; tick <= 5; tick++)
{
    simS14.World.Advance();
    Console.WriteLine($"Slice 14 tick {tick}: enemies {simS14.World.Enemies.Count}{(simS14.World.Enemies.Count > 0 ? $" at ({simS14.World.Enemies[0].X},5,{simS14.World.Enemies[0].Z}) {simS14.World.Enemies[0].Health} HP" : " (Conduit-fed pylon kill)")} , powered gathered {simS14.World.PoweredBlocksGathered}, kinetic gathered {simS14.World.KineticBlocksGathered}, shields {simS14.World.Player.Shields}");
}
Console.WriteLine($"Slice 14 after 5 ticks: powered gathered {simS14.World.PoweredBlocksGathered} (Dynamo pocket exhausted), kinetic gathered {simS14.World.KineticBlocksGathered} (direct-Conduit mill, 1 in 5 ticks), destroyed {simS14.World.DestroyedMills}, stone {simS14.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS14.World.Player.Shields}, enemies {simS14.World.Enemies.Count}");
var saveS14 = simS14.SaveToJson();
var simS14Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS14Loaded.LoadFromJson(saveS14)}");
var s14LoadedHud = simS14Loaded.Hud();
Console.WriteLine($"Slice 14 after load: Conduit cable mill cell {simS14Loaded.World.Chunk.Get(10, 20, 10)} (still gone, no resurrection), Dynamo mill {simS14Loaded.World.Chunk.Get(10, 20, 14)}, direct-Conduit mill {simS14Loaded.World.Chunk.Get(10, 20, 18)}, pylon {simS14Loaded.World.Chunk.Get(10, 5, 16)}, step-down workstation {simS14Loaded.World.Chunk.Get(17, 5, 16)}, power {s14LoadedHud.PoweredMills}/{s14LoadedHud.TotalMills} mills, {s14LoadedHud.PoweredWorkstations}/{s14LoadedHud.TotalWorkstations} workstations, {s14LoadedHud.PoweredPylons}/{s14LoadedHud.TotalPylons} pylons, WARNED {s14LoadedHud.WarnedMills}/{s14LoadedHud.WarnedWorkstations}/{s14LoadedHud.WarnedPylons}, POWER: {s14LoadedHud.LiveNetworks} LIVE NETWORKS, {s14LoadedHud.LoneSources} LONE SOURCES (the direct Conduit still counts 1 lone after load, since Slice 17 Step 2: the decided count re-derives, it is not a saved number), states Dynamo mill {simS14Loaded.World.PowerState(10, 20, 14)}, direct-Conduit mill {simS14Loaded.World.PowerState(10, 20, 18)}, pylon {simS14Loaded.World.PowerState(10, 5, 16)}, single cable {simS14Loaded.World.PowerState(11, 5, 16)} (Energised after load: the decided reading re-derives, it is not a stale tint), workstation {simS14Loaded.World.PowerState(17, 5, 16)} (all re-derived, not saved), session counters reset: powered {simS14Loaded.World.PoweredBlocksGathered}, kinetic {simS14Loaded.World.KineticBlocksGathered}, destroyed {simS14Loaded.World.DestroyedMills}, stone {simS14Loaded.World.Player.Inventory.Get(BlockType.Stone)}, player back at bench ({simS14Loaded.World.Player.X},{simS14Loaded.World.Player.Y},{simS14Loaded.World.Player.Z}), effective regen {simS14Loaded.World.EffectiveWorkstationRegen()}");
simS14Loaded.World.Player.TakeDamage(20);
SeedPocketS14(simS14Loaded.World, 10, 14);
SeedPocketS14(simS14Loaded.World, 10, 18);
for (int i = 0; i < 5; i++) simS14Loaded.World.Advance();
Console.WriteLine($"Slice 14 loaded phase after 5 ticks: powered gathered {simS14Loaded.World.PoweredBlocksGathered} (5 again), kinetic gathered {simS14Loaded.World.KineticBlocksGathered} (1 again), destroyed {simS14Loaded.World.DestroyedMills} (no new destruction), stone {simS14Loaded.World.Player.Inventory.Get(BlockType.Stone)}, shields {simS14Loaded.World.Player.Shields}, enemies {simS14Loaded.World.Enemies.Count} (the pylon kill stays dead)");
Console.WriteLine($"Slice 14 done: powered {simS14.World.PoweredBlocksGathered} + loaded {simS14Loaded.World.PoweredBlocksGathered}, kinetic {simS14.World.KineticBlocksGathered} + loaded {simS14Loaded.World.KineticBlocksGathered}, destroyed {simS14.World.DestroyedMills}, save JSON length: {saveS14.Length} chars");

// Slice 15 Step 2 scenario: Slice 15 composed in one run —
// the new doctrine walking its route beside an existing
// doctrine doing its own job, with an enemy encounter on
// the route so the demo proves engagement, not just walking.
// A Recon Patrol (range 5, 12 damage per tick, so a 30-HP
// enemy takes three hits, not one — a Vanguard kill in a
// single tick would not show firing while still walking)
// deploys paid through the picker to rally (20,5,20), and
// a Heavy HoldGround deploys paid through the picker to
// rally (10,5,10). Both deploy at the player (16,5,16):
// the patrol marches to its rally and then walks its loop
// (rally, 3 east, 3 south, 3 west, 3 north, repeat), while
// the Heavy marches to its rally and holds there, firing
// only once holding — the contrast that keeps Patrol from
// collapsing into HoldGround. One enemy is staged east of
// the loop at (26,5,20): it walks toward the nearest squad
// or the player, so it meets the patrol on the route, is
// shot in passing without being chased, and the patrol is
// still on route on the next tick. The layout is staged
// directly only for the enemy (TrySet staging does not
// apply to squads, which deploy through the picker, as in
// Slice 13); no electrical layout is staged, because this
// demo claims orbital composition, not wiring. What this
// demo cannot show, by construction, is any adapter feel:
// no Alpha7 key press, no kind marker shape, and no scene
// visibility appear in a headless run, so the Alpha7 key
// stays in playtest-gate checklist item 29 and
// orbital-command checklist item 13 (both Windows-only),
// not in the numbers below.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 15 Step 2 scenario — Recon Patrol walking its rally loop and firing in passing beside a Heavy HoldGround, save/load round-trip");
var simS15 = new Simulation(42);
simS15.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS15.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
simS15.OrbitalCommand.SelectKind(SquadKind.Recon);
simS15.BeginOrbitalPick();
simS15.SetOrbitalRally(20, 5, 20);
var s15Patrol = simS15.ConfirmOrbitalDeploy();
simS15.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
simS15.OrbitalCommand.SelectKind(SquadKind.Heavy);
simS15.BeginOrbitalPick();
simS15.SetOrbitalRally(10, 5, 10);
var s15Heavy = simS15.ConfirmOrbitalDeploy();
simS15.World.Enemies.Add(new Enemy(26, 5, 20));
Console.WriteLine($"Paid squads deployed: Recon Patrol {(s15Patrol == null ? "REJECTED" : "rally (20,5,20)")}, Heavy HoldGround {(s15Heavy == null ? "REJECTED" : "rally (10,5,10)")}, ore left {simS15.World.Player.Inventory.Get(BlockType.EmberOre)} (squads deploy at the player, (16,5,16), and march to their rallies)");
foreach (var sq in OrbitalView.Snapshot(simS15.World))
    Console.WriteLine($"Slice 15 snapshot before: {sq.Kind} {sq.Doctrine} at ({sq.X},5,{sq.Z}) rally ({sq.RallyX},5,{sq.RallyZ}) reads {sq.Activity}");
Console.WriteLine($"Slice 15 enemy before: at (26,5,20) 30 HP, east of the patrol loop");
for (int tick = 1; tick <= 32; tick++)
{
    simS15.World.Advance();
    var patrol = simS15.World.Squads[0];
    var heavy = simS15.World.Squads[1];
    var snaps = OrbitalView.Snapshot(simS15.World);
    Console.WriteLine($"Slice 15 tick {tick}: patrol at ({patrol.X},5,{patrol.Z}) waypoint {patrol.PatrolWaypointIndex} target ({patrol.PatrolTarget.X},5,{patrol.PatrolTarget.Z}) reads {snaps[0].Activity}, heavy at ({heavy.X},5,{heavy.Z}) reads {snaps[1].Activity}, enemies {simS15.World.Enemies.Count}{(simS15.World.Enemies.Count > 0 ? $" at ({simS15.World.Enemies[0].X},5,{simS15.World.Enemies[0].Z}) {simS15.World.Enemies[0].Health} HP" : " (enemy dead)")}");
}
Console.WriteLine($"Slice 15 after 32 ticks: patrol at ({simS15.World.Squads[0].X},5,{simS15.World.Squads[0].Z}) waypoint {simS15.World.Squads[0].PatrolWaypointIndex}, heavy at ({simS15.World.Squads[1].X},5,{simS15.World.Squads[1].Z}) holding its rally, enemies {simS15.World.Enemies.Count}");
var saveS15 = simS15.SaveToJson();
var simS15Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS15Loaded.LoadFromJson(saveS15)}");
foreach (var sq in OrbitalView.Snapshot(simS15Loaded.World))
    Console.WriteLine($"Slice 15 snapshot after load: {sq.Kind} {sq.Doctrine} at ({sq.X},5,{sq.Z}) rally ({sq.RallyX},5,{sq.RallyZ}) reads {sq.Activity}");
Console.WriteLine($"Slice 15 loaded patrol waypoint index: {simS15Loaded.World.Squads[0].PatrolWaypointIndex} (reset to 0 on load: the patrol was saved at its rally, so the reset is proved by the first loaded tick turning east — a persisted waypoint 4 would have stepped north instead)");
for (int tick = 1; tick <= 8; tick++)
{
    simS15Loaded.World.Advance();
    var patrol = simS15Loaded.World.Squads[0];
    var heavy = simS15Loaded.World.Squads[1];
    var snaps = OrbitalView.Snapshot(simS15Loaded.World);
    Console.WriteLine($"Slice 15 loaded tick {tick}: patrol at ({patrol.X},5,{patrol.Z}) waypoint {patrol.PatrolWaypointIndex} target ({patrol.PatrolTarget.X},5,{patrol.PatrolTarget.Z}) reads {snaps[0].Activity}, heavy at ({heavy.X},5,{heavy.Z}) reads {snaps[1].Activity}, enemies {simS15Loaded.World.Enemies.Count}");
}
Console.WriteLine($"Slice 15 done: patrol {simS15.World.Squads[0].Kind} {simS15.World.Squads[0].Doctrine} + loaded patrol, heavy holding, save JSON length: {saveS15.Length} chars");
