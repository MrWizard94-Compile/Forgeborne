using Forgeborne.Core;

// Minimal test runner, no external packages: each Check must pass or the
// process exits non-zero with the failing test name. Run: dotnet run
int passed = 0, failed = 0;
void Check(bool condition, string name)
{
    if (condition) { passed++; Console.WriteLine($"PASS {name}"); }
    else { failed++; Console.WriteLine($"FAIL {name}"); }
}
void Eq<T>(T expected, T actual, string name) => Check(EqualityComparer<T>.Default.Equals(expected, actual), $"{name} (expected {expected}, got {actual})");

// Chunk
var chunk = new VoxelChunk();
Check(chunk.TrySet(1, 1, 1, BlockType.Stone), "chunk set in bounds");
Eq(BlockType.Stone, chunk.Get(1, 1, 1), "chunk get");
Check(!chunk.TrySet(-1, 0, 0, BlockType.Stone), "chunk set out of bounds fails");
Eq(BlockType.Air, chunk.Get(99, 99, 99), "chunk get out of bounds is air");
Eq(VoxelChunk.Size * VoxelChunk.Size * VoxelChunk.Size, VoxelChunk.Volume, "chunk volume 32^3");

// Deterministic generation
var a = VoxelChunk.Generate(42);
var b = VoxelChunk.Generate(42);
var c = VoxelChunk.Generate(43);
Check(a.ToBytes().SequenceEqual(b.ToBytes()), "generation same seed equal");
Check(!a.ToBytes().SequenceEqual(c.ToBytes()), "generation different seed differs");
Eq(BlockType.Bedrock, a.Get(5, 0, 5), "generation bedrock layer");
Check(a.CountSolid() > 3000, "generation has solid ground");

// World: gather
var world = new WorldZone(42);
// Find a stone block in the generated ground.
int sx = -1, sy = -1, sz = -1;
for (int x = 0; x < 32 && sx < 0; x++)
for (int y = 1; y <= 3 && sx < 0; y++)
for (int z = 0; z < 32 && sx < 0; z++)
    if (world.Chunk.Get(x, y, z) == BlockType.Stone) { sx = x; sy = y; sz = z; }
Check(sx >= 0, "found stone to gather");
Eq(GatherResult.Success, world.Gather(sx, sy, sz), "gather stone succeeds");
Eq(BlockType.Air, world.Chunk.Get(sx, sy, sz), "gathered block becomes air");
Eq(1, world.Player.Inventory.Get(BlockType.Stone), "gather gives 1 stone");
Eq(GatherResult.Air, world.Gather(sx, sy, sz), "gather air fails");
Eq(GatherResult.OutOfBounds, world.Gather(-1, 0, 0), "gather out of bounds fails");
Eq(GatherResult.NotGatherable, world.Gather(5, 0, 5), "gather bedrock fails");

// Build: wall needs 2 stone, adjacent support, air target
world.Player.Inventory.Add(BlockType.Stone, 10);
// Place a wall directly above ground (y=4) adjacent to ground below (y=3 solid, unless that cell was gathered — use a fresh cell).
int bx = 10, bz = 10;
Eq(BuildResult.Success, world.Build(bx, 4, bz, BlockType.Wall), "build wall above ground succeeds");
Eq(BlockType.Wall, world.Chunk.Get(bx, 4, bz), "wall placed");
Eq(BuildResult.NotAir, world.Build(bx, 4, bz, BlockType.Wall), "build on solid fails");
Eq(BuildResult.Floating, world.Build(20, 20, 20, BlockType.Wall), "build floating fails");
Eq(BuildResult.OutOfBounds, world.Build(-1, 0, 0, BlockType.Wall), "build out of bounds fails");
Eq(BuildResult.InvalidType, world.Build(11, 4, 11, BlockType.Stone), "build invalid type fails");
// Insufficient resources: drain stone, then try again on a new supported cell.
while (world.Player.Inventory.Remove(BlockType.Stone, 1)) { }
Eq(BuildResult.InsufficientResources, world.Build(12, 4, 12, BlockType.Wall), "build without stone fails");

// Workstation: needs 10 stone + 5 ore. Gather ore first from a fresh world.
var w2 = new WorldZone(7);
int ox = -1, oy = -1, oz = -1;
for (int x = 0; x < 32 && ox < 0; x++)
for (int y = 1; y <= 3 && ox < 0; y++)
for (int z = 0; z < 32 && ox < 0; z++)
    if (w2.Chunk.Get(x, y, z) == BlockType.EmberOre) { ox = x; oy = y; oz = z; }
Check(ox >= 0, "found ore for workstation test");
if (ox >= 0)
{
    // Gather ore blocks until we have 5, plus stone until 10, by gathering in a scan.
    for (int x = 0; x < 32; x++)
    for (int y = 1; y <= 3; y++)
    for (int z = 0; z < 32; z++)
    {
        if (w2.Player.Inventory.Get(BlockType.EmberOre) >= 5 && w2.Player.Inventory.Get(BlockType.Stone) >= 10) break;
        w2.Gather(x, y, z);
    }
    Check(w2.Player.Inventory.Get(BlockType.EmberOre) >= 5 && w2.Player.Inventory.Get(BlockType.Stone) >= 10, "gathered workstation costs");
    Eq(BuildResult.Success, w2.Build(15, 4, 15, BlockType.Workstation), "build workstation succeeds");
    Eq(BlockType.Workstation, w2.Chunk.Get(15, 4, 15), "workstation placed");
}

// Damage: shields first, then health, death
var p = new Player(16, 5, 16);
p.TakeDamage(50);
Eq(35, p.Shields, "damage hits shields first");
Eq(100, p.Health, "health untouched while shields remain");
p.TakeDamage(50);
Eq(0, p.Shields, "shields depleted");
Eq(85, p.Health, "health after shields depleted");
p.TakeDamage(200);
Eq(0, p.Health, "health clamped at zero");
Check(p.IsDead, "player dead at zero health");

// Enemy movement and attack
var w3 = new WorldZone(99);
w3.Player.X = 16; w3.Player.Y = 5; w3.Player.Z = 16;
var enemy = new Enemy(16, 5, 20);
w3.Enemies.Add(enemy);
int distBefore = Math.Abs(w3.Player.X - enemy.X) + Math.Abs(w3.Player.Y - enemy.Y) + Math.Abs(w3.Player.Z - enemy.Z);
w3.Advance();
int distAfter = Math.Abs(w3.Player.X - enemy.X) + Math.Abs(w3.Player.Y - enemy.Y) + Math.Abs(w3.Player.Z - enemy.Z);
Check(distAfter < distBefore, "enemy moves toward player");
w3.AttackEnemy(enemy, 30);
Check(enemy.IsDead, "enemy dies at zero health");
Check(!w3.Enemies.Contains(enemy), "dead enemy removed");

// Save / load round-trip
var w4 = new WorldZone(123);
w4.Gather(sx, sy, sz); // may be air/stone depending on seed; do a deterministic gather scan instead
for (int x = 0; x < 32; x++)
for (int y = 1; y <= 3; y++)
for (int z = 0; z < 32; z++) w4.Gather(x, y, z);
w4.Player.TakeDamage(10);
w4.SpawnWave(2);
var json = SaveSystem.SaveZone(w4);
var loaded = SaveSystem.LoadZone(json);
Check(loaded.Chunk.ToBytes().SequenceEqual(w4.Chunk.ToBytes()), "save/load chunk equal");
Eq(w4.Player.Health, loaded.Player.Health, "save/load health equal");
Eq(w4.Player.Shields, loaded.Player.Shields, "save/load shields equal");
Eq(w4.Player.Inventory.Get(BlockType.Stone), loaded.Player.Inventory.Get(BlockType.Stone), "save/load stone equal");
Eq(w4.Enemies.Count, loaded.Enemies.Count, "save/load enemy count equal");
Eq(w4.Seed, loaded.Seed, "save/load seed equal");

// ---- Slice 1 Step 2: mesher, FPS, weapon, HUD ----
// Mesher: single voxel
var single = new VoxelChunk();
single.TrySet(10, 10, 10, BlockType.Stone);
Eq(6, Mesher.GreedyMesh(single).Count, "mesher single voxel 6 quads");
// Mesher: 2x2x2 cube
var cube = new VoxelChunk();
for (int x = 10; x < 12; x++) for (int y = 10; y < 12; y++) for (int z = 10; z < 12; z++) cube.TrySet(x, y, z, BlockType.Stone);
Eq(6, Mesher.GreedyMesh(cube).Count, "mesher 2x2x2 cube 6 quads (greedy merge)");
// Mesher: empty
Eq(0, Mesher.GreedyMesh(new VoxelChunk()).Count, "mesher empty 0 quads");
// Mesher: flat slab 32x1x32 stone at y=1 -> top 1 + bottom 1 + 4 sides = 6
var slab = new VoxelChunk();
for (int x = 0; x < 32; x++) for (int z = 0; z < 32; z++) slab.TrySet(x, 1, z, BlockType.Stone);
Eq(6, Mesher.GreedyMesh(slab).Count, "mesher flat slab 6 quads");
// Mesher: generated world greedy < naive
var gen = VoxelChunk.Generate(42);
int greedy = Mesher.GreedyMesh(gen).Count;
int naive = Mesher.NaiveFaceCount(gen);
Check(greedy > 0 && greedy < naive, $"mesher generated greedy {greedy} < naive {naive}");
Check(Mesher.GreedyMesh(gen)[0].Corners().Length == 4, "mesher quad has 4 corners");

// FPS: free move, wall collision, fall, jump
var fpsWorld = new WorldZone(42);
var fps = new FpsController(16f, 4f, 16f);
fps.Update(0.1f, 0, 0, false, fpsWorld.Chunk); // settle to ground
Check(fps.IsGrounded, "fps grounded after settle");
float startX = fps.X;
fps.Update(0.1f, 1f, 0f, false, fpsWorld.Chunk);
Check(fps.Z > 16f, "fps moves forward");
// Wall collision: place a wall directly ahead in Z at feet level, then try to move through it.
var wallWorld = new VoxelChunk();
for (int x = 0; x < 32; x++) for (int z = 0; z < 32; z++) for (int y = 1; y <= 3; y++) wallWorld.TrySet(x, y, z, BlockType.Stone);
wallWorld.TrySet(16, 4, 18, BlockType.Stone);
wallWorld.TrySet(16, 5, 18, BlockType.Stone);
var fps2 = new FpsController(16f, 4f, 16f);
for (int i = 0; i < 20; i++) fps2.Update(0.1f, 1f, 0f, false, wallWorld);
Check(fps2.Z < 18f - FpsController.HalfWidth, $"fps wall blocks movement (Z={fps2.Z})");
// Fall: spawn high, fall to ground top (y=4)
var fps3 = new FpsController(16f, 10f, 16f);
for (int i = 0; i < 30; i++) fps3.Update(0.1f, 0, 0, false, fpsWorld.Chunk);
Check(Math.Abs(fps3.Y - 4f) < 0.1f, $"fps falls to ground (Y={fps3.Y})");
Check(fps3.IsGrounded, "fps grounded after fall");
// Jump: from grounded, jump rises, then lands again
var fps4 = new FpsController(16f, 4f, 16f);
fps4.Update(0.1f, 0, 0, false, fpsWorld.Chunk);
fps4.Update(0.1f, 0, 0, true, fpsWorld.Chunk);
Check(fps4.Y > 4f, $"fps jump rises (Y={fps4.Y})");
for (int i = 0; i < 30; i++) fps4.Update(0.1f, 0, 0, false, fpsWorld.Chunk);
Check(Math.Abs(fps4.Y - 4f) < 0.1f && fps4.IsGrounded, "fps jump lands back on ground");

// Weapon + HUD
var weapon = new Weapon();
Eq(42, weapon.Mag, "weapon starts 42 mag");
Eq(120, weapon.Reserve, "weapon starts 120 reserve");
Check(weapon.Fire(), "weapon fires");
Eq(41, weapon.Mag, "weapon mag decreases on fire");
for (int i = 0; i < 41; i++) weapon.Fire();
Eq(0, weapon.Mag, "weapon mag empty after 42 shots");
Check(!weapon.Fire(), "weapon cannot fire empty");
weapon.Reload();
Eq(42, weapon.Mag, "weapon reload refills mag");
Eq(78, weapon.Reserve, "weapon reload consumes reserve (120 - 42 refilled = 78; test arithmetic corrected, not code)");
var hudWorld = new WorldZone(42);
var hud = HudState.FromWorld(hudWorld, new Weapon());
Eq(100, hud.Health, "hud health 100");
Eq(85, hud.Shields, "hud shields 85");
Eq("DESTROY CORE", hud.ObjectiveName, "hud objective destroy core");
Check(hud.ObjectiveDistanceMetres > 0, "hud objective distance positive");
Eq(0, hud.ObjectivePercentCompleted, "hud objective 0% at start");
hudWorld.DamageCore(45);
Eq(45, HudState.FromWorld(hudWorld, new Weapon()).ObjectivePercentCompleted, "hud objective 45% after core damage");
Check(HudState.FromWorld(hudWorld, new Weapon()).MiniMapSolid[16, 16], "hud minimap solid at centre ground");

// ---- Build-around step: MeshBuilder (Unity/GPU not available headlessly) ----
var singleMesh = MeshBuilder.Build(Mesher.GreedyMesh(single));
Eq(6, singleMesh.QuadCount, "meshbuilder single voxel quad count");
Eq(24, singleMesh.Vertices.Count, "meshbuilder single voxel 24 vertices");
Eq(36, singleMesh.Indices.Count, "meshbuilder single voxel 36 indices");
Check(singleMesh.Indices.All(i => i >= 0 && i < singleMesh.Vertices.Count), "meshbuilder indices in range");
Check(singleMesh.Normals.All(n => Math.Abs(Math.Abs(n.Item1) + Math.Abs(n.Item2) + Math.Abs(n.Item3) - 1f) < 0.001f), "meshbuilder normals are unit axis vectors");
var genMesh = MeshBuilder.BuildFromChunk(gen);
Eq(genMesh.QuadCount * 4, genMesh.Vertices.Count, "meshbuilder vertices = quads x 4");
Eq(genMesh.QuadCount * 6, genMesh.Indices.Count, "meshbuilder indices = quads x 6");
Check(genMesh.QuadCount == greedy, "meshbuilder quad count matches mesher");

// ---- Simulation loop (headless playable loop) ----
var sim = new Simulation(42);
sim.Tick(0.1f, 0, 0, false);
Check(!sim.IsPlayerDead, "simulation player alive after first tick");
var gatherResult = sim.GatherAhead();
Check(gatherResult == GatherResult.Success, $"simulation gather ahead succeeds (got {gatherResult})");
Check(sim.BlocksGathered == 1, "simulation blocks gathered counts");
sim.World.Player.Inventory.Add(BlockType.Stone, 10);
var buildResult = sim.BuildWallAhead();
Check(buildResult == BuildResult.Success, $"simulation build wall ahead succeeds (got {buildResult})");
sim.World.SpawnWave(1);
Check(sim.World.Enemies.Count == 1, "simulation wave spawns 1 enemy");
for (int i = 0; i < 3; i++) sim.FireAtNearestEnemy();
Check(sim.EnemiesKilled == 1, "simulation fire kills enemy in 3 shots (30 HP, 10 damage)");
sim.World.DamageCore(100);
Check(sim.IsObjectiveComplete, "simulation objective complete at core 0 HP");
var simHud = sim.Hud();
Eq(100, simHud.ObjectivePercentCompleted, "simulation hud objective 100% after core destroyed");

// ---- Step 4: core attack + mesher benchmark ----
var sim2 = new Simulation(42);
// Move FPS close to the core by teleporting via a fresh controller position is not
// possible (position is private-set in FpsController), so test FireAtCore at spawn
// distance: it should fire but not damage (core is >10m away).
int coreBefore = sim2.World.CoreHealth;
sim2.FireAtCore();
Check(sim2.World.CoreHealth == coreBefore, "fire at core out of range does not damage core");
Check(sim2.Weapon.Mag == 41, "fire at core consumes ammo even out of range");
// Mesher benchmark: one generated chunk must mesh in under 500 ms on this machine.
var benchSw = System.Diagnostics.Stopwatch.StartNew();
var benchMesh = MeshBuilder.BuildFromChunk(VoxelChunk.Generate(42));
benchSw.Stop();
Check(benchSw.ElapsedMilliseconds < 500, $"mesher benchmark <500ms (got {benchSw.ElapsedMilliseconds}ms, {benchMesh.QuadCount} quads)");

// ---- Slice 2 Step 1: procedural loot ----
var lootA = LootGenerator.Generate(42, 1);
var lootB = LootGenerator.Generate(42, 1);
Check(lootA.Name == lootB.Name && lootA.Damage == lootB.Damage && lootA.Rarity == lootB.Rarity, "loot same seed same weapon");
var lootC = LootGenerator.Generate(43, 1);
Check(lootC.Name != lootA.Name || lootC.Damage != lootA.Damage, "loot different seed usually differs");
Check(lootA.Damage > 0 && lootA.MagSize > 0 && lootA.FireRatePerSecond > 0, "loot stats positive");
Check(Enum.IsDefined(lootA.Rarity) && Enum.IsDefined(lootA.Element), "loot rarity and element valid");
var legendaryFound = false;
for (int s = 0; s < 500; s++) if (LootGenerator.Generate(s, 10).Rarity == Rarity.Legendary) { legendaryFound = true; break; }
Check(legendaryFound, "loot legendary exists in 500 seeds (rare, not impossible)");
var drop = LootGenerator.EnemyDrop(42, 1, 1);
Check(drop == null || drop.Damage > 0, "loot enemy drop is null or a valid weapon");
var highLevel = LootGenerator.Generate(42, 20);
var lowLevel = LootGenerator.Generate(42, 1);
// Level scaling is via base damage formula, but rarity is seed-driven, so only check both are valid and level 20 is not weaker on average across seeds.
int sumLow = 0, sumHigh = 0;
for (int s = 0; s < 50; s++) { sumLow += LootGenerator.Generate(s, 1).Damage; sumHigh += LootGenerator.Generate(s, 20).Damage; }
Check(sumHigh > sumLow, $"loot level scaling: level 20 total {sumHigh} > level 1 total {sumLow}");

// ---- Begin work: workstation real effect ----
var wsWorld = new WorldZone(42);
wsWorld.Player.TakeDamage(20); // shields 85 -> 65
Check(wsWorld.Player.Shields == 65, $"workstation test setup shields 65 (got {wsWorld.Player.Shields})");
wsWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
Check(wsWorld.IsNearWorkstation(), "workstation near player detected");
wsWorld.Advance();
Check(wsWorld.Player.Shields == 70, $"workstation restores 5 shields per tick (got {wsWorld.Player.Shields})");
var farWorld = new WorldZone(42);
farWorld.Player.TakeDamage(20);
farWorld.Chunk.TrySet(1, 5, 1, BlockType.Workstation);
farWorld.Advance();
Check(farWorld.Player.Shields == 65, $"far workstation does not restore shields (got {farWorld.Player.Shields})");

// ---- Slice 2 Step 2: passive skill cluster ----
var build = new SkillBuild();
Check(!build.Allocate("N1"), "skill prereq blocks N1 before S1+S2");
Check(build.Allocate("S1") && build.Allocate("S2"), "skill travel nodes allocate");
Check(!build.Allocate("S1"), "skill duplicate allocate rejected");
Check(build.Allocate("N1"), "skill N1 allocates after prereqs");
Check(!build.Allocate("K1"), "skill keystone blocks before N2");
Check(build.Allocate("S3") && build.Allocate("N2") && build.Allocate("K1"), "skill full cluster allocates in order");
Check(build.TotalEffect("DamagePct") == 20, $"skill total DamagePct 20 (got {build.TotalEffect("DamagePct")})");
Check(build.TotalEffect("ShieldBonus") == 20, $"skill total ShieldBonus 20 (got {build.TotalEffect("ShieldBonus")})");
Check(build.TotalEffect("CoreDamagePct") == 25, "skill keystone CoreDamagePct 25");
var encoded = build.Encode();
var decoded = SkillBuild.Decode(encoded);
Check(decoded.Allocated.Count == 6 && decoded.TotalEffect("DamagePct") == 20, "skill encode/decode round-trip preserves build");
Check(!new SkillBuild().Allocate("NOPE"), "skill unknown node rejected");

// ---- Slice 2 Step 3: GeneratedWeapon integrated into Simulation ----
// Seed 0 is chosen deliberately: it generates a 20-damage / 30-mag weapon,
// unlike seed 42's 10-damage / 42-mag loot, so these tests cannot pass
// by accident on the fixed Pulse Rifle's stats.
var genLoot = LootGenerator.Generate(0, 1);
var genWeapon = Weapon.FromGenerated(genLoot);
Eq(genLoot.Name, genWeapon.Name, "generated weapon name preserved");
Eq(genLoot.Damage, genWeapon.Damage, "generated weapon damage preserved");
Eq(genLoot.MagSize, genWeapon.MagCapacity, "generated weapon mag capacity preserved");
Eq(genLoot.MagSize, genWeapon.Mag, "generated weapon starts with full mag");
var simGen = new Simulation(42, genLoot);
Eq(genLoot.Damage, simGen.Weapon.Damage, "simulation equipped generated damage");
Eq(genLoot.MagSize, simGen.Weapon.Mag, "simulation equipped generated mag");
simGen.World.SpawnWave(1);
int enemyHpBefore = simGen.World.Enemies[0].Health;
simGen.FireAtNearestEnemy();
Eq(Math.Max(0, enemyHpBefore - genLoot.Damage), simGen.World.Enemies.Count == 0 ? 0 : simGen.World.Enemies[0].Health, "simulation fire deals generated damage, not fixed 10");
var simEquip = new Simulation(42);
Eq("Pulse Rifle", simEquip.Weapon.Name, "simulation default weapon is still Pulse Rifle");
Eq(10, simEquip.Weapon.Damage, "simulation default weapon damage still 10");
Check(simEquip.Equip(genLoot), "simulation equip generated weapon succeeds");
Eq(genLoot.Name, simEquip.Weapon.Name, "simulation equip changes weapon name");
Eq(genLoot.Damage, simEquip.Weapon.Damage, "simulation equip changes weapon damage");
Eq(genLoot.MagSize, simEquip.Weapon.Mag, "simulation equip gives full generated mag");
int equipMagBefore = simEquip.Weapon.Mag;
simEquip.FireAtNearestEnemy();
Eq(equipMagBefore - 1, simEquip.Weapon.Mag, "simulation equip fire consumes generated mag");
Check(!simEquip.Equip(null), "simulation equip null rejected, weapon unchanged");
Eq(genLoot.Name, simEquip.Weapon.Name, "simulation equip null leaves weapon unchanged");
Eq(genLoot.Name.ToUpperInvariant(), simGen.Hud().WeaponName, "hud weapon name follows equipped generated weapon");

// ---- Slice 2 Step 4: Forge Survivor skill effects applied to gameplay ----
var fullBuild = new SkillBuild();
Check(fullBuild.Allocate("S1") && fullBuild.Allocate("S2") && fullBuild.Allocate("N1") && fullBuild.Allocate("S3") && fullBuild.Allocate("N2") && fullBuild.Allocate("K1"), "skill full cluster allocates for application test");
// Seed 0 loot is 20 damage, so DamagePct 20% -> 24 enemy / 29 core (20% + 25%) is discriminating, not the base 20.
var simSkill = new Simulation(42, genLoot, fullBuild);
Eq(24, simSkill.EffectiveDamage, "skill effective enemy damage 24 (20 x 1.20)");
Eq(29, simSkill.EffectiveCoreDamage, "skill effective core damage 29 (20 x 1.45)");
Eq(105, simSkill.World.Player.MaxShields, "skill shield cap 105 (85 + 20)");
Eq(10, simSkill.World.WorkstationRegenPerTick, "skill workstation regen 10 per tick (5 + 5)");
Check(Math.Abs(simSkill.Fps.SpeedMultiplier - 1.05f) < 0.001f, "skill move speed multiplier 1.05");
simSkill.World.SpawnWave(1);
simSkill.FireAtNearestEnemy();
Eq(6, simSkill.World.Enemies[0].Health, "skill fire deals 24 damage to 30 HP enemy, leaving 6");
var simNoSkill = new Simulation(42, genLoot);
Eq(20, simNoSkill.EffectiveDamage, "no-skill effective damage equals base weapon damage");
Eq(20, simNoSkill.EffectiveCoreDamage, "no-skill effective core damage equals base weapon damage");
Check(!simNoSkill.ApplySkills(null), "skill apply null rejected");
Eq(20, simNoSkill.EffectiveDamage, "skill apply null leaves damage unchanged");
// Respec replaces, never stacks: full build then empty build returns to base.
var simRespec = new Simulation(42, genLoot, fullBuild);
Check(simRespec.ApplySkills(new SkillBuild()), "skill respec to empty build succeeds");
Eq(85, simRespec.World.Player.MaxShields, "skill respec restores shield cap 85");
Eq(20, simRespec.EffectiveDamage, "skill respec restores base damage");
// Workstation regen with skills: 65 -> 75 (+10), not the base +5 -> 70.
var simWsSkill = new Simulation(42, null, fullBuild);
simWsSkill.World.Player.TakeDamage(20);
simWsSkill.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
simWsSkill.World.Advance();
Eq(75, simWsSkill.World.Player.Shields, "skill workstation restores 10 shields per tick");
// Move speed with Swift Feet only: 6 -> 6.3 m/s, so 0.1 s forward moves 0.63 m.
var swiftBuild = new SkillBuild();
Check(swiftBuild.Allocate("S3"), "skill Swift Feet allocates alone");
var simSwift = new Simulation(42, null, swiftBuild);
Check(Math.Abs(simSwift.Fps.EffectiveSpeed - 6.3f) < 0.01f, "skill Swift Feet effective speed 6.3");
simSwift.Tick(0.1f, 1, 0, false);
Check(Math.Abs(simSwift.Fps.Z - 16.63f) < 0.01f, $"skill Swift Feet moves 0.63m in 0.1s (got Z {simSwift.Fps.Z})");
// Gather bonus: +20% = one bonus unit per 5 successful gathers; a failed
// gather in between must not advance the bonus. Target cell is (16,3,17).
var simGather = new Simulation(42, null, fullBuild);
for (int i = 0; i < 4; i++)
{
    simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Stone);
    Check(simGather.GatherAhead() == GatherResult.Success, "skill gather bonus setup gather succeeds");
}
Check(simGather.GatherAhead() == GatherResult.Air, "skill gather bonus failed gather is air");
simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Stone);
Check(simGather.GatherAhead() == GatherResult.Success, "skill gather bonus 5th gather succeeds");
Eq(5, simGather.BlocksGathered, "skill gather counts 5 gathers, not bonus units");
Eq(6, simGather.World.Player.Inventory.Get(BlockType.Stone), "skill gather bonus gives 6 stone from 5 gathers (+20%)");

// ---- Slice 2 Step 5: automation tier 1 (Kinetic only) ----
// Build cost: 10 Stone + 5 Wood, placed above ground (supported below).
var millWorld = new WorldZone(42);
millWorld.Player.Inventory.Add(BlockType.Stone, 10);
millWorld.Player.Inventory.Add(BlockType.Wood, 5);
Eq(BuildResult.Success, millWorld.Build(15, 4, 15, BlockType.KineticMill), "kinetic mill builds with 10 stone + 5 wood");
Eq(BlockType.KineticMill, millWorld.Chunk.Get(15, 4, 15), "kinetic mill placed");
Eq(0, millWorld.Player.Inventory.Get(BlockType.Stone), "kinetic mill consumes 10 stone");
Eq(0, millWorld.Player.Inventory.Get(BlockType.Wood), "kinetic mill consumes 5 wood");
Eq(GatherResult.NotGatherable, millWorld.Gather(15, 4, 15), "kinetic mill itself is not gatherable");
// Insufficient wood: stone alone must not build, and must not be spent.
var poorWorld = new WorldZone(42);
poorWorld.Player.Inventory.Add(BlockType.Stone, 10);
Eq(BuildResult.InsufficientResources, poorWorld.Build(15, 4, 15, BlockType.KineticMill), "kinetic mill without wood fails");
Eq(BlockType.Air, poorWorld.Chunk.Get(15, 4, 15), "failed kinetic mill leaves air");
Eq(10, poorWorld.Player.Inventory.Get(BlockType.Stone), "failed kinetic mill does not spend stone");
// Timing + rate: a mill above ground gathers the ground stone below
// on the 5th tick, not before, and exactly one block per activation.
var autoWorld = new WorldZone(42);
autoWorld.Chunk.TrySet(16, 4, 16, BlockType.KineticMill);
int stoneBefore = autoWorld.Player.Inventory.Get(BlockType.Stone);
for (int i = 0; i < 4; i++) autoWorld.Advance();
Eq(stoneBefore, autoWorld.Player.Inventory.Get(BlockType.Stone), "kinetic mill gathers nothing in first 4 ticks");
Eq(0, autoWorld.KineticBlocksGathered, "kinetic mill counter 0 before activation tick");
autoWorld.Advance(); // tick 5
Eq(stoneBefore + 1, autoWorld.Player.Inventory.Get(BlockType.Stone), "kinetic mill gathers 1 stone on 5th tick");
Eq(1, autoWorld.KineticBlocksGathered, "kinetic mill counter 1 after activation");
Eq(BlockType.Air, autoWorld.Chunk.Get(16, 3, 16), "kinetic mill excavated the ground block below it");
// Built blocks and bedrock are never excavated, even with nothing else adjacent.
var safeWorld = new WorldZone(42);
safeWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
safeWorld.Chunk.TrySet(17, 20, 16, BlockType.Wall);
safeWorld.Chunk.TrySet(15, 20, 16, BlockType.Workstation);
safeWorld.Chunk.TrySet(16, 21, 16, BlockType.KineticMill);
safeWorld.Chunk.TrySet(16, 19, 16, BlockType.Bedrock);
for (int i = 0; i < 5; i++) safeWorld.Advance();
Eq(BlockType.Wall, safeWorld.Chunk.Get(17, 20, 16), "kinetic mill does not excavate a wall");
Eq(BlockType.Workstation, safeWorld.Chunk.Get(15, 20, 16), "kinetic mill does not excavate a workstation");
Eq(BlockType.KineticMill, safeWorld.Chunk.Get(16, 21, 16), "kinetic mill does not excavate another mill");
Eq(BlockType.Bedrock, safeWorld.Chunk.Get(16, 19, 16), "kinetic mill does not excavate bedrock");
Eq(0, safeWorld.KineticBlocksGathered, "kinetic mill gathers nothing when only ungatherable neighbours exist");
// Two mills, one shared stone: exactly one gather, first in scan order wins.
var raceWorld = new WorldZone(42);
raceWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
raceWorld.Chunk.TrySet(12, 20, 10, BlockType.KineticMill);
raceWorld.Chunk.TrySet(11, 20, 10, BlockType.Stone);
for (int i = 0; i < 5; i++) raceWorld.Advance();
Eq(BlockType.Air, raceWorld.Chunk.Get(11, 20, 10), "two mills shared stone is excavated");
Eq(1, raceWorld.KineticBlocksGathered, "two mills competing gather exactly 1 block, not 2");
// Full inventory: nothing is consumed.
var fullWorld = new WorldZone(42);
fullWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
fullWorld.Chunk.TrySet(11, 20, 10, BlockType.Stone);
fullWorld.Player.Inventory.Add(BlockType.Stone, Inventory.MaxStack);
for (int i = 0; i < 5; i++) fullWorld.Advance();
Eq(BlockType.Stone, fullWorld.Chunk.Get(11, 20, 10), "kinetic mill with full inventory leaves the block");
Eq(0, fullWorld.KineticBlocksGathered, "kinetic mill with full inventory gathers 0");
// Save/load: the mill block survives the chunk round-trip (byte array).
var saveMillWorld = new WorldZone(42);
saveMillWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
var saveMillLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveMillWorld));
Eq(BlockType.KineticMill, saveMillLoaded.Chunk.Get(15, 4, 15), "kinetic mill survives save/load");
// Simulation: build a mill ahead, then 5 ticks run its automation.
var simMill = new Simulation(42);
simMill.Tick(0.1f, 0, 0, false);
simMill.World.Player.Inventory.Add(BlockType.Stone, 10);
simMill.World.Player.Inventory.Add(BlockType.Wood, 5);
Eq(BuildResult.Success, simMill.BuildKineticMillAhead(), "simulation build kinetic mill ahead succeeds");
for (int i = 0; i < 5; i++) simMill.Tick(0.1f, 0, 0, false);
Eq(1, simMill.World.KineticBlocksGathered, "simulation kinetic mill gathers 1 block in 5 ticks");
Check(simMill.World.Player.Inventory.Get(BlockType.Stone) >= 1, "simulation kinetic mill output reaches player inventory");

// ---- Slice 2 Step 6: Orbital Tactical doctrine prototype (one doctrine, one squad type) ----
// Player starts at (16,5,16) in a fresh WorldZone; y=5 is open air, so
// X/Z movement at that height is unblocked unless a test places a block.
var orbWorld = new WorldZone(42);
var squad = orbWorld.DeploySquad(16, 5, 16);
Check(squad != null, "orbital deploy at player rally succeeds");
Eq(1, orbWorld.Squads.Count, "orbital deploy adds 1 squad");
Eq(SquadKind.Vanguard, squad!.Kind, "orbital squad kind is Vanguard");
Eq(TacticalDoctrine.HoldGround, squad.Doctrine, "orbital squad doctrine is HoldGround");
Eq(16, squad.X, "orbital squad spawns at player X");
Eq(3, squad.AliveMembers, "orbital squad starts with 3 members");
Eq(90, squad.Health, "orbital squad starts at 90 health");
Eq(30, squad.DamagePerTick, "orbital squad deals 30 per tick at full strength");
Check(squad.AtRally, "orbital squad at rally when rally is spawn point");
Check(orbWorld.DeploySquad(-1, 5, 5) == null, "orbital deploy out-of-bounds rally rejected");
Eq(1, orbWorld.Squads.Count, "orbital rejected deploy adds no squad");
Check(orbWorld.DeploySquad(5, 5, 5, (TacticalDoctrine)99) == null, "orbital deploy unknown doctrine rejected");
Eq(1, orbWorld.Squads.Count, "orbital unknown doctrine adds no squad");
// Movement: one voxel per tick, largest-gap axis first, ties X before Z.
var moveWorld = new WorldZone(42);
var mover = moveWorld.DeploySquad(20, 5, 20)!;
moveWorld.Advance();
Eq(17, mover.X, "orbital squad moves one voxel in X first on diagonal rally");
Eq(16, mover.Z, "orbital squad does not also move in Z on the same tick");
Check(!mover.AtRally, "orbital squad not at rally while moving");
for (int i = 0; i < 7; i++) moveWorld.Advance();
Eq(20, mover.X, "orbital squad reaches rally X after 8 ticks");
Eq(20, mover.Z, "orbital squad reaches rally Z after 8 ticks");
Check(mover.AtRally, "orbital squad at rally after moving");
moveWorld.Advance();
Eq(20, mover.X, "orbital squad holds rally X on later ticks");
Eq(20, mover.Z, "orbital squad holds rally Z on later ticks");
// Blocked movement: a solid block on the path stops the squad that tick.
var blockWorld = new WorldZone(42);
blockWorld.Chunk.TrySet(17, 5, 16, BlockType.Stone);
var blocked = blockWorld.DeploySquad(20, 5, 16)!;
blockWorld.Advance();
Eq(16, blocked.X, "orbital squad does not move into a solid block");
Check(!blocked.AtRally, "orbital squad blocked short of rally is not at rally");
// An enemy on the path cell also blocks movement, and a moving squad does not fire.
var pathWorld = new WorldZone(42);
var pathEnemy = new Enemy(17, 5, 16);
pathWorld.Enemies.Add(pathEnemy);
var pathSquad = pathWorld.DeploySquad(20, 5, 16)!;
pathWorld.Advance();
Eq(16, pathSquad.X, "orbital squad does not move into an enemy-occupied cell");
Eq(30, pathEnemy.Health, "orbital moving squad does not fire at the blocking enemy");
// En-route squad does not fire even when an enemy is in range.
var routeWorld = new WorldZone(42);
var routeEnemy = new Enemy(17, 5, 18); // distance 3 from spawn (16,5,16)
routeWorld.Enemies.Add(routeEnemy);
routeWorld.DeploySquad(22, 5, 16);
routeWorld.Advance();
Eq(30, routeEnemy.Health, "orbital en-route squad does not fire in-range enemy");
// Holding squad fires: 3 members x 10 = 30 kills a 30 HP enemy in one tick.
var holdWorld = new WorldZone(42);
var holdEnemy = new Enemy(18, 5, 16); // distance 2 from rally (16,5,16)
holdWorld.Enemies.Add(holdEnemy);
holdWorld.DeploySquad(16, 5, 16);
holdWorld.Advance();
Eq(0, holdWorld.Enemies.Count, "orbital holding squad kills in-range enemy in one tick");
// Range limit: distance 4 is out of the 3-range engagement. The
// player is moved far away first. UPDATED in Slice 4 Step 1 for
// the intended behaviour change: enemies now pursue the nearest
// live target, so this enemy would walk toward the nearer squad
// (distance 4) into range 3 and be killed before the range rule
// could be isolated. A solid block on its step (19,5,16) blocks
// that move, so the enemy stays at distance 4 and the range rule
// itself is still what is tested — the pursuit-into-range case
// is covered by the Slice 4 tests below instead.
var rangeWorld = new WorldZone(42);
var farEnemy = new Enemy(20, 5, 16); // distance 4 from rally (16,5,16)
rangeWorld.Enemies.Add(farEnemy);
rangeWorld.Chunk.TrySet(19, 5, 16, BlockType.Stone);
rangeWorld.DeploySquad(16, 5, 16);
rangeWorld.Player.X = 28; rangeWorld.Player.Z = 28;
rangeWorld.Advance();
Eq(30, farEnemy.Health, "orbital holding squad does not fire beyond range 3");
Eq(1, rangeWorld.Enemies.Count, "orbital out-of-range enemy survives");
// Firepower steps down with members: after 30 damage, 2 members deal 20.
var weakWorld = new WorldZone(42);
var weakEnemy = new Enemy(18, 5, 16);
weakWorld.Enemies.Add(weakEnemy);
var weakSquad = weakWorld.DeploySquad(16, 5, 16)!;
weakSquad.TakeDamage(30);
Eq(2, weakSquad.AliveMembers, "orbital squad drops to 2 members after 30 damage");
Eq(20, weakSquad.DamagePerTick, "orbital 2-member squad deals 20 per tick");
weakWorld.Advance();
Eq(10, weakEnemy.Health, "orbital 2-member squad leaves 10 HP on a 30 HP enemy");
// Squad takes adjacent-enemy damage after firing: 1 member deals 10,
// the surviving adjacent enemy then deals 10 back.
var hurtWorld = new WorldZone(42);
var hurtEnemy = new Enemy(17, 5, 16); // adjacent to rally (16,5,16)
hurtWorld.Enemies.Add(hurtEnemy);
var hurtSquad = hurtWorld.DeploySquad(16, 5, 16)!;
hurtSquad.TakeDamage(60); // 90 -> 30, 1 member
Eq(1, hurtSquad.AliveMembers, "orbital squad drops to 1 member after 60 damage");
hurtWorld.Advance();
Eq(20, hurtEnemy.Health, "orbital 1-member squad deals 10 to adjacent enemy");
Eq(20, hurtSquad.Health, "orbital squad takes 10 from surviving adjacent enemy");
// A squad that kills its adjacent attacker that tick is not hit by it.
var killWorld = new WorldZone(42);
var killEnemy = new Enemy(17, 5, 16);
killWorld.Enemies.Add(killEnemy);
var killSquad = killWorld.DeploySquad(16, 5, 16)!;
killWorld.Advance();
Eq(0, killWorld.Enemies.Count, "orbital full squad kills adjacent enemy before it can hit back");
Eq(90, killSquad.Health, "orbital squad unharmed when it kills its adjacent attacker");
// Dead squads are removed by Advance.
var deadWorld = new WorldZone(42);
var deadSquad = deadWorld.DeploySquad(16, 5, 16)!;
deadSquad.TakeDamage(90);
Check(deadSquad.IsDead, "orbital squad dead at 0 health");
deadWorld.Advance();
Eq(0, deadWorld.Squads.Count, "orbital dead squad removed on tick");
// Determinism: same deploy + advance gives the same squad position.
var detA = new WorldZone(42); var detB = new WorldZone(42);
var sA = detA.DeploySquad(20, 5, 16)!; var sB = detB.DeploySquad(20, 5, 16)!;
detA.Advance(); detB.Advance();
Check(sA.X == sB.X && sA.Z == sB.Z, "orbital squad movement is deterministic across worlds");
// Simulation command: deploy via Simulation, squad moves on sim ticks.
var simOrb = new Simulation(42);
var simSquad = simOrb.DeployOrbitalSquad(20, 5, 16);
Check(simSquad != null, "simulation deploy orbital squad succeeds");
Eq(1, simOrb.World.Squads.Count, "simulation deploy adds squad to world");
simOrb.Tick(0.1f, 0, 0, false);
Eq(17, simSquad!.X, "simulation orbital squad moves on sim tick");
Check(simOrb.DeployOrbitalSquad(-1, 5, 5) == null, "simulation deploy out-of-bounds rally rejected");

// ---- Slice 2 Step 7: Orbital squad save / load persistence ----
// A wounded squad survives a save/load round-trip with position,
// rally, health, and member count intact, and keeps moving after load.
var saveSquadWorld = new WorldZone(42);
var saveSquad = saveSquadWorld.DeploySquad(20, 5, 16)!;
saveSquadWorld.Advance(); // squad moves 16 -> 17 toward its rally
saveSquad.TakeDamage(60); // 90 -> 30, 1 member left
var saveSquadFull = saveSquadWorld.DeploySquad(20, 5, 16)!;
var saveSquadJson = SaveSystem.SaveZone(saveSquadWorld);
var saveSquadLoaded = SaveSystem.LoadZone(saveSquadJson);
Eq(2, saveSquadLoaded.Squads.Count, "save/load squad count preserved");
var loadedWounded = saveSquadLoaded.Squads[0];
Eq(17, loadedWounded.X, "save/load wounded squad position preserved");
Eq(20, loadedWounded.RallyX, "save/load squad rally preserved");
Eq(30, loadedWounded.Health, "save/load wounded squad health preserved");
Eq(1, loadedWounded.AliveMembers, "save/load wounded squad has 1 member");
Eq(TacticalDoctrine.HoldGround, loadedWounded.Doctrine, "save/load squad doctrine preserved");
Eq(SquadKind.Vanguard, loadedWounded.Kind, "save/load squad kind preserved");
Eq(90, saveSquadLoaded.Squads[1].Health, "save/load full squad health preserved");
saveSquadLoaded.Advance();
Eq(18, loadedWounded.X, "loaded squad keeps moving toward rally after load");
// Old saves (no Squads field) still load, with no squads — not a crash.
var oldJson = saveSquadJson[..saveSquadJson.LastIndexOf(",\"Squads\":", StringComparison.Ordinal)] + "}";
var oldLoaded = SaveSystem.LoadZone(oldJson);
Eq(0, oldLoaded.Squads.Count, "old save without squads loads with no squads");
// A dead squad in the save is skipped on load, not resurrected.
var deadSaveWorld = new WorldZone(42);
var deadSaveSquad = deadSaveWorld.DeploySquad(20, 5, 16)!;
deadSaveSquad.TakeDamage(90);
var deadSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(deadSaveWorld));
Eq(0, deadSaveLoaded.Squads.Count, "dead squad in save is skipped on load");

// ---- Slice 2 Step 8: Orbital view snapshot + Unity adapter data ----
// An empty world snapshots to an empty list, not null.
var viewEmpty = new WorldZone(42);
Eq(0, OrbitalView.Snapshot(viewEmpty).Count, "orbital view empty world snapshots to empty list");
// A deployed squad snapshots with position, rally, health, members,
// damage, doctrine, and kind — holding at its spawn-point rally.
var viewWorld = new WorldZone(42);
viewWorld.DeploySquad(16, 5, 16);
var viewSnap = OrbitalView.Snapshot(viewWorld);
Eq(1, viewSnap.Count, "orbital view snapshot has 1 squad");
Eq(16, viewSnap[0].X, "orbital view snapshot squad X");
Eq(16, viewSnap[0].RallyX, "orbital view snapshot squad rally X");
Eq(90, viewSnap[0].Health, "orbital view snapshot squad health");
Eq(3, viewSnap[0].AliveMembers, "orbital view snapshot squad members");
Eq(30, viewSnap[0].DamagePerTick, "orbital view snapshot squad damage");
Check(viewSnap[0].AtRally, "orbital view snapshot squad at rally");
Check(!viewSnap[0].IsMoving, "orbital view snapshot holding squad is not moving");
Eq(SquadKind.Vanguard, viewSnap[0].Kind, "orbital view snapshot squad kind");
Eq(TacticalDoctrine.HoldGround, viewSnap[0].Doctrine, "orbital view snapshot squad doctrine");
// A marching squad snapshots as moving, and the snapshot is a copy:
// advancing the world after the snapshot leaves the old snapshot
// at its pre-tick position.
var viewMove = new WorldZone(42);
viewMove.DeploySquad(20, 5, 16);
var moveSnap = OrbitalView.Snapshot(viewMove);
Check(moveSnap[0].IsMoving, "orbital view snapshot marching squad is moving");
Check(!moveSnap[0].AtRally, "orbital view snapshot marching squad not at rally");
viewMove.Advance();
Eq(16, moveSnap[0].X, "orbital view snapshot is a copy, not the live squad");
Eq(17, viewMove.Squads[0].X, "orbital view live squad moved after snapshot");
// A wounded squad snapshots its reduced member count and damage.
var viewWeak = new WorldZone(42);
var weakViewSquad = viewWeak.DeploySquad(16, 5, 16)!;
weakViewSquad.TakeDamage(60);
var weakSnap = OrbitalView.Snapshot(viewWeak);
Eq(1, weakSnap[0].AliveMembers, "orbital view wounded snapshot has 1 member");
Eq(10, weakSnap[0].DamagePerTick, "orbital view wounded snapshot deals 10 per tick");
// Two squads snapshot in deploy order, and a dead squad (removed
// by Advance) never appears in a snapshot.
var viewTwo = new WorldZone(42);
viewTwo.DeploySquad(16, 5, 16);
var viewTwoDead = viewTwo.DeploySquad(20, 5, 16)!;
viewTwoDead.TakeDamage(90);
viewTwo.Advance();
Eq(1, OrbitalView.Snapshot(viewTwo).Count, "orbital view snapshot omits dead squad");
// Snapshot through Simulation, the path the Unity adapter uses.
var viewSim = new Simulation(42);
viewSim.DeployOrbitalSquad(20, 5, 16);
Eq(1, OrbitalView.Snapshot(viewSim.World).Count, "orbital view snapshot via simulation world has 1 squad");

// ---- Slice 2 composition: loot + skills + mill + squad in one Simulation ----
var compLoot = LootGenerator.Generate(0, 1);
var compBuild = new SkillBuild();
Check(compBuild.Allocate("S1") && compBuild.Allocate("S2") && compBuild.Allocate("N1") && compBuild.Allocate("S3") && compBuild.Allocate("N2") && compBuild.Allocate("K1"), "slice 2 composition full cluster allocates");
var compSim = new Simulation(42, compLoot, compBuild);
Eq("Titan Long Pulse Rifle", compSim.Weapon.Name, "slice 2 composition loot weapon equipped");
Eq(24, compSim.EffectiveDamage, "slice 2 composition skilled damage 20 -> 24");
Eq(29, compSim.EffectiveCoreDamage, "slice 2 composition skilled core damage 20 -> 29");
Eq(105, compSim.World.Player.MaxShields, "slice 2 composition shields cap 85 -> 105");
compSim.World.Player.Inventory.Add(BlockType.Stone, 10);
compSim.World.Player.Inventory.Add(BlockType.Wood, 5);
Eq(BuildResult.Success, compSim.BuildKineticMillAhead(), "slice 2 composition kinetic mill builds");
Check(compSim.DeployOrbitalSquad(compSim.World.Player.X, compSim.World.Player.Y, compSim.World.Player.Z) != null, "slice 2 composition squad deploys");
Eq(1, OrbitalView.Snapshot(compSim.World).Count, "slice 2 composition snapshot has 1 squad");
var compLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(compSim.World));
Eq(1, compLoaded.Squads.Count, "slice 2 composition save/load keeps the squad");

// ---- Slice 2 follow-up: GameManager.LoadFromJson in-place restore ----
// Teleport moves the feet position and clears vertical velocity.
var tpFps = new FpsController(1, 4, 1);
tpFps.Teleport(10, 5, 12);
Eq(10f, tpFps.X, "fps teleport sets X");
Eq(5f, tpFps.Y, "fps teleport sets Y");
Eq(12f, tpFps.Z, "fps teleport sets Z");
Eq(0f, tpFps.VelocityY, "fps teleport clears vertical velocity");
// Build a discriminating saved world: different seed, moved player,
// inventory, and a squad — then restore it into a live Simulation.
var saveSource = new WorldZone(7);
saveSource.Player.X = 10; saveSource.Player.Y = 5; saveSource.Player.Z = 12;
saveSource.Player.Inventory.Add(BlockType.Stone, 4);
saveSource.DeploySquad(10, 5, 12);
var saveJson = SaveSystem.SaveZone(saveSource);
var restoreSim = new Simulation(42);
var fpsBefore = restoreSim.Fps;
var loadedWorld = SaveSystem.LoadZone(saveJson);
Check(restoreSim.RestoreWorld(loadedWorld), "restore world accepts a loaded world");
Check(ReferenceEquals(loadedWorld, restoreSim.World), "restore world swaps in the loaded world object");
Check(ReferenceEquals(fpsBefore, restoreSim.Fps), "restore keeps the same fps object in place");
Eq(7, restoreSim.World.Seed, "restore world has the saved seed");
Eq(10, restoreSim.World.Player.X, "restore world player X");
Eq(4, restoreSim.World.Player.Inventory.Get(BlockType.Stone), "restore world inventory");
Eq(1, restoreSim.World.Squads.Count, "restore world keeps the saved squad");
Eq(10f, restoreSim.Fps.X, "restore teleports fps to saved player X");
Eq(5f, restoreSim.Fps.Y, "restore teleports fps to saved player Y");
Eq(12f, restoreSim.Fps.Z, "restore teleports fps to saved player Z");
// Restored play continues: one tick advances the restored world.
int tickBefore = restoreSim.World.Tick;
restoreSim.Tick(0.016f, 0, 0, false);
Check(restoreSim.World.Tick > tickBefore, "restored simulation ticks after restore");
// Null restore fails closed and leaves the world untouched.
var nullSim = new Simulation(42);
Check(!nullSim.RestoreWorld(null), "restore world rejects null");
Eq(42, nullSim.World.Seed, "null restore leaves the world unchanged");
// Session counters reset on restore, and a skill build is re-applied
// to the new world instead of silently reverting to base values.
var counterSim = new Simulation(42);
counterSim.World.Player.Inventory.Add(BlockType.Stone, 10);
counterSim.World.Player.Inventory.Add(BlockType.Wood, 5);
Eq(BuildResult.Success, counterSim.BuildKineticMillAhead(), "restore test mill builds for counter");
Eq(1, counterSim.BlocksBuilt, "restore test counter before restore is 1");
var skillBuild = new SkillBuild();
Check(skillBuild.Allocate("S1") && skillBuild.Allocate("S2") && skillBuild.Allocate("N1") && skillBuild.Allocate("S3") && skillBuild.Allocate("N2") && skillBuild.Allocate("K1"), "restore test full cluster allocates");
counterSim.ApplySkills(skillBuild);
Eq(105, counterSim.World.Player.MaxShields, "restore test shields cap before restore is 105");
Check(counterSim.RestoreWorld(new WorldZone(9)), "restore with skills succeeds");
Eq(0, counterSim.BlocksBuilt, "restore resets session counters");
Eq(105, counterSim.World.Player.MaxShields, "restore re-applies skill shield cap");
Check(Math.Abs(counterSim.Fps.EffectiveSpeed - 6.3f) < 0.001f, "restore re-applies skill move speed");
// LoadFromJson: valid JSON swaps the world; malformed, empty, and
// null input fail closed with the current world untouched.
var jsonSim = new Simulation(42);
Check(jsonSim.LoadFromJson(saveJson), "load from json accepts a valid save");
Eq(7, jsonSim.World.Seed, "load from json swaps to the saved seed");
Check(!jsonSim.LoadFromJson("this is not json"), "load from json rejects malformed json");
Eq(7, jsonSim.World.Seed, "malformed load leaves the world unchanged");
Check(!jsonSim.LoadFromJson(""), "load from json rejects empty input");
Check(!jsonSim.LoadFromJson(null), "load from json rejects null");
Eq(7, jsonSim.World.Seed, "rejected loads leave the world unchanged");
// Round-trip through LoadFromJson keeps the chunk identical.
var chunkSim = new Simulation(42);
var chunkJson = SaveSystem.SaveZone(chunkSim.World);
var chunkSim2 = new Simulation(43);
Check(chunkSim2.LoadFromJson(chunkJson), "load from json round-trip succeeds");
Check(chunkSim.World.Chunk.ToBytes().SequenceEqual(chunkSim2.World.Chunk.ToBytes()), "load from json round-trip keeps the chunk");

// ---- Slice 2 follow-up: doctrine menu / rally picker (0.15.0) ----
// Defaults: the only current doctrine/kind pair, no pick in progress.
var pickSim = new Simulation(42);
Eq(TacticalDoctrine.HoldGround, pickSim.OrbitalCommand.Doctrine, "orbital picker defaults to HoldGround");
Eq(SquadKind.Vanguard, pickSim.OrbitalCommand.Kind, "orbital picker defaults to Vanguard");
Check(!pickSim.OrbitalCommand.IsPicking, "orbital picker starts idle");
Check(!pickSim.OrbitalCommand.HasRally, "orbital picker starts with no rally");
// Confirm with no pick deploys nothing and changes nothing.
Check(pickSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm with no pick deploys nothing");
Eq(0, pickSim.World.Squads.Count, "orbital picker confirm with no pick adds no squad");
// SetRally outside picking fails closed.
Check(!pickSim.SetOrbitalRally(20, 5, 16), "orbital picker rally outside picking rejected");
Check(!pickSim.OrbitalCommand.HasRally, "orbital picker rally outside picking leaves no rally");
// Pick, rally, confirm: one squad, selected doctrine/kind, at the rally.
// Since 0.18.0 the picker confirm is paid, so this confirm is funded
// with 10 EmberOre first; the unfunded-confirm refusal is tested in
// the Slice 3 Step 2 section below.
pickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
pickSim.BeginOrbitalPick();
Check(pickSim.OrbitalCommand.IsPicking, "orbital picker begin opens picking");
Check(pickSim.SetOrbitalRally(20, 5, 16), "orbital picker rally while picking accepted");
Check(pickSim.OrbitalCommand.HasRally, "orbital picker has rally after set");
var picked = pickSim.ConfirmOrbitalDeploy();
Check(picked != null, "orbital picker confirm deploys a squad");
Eq(1, pickSim.World.Squads.Count, "orbital picker confirm adds exactly one squad");
Eq(20, picked!.RallyX, "orbital picker squad rally X");
Eq(TacticalDoctrine.HoldGround, picked.Doctrine, "orbital picker squad doctrine");
Eq(SquadKind.Vanguard, picked.Kind, "orbital picker squad kind");
Check(!pickSim.OrbitalCommand.IsPicking, "orbital picker closes after confirm");
Check(!pickSim.OrbitalCommand.HasRally, "orbital picker clears rally after confirm");
// A second confirm without a new pick deploys nothing.
Check(pickSim.ConfirmOrbitalDeploy() == null, "orbital picker second confirm without pick deploys nothing");
Eq(1, pickSim.World.Squads.Count, "orbital picker second confirm adds no squad");
// Out-of-bounds rally rejected; the picker stays open for a re-pick.
var oobSim = new Simulation(42);
oobSim.BeginOrbitalPick();
Check(!oobSim.SetOrbitalRally(99, 5, 16), "orbital picker out-of-bounds rally rejected");
Check(!oobSim.OrbitalCommand.HasRally, "orbital picker rejected rally leaves no rally");
Check(oobSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm with no rally deploys nothing");
Check(oobSim.OrbitalCommand.IsPicking, "orbital picker stays open after failed confirm");
Eq(0, oobSim.World.Squads.Count, "orbital picker failed confirm adds no squad");
// Re-picking a rally overwrites the earlier pending point.
Check(oobSim.SetOrbitalRally(18, 5, 16), "orbital picker first rally accepted");
Check(oobSim.SetOrbitalRally(22, 5, 16), "orbital picker re-pick accepted");
oobSim.World.Player.Inventory.Add(BlockType.EmberOre, 10); // picker confirm is paid since 0.18.0
var repicked = oobSim.ConfirmOrbitalDeploy();
Eq(22, repicked!.RallyX, "orbital picker confirm uses the latest rally");
// BeginPick clears a stale pending rally, so Enter cannot deploy it.
var staleSim = new Simulation(42);
staleSim.BeginOrbitalPick();
Check(staleSim.SetOrbitalRally(20, 5, 16), "orbital picker stale rally set");
staleSim.BeginOrbitalPick();
Check(!staleSim.OrbitalCommand.HasRally, "orbital picker begin clears stale rally");
Check(staleSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm after stale clear deploys nothing");
// Cancel abandons the pick with no squad.
var cancelSim = new Simulation(42);
cancelSim.BeginOrbitalPick();
Check(cancelSim.SetOrbitalRally(20, 5, 16), "orbital picker cancel-test rally set");
cancelSim.CancelOrbitalPick();
Check(!cancelSim.OrbitalCommand.IsPicking, "orbital picker cancel closes picking");
Check(!cancelSim.OrbitalCommand.HasRally, "orbital picker cancel drops rally");
Check(cancelSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm after cancel deploys nothing");
Eq(0, cancelSim.World.Squads.Count, "orbital picker cancel adds no squad");
// Unknown doctrine/kind selections are rejected, selection unchanged.
var selSim = new Simulation(42);
Check(!selSim.OrbitalCommand.SelectDoctrine((TacticalDoctrine)99), "orbital picker unknown doctrine rejected");
Eq(TacticalDoctrine.HoldGround, selSim.OrbitalCommand.Doctrine, "orbital picker doctrine unchanged after rejection");
Check(!selSim.OrbitalCommand.SelectKind((SquadKind)99), "orbital picker unknown kind rejected");
Eq(SquadKind.Vanguard, selSim.OrbitalCommand.Kind, "orbital picker kind unchanged after rejection");
Check(selSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "orbital picker valid doctrine accepted");
Check(selSim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "orbital picker valid kind accepted");
// A picked squad actually marches to its rally on later ticks.
var marchSim = new Simulation(42);
marchSim.World.Player.Inventory.Add(BlockType.EmberOre, 10); // picker confirm is paid since 0.18.0
marchSim.BeginOrbitalPick();
Check(marchSim.SetOrbitalRally(20, 5, 16), "orbital picker march rally set");
var marchSquad = marchSim.ConfirmOrbitalDeploy();
Eq(16, marchSquad!.X, "orbital picker squad spawns at player X");
marchSim.World.Advance();
Eq(17, marchSquad.X, "orbital picker squad marches toward rally on tick");

// ---- Slice 3 Step 1: save-format completion (0.17.0) ----
// Core health survives a save/load round-trip, including destroyed.
var coreWorld = new WorldZone(42);
coreWorld.DamageCore(40);
Eq(60, SaveSystem.LoadZone(SaveSystem.SaveZone(coreWorld)).CoreHealth, "save/load core health survives");
var deadCoreWorld = new WorldZone(42);
deadCoreWorld.DamageCore(100);
Eq(0, SaveSystem.LoadZone(SaveSystem.SaveZone(deadCoreWorld)).CoreHealth, "save/load destroyed core stays destroyed, not healed");
// An old save (no CoreHealth field) still loads, with a fresh 100 core.
var coreJson = SaveSystem.SaveZone(coreWorld);
var coreOldJson = coreJson[..coreJson.LastIndexOf(",\"CoreHealth\":", StringComparison.Ordinal)] + "}";
Eq(100, SaveSystem.LoadZone(coreOldJson).CoreHealth, "old save without core health loads fresh 100 core");
// Out-of-range core values in a save are clamped, never fatal.
Eq(100, SaveSystem.LoadZone(coreJson.Replace("\"CoreHealth\":60", "\"CoreHealth\":999")).CoreHealth, "save/load core health above 100 clamps to 100");
Eq(0, SaveSystem.LoadZone(coreJson.Replace("\"CoreHealth\":60", "\"CoreHealth\":-20")).CoreHealth, "save/load core health below 0 clamps to 0");
// World-level skill effects restore through the SaveZone overload:
// the shield cap and workstation regen arrive with the world, so a
// skilled save's shields are not clamped to the base cap on load.
var worldSkillBuild = new SkillBuild();
Check(worldSkillBuild.Allocate("S1") && worldSkillBuild.Allocate("S2") && worldSkillBuild.Allocate("N1") && worldSkillBuild.Allocate("S3") && worldSkillBuild.Allocate("N2") && worldSkillBuild.Allocate("K1"), "save-format test full cluster allocates");
var worldSkillLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(new WorldZone(42), worldSkillBuild, null));
Eq(105, worldSkillLoaded.Player.MaxShields, "save/load world-level skill shield cap restored");
Eq(10, worldSkillLoaded.WorkstationRegenPerTick, "save/load world-level skill workstation regen restored");
// Full Simulation round-trip: loot weapon (one shot fired, so the
// mag is 29 not 30), full skill build, shields above the base cap,
// and a damaged core all survive into a fresh Simulation.
var saveLoot = LootGenerator.Generate(0, 1);
var saveBuild = new SkillBuild();
Check(saveBuild.Allocate("S1") && saveBuild.Allocate("S2") && saveBuild.Allocate("N1") && saveBuild.Allocate("S3") && saveBuild.Allocate("N2") && saveBuild.Allocate("K1"), "save-format simulation full cluster allocates");
var saveSim = new Simulation(42, saveLoot, saveBuild);
saveSim.World.Player.Restore(100, 100);
saveSim.World.DamageCore(35);
Check(saveSim.Weapon.Fire(), "save-format test fires one shot before saving");
var fullJson = saveSim.SaveToJson();
var loadSim = new Simulation(99);
Check(loadSim.LoadFromJson(fullJson), "simulation save/load round-trip succeeds");
Eq("Titan Long Pulse Rifle", loadSim.Weapon.Name, "save/load weapon name survives");
Eq(20, loadSim.Weapon.Damage, "save/load weapon damage survives");
Eq(30, loadSim.Weapon.MagCapacity, "save/load weapon mag capacity survives");
Eq(29, loadSim.Weapon.Mag, "save/load weapon mag survives with no free refill");
Eq(120, loadSim.Weapon.Reserve, "save/load weapon reserve survives");
Eq(24, loadSim.EffectiveDamage, "save/load skill build restores damage");
Eq(105, loadSim.World.Player.MaxShields, "save/load skill build restores shield cap");
Eq(100, loadSim.World.Player.Shields, "save/load shields above base cap survive (cap raised before restore)");
Check(Math.Abs(loadSim.Fps.EffectiveSpeed - 6.3f) < 0.001f, "save/load skill build restores move speed");
Eq(65, loadSim.World.CoreHealth, "save/load core health survives via simulation save");
// An empty build (a respec) persists as an empty build, not as
// "no build": loading it returns to base values, not to the
// previous build.
var respecSim = new Simulation(42, saveLoot, saveBuild);
respecSim.ApplySkills(new SkillBuild());
var respecLoadedSim = new Simulation(99);
Check(respecLoadedSim.LoadFromJson(respecSim.SaveToJson()), "save/load respec round-trip succeeds");
Eq(85, respecLoadedSim.World.Player.MaxShields, "save/load empty build respecs to base cap");
Eq(20, respecLoadedSim.EffectiveDamage, "save/load empty build restores base damage");
Check(respecLoadedSim.AppliedSkills != null && respecLoadedSim.AppliedSkills.Allocated.Count == 0, "save/load empty build is present but empty");
// The default Pulse Rifle persists too, not just loot weapons.
var defaultLoadedSim = new Simulation(99);
Check(defaultLoadedSim.LoadFromJson(new Simulation(42).SaveToJson()), "save/load default weapon round-trip succeeds");
Eq("Pulse Rifle", defaultLoadedSim.Weapon.Name, "save/load default weapon name survives");
Eq(42, defaultLoadedSim.Weapon.MagCapacity, "save/load default weapon mag capacity survives");
// An old save (core, build, and weapon fields stripped) loaded
// into a skilled, loot-equipped Simulation keeps the current
// weapon and build — in-place semantics — while its core, having
// no saved value, loads fresh at 100, as before.
var keepSim = new Simulation(42, saveLoot, saveBuild);
keepSim.World.DamageCore(50);
var oldFullJson = fullJson[..fullJson.LastIndexOf(",\"CoreHealth\":", StringComparison.Ordinal)] + "}";
Check(keepSim.LoadFromJson(oldFullJson), "old save loads into skilled simulation");
Eq("Titan Long Pulse Rifle", keepSim.Weapon.Name, "old save keeps current weapon");
Eq(24, keepSim.EffectiveDamage, "old save keeps current skill build");
Eq(100, keepSim.World.CoreHealth, "old save loads fresh 100 core");
// A malformed skill string in an otherwise valid save is skipped,
// never fatal: the load succeeds, the weapon and core still
// restore, and no build is applied.
int badSkillStart = fullJson.IndexOf("\"SkillBuild\":\"", StringComparison.Ordinal) + "\"SkillBuild\":\"".Length;
int badSkillEnd = fullJson.IndexOf("\"", badSkillStart);
var badSkillJson = fullJson[..badSkillStart] + "!!!not-base64!!!" + fullJson[badSkillEnd..];
var badSkillSim = new Simulation(99);
Check(badSkillSim.LoadFromJson(badSkillJson), "malformed skill string still loads");
Eq("Titan Long Pulse Rifle", badSkillSim.Weapon.Name, "malformed skill string keeps weapon restore");
Check(badSkillSim.AppliedSkills == null, "malformed skill string restores no build");
Eq(65, badSkillSim.World.CoreHealth, "malformed skill string keeps core restore");
// A partial weapon (mag and reserve stripped) is skipped, never
// fatal, while the skill build in the same save still applies.
var partialWeaponJson = fullJson[..fullJson.LastIndexOf(",\"WeaponMag\":", StringComparison.Ordinal)] + "}";
var partialSim = new Simulation(99);
Check(partialSim.LoadFromJson(partialWeaponJson), "partial weapon still loads");
Eq("Pulse Rifle", partialSim.Weapon.Name, "partial weapon keeps current weapon");
// The build still applies to the kept weapon: Pulse Rifle 10 damage
// with the cluster's +20% is 12, which proves the skill restore
// happened even though the weapon restore was skipped.
Eq(12, partialSim.EffectiveDamage, "partial weapon keeps skill restore");

// ---- Slice 3 Step 2: squad economy (cost and cap) ----
// Constants: one Vanguard squad costs 10 EmberOre; at most 3 live
// squads may be fielded at once. Both are first-pass values.
Eq(3, SquadEconomy.MaxConcurrentSquads, "squad economy cap is 3 live squads");
Eq(10, SquadEconomy.EmberOreCost, "squad economy cost is 10 EmberOre");
// Paid deploy with exactly the price: succeeds, charges all 10 ore.
var paidWorld = new WorldZone(42);
paidWorld.Player.Inventory.Add(BlockType.EmberOre, 10);
Check(SquadEconomy.CanDeploy(paidWorld), "squad economy affordable and under cap can deploy");
var paidSquad = paidWorld.TryDeployPaidSquad(16, 5, 16);
Check(paidSquad != null, "squad economy paid deploy succeeds with exactly 10 ore");
Eq(1, paidWorld.Squads.Count, "squad economy paid deploy adds exactly one squad");
Eq(0, paidWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy paid deploy charges all 10 ore");
Eq(1, SquadEconomy.LiveSquadCount(paidWorld), "squad economy live count is 1 after paid deploy");
// Unaffordable by 1 ore: refused, no squad, no partial charge.
var brokeWorld = new WorldZone(42);
brokeWorld.Player.Inventory.Add(BlockType.EmberOre, 9);
Check(!SquadEconomy.CanDeploy(brokeWorld), "squad economy unaffordable cannot deploy");
Check(brokeWorld.TryDeployPaidSquad(16, 5, 16) == null, "squad economy 9 ore deploy refused");
Eq(0, brokeWorld.Squads.Count, "squad economy refused deploy adds no squad");
Eq(9, brokeWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy refused deploy charges nothing");
// Zero ore: same fail-closed behaviour.
var emptyWorld = new WorldZone(42);
Check(emptyWorld.TryDeployPaidSquad(16, 5, 16) == null, "squad economy zero ore deploy refused");
Eq(0, emptyWorld.Squads.Count, "squad economy zero ore deploy adds no squad");
// Cap: 3 paid squads field, the 4th is refused even with ore in hand,
// and the refused attempt charges nothing.
var capWorld = new WorldZone(42);
capWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
Check(capWorld.TryDeployPaidSquad(16, 5, 16) != null, "squad economy cap deploy 1 succeeds");
Check(capWorld.TryDeployPaidSquad(17, 5, 16) != null, "squad economy cap deploy 2 succeeds");
Check(capWorld.TryDeployPaidSquad(18, 5, 16) != null, "squad economy cap deploy 3 succeeds");
Eq(3, capWorld.Squads.Count, "squad economy 3 squads fielded");
Eq(10, capWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy 10 ore left after 3 deploys");
Check(!SquadEconomy.CanDeploy(capWorld), "squad economy at cap cannot deploy");
Check(capWorld.TryDeployPaidSquad(19, 5, 16) == null, "squad economy 4th deploy refused at cap");
Eq(3, capWorld.Squads.Count, "squad economy refused 4th deploy adds no squad");
Eq(10, capWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy refused 4th deploy charges nothing");
// A dead squad frees its cap slot without waiting for Advance to
// remove it: live count excludes the dead, so a replacement deploys.
var deadCapWorld = new WorldZone(42);
deadCapWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
deadCapWorld.TryDeployPaidSquad(16, 5, 16);
deadCapWorld.TryDeployPaidSquad(17, 5, 16);
var doomed = deadCapWorld.TryDeployPaidSquad(18, 5, 16)!;
doomed.TakeDamage(90);
Check(doomed.IsDead, "squad economy cap test squad is dead");
Eq(2, SquadEconomy.LiveSquadCount(deadCapWorld), "squad economy dead squad does not count toward cap");
Check(deadCapWorld.TryDeployPaidSquad(19, 5, 16) != null, "squad economy replacement deploys after a squad dies");
Eq(3, SquadEconomy.LiveSquadCount(deadCapWorld), "squad economy live count back to 3 after replacement");
// Picker enforces the economy: a funded confirm deploys and closes;
// an unfunded confirm fails closed — picker stays open, no squad,
// no charge — so the player can gather ore and re-confirm.
var econPickSim = new Simulation(42);
econPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
econPickSim.BeginOrbitalPick();
Check(econPickSim.SetOrbitalRally(20, 5, 16), "squad economy picker rally set");
Check(econPickSim.ConfirmOrbitalDeploy() != null, "squad economy picker funded confirm deploys");
Eq(1, econPickSim.World.Squads.Count, "squad economy picker funded confirm adds one squad");
Eq(0, econPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy picker funded confirm charges 10 ore");
Check(!econPickSim.OrbitalCommand.IsPicking, "squad economy picker closes after funded confirm");
econPickSim.BeginOrbitalPick();
Check(econPickSim.SetOrbitalRally(22, 5, 16), "squad economy picker second rally set");
Check(econPickSim.ConfirmOrbitalDeploy() == null, "squad economy picker unfunded confirm refused");
Eq(1, econPickSim.World.Squads.Count, "squad economy picker refused confirm adds no squad");
Eq(0, econPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy picker refused confirm charges nothing");
Check(econPickSim.OrbitalCommand.IsPicking, "squad economy picker stays open after refused confirm");
// Picker enforces the cap too: 3 funded picker deploys, then a 4th
// funded confirm is refused and stays open with ore untouched.
var econCapSim = new Simulation(42);
econCapSim.World.Player.Inventory.Add(BlockType.EmberOre, 40);
for (int i = 0; i < 3; i++)
{
    econCapSim.BeginOrbitalPick();
    Check(econCapSim.SetOrbitalRally(20, 5, 16), "squad economy picker cap rally set");
    Check(econCapSim.ConfirmOrbitalDeploy() != null, "squad economy picker cap deploy succeeds");
}
Eq(3, econCapSim.World.Squads.Count, "squad economy picker 3 squads fielded");
econCapSim.BeginOrbitalPick();
Check(econCapSim.SetOrbitalRally(22, 5, 16), "squad economy picker over-cap rally set");
Check(econCapSim.ConfirmOrbitalDeploy() == null, "squad economy picker over-cap confirm refused");
Eq(3, econCapSim.World.Squads.Count, "squad economy picker over-cap confirm adds no squad");
Eq(10, econCapSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy picker over-cap confirm charges nothing");
Check(econCapSim.OrbitalCommand.IsPicking, "squad economy picker stays open after over-cap refusal");
// Simulation paid path mirrors the picker: affordable succeeds,
// unaffordable fails closed, and the CanDeploy query tracks both.
var econSim = new Simulation(42);
Check(!econSim.CanDeployOrbitalSquad(), "squad economy simulation broke cannot deploy");
Check(econSim.DeployOrbitalSquadPaid(20, 5, 16) == null, "squad economy simulation paid deploy broke refused");
econSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
Check(econSim.CanDeployOrbitalSquad(), "squad economy simulation funded can deploy");
Check(econSim.DeployOrbitalSquadPaid(20, 5, 16) != null, "squad economy simulation paid deploy funded succeeds");
Eq(0, econSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy simulation paid deploy charges 10 ore");
// The free direct path is deliberately unchanged for headless/demo
// use: it still deploys with no ore. This is the documented
// distinction, not a bypass in play — Unity play goes through the
// picker, which is paid (see the picker tests above).
var freeSim = new Simulation(42);
Check(freeSim.DeployOrbitalSquad(20, 5, 16) != null, "squad economy free direct path still deploys without ore (headless only)");
Eq(1, freeSim.World.Squads.Count, "squad economy free direct path adds one squad");
// Paid squads persist through a save/load round-trip like any squad.
var paidSaveWorld = new WorldZone(42);
paidSaveWorld.Player.Inventory.Add(BlockType.EmberOre, 10);
paidSaveWorld.TryDeployPaidSquad(20, 5, 16);
var paidLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(paidSaveWorld));
Eq(1, paidLoaded.Squads.Count, "squad economy paid squad survives save/load");

// ---- Slice 3 Step 3: demo scenario + review (0.19.0) ----
// One Simulation composing Slice 3 end to end, mirroring the demo:
// a damaged core, a skilled build, a loot weapon with an
// in-progress mag, three paid squads, a picker refusal at the
// cap, and a save/load round-trip that keeps all of it.
var s3Loot = LootGenerator.Generate(0, 1);
var s3Build = new SkillBuild();
Check(s3Build.Allocate("S1") && s3Build.Allocate("S2") && s3Build.Allocate("N1") && s3Build.Allocate("S3") && s3Build.Allocate("N2") && s3Build.Allocate("K1"), "slice 3 demo full cluster allocates");
var s3Sim = new Simulation(42, s3Loot, s3Build);
s3Sim.World.Player.Restore(100, 100);
s3Sim.Fps.Teleport(24, 5, 24);
Check(s3Sim.FireAtCore() && s3Sim.FireAtCore(), "slice 3 demo two core shots fired");
Eq(42, s3Sim.World.CoreHealth, "slice 3 demo core damaged to 42 before save");
Eq(28, s3Sim.Weapon.Mag, "slice 3 demo weapon mag is 28 after two core shots");
s3Sim.World.Player.Inventory.Add(BlockType.EmberOre, 40);
Check(s3Sim.DeployOrbitalSquadPaid(18, 5, 16) != null, "slice 3 demo paid squad 1 deploys");
Check(s3Sim.DeployOrbitalSquadPaid(19, 5, 16) != null, "slice 3 demo paid squad 2 deploys");
Check(s3Sim.DeployOrbitalSquadPaid(20, 5, 16) != null, "slice 3 demo paid squad 3 deploys");
Eq(3, s3Sim.World.Squads.Count, "slice 3 demo 3 paid squads fielded");
Eq(10, s3Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice 3 demo 10 ore left after 3 paid squads");
Eq(90, s3Sim.World.Squads[0].Health, "slice 3 demo squad at full 90 HP before save");
s3Sim.BeginOrbitalPick();
Check(s3Sim.SetOrbitalRally(21, 5, 16), "slice 3 demo picker 4th rally set");
Check(s3Sim.ConfirmOrbitalDeploy() == null, "slice 3 demo picker 4th deploy refused at cap");
Eq(3, s3Sim.World.Squads.Count, "slice 3 demo refused 4th deploy adds no squad");
Eq(10, s3Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice 3 demo refused 4th deploy charges nothing");
Check(s3Sim.OrbitalCommand.IsPicking, "slice 3 demo picker stays open after cap refusal");
var s3Json = s3Sim.SaveToJson();
var s3Loaded = new Simulation(99);
Check(s3Loaded.LoadFromJson(s3Json), "slice 3 demo save/load round-trip succeeds");
Eq(42, s3Loaded.World.CoreHealth, "slice 3 demo core health survives round-trip");
Eq("Titan Long Pulse Rifle", s3Loaded.Weapon.Name, "slice 3 demo weapon name survives round-trip");
Eq(28, s3Loaded.Weapon.Mag, "slice 3 demo weapon mag survives round-trip");
Eq(120, s3Loaded.Weapon.Reserve, "slice 3 demo weapon reserve survives round-trip");
Eq(24, s3Loaded.EffectiveDamage, "slice 3 demo skill build survives round-trip");
Eq(105, s3Loaded.World.Player.MaxShields, "slice 3 demo shield cap survives round-trip");
Eq(100, s3Loaded.World.Player.Shields, "slice 3 demo shields above base cap survive round-trip");
Eq(3, s3Loaded.World.Squads.Count, "slice 3 demo 3 squads survive round-trip");
Eq(90, s3Loaded.World.Squads[0].Health, "slice 3 demo squad health survives round-trip");
Eq(10, s3Loaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice 3 demo leftover ore survives round-trip");
Check(!s3Loaded.CanDeployOrbitalSquad(), "slice 3 demo loaded game still at cap, cannot deploy more");
var s3Snap = OrbitalView.Snapshot(s3Loaded.World);
Eq(3, s3Snap.Count, "slice 3 demo snapshot shows 3 loaded squads");
Eq(3, s3Snap[0].AliveMembers, "slice 3 demo loaded squad has 3 living members");

// ---- Slice 4 Step 1: enemy AI targeting squads ----
// Targeting rule (decided here, rationale in DECISIONS.md):
// nearest live target by Manhattan distance, player wins ties,
// squads in deploy order win squad-squad ties, moving and holding
// squads alike, dead squads never targeted, re-derived every tick.
// Damage reconciliation: squad damage stays ONLY in the squad
// phase (10 per surviving adjacent enemy, after squad fire) —
// the enemy phase adds no squad damage, so a pursued squad is
// never counted twice for the same hit.
// Enemy pursues the nearer squad: player far away, squad holding
// at its spawn rally, enemy 4 away. The squad is weakened to 1
// member so it deals 10 (not 30) and the enemy survives to show
// both the move and the fire.
var pursueWorld = new WorldZone(42);
var pursueSquad = pursueWorld.DeploySquad(16, 5, 16)!;
pursueSquad.TakeDamage(60); // 90 -> 30, 1 member
pursueWorld.Player.X = 28; pursueWorld.Player.Z = 28;
var pursueEnemy = new Enemy(20, 5, 16);
pursueWorld.Enemies.Add(pursueEnemy);
pursueWorld.Advance();
Eq(19, pursueEnemy.X, "slice4 enemy moves toward nearer squad, not far player");
Eq(20, pursueEnemy.Health, "slice4 1-member squad deals 10 to pursued enemy");
Eq(30, pursueSquad.Health, "slice4 pursued squad unharmed at distance 3");
// Player nearer wins: squad marches west, player stays at spawn,
// enemy starts equidistant at tick start (both at 16) — tie goes
// to the player, so the enemy steps east toward the player.
var nearPlayerWorld = new WorldZone(42);
var nearSquad = nearPlayerWorld.DeploySquad(10, 5, 16)!;
var nearEnemy = new Enemy(20, 5, 16);
nearPlayerWorld.Enemies.Add(nearEnemy);
nearPlayerWorld.Advance();
Eq(19, nearEnemy.X, "slice4 enemy moves toward nearer player, not farther squad");
Eq(15, nearSquad.X, "slice4 squad still marches west while enemy goes for player");
// Tie, distinct positions: squad holding at 16, player moved to
// 24, enemy at 20 — distance 4 to both. Player wins the tie, so
// the enemy steps east (21), not west toward the squad (19).
var tieWorld = new WorldZone(42);
tieWorld.DeploySquad(16, 5, 16);
tieWorld.Player.X = 24;
var tieEnemy = new Enemy(20, 5, 16);
tieWorld.Enemies.Add(tieEnemy);
tieWorld.Advance();
Eq(21, tieEnemy.X, "slice4 tie goes to player, enemy moves toward player not squad");
// Dead squad never targeted: dead squad at 16 is nearer than the
// player at 28, but the enemy still steps toward the player.
var deadTargetWorld = new WorldZone(42);
var deadTargetSquad = deadTargetWorld.DeploySquad(16, 5, 16)!;
deadTargetSquad.TakeDamage(90);
deadTargetWorld.Player.X = 28;
var deadTargetEnemy = new Enemy(20, 5, 16);
deadTargetWorld.Enemies.Add(deadTargetEnemy);
deadTargetWorld.Advance();
Eq(21, deadTargetEnemy.X, "slice4 dead squad never targeted, enemy moves toward player");
Eq(0, deadTargetWorld.Squads.Count, "slice4 dead squad removed on tick");
// Moving squad targeted alike: squad marches east from 16, player
// far away, enemy at 22 follows the squad's position tick by tick.
var movingTargetWorld = new WorldZone(42);
var movingTargetSquad = movingTargetWorld.DeploySquad(20, 5, 16)!;
movingTargetWorld.Player.X = 28; movingTargetWorld.Player.Z = 28;
var movingTargetEnemy = new Enemy(22, 5, 16);
movingTargetWorld.Enemies.Add(movingTargetEnemy);
movingTargetWorld.Advance();
Eq(21, movingTargetEnemy.X, "slice4 enemy follows moving squad tick 1");
Eq(17, movingTargetSquad.X, "slice4 moving squad marches while pursued tick 1");
movingTargetWorld.Advance();
Eq(20, movingTargetEnemy.X, "slice4 enemy follows moving squad tick 2");
Eq(18, movingTargetSquad.X, "slice4 moving squad marches while pursued tick 2");
// No double-count, non-killing case: 1-member squad adjacent to
// an enemy, player far. Squad deals 10, surviving enemy deals 10
// back in the squad phase — squad 30 -> 20, not 10 (double) and
// not 30 (enemy phase hitting first would still leave 20 here,
// so the killing case below is the sharper proof).
var onceWorld = new WorldZone(42);
var onceSquad = onceWorld.DeploySquad(16, 5, 16)!;
onceSquad.TakeDamage(60); // 90 -> 30, 1 member
onceWorld.Player.X = 28; onceWorld.Player.Z = 28;
var onceEnemy = new Enemy(17, 5, 16);
onceWorld.Enemies.Add(onceEnemy);
onceWorld.Advance();
Eq(20, onceEnemy.Health, "slice4 adjacent squad deals 10, enemy survives");
Eq(20, onceSquad.Health, "slice4 squad takes adjacent damage exactly once");
// No double-count, killing case: full squad adjacent to an enemy,
// player far. If the enemy phase hit the squad before the squad
// fired, the squad would be 80 even though it killed its attacker;
// it stays 90, proving the enemy phase adds no squad damage.
var killTargetWorld = new WorldZone(42);
var killTargetSquad = killTargetWorld.DeploySquad(16, 5, 16)!;
killTargetWorld.Player.X = 28; killTargetWorld.Player.Z = 28;
var killTargetEnemy = new Enemy(17, 5, 16);
killTargetWorld.Enemies.Add(killTargetEnemy);
killTargetWorld.Advance();
Eq(0, killTargetWorld.Enemies.Count, "slice4 full squad kills adjacent pursued enemy");
Eq(90, killTargetSquad.Health, "slice4 squad unharmed when it kills its adjacent attacker");
// Retargeting, derived each tick: enemy pursues the squad on tick
// 1, the squad is then killed outright, and on tick 2 the enemy
// turns toward the player (largest gap is Z, so it steps in Z).
var retargetWorld = new WorldZone(42);
var retargetSquad = retargetWorld.DeploySquad(16, 5, 16)!;
retargetSquad.TakeDamage(60); // 1 member, so tick 1 does not kill the enemy
retargetWorld.Player.X = 28; retargetWorld.Player.Z = 28;
var retargetEnemy = new Enemy(20, 5, 16);
retargetWorld.Enemies.Add(retargetEnemy);
retargetWorld.Advance();
Eq(19, retargetEnemy.X, "slice4 retarget setup enemy pursues squad first");
retargetSquad.TakeDamage(90); // killed outright between ticks
retargetWorld.Advance();
Eq(17, retargetEnemy.Z, "slice4 enemy retargets player after squad dies");
Eq(19, retargetEnemy.X, "slice4 retargeted enemy does not also step in X that tick");
Eq(0, retargetWorld.Squads.Count, "slice4 retargeted dead squad removed");
// Movement generalisation, direct: one rule for any target —
// largest-gap axis, X tie-break, adjacent stop, solid block, and
// an occupied cell all refuse the step.
var genWorld = new WorldZone(42);
var genEnemy = new Enemy(16, 5, 20);
genEnemy.TickTowardPosition(20, 5, 20, genWorld.Chunk);
Eq(17, genEnemy.X, "slice4 generalised movement steps in X on diagonal target");
Eq(20, genEnemy.Z, "slice4 generalised movement does not also step in Z");
var genTie = new Enemy(16, 5, 16);
genTie.TickTowardPosition(20, 5, 20, genWorld.Chunk);
Eq(17, genTie.X, "slice4 generalised movement tie goes to X first");
Eq(16, genTie.Z, "slice4 generalised movement tie does not step in Z");
var genAdjacent = new Enemy(16, 5, 17);
genAdjacent.TickTowardPosition(16, 5, 16, genWorld.Chunk);
Eq(17, genAdjacent.Z, "slice4 generalised movement stops when adjacent to target");
var genBlockedWorld = new WorldZone(42);
genBlockedWorld.Chunk.TrySet(17, 5, 16, BlockType.Stone);
var genBlocked = new Enemy(16, 5, 16);
genBlocked.TickTowardPosition(20, 5, 16, genBlockedWorld.Chunk);
Eq(16, genBlocked.X, "slice4 generalised movement blocked by solid block");
var genOccupied = new Enemy(16, 5, 16);
genOccupied.TickTowardPosition(20, 5, 16, genWorld.Chunk, new[] { (17, 5, 16) });
Eq(16, genOccupied.X, "slice4 generalised movement blocked by occupied cell");
// Targeting needs no new save field: after a save/load round-trip
// the enemy still pursues the nearer squad, because the target is
// recomputed from saved positions and health, not read from a field.
var saveTargetWorld = new WorldZone(42);
var saveTargetSquad = saveTargetWorld.DeploySquad(16, 5, 16)!;
saveTargetSquad.TakeDamage(60);
saveTargetWorld.Player.X = 28; saveTargetWorld.Player.Z = 28;
saveTargetWorld.Enemies.Add(new Enemy(20, 5, 16));
var saveTargetLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveTargetWorld));
saveTargetLoaded.Advance();
Eq(19, saveTargetLoaded.Enemies[0].X, "slice4 targeting derived after load, no new save field");
Eq(20, saveTargetLoaded.Enemies[0].Health, "slice4 loaded squad fires at pursued enemy");

// ---- Slice 4 Step 2: second doctrine — Advance (chosen from Advance / Defend Core / Patrol) ----
// Advance is a hunting doctrine, not HoldGround at a moving rally:
// it seeks the nearest living enemy every tick, fires in range
// whether or not it is at its rally, and stands still when no
// enemy lives — it never falls back to marching to its rally.
// Defend Core and Patrol are deferred, not rejected (see DECISIONS).
// Deploy accepts Advance; HoldGround behaviour is unchanged elsewhere.
var advWorld = new WorldZone(42);
var advSquad = advWorld.DeploySquad(20, 5, 16, TacticalDoctrine.Advance)!;
Eq(TacticalDoctrine.Advance, advSquad.Doctrine, "slice4 advance deploy doctrine is Advance");
Eq(SquadKind.Vanguard, advSquad.Kind, "slice4 advance deploy kind stays Vanguard");
Check(advWorld.DeploySquad(5, 5, 5, (TacticalDoctrine)99) == null, "slice4 advance unknown doctrine still rejected");
// No living enemy: an Advance squad stands still, it does NOT march to its rally.
var advIdleWorld = new WorldZone(42);
var advIdle = advIdleWorld.DeploySquad(20, 5, 16, TacticalDoctrine.Advance)!;
for (int i = 0; i < 3; i++) advIdleWorld.Advance();
Eq(16, advIdle.X, "slice4 advance with no enemy does not march to rally X");
Eq(16, advIdle.Z, "slice4 advance with no enemy does not march to rally Z");
// Advance moves toward its enemy, not its rally: rally east, enemy
// west. Weakened to 1 member so its fire (10) does not kill the
// enemy outright and hide the movement. Enemy phase first: the
// enemy is tied with the player at 4, so the player wins the tie
// and the enemy steps east toward the player, then the squad
// steps west toward the enemy and fires.
var advWestWorld = new WorldZone(42);
var advWest = advWestWorld.DeploySquad(20, 5, 16, TacticalDoctrine.Advance)!;
advWest.TakeDamage(60); // 90 -> 30, 1 member
advWestWorld.Enemies.Add(new Enemy(12, 5, 16));
advWestWorld.Advance();
Eq(15, advWest.X, "slice4 advance moves toward enemy, not rally");
Eq(13, advWestWorld.Enemies[0].X, "slice4 advance enemy steps toward player on tie before squad moves");
Eq(20, advWestWorld.Enemies[0].Health, "slice4 advance weakened squad deals 10 to enemy it moved toward");
// Advance fires while NOT at its rally: adjacent enemy, rally far
// east. The step onto the enemy's cell is refused, so the squad
// stays put and fires — HoldGround would stay silent while moving.
var advFireWorld = new WorldZone(42);
var advFire = advFireWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance)!;
advFire.TakeDamage(60); // 1 member, 10 damage
advFireWorld.Player.X = 28; advFireWorld.Player.Z = 28;
advFireWorld.Enemies.Add(new Enemy(17, 5, 16));
advFireWorld.Advance();
Eq(16, advFire.X, "slice4 advance does not step onto adjacent enemy cell");
Check(!advFire.AtRally, "slice4 advance firing squad is not at its rally");
Eq(20, advFireWorld.Enemies[0].Health, "slice4 advance fires at adjacent enemy despite not at rally");
Eq(20, advFire.Health, "slice4 advance takes single adjacent damage after firing");
// Advance moves, then fires in the same tick on entering range:
// enemy 4 away, player far so the enemy pursues the squad. Enemy
// steps 20 -> 19, squad steps 16 -> 17 (distance 2), fires 10.
var advRangeWorld = new WorldZone(42);
var advRange = advRangeWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance)!;
advRange.TakeDamage(60);
advRangeWorld.Player.X = 28; advRangeWorld.Player.Z = 28;
advRangeWorld.Enemies.Add(new Enemy(20, 5, 16));
advRangeWorld.Advance();
Eq(17, advRange.X, "slice4 advance moves into range in one tick");
Eq(19, advRangeWorld.Enemies[0].X, "slice4 advance enemy pursues advancing squad");
Eq(20, advRangeWorld.Enemies[0].Health, "slice4 advance fires in the same tick it enters range");
// Advance picks the nearest enemy, ties to the first in list order:
// enemies west and east both 4 away at tick start; the player at
// the spawn is tied with both, so both enemies step toward the
// player first, leaving a 3-3 tie the squad breaks west (first).
var advTieWorld = new WorldZone(42);
var advTie = advTieWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance)!;
advTie.TakeDamage(60);
advTieWorld.Enemies.Add(new Enemy(12, 5, 16));
advTieWorld.Enemies.Add(new Enemy(20, 5, 16));
advTieWorld.Advance();
Eq(15, advTie.X, "slice4 advance tie goes to first enemy in list order");
// Advance blocked by a solid block holds its position that tick
// but still fires at the in-range enemy it cannot reach.
var advBlockWorld = new WorldZone(42);
advBlockWorld.Chunk.TrySet(17, 5, 16, BlockType.Stone);
var advBlock = advBlockWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance)!;
advBlock.TakeDamage(60);
advBlockWorld.Enemies.Add(new Enemy(20, 5, 16));
advBlockWorld.Advance();
Eq(16, advBlock.X, "slice4 advance blocked by solid block stays put");
Eq(20, advBlockWorld.Enemies[0].Health, "slice4 advance blocked squad still fires in range");
// Advance stops hunting when its prey dies: full squad kills an
// enemy on tick 1, then on tick 2 it stands still instead of
// marching to the rally it was deployed with.
var advStopWorld = new WorldZone(42);
var advStop = advStopWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance)!;
advStopWorld.Player.X = 28; advStopWorld.Player.Z = 28;
advStopWorld.Enemies.Add(new Enemy(19, 5, 16));
advStopWorld.Advance();
Eq(0, advStopWorld.Enemies.Count, "slice4 advance full squad kills enemy in range");
Eq(17, advStop.X, "slice4 advance position after killing its enemy");
advStopWorld.Advance();
Eq(17, advStop.X, "slice4 advance stands still after prey dies, no rally fallback");
// Advance persists through save/load and keeps hunting afterwards:
// loaded squad doctrine is Advance and it moves toward its enemy,
// not toward the rally stored in the save.
var advSaveWorld = new WorldZone(42);
var advSave = advSaveWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance)!;
advSave.TakeDamage(60);
advSaveWorld.Player.X = 28; advSaveWorld.Player.Z = 28;
advSaveWorld.Enemies.Add(new Enemy(20, 5, 16));
var advSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(advSaveWorld));
Eq(TacticalDoctrine.Advance, advSaveLoaded.Squads[0].Doctrine, "slice4 advance doctrine survives save/load");
Eq(24, advSaveLoaded.Squads[0].RallyX, "slice4 advance rally survives save/load");
advSaveLoaded.Advance();
Eq(17, advSaveLoaded.Squads[0].X, "slice4 advance loaded squad moves toward enemy, not rally");
Eq(20, advSaveLoaded.Enemies[0].Health, "slice4 advance loaded squad fires after moving into range");
// Picker: Advance is selectable, and a picker confirm deploys it
// through the paid path — 10 EmberOre charged, cap unchanged —
// with no new price invented for the doctrine.
var advPickSim = new Simulation(42);
Check(advPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice4 advance picker doctrine selectable");
Eq(TacticalDoctrine.Advance, advPickSim.OrbitalCommand.Doctrine, "slice4 advance picker selection sticks");
advPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
advPickSim.BeginOrbitalPick();
Check(advPickSim.SetOrbitalRally(20, 5, 16), "slice4 advance picker rally set");
var advPicked = advPickSim.ConfirmOrbitalDeploy();
Check(advPicked != null, "slice4 advance picker confirm deploys");
Eq(TacticalDoctrine.Advance, advPicked!.Doctrine, "slice4 advance picker squad doctrine");
Eq(0, advPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice4 advance picker deploy charges the settled 10 ore");
Check(!advPickSim.OrbitalCommand.IsPicking, "slice4 advance picker closes after confirm");
// Snapshot carries the new doctrine, so the Unity display can show it.
var advViewWorld = new WorldZone(42);
advViewWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
Eq(TacticalDoctrine.Advance, OrbitalView.Snapshot(advViewWorld)[0].Doctrine, "slice4 advance snapshot doctrine");

// ---- Slice 4 Step 3: demo scenario + review (0.23.0) ----
// One Simulation composing Slice 4 end to end, mirroring the demo:
// a HoldGround squad pursued by an enemy, an Advance squad
// hunting in the same world, both deployed through the paid
// picker, both weakened to 1 member so their 10-damage fire does
// not kill an enemy outright and hide the engagement, and the
// player moved far away so every enemy step is a squad pursuit.
// Enemy A starts east, enemy B west; the Advance squad hunts A,
// then turns to B after A dies. A save/load round-trip after
// tick 4 proves both doctrines keep their behaviour from the
// saved positions and health — the loaded Advance squad moves
// WEST toward B, not east toward its stored rally.
var s4Sim = new Simulation(42);
s4Sim.World.Player.Inventory.Add(BlockType.EmberOre, 20);
s4Sim.BeginOrbitalPick();
Check(s4Sim.SetOrbitalRally(20, 5, 16), "slice 4 demo picker HoldGround rally set");
var s4Hold = s4Sim.ConfirmOrbitalDeploy();
Check(s4Hold != null, "slice 4 demo paid HoldGround squad deploys");
Check(s4Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice 4 demo picker Advance doctrine selectable");
s4Sim.BeginOrbitalPick();
Check(s4Sim.SetOrbitalRally(24, 5, 16), "slice 4 demo picker Advance rally set");
var s4Adv = s4Sim.ConfirmOrbitalDeploy();
Check(s4Adv != null, "slice 4 demo paid Advance squad deploys");
Eq(TacticalDoctrine.HoldGround, s4Hold!.Doctrine, "slice 4 demo HoldGround squad doctrine");
Eq(TacticalDoctrine.Advance, s4Adv!.Doctrine, "slice 4 demo Advance squad doctrine");
Eq(0, s4Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice 4 demo two paid squads cost all 20 ore");
s4Hold.TakeDamage(60); // 90 -> 30, 1 member
s4Adv.TakeDamage(60); // 90 -> 30, 1 member
s4Sim.World.Player.X = 28; s4Sim.World.Player.Z = 28;
s4Sim.World.Enemies.Add(new Enemy(22, 5, 16));
s4Sim.World.Enemies.Add(new Enemy(10, 5, 16));
// Tick 1: both enemies step toward the squads, both squads march
// east, nobody in range yet, nobody hurt.
s4Sim.World.Advance();
Eq(17, s4Hold.X, "slice 4 demo tick 1 HoldGround marches east");
Eq(17, s4Adv.X, "slice 4 demo tick 1 Advance marches east with HoldGround");
Eq(21, s4Sim.World.Enemies[0].X, "slice 4 demo tick 1 east enemy pursues squads");
Eq(11, s4Sim.World.Enemies[1].X, "slice 4 demo tick 1 west enemy pursues squads");
Eq(30, s4Hold.Health, "slice 4 demo tick 1 squads unharmed out of range");
Eq(30, s4Adv.Health, "slice 4 demo tick 1 Advance unharmed out of range");
// Tick 2: Advance moves into range of enemy A and fires 10;
// HoldGround is still marching, so it does not fire.
s4Sim.World.Advance();
Eq(18, s4Hold.X, "slice 4 demo tick 2 HoldGround marches east");
Eq(18, s4Adv.X, "slice 4 demo tick 2 Advance marches east");
Eq(20, s4Sim.World.Enemies[0].Health, "slice 4 demo tick 2 Advance fires 10 at east enemy");
Eq(30, s4Sim.World.Enemies[1].Health, "slice 4 demo tick 2 west enemy untouched");
Eq(30, s4Hold.Health, "slice 4 demo tick 2 squads still unharmed");
// Tick 3: enemy A reaches adjacency; both squads take exactly
// 10 each (single adjacent damage, not double), and Advance
// fires A down to 10 while HoldGround, still moving, stays silent.
s4Sim.World.Advance();
Eq(18, s4Hold.X, "slice 4 demo tick 3 HoldGround blocked by adjacent enemy");
Eq(18, s4Adv.X, "slice 4 demo tick 3 Advance blocked by adjacent enemy");
Eq(10, s4Sim.World.Enemies[0].Health, "slice 4 demo tick 3 Advance fires adjacent enemy to 10");
Eq(20, s4Hold.Health, "slice 4 demo tick 3 HoldGround takes single adjacent damage");
Eq(20, s4Adv.Health, "slice 4 demo tick 3 Advance takes single adjacent damage");
// Tick 4: HoldGround takes a second 10 from the surviving enemy,
// while Advance kills that enemy and takes nothing from it —
// the no-double-count killing case, inside the composed run.
s4Sim.World.Advance();
Eq(10, s4Hold.Health, "slice 4 demo tick 4 HoldGround takes second adjacent damage");
Eq(20, s4Adv.Health, "slice 4 demo tick 4 Advance unharmed when it kills its attacker");
Eq(1, s4Sim.World.Enemies.Count, "slice 4 demo tick 4 east enemy dead, west enemy remains");
Eq(14, s4Sim.World.Enemies[0].X, "slice 4 demo tick 4 west enemy pursued squads");
Eq(30, s4Sim.World.Enemies[0].Health, "slice 4 demo tick 4 west enemy unharmed");
// Save/load after tick 4: positions, health, doctrines, rallies,
// and the spent ore all survive; the loaded game cannot deploy
// more (no ore left), even though it is under the squad cap.
var s4Json = s4Sim.SaveToJson();
var s4Loaded = new Simulation(99);
Check(s4Loaded.LoadFromJson(s4Json), "slice 4 demo save/load round-trip succeeds");
Eq(2, s4Loaded.World.Squads.Count, "slice 4 demo 2 squads survive round-trip");
Eq(TacticalDoctrine.HoldGround, s4Loaded.World.Squads[0].Doctrine, "slice 4 demo HoldGround doctrine survives round-trip");
Eq(TacticalDoctrine.Advance, s4Loaded.World.Squads[1].Doctrine, "slice 4 demo Advance doctrine survives round-trip");
Eq(20, s4Loaded.World.Squads[0].RallyX, "slice 4 demo HoldGround rally survives round-trip");
Eq(24, s4Loaded.World.Squads[1].RallyX, "slice 4 demo Advance rally survives round-trip");
Eq(18, s4Loaded.World.Squads[0].X, "slice 4 demo HoldGround position survives round-trip");
Eq(10, s4Loaded.World.Squads[0].Health, "slice 4 demo HoldGround health survives round-trip");
Eq(20, s4Loaded.World.Squads[1].Health, "slice 4 demo Advance health survives round-trip");
Eq(0, s4Loaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice 4 demo spent ore survives round-trip");
Check(!s4Loaded.CanDeployOrbitalSquad(), "slice 4 demo loaded game cannot deploy without ore");
// Tick 5 (loaded): HoldGround marches on toward its rally, but
// the loaded Advance squad moves WEST toward enemy B and fires
// it — hunting from the saved position, not marching east to
// its stored rally, which is what proves the doctrine survived.
s4Loaded.World.Advance();
Eq(19, s4Loaded.World.Squads[0].X, "slice 4 demo tick 5 loaded HoldGround marches toward rally");
Eq(17, s4Loaded.World.Squads[1].X, "slice 4 demo tick 5 loaded Advance moves west toward enemy, not rally");
Eq(20, s4Loaded.World.Enemies[0].Health, "slice 4 demo tick 5 loaded Advance fires west enemy to 20");
// Tick 6 (loaded): HoldGround reaches its rally; enemy B, now
// nearer the Advance squad than HoldGround, retargets to it and
// steps adjacent; Advance fires it to 10 and takes exactly one
// adjacent 10, while HoldGround at its rally is untouched and
// out of range, so it does not fire.
s4Loaded.World.Advance();
Eq(20, s4Loaded.World.Squads[0].X, "slice 4 demo tick 6 loaded HoldGround reaches rally");
Check(s4Loaded.World.Squads[0].AtRally, "slice 4 demo tick 6 loaded HoldGround is at rally");
Eq(17, s4Loaded.World.Squads[1].X, "slice 4 demo tick 6 loaded Advance holds against adjacent enemy");
Eq(16, s4Loaded.World.Enemies[0].X, "slice 4 demo tick 6 enemy retargets to nearer Advance squad");
Eq(10, s4Loaded.World.Enemies[0].Health, "slice 4 demo tick 6 loaded Advance fires enemy to 10");
Eq(10, s4Loaded.World.Squads[0].Health, "slice 4 demo tick 6 HoldGround at rally unharmed");
Eq(10, s4Loaded.World.Squads[1].Health, "slice 4 demo tick 6 Advance takes single adjacent damage");
var s4LoadedSnap = OrbitalView.Snapshot(s4Loaded.World);
Eq(TacticalDoctrine.Advance, s4LoadedSnap[1].Doctrine, "slice 4 demo snapshot carries Advance doctrine after load");

// ---- Slice 5 Step 1: automation tier 2, first electrical loop ----
// Source, distribution, and powered behaviour are each exactly one
// choice: one Dynamo source, face-adjacency distribution, and one
// powered behaviour (a powered Kinetic Mill runs every tick instead
// of every 5th). Cost: 5 EmberOre only.
var dynWorld = new WorldZone(42);
dynWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, dynWorld.Build(15, 4, 15, BlockType.Dynamo), "dynamo builds with 5 ember ore");
Eq(BlockType.Dynamo, dynWorld.Chunk.Get(15, 4, 15), "dynamo placed");
Eq(0, dynWorld.Player.Inventory.Get(BlockType.EmberOre), "dynamo consumes 5 ember ore");
Eq(GatherResult.NotGatherable, dynWorld.Gather(15, 4, 15), "dynamo itself is not gatherable");
var poorDynWorld = new WorldZone(42);
poorDynWorld.Player.Inventory.Add(BlockType.EmberOre, 4);
Eq(BuildResult.InsufficientResources, poorDynWorld.Build(15, 4, 15, BlockType.Dynamo), "dynamo without 5 ore fails");
Eq(BlockType.Air, poorDynWorld.Chunk.Get(15, 4, 15), "failed dynamo leaves air");
Eq(4, poorDynWorld.Player.Inventory.Get(BlockType.EmberOre), "failed dynamo does not spend ore");
var floatDynWorld = new WorldZone(42);
floatDynWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Floating, floatDynWorld.Build(16, 20, 16, BlockType.Dynamo), "dynamo floating in air fails like other machines");
// Distribution: face-adjacent only, no diagonals, no chaining.
var powWorld = new WorldZone(42);
powWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Check(powWorld.IsPowered(16, 20, 16), "mill face-adjacent to dynamo is powered");
var diagWorld = new WorldZone(42);
diagWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
diagWorld.Chunk.TrySet(17, 21, 16, BlockType.Dynamo);
Check(!diagWorld.IsPowered(16, 20, 16), "mill diagonally adjacent to dynamo is not powered");
var chainWorld = new WorldZone(42);
chainWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
chainWorld.Chunk.TrySet(11, 20, 10, BlockType.KineticMill);
chainWorld.Chunk.TrySet(12, 20, 10, BlockType.Dynamo);
Check(!chainWorld.IsPowered(10, 20, 10), "mill two cells from dynamo is not powered through another mill");
Check(chainWorld.IsPowered(11, 20, 10), "mill face-adjacent to dynamo is powered in chain setup");
// Rate proof, controlled pocket: a powered mill with 5 stone
// neighbours excavates one per tick (5 in 5 ticks); the same pocket
// without a Dynamo excavates one per 5 ticks (1 in 5 ticks).
var poweredWorld = new WorldZone(42);
poweredWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
poweredWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
poweredWorld.Chunk.TrySet(15, 20, 16, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 21, 16, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 19, 16, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 20, 17, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 20, 15, BlockType.Stone);
int powStoneBefore = poweredWorld.Player.Inventory.Get(BlockType.Stone);
for (int i = 0; i < 5; i++) poweredWorld.Advance();
Eq(5, poweredWorld.PoweredBlocksGathered, "powered mill gathers 5 blocks in 5 ticks");
Eq(0, poweredWorld.KineticBlocksGathered, "powered mill counts nothing in kinetic counter (no double activation on tick 5)");
Eq(powStoneBefore + 5, poweredWorld.Player.Inventory.Get(BlockType.Stone), "powered mill output reaches player inventory, no buffer");
Eq(BlockType.Dynamo, poweredWorld.Chunk.Get(17, 20, 16), "powered mill never excavates its dynamo");
var unpoweredWorld = new WorldZone(42);
unpoweredWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
unpoweredWorld.Chunk.TrySet(15, 20, 16, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 21, 16, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 19, 16, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 20, 17, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 20, 15, BlockType.Stone);
for (int i = 0; i < 5; i++) unpoweredWorld.Advance();
Eq(1, unpoweredWorld.KineticBlocksGathered, "unpowered mill gathers 1 block in 5 ticks");
Eq(0, unpoweredWorld.PoweredBlocksGathered, "unpowered mill counts nothing powered");
// Power is derived, not stored: removing the Dynamo drops the mill
// back to kinetic cadence on the next tick.
var lossWorld = new WorldZone(42);
lossWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
lossWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
lossWorld.Chunk.TrySet(15, 20, 16, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 21, 16, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 19, 16, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 20, 17, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 20, 15, BlockType.Stone);
lossWorld.Advance(); lossWorld.Advance();
Eq(2, lossWorld.PoweredBlocksGathered, "powered mill gathers 2 in 2 ticks before dynamo removal");
lossWorld.Chunk.TrySet(17, 20, 16, BlockType.Air);
Check(!lossWorld.IsPowered(16, 20, 16), "mill is unpowered after dynamo removal");
for (int i = 0; i < 3; i++) lossWorld.Advance();
Eq(2, lossWorld.PoweredBlocksGathered, "no powered gathers after dynamo removal");
Eq(1, lossWorld.KineticBlocksGathered, "mill falls back to kinetic cadence, gathering once on tick 5");
// Full inventory: a powered mill consumes nothing, like a kinetic one.
var fullPowWorld = new WorldZone(42);
fullPowWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
fullPowWorld.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
fullPowWorld.Chunk.TrySet(9, 20, 10, BlockType.Stone);
fullPowWorld.Player.Inventory.Add(BlockType.Stone, Inventory.MaxStack);
for (int i = 0; i < 5; i++) fullPowWorld.Advance();
Eq(BlockType.Stone, fullPowWorld.Chunk.Get(9, 20, 10), "powered mill with full inventory leaves the block");
Eq(0, fullPowWorld.PoweredBlocksGathered, "powered mill with full inventory gathers 0");
// Save/load: the Dynamo block survives as a chunk byte (no new save
// field), and the loaded mill is powered again from its placement.
var savePowWorld = new WorldZone(42);
savePowWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
savePowWorld.Chunk.TrySet(16, 4, 15, BlockType.Dynamo);
savePowWorld.Chunk.TrySet(14, 4, 15, BlockType.Stone);
var savePowLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(savePowWorld));
Eq(BlockType.KineticMill, savePowLoaded.Chunk.Get(15, 4, 15), "mill survives save/load");
Eq(BlockType.Dynamo, savePowLoaded.Chunk.Get(16, 4, 15), "dynamo survives save/load");
Check(savePowLoaded.IsPowered(15, 4, 15), "loaded mill is powered from placement, no flag needed");
savePowLoaded.Advance();
Eq(1, savePowLoaded.PoweredBlocksGathered, "loaded powered mill gathers on its first tick");
// Simulation helper: a Dynamo builds ahead, front-centre.
var simDyn = new Simulation(42);
simDyn.Tick(0.1f, 0, 0, false);
simDyn.World.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, simDyn.BuildDynamoAhead(), "simulation build dynamo ahead succeeds");
Eq(BlockType.Dynamo, simDyn.World.Chunk.Get(16, 4, 17), "simulation dynamo placed front-centre");
Eq(0, simDyn.World.Player.Inventory.Get(BlockType.EmberOre), "simulation dynamo spends its ore");

// ---- Slice 5 Step 2: demo scenario + review (composition only) ----
// Mirrors the Slice 5 demo scenario step for step: a powered pair
// built through the Simulation builders, a seeded stone pocket, an
// unpowered mill pocket as the kinetic baseline, 5 ticks, a
// save/load round-trip, a freshly seeded pocket in the loaded
// game, and 5 more ticks. No new core system rides along.
var simS5 = new Simulation(42);
simS5.Tick(0.1f, 0, 0, false);
simS5.World.Player.Inventory.Add(BlockType.Stone, 10);
simS5.World.Player.Inventory.Add(BlockType.Wood, 5);
simS5.World.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, simS5.BuildKineticMillAhead(), "slice 5 demo mill builds through simulation builder");
Eq(BuildResult.Success, simS5.BuildDynamoAhead(), "slice 5 demo dynamo builds through simulation builder");
Eq(BlockType.KineticMill, simS5.World.Chunk.Get(15, 4, 17), "slice 5 demo mill placed front-left");
Eq(BlockType.Dynamo, simS5.World.Chunk.Get(16, 4, 17), "slice 5 demo dynamo placed front-centre");
Eq(0, simS5.World.Player.Inventory.Get(BlockType.EmberOre), "slice 5 demo dynamo spends its ore");
Check(simS5.World.IsPowered(15, 4, 17), "slice 5 demo builder pair is powered with no move between builds");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS5.World.Chunk.TrySet(x, y, z, BlockType.Stone);
simS5.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
foreach (var (x, y, z) in new[] { (9, 20, 10), (10, 21, 10), (10, 19, 10), (10, 20, 11), (10, 20, 9) })
    simS5.World.Chunk.TrySet(x, y, z, BlockType.Stone);
Check(!simS5.World.IsPowered(10, 20, 10), "slice 5 demo unpowered pocket mill is not powered");
Eq(0, simS5.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo stone spent on mill, none banked before ticks");
for (int i = 0; i < 5; i++) simS5.World.Advance();
Eq(5, simS5.World.PoweredBlocksGathered, "slice 5 demo powered mill gathers 5 in 5 ticks");
Eq(1, simS5.World.KineticBlocksGathered, "slice 5 demo kinetic mill gathers 1 in 5 ticks alongside");
Eq(6, simS5.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo powered + kinetic output both reach player inventory");
Eq(BlockType.Dynamo, simS5.World.Chunk.Get(16, 4, 17), "slice 5 demo powered mill never excavates its dynamo");
var saveS5 = simS5.SaveToJson();
var simS5Loaded = new Simulation(99);
Check(simS5Loaded.LoadFromJson(saveS5), "slice 5 demo save loads");
Eq(BlockType.KineticMill, simS5Loaded.World.Chunk.Get(15, 4, 17), "slice 5 demo mill survives save/load");
Eq(BlockType.Dynamo, simS5Loaded.World.Chunk.Get(16, 4, 17), "slice 5 demo dynamo survives save/load");
Check(simS5Loaded.World.IsPowered(15, 4, 17), "slice 5 demo loaded mill is powered from placement, no flag needed");
Eq(0, simS5Loaded.World.PoweredBlocksGathered, "slice 5 demo session powered counter resets on load");
Eq(0, simS5Loaded.World.KineticBlocksGathered, "slice 5 demo session kinetic counter resets on load");
Eq(6, simS5Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo gathered stone survives save/load");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS5Loaded.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS5Loaded.World.Advance();
Eq(5, simS5Loaded.World.PoweredBlocksGathered, "slice 5 demo loaded powered mill gathers 5 in 5 ticks");
Eq(1, simS5Loaded.World.KineticBlocksGathered, "slice 5 demo loaded kinetic mill gathers 1 in 5 ticks alongside");
Eq(12, simS5Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo loaded stone total is pre-load 6 plus loaded 6");
Eq(BlockType.Dynamo, simS5Loaded.World.Chunk.Get(16, 4, 17), "slice 5 demo loaded powered mill never excavates its dynamo");

// ---- Slice 6 Step 1: FPS pitch aiming (core Look) ----
// Decides the scope's open questions in core: pitch lives in a
// testable core Look (yaw + clamped pitch + forward vector), the
// adapter feeds it scaled mouse deltas and renders from it, and
// camera + picker are the only consumers in this step — gather,
// build, fire, and movement keep their Slice 1 rules.
var look0 = new Look();
Eq(0f, look0.Yaw, "look starts at yaw 0");
Eq(0f, look0.Pitch, "look starts at pitch 0");
var f0 = look0.Forward();
Check(Math.Abs(f0.X) < 0.001f && Math.Abs(f0.Y) < 0.001f && Math.Abs(f0.Z - 1f) < 0.001f, $"look forward at 0,0 is +Z (got {f0.X},{f0.Y},{f0.Z})");
var lookYaw = new Look();
lookYaw.Add(90f, 0f);
var fYaw = lookYaw.Forward();
Check(Math.Abs(fYaw.X - 1f) < 0.001f && Math.Abs(fYaw.Y) < 0.001f && Math.Abs(fYaw.Z) < 0.001f, $"look yaw 90 faces +X (got {fYaw.X},{fYaw.Y},{fYaw.Z})");
var lookBack = new Look();
lookBack.Add(180f, 0f);
var fBack = lookBack.Forward();
Check(Math.Abs(fBack.X) < 0.001f && Math.Abs(fBack.Y) < 0.001f && Math.Abs(fBack.Z + 1f) < 0.001f, $"look yaw 180 faces -Z (got {fBack.X},{fBack.Y},{fBack.Z})");
var lookLeft = new Look();
lookLeft.Add(-90f, 0f);
var fLeft = lookLeft.Forward();
Check(Math.Abs(fLeft.X + 1f) < 0.001f && Math.Abs(fLeft.Z) < 0.001f, $"look yaw -90 faces -X (got {fLeft.X},{fLeft.Y},{fLeft.Z})");
var lookUp = new Look();
lookUp.Add(0f, 45f);
var fUp = lookUp.Forward();
Check(Math.Abs(fUp.X) < 0.001f && Math.Abs(fUp.Y - 0.7071f) < 0.001f && Math.Abs(fUp.Z - 0.7071f) < 0.001f, $"look pitch 45 lifts forward equally up and ahead (got {fUp.X},{fUp.Y},{fUp.Z})");
var lookDown = new Look();
lookDown.Add(0f, -45f);
var fDown = lookDown.Forward();
Check(Math.Abs(fDown.Y + 0.7071f) < 0.001f && Math.Abs(fDown.Z - 0.7071f) < 0.001f, $"look pitch -45 drops forward equally down and ahead (got {fDown.X},{fDown.Y},{fDown.Z})");
var lookCombo = new Look();
lookCombo.Add(90f, 45f);
var fCombo = lookCombo.Forward();
Check(Math.Abs(fCombo.X - 0.7071f) < 0.001f && Math.Abs(fCombo.Y - 0.7071f) < 0.001f && Math.Abs(fCombo.Z) < 0.001f, $"look yaw 90 pitch 45 faces +X and up, no +Z (got {fCombo.X},{fCombo.Y},{fCombo.Z})");
var lookClampUp = new Look();
lookClampUp.Add(0f, 200f);
Eq(89f, lookClampUp.Pitch, "look pitch clamps at +89, never past vertical");
Check(lookClampUp.ForwardY > 0.999f && Math.Abs(lookClampUp.ForwardZ) < 0.02f, $"look clamped-up forward is almost straight up (got Y {lookClampUp.ForwardY}, Z {lookClampUp.ForwardZ})");
var lookClampDown = new Look();
lookClampDown.Add(0f, -200f);
Eq(-89f, lookClampDown.Pitch, "look pitch clamps at -89, never past vertical");
var lookClampStep = new Look();
lookClampStep.Add(0f, 80f);
lookClampStep.Add(0f, 20f);
Eq(89f, lookClampStep.Pitch, "look pitch clamps in steps too, 80 + 20 stops at 89 not 100");
var lookWrap = new Look();
lookWrap.Add(360f, 0f);
Eq(360f, lookWrap.Yaw, "look yaw accumulates unbounded, no hidden normalisation");
var fWrap = lookWrap.Forward();
Check(Math.Abs(fWrap.X) < 0.001f && Math.Abs(fWrap.Z - 1f) < 0.001f, $"look yaw 360 faces +Z again (got {fWrap.X},{fWrap.Y},{fWrap.Z})");
var lookUnit = new Look();
lookUnit.Add(30f, -20f);
var fUnit = lookUnit.Forward();
double unitLen = Math.Sqrt(fUnit.X * fUnit.X + fUnit.Y * fUnit.Y + fUnit.Z * fUnit.Z);
Check(Math.Abs(unitLen - 1.0) < 0.001, $"look forward vector is unit length (got {unitLen})");
// Simulation owns one Look; the adapter feeds it, it holds no yaw of its own.
var simLook = new Simulation(42);
Eq(0f, simLook.Look.Yaw, "simulation look starts at yaw 0");
Eq(0f, simLook.Look.Pitch, "simulation look starts at pitch 0");
simLook.ApplyLook(90f, 45f);
Eq(90f, simLook.Look.Yaw, "simulation apply look sets yaw");
Eq(45f, simLook.Look.Pitch, "simulation apply look sets pitch");
Check(Math.Abs(simLook.Look.ForwardX - 0.7071f) < 0.001f, $"simulation look forward X follows yaw+pitch (got {simLook.Look.ForwardX})");
// Look is session state, not saved: a load into a fresh Simulation
// starts looking forward, whatever the saving Simulation looked at.
var simLookSave = new Simulation(42);
simLookSave.ApplyLook(30f, -20f);
var lookSaveJson = simLookSave.SaveToJson();
var simLookLoaded = new Simulation(99);
Check(simLookLoaded.LoadFromJson(lookSaveJson), "look save/load round-trip succeeds");
Eq(0f, simLookLoaded.Look.Yaw, "look yaw is not saved: loaded simulation starts at yaw 0");
Eq(0f, simLookLoaded.Look.Pitch, "look pitch is not saved: loaded simulation starts at pitch 0");
// …and an in-place restore keeps the current session's view, so a
// Unity load into the same Simulation does not snap the camera.
var simLookRestore = new Simulation(42);
simLookRestore.ApplyLook(30f, -20f);
Check(simLookRestore.RestoreWorld(new WorldZone(9)), "look restore world succeeds");
Eq(30f, simLookRestore.Look.Yaw, "look yaw survives in-place restore");
Eq(-20f, simLookRestore.Look.Pitch, "look pitch survives in-place restore");
// What pitch does NOT change in this step, proved, not asserted:
// gather, build, and fire keep their Slice 1 cells/rules while
// pitched up 45 degrees, and movement stays world-aligned even
// while yawed 90 degrees (yaw-rotated movement is a later step).
var simPitch = new Simulation(42);
simPitch.Tick(0.1f, 0, 0, false);
simPitch.ApplyLook(0f, 45f);
Check(simPitch.GatherAhead() == GatherResult.Success, "pitched look does not change gather ahead cell");
simPitch.World.Player.Inventory.Add(BlockType.Stone, 10);
Check(simPitch.BuildWallAhead() == BuildResult.Success, "pitched look does not change build wall ahead cell");
simPitch.World.SpawnWave(1);
for (int i = 0; i < 3; i++) simPitch.FireAtNearestEnemy();
Eq(1, simPitch.EnemiesKilled, "pitched look does not change fire at nearest enemy rule");
var simYawMove = new Simulation(42);
simYawMove.Tick(0.1f, 0, 0, false);
float moveStartX = simYawMove.Fps.X, moveStartZ = simYawMove.Fps.Z;
simYawMove.ApplyLook(90f, 0f);
simYawMove.Tick(0.1f, 1f, 0f, false);
Check(Math.Abs(simYawMove.Fps.X - moveStartX) < 0.001f, $"yaw 90 does not rotate movement into +X (X moved {simYawMove.Fps.X - moveStartX})");
Check(simYawMove.Fps.Z > moveStartZ, $"yaw 90 movement stays world-aligned +Z (Z moved {simYawMove.Fps.Z - moveStartZ})");

// ---- Slice 6 Step 2: squad activity word + hunting target (0.29.0) ----
// HoldGround reads by its rally: marching while away, holding at it, no prey ever.
var actMarchWorld = new WorldZone(42);
actMarchWorld.DeploySquad(20, 5, 16);
var actMarch = OrbitalView.Snapshot(actMarchWorld)[0];
Eq(SquadActivity.Marching, actMarch.Activity, "slice6 HoldGround marching squad reads MARCHING");
Check(!actMarch.HasHuntingTarget, "slice6 marching squad has no hunting target");
Check(actMarch.HuntingTargetX == null, "slice6 marching squad hunting target cell is null");
var actHoldWorld = new WorldZone(42);
actHoldWorld.DeploySquad(16, 5, 16);
var actHold = OrbitalView.Snapshot(actHoldWorld)[0];
Eq(SquadActivity.Holding, actHold.Activity, "slice6 HoldGround squad at rally reads HOLDING");
// Advance with a living enemy reads HUNTING even beyond engagement
// range (4 > 3): hunting is not firing, and the old rally-relative
// word would have called this same squad MOVING toward a rally it
// ignores. The target is the prey cell, not the rally.
var actHuntWorld = new WorldZone(42);
actHuntWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
actHuntWorld.Enemies.Add(new Enemy(20, 5, 16));
var actHunt = OrbitalView.Snapshot(actHuntWorld)[0];
Eq(SquadActivity.Hunting, actHunt.Activity, "slice6 Advance squad beyond range reads HUNTING not MOVING");
Check(actHunt.HasHuntingTarget, "slice6 hunting squad has a hunting target");
Eq(20, actHunt.HuntingTargetX, "slice6 hunting target X is the prey cell");
Eq(5, actHunt.HuntingTargetY, "slice6 hunting target Y is the prey cell");
Eq(16, actHunt.HuntingTargetZ, "slice6 hunting target Z is the prey cell");
// Advance adjacent to its prey also reads HUNTING: the old word
// would have read HOLDING (at its rally) or MOVING (away from it)
// for a squad that is, in fact, standing and firing.
var actAdjWorld = new WorldZone(42);
actAdjWorld.DeploySquad(16, 5, 16, TacticalDoctrine.Advance);
actAdjWorld.Enemies.Add(new Enemy(17, 5, 16));
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(actAdjWorld)[0].Activity, "slice6 Advance adjacent to prey reads HUNTING");
// Advance with no living enemy reads IDLE and stands still —
// documented behaviour, not a bug to hide — and a dead enemy
// alone does not make it hunt.
var actIdleWorld = new WorldZone(42);
actIdleWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
Eq(SquadActivity.Idle, OrbitalView.Snapshot(actIdleWorld)[0].Activity, "slice6 Advance with no enemy reads IDLE");
var actDeadWorld = new WorldZone(42);
actDeadWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
var actDeadEnemy = new Enemy(20, 5, 16);
actDeadWorld.Enemies.Add(actDeadEnemy);
actDeadWorld.AttackEnemy(actDeadEnemy, 30);
Eq(SquadActivity.Idle, OrbitalView.Snapshot(actDeadWorld)[0].Activity, "slice6 Advance with only a dead enemy reads IDLE");
// Hunting target ties go to the first enemy in list order, the
// same rule movement and fire use, so display cannot disagree
// with either about the prey.
var actTieWorld = new WorldZone(42);
actTieWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
actTieWorld.Enemies.Add(new Enemy(12, 5, 16));
actTieWorld.Enemies.Add(new Enemy(20, 5, 16));
var actTie = OrbitalView.Snapshot(actTieWorld)[0];
Eq(12, actTie.HuntingTargetX, "slice6 hunting tie goes to first enemy in list order");
// The target is re-derived on every snapshot, not stored: kill
// the first-listed prey (both prey are 4 away, so the first
// wins the tie) and the next snapshot hunts the other one,
// while the old snapshot still shows the old prey (copy, as
// with position since 0.11.0).
var actReWorld = new WorldZone(42);
actReWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
var actPreyNear = new Enemy(20, 5, 16);
var actPreyFar = new Enemy(12, 5, 16);
actReWorld.Enemies.Add(actPreyNear);
actReWorld.Enemies.Add(actPreyFar);
var actBefore = OrbitalView.Snapshot(actReWorld)[0];
Eq(20, actBefore.HuntingTargetX, "slice6 hunting target before kill is the nearest prey");
actReWorld.AttackEnemy(actPreyNear, 30);
var actAfter = OrbitalView.Snapshot(actReWorld)[0];
Eq(12, actAfter.HuntingTargetX, "slice6 hunting target re-derived after prey dies");
Eq(20, actBefore.HuntingTargetX, "slice6 old snapshot keeps its old hunting target");
Eq(SquadActivity.Hunting, actAfter.Activity, "slice6 squad still hunting after retargeting");
// Activity is derived, not saved: an Advance squad round-trips
// through save/load and still reads HUNTING at the same prey,
// with no new save field created for it.
var actSaveWorld = new WorldZone(42);
actSaveWorld.DeploySquad(24, 5, 16, TacticalDoctrine.Advance);
actSaveWorld.Enemies.Add(new Enemy(20, 5, 16));
var actSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(actSaveWorld));
var actSaveSnap = OrbitalView.Snapshot(actSaveLoaded)[0];
Eq(SquadActivity.Hunting, actSaveSnap.Activity, "slice6 hunting activity survives save/load by derivation");
Eq(20, actSaveSnap.HuntingTargetX, "slice6 hunting target survives save/load by derivation");
// The old rally-relative words are gone from the core display
// contract: AtRally / IsMoving remain as position facts (kept
// for existing tests), but Activity is the display word, and a
// hunting squad's Activity contradicts neither — it is simply
// not a function of AtRally at all.
Check(actHunt.AtRally == false && actHunt.Activity == SquadActivity.Hunting, "slice6 hunting word is independent of rally position");

// ---- Slice 6 Step 3: powered Unity presence (core indicator only) ----
// The indicator is a HudState count derived from placement through
// WorldZone.IsPowered, not a new core rule and not a new save field.
// The adapter keys (T mill, Y Dynamo) and the HUD text line cannot
// be compiled headlessly, so these tests prove the numbers the
// adapter will render — the same core/adapter split as Steps 1-2.
var powHudFresh = HudState.FromWorld(new WorldZone(42), new Weapon());
Eq(0, powHudFresh.TotalMills, "slice6 fresh world has no mills for the power indicator");
Eq(0, powHudFresh.PoweredMills, "slice6 fresh world has no powered mills");
var powHudUn = new WorldZone(42);
powHudUn.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
var powHudUnSnap = HudState.FromWorld(powHudUn, new Weapon());
Eq(1, powHudUnSnap.TotalMills, "slice6 unpowered mill counts as one mill");
Eq(0, powHudUnSnap.PoweredMills, "slice6 unpowered mill counts as zero powered");
var powHudPair = new WorldZone(42);
powHudPair.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powHudPair.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
var powHudPairSnap = HudState.FromWorld(powHudPair, new Weapon());
Eq(1, powHudPairSnap.TotalMills, "slice6 powered pair counts as one mill");
Eq(1, powHudPairSnap.PoweredMills, "slice6 powered pair counts as one powered mill");
// Two mills, one powered: the count is per mill, not per Dynamo,
// and the chained mill (two cells from the Dynamo, through the
// powered mill) stays unpowered, per the Slice 5 distribution rule.
var powHudTwo = new WorldZone(42);
powHudTwo.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
powHudTwo.Chunk.TrySet(11, 20, 10, BlockType.KineticMill);
powHudTwo.Chunk.TrySet(12, 20, 10, BlockType.Dynamo);
var powHudTwoSnap = HudState.FromWorld(powHudTwo, new Weapon());
Eq(2, powHudTwoSnap.TotalMills, "slice6 two mills count as two");
Eq(1, powHudTwoSnap.PoweredMills, "slice6 only the face-adjacent mill of two counts powered");
// A diagonal Dynamo powers nothing, and neither a Dynamo alone
// nor a wall next to a Dynamo counts as a mill at all.
var powHudDiag = new WorldZone(42);
powHudDiag.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powHudDiag.Chunk.TrySet(17, 21, 16, BlockType.Dynamo);
Eq(0, HudState.FromWorld(powHudDiag, new Weapon()).PoweredMills, "slice6 diagonal dynamo powers no mill");
var powHudDynOnly = new WorldZone(42);
powHudDynOnly.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
var powHudDynOnlySnap = HudState.FromWorld(powHudDynOnly, new Weapon());
Eq(0, powHudDynOnlySnap.TotalMills, "slice6 a dynamo alone is not a mill");
Eq(0, powHudDynOnlySnap.PoweredMills, "slice6 a dynamo alone is not a powered mill");
var powHudWall = new WorldZone(42);
powHudWall.Chunk.TrySet(16, 20, 16, BlockType.Wall);
powHudWall.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Eq(0, HudState.FromWorld(powHudWall, new Weapon()).TotalMills, "slice6 a wall next to a dynamo is not a mill");
// Derived, not stored: removing the Dynamo drops the count on
// the next snapshot, with no save or flag involved.
var powHudLoss = new WorldZone(42);
powHudLoss.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powHudLoss.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Eq(1, HudState.FromWorld(powHudLoss, new Weapon()).PoweredMills, "slice6 powered count before dynamo removal is one");
powHudLoss.Chunk.TrySet(17, 20, 16, BlockType.Air);
Eq(0, HudState.FromWorld(powHudLoss, new Weapon()).PoweredMills, "slice6 powered count after dynamo removal is zero");
Eq(1, HudState.FromWorld(powHudLoss, new Weapon()).TotalMills, "slice6 mill itself survives dynamo removal in the count");
// Derived across save/load too: the pair survives as chunk bytes
// (no new save field), and the loaded snapshot counts it again.
var powHudSave = new WorldZone(42);
powHudSave.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
powHudSave.Chunk.TrySet(16, 4, 15, BlockType.Dynamo);
var powHudSaveSnap = HudState.FromWorld(SaveSystem.LoadZone(SaveSystem.SaveZone(powHudSave)), new Weapon());
Eq(1, powHudSaveSnap.TotalMills, "slice6 loaded powered pair counts as one mill");
Eq(1, powHudSaveSnap.PoweredMills, "slice6 loaded powered pair counts as one powered mill");
// Through the real Simulation builders, with no move between
// the two builds: mill front-left, Dynamo front-centre, and the
// two cells are face-adjacent, so Simulation.Hud() — the exact
// call HudDisplay renders — reads 1/1. A mill built alone reads
// 0/1, so the indicator distinguishes the pair from the mill.
var powSim = new Simulation(42);
powSim.Tick(0.1f, 0, 0, false);
powSim.World.Player.Inventory.Add(BlockType.Stone, 10);
powSim.World.Player.Inventory.Add(BlockType.Wood, 5);
powSim.World.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, powSim.BuildKineticMillAhead(), "slice6 simulation mill ahead succeeds for the indicator pair");
Eq(BuildResult.Success, powSim.BuildDynamoAhead(), "slice6 simulation dynamo ahead succeeds for the indicator pair");
Eq(1, powSim.Hud().TotalMills, "slice6 simulation builder pair counts as one mill in Hud()");
Eq(1, powSim.Hud().PoweredMills, "slice6 simulation builder pair counts as one powered mill in Hud()");
var powSimSolo = new Simulation(42);
powSimSolo.Tick(0.1f, 0, 0, false);
powSimSolo.World.Player.Inventory.Add(BlockType.Stone, 10);
powSimSolo.World.Player.Inventory.Add(BlockType.Wood, 5);
Eq(BuildResult.Success, powSimSolo.BuildKineticMillAhead(), "slice6 simulation mill alone succeeds for the indicator");
Eq(1, powSimSolo.Hud().TotalMills, "slice6 simulation mill alone counts as one mill in Hud()");
Eq(0, powSimSolo.Hud().PoweredMills, "slice6 simulation mill alone counts as zero powered in Hud()");

// ---- Slice 6 Step 4: demo scenario + review (composition only) ----
// Mirrors the Slice 6 demo scenario step for step: a clamped Look
// and its forward vector; a HoldGround squad reading Marching
// then Holding; an Advance squad reading Hunting then Idle as
// its prey lives and dies; and a powered pair built through the
// Simulation builders, through a save/load round-trip, with
// the loaded loop running again at the powered rate. No new
// core system rides along — every piece below was proved in
// steps 1-3, and this section proves only that they compose.
var simS6 = new Simulation(42);
var s6f0 = simS6.Look.Forward();
Check(Math.Abs(s6f0.X) < 0.001f && Math.Abs(s6f0.Y) < 0.001f && Math.Abs(s6f0.Z - 1f) < 0.001f, "slice 6 demo look starts facing +Z");
simS6.ApplyLook(90f, 45f);
var s6f1 = simS6.Look.Forward();
Check(Math.Abs(s6f1.X - 0.7071f) < 0.001f && Math.Abs(s6f1.Y - 0.7071f) < 0.001f && Math.Abs(s6f1.Z) < 0.001f, "slice 6 demo look at 90 yaw / 45 pitch faces diagonally up");
simS6.ApplyLook(0f, 100f);
Eq(89f, simS6.Look.Pitch, "slice 6 demo look pitch clamps at 89 after +100 more");
var s6f2 = simS6.Look.Forward();
Check(s6f2.Y > 0.999f && Math.Abs(s6f2.X - 0.0175f) < 0.001f, "slice 6 demo clamped look faces almost straight up");
simS6.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS6.BeginOrbitalPick();
simS6.SetOrbitalRally(20, 5, 16);
var s6Hold = simS6.ConfirmOrbitalDeploy();
Check(s6Hold != null, "slice 6 demo HoldGround squad deploys through the paid picker");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(simS6.World)[0].Activity, "slice 6 demo HoldGround squad reads MARCHING before marching");
for (int i = 0; i < 4; i++) simS6.World.Advance();
var s6HoldSnap = OrbitalView.Snapshot(simS6.World)[0];
Eq(SquadActivity.Holding, s6HoldSnap.Activity, "slice 6 demo HoldGround squad reads HOLDING at its rally after 4 ticks");
Eq(20, s6HoldSnap.X, "slice 6 demo HoldGround squad reaches its rally x after 4 ticks");
Eq(90, s6HoldSnap.Health, "slice 6 demo HoldGround squad takes no damage marching with no enemy");
simS6.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
simS6.BeginOrbitalPick();
simS6.SetOrbitalRally(24, 5, 16);
var s6Adv = simS6.ConfirmOrbitalDeploy();
Check(s6Adv != null, "slice 6 demo Advance squad deploys through the paid picker");
Eq(0, simS6.World.Player.Inventory.Get(BlockType.EmberOre), "slice 6 demo two paid squads spend both 10-ore deploys");
simS6.World.Enemies.Add(new Enemy(16, 5, 20));
var s6AdvSnap = OrbitalView.Snapshot(simS6.World)[1];
Eq(SquadActivity.Hunting, s6AdvSnap.Activity, "slice 6 demo Advance squad reads HUNTING with a live prey");
Eq(20, s6AdvSnap.HuntingTargetZ, "slice 6 demo Advance hunting target is the live prey cell");
simS6.World.Advance();
Eq(0, simS6.World.Enemies.Count, "slice 6 demo full-strength Advance squad kills its 30-HP prey in one engagement tick");
var s6AdvAfter = OrbitalView.Snapshot(simS6.World)[1];
Eq(SquadActivity.Idle, s6AdvAfter.Activity, "slice 6 demo Advance squad reads IDLE once its prey is dead");
Eq(16, s6AdvAfter.X, "slice 6 demo Advance squad does not march to its ignored rally after the kill");
Check(s6AdvAfter.Z != 16 || s6AdvAfter.X != 16, "slice 6 demo Advance squad moved toward its prey before killing it");
Eq(90, s6AdvAfter.Health, "slice 6 demo Advance squad takes no damage: its prey dies from squad fire before adjacent damage");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(simS6.World)[0].Activity, "slice 6 demo HoldGround squad still reads HOLDING after the Advance engagement");
simS6.World.Player.Inventory.Add(BlockType.Stone, 10);
simS6.World.Player.Inventory.Add(BlockType.Wood, 5);
simS6.World.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, simS6.BuildKineticMillAhead(), "slice 6 demo mill builds through simulation builder");
Eq(BuildResult.Success, simS6.BuildDynamoAhead(), "slice 6 demo dynamo builds through simulation builder");
var s6Hud = simS6.Hud();
Eq(1, s6Hud.TotalMills, "slice 6 demo power indicator counts one mill");
Eq(1, s6Hud.PoweredMills, "slice 6 demo power indicator counts one powered mill");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS6.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS6.World.Advance();
Eq(5, simS6.World.PoweredBlocksGathered, "slice 6 demo powered mill gathers 5 in 5 ticks");
Eq(5, simS6.World.Player.Inventory.Get(BlockType.Stone), "slice 6 demo powered output reaches player inventory");
var saveS6 = simS6.SaveToJson();
var simS6Loaded = new Simulation(99);
Check(simS6Loaded.LoadFromJson(saveS6), "slice 6 demo save loads");
Eq(BlockType.KineticMill, simS6Loaded.World.Chunk.Get(15, 4, 17), "slice 6 demo mill survives save/load");
Eq(BlockType.Dynamo, simS6Loaded.World.Chunk.Get(16, 4, 17), "slice 6 demo dynamo survives save/load");
Eq(1, simS6Loaded.Hud().PoweredMills, "slice 6 demo loaded power indicator still counts one powered mill");
Eq(0, simS6Loaded.World.PoweredBlocksGathered, "slice 6 demo session powered counter resets on load");
Eq(2, OrbitalView.Snapshot(simS6Loaded.World).Count, "slice 6 demo both squads survive save/load");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(simS6Loaded.World)[0].Activity, "slice 6 demo loaded HoldGround squad still reads HOLDING by derivation");
Eq(SquadActivity.Idle, OrbitalView.Snapshot(simS6Loaded.World)[1].Activity, "slice 6 demo loaded Advance squad still reads IDLE by derivation");
Eq(0f, simS6Loaded.Look.Pitch, "slice 6 demo Look is not saved: a fresh loaded simulation starts at pitch 0");
var s6LoadedF = simS6Loaded.Look.Forward();
Check(Math.Abs(s6LoadedF.Z - 1f) < 0.001f, "slice 6 demo loaded Look faces +Z, not the pre-save view");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS6Loaded.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS6Loaded.World.Advance();
Eq(5, simS6Loaded.World.PoweredBlocksGathered, "slice 6 demo loaded powered mill gathers 5 in 5 ticks");
// The demo's own save length, as a composition fingerprint: the
// Slice 6 save carries two squads, a mill, and a Dynamo.
Eq(44024, saveS6.Length, "slice 6 demo save JSON length matches the demo run");

// ---- Slice 7 Step 1: second squad type — Recon (chosen from recon / heavy / flyer) ----
// Recon is the long-range screen Vanguard cannot be: it engages
// from range 5, but pays with fragility (2 x 20 HP = 40, against
// Vanguard's 3 x 30 = 90) and low firepower (2 x 6 = 12 per tick,
// against Vanguard's 30). It is better than Vanguard in exactly
// one dimension (range) and worse in the other two, so it is a
// choice, not a replacement. Heavy was not chosen: a tougher
// anchor under HoldGround is Vanguard with more HP, not a new
// tactical choice. Flyer was not chosen: it would need a new
// movement rule (ignoring solids / air) with targeting and
// occupancy consequences, which would make the kind untestable
// in isolation — the scope's own warning. Movement stays the
// shared one-voxel rule for both kinds.
// Per-kind shape table: Vanguard's numbers are unchanged since 0.9.0.
Eq(3, SquadShapes.For(SquadKind.Vanguard).Members, "slice7 vanguard shape members unchanged");
Eq(30, SquadShapes.For(SquadKind.Vanguard).MemberHealth, "slice7 vanguard shape member health unchanged");
Eq(10, SquadShapes.For(SquadKind.Vanguard).MemberDamage, "slice7 vanguard shape member damage unchanged");
Eq(3, SquadShapes.For(SquadKind.Vanguard).EngagementRange, "slice7 vanguard shape range unchanged");
Eq(2, SquadShapes.For(SquadKind.Recon).Members, "slice7 recon shape members");
Eq(20, SquadShapes.For(SquadKind.Recon).MemberHealth, "slice7 recon shape member health");
Eq(6, SquadShapes.For(SquadKind.Recon).MemberDamage, "slice7 recon shape member damage");
Eq(5, SquadShapes.For(SquadKind.Recon).EngagementRange, "slice7 recon shape range");
bool shapeThrows = false;
try { SquadShapes.For((SquadKind)99); } catch (ArgumentOutOfRangeException) { shapeThrows = true; }
Check(shapeThrows, "slice7 unknown kind has no shape, throws rather than falling back to Vanguard");
// A deployed Recon squad reads its own shape, not Vanguard's.
var reconWorld = new WorldZone(42);
var recon = reconWorld.DeploySquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
Eq(SquadKind.Recon, recon.Kind, "slice7 recon deploy kind");
Eq(40, recon.Health, "slice7 recon full health is 40, not 90");
Eq(40, recon.MaxHealth, "slice7 recon max health is 40");
Eq(2, recon.AliveMembers, "slice7 recon full members");
Eq(12, recon.DamagePerTick, "slice7 recon full damage per tick");
var vanWorld = new WorldZone(42);
var van = vanWorld.DeploySquad(20, 5, 16)!;
Eq(90, van.Health, "slice7 vanguard full health still 90");
Eq(3, van.AliveMembers, "slice7 vanguard full members still 3");
Eq(30, van.DamagePerTick, "slice7 vanguard full damage still 30");
// Firepower drops in member steps, per kind: Recon 40 -> 20 HP
// is 1 member dealing 6, and 40 damage kills it outright.
recon.TakeDamage(20);
Eq(20, recon.Health, "slice7 recon wounded health");
Eq(1, recon.AliveMembers, "slice7 recon wounded has 1 member");
Eq(6, recon.DamagePerTick, "slice7 recon wounded damage per tick");
recon.TakeDamage(20);
Check(recon.IsDead, "slice7 recon at 0 health is dead");
Eq(0, recon.DamagePerTick, "slice7 dead recon deals nothing");
// Recon fires at range 5: enemy starts 6 away, steps to 5 in the
// enemy phase (player far, so the squad is the nearer target),
// then the holding Recon fires 12. Vanguard at the same geometry
// stays silent, and no kind fires at range 6.
var reconRangeWorld = new WorldZone(42);
var reconRange = reconRangeWorld.DeploySquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
reconRangeWorld.Player.X = 28; reconRangeWorld.Player.Z = 28;
reconRangeWorld.Enemies.Add(new Enemy(22, 5, 16));
reconRangeWorld.Advance();
Eq(21, reconRangeWorld.Enemies[0].X, "slice7 recon range enemy steps to distance 5");
Eq(18, reconRangeWorld.Enemies[0].Health, "slice7 recon fires at range 5 for 12");
var reconFarWorld = new WorldZone(42);
reconFarWorld.DeploySquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
reconFarWorld.Player.X = 28; reconFarWorld.Player.Z = 28;
reconFarWorld.Enemies.Add(new Enemy(23, 5, 16));
reconFarWorld.Advance();
Eq(22, reconFarWorld.Enemies[0].X, "slice7 recon range-6 enemy steps to distance 6");
Eq(30, reconFarWorld.Enemies[0].Health, "slice7 recon does not fire at range 6");
var vanRangeWorld = new WorldZone(42);
vanRangeWorld.DeploySquad(16, 5, 16);
vanRangeWorld.Player.X = 28; vanRangeWorld.Player.Z = 28;
vanRangeWorld.Enemies.Add(new Enemy(21, 5, 16));
vanRangeWorld.Advance();
Eq(20, vanRangeWorld.Enemies[0].X, "slice7 vanguard range enemy steps to distance 4");
Eq(30, vanRangeWorld.Enemies[0].Health, "slice7 vanguard does not fire at range 4, range still 3");
// Movement is the shared rule: a Recon HoldGround squad marches
// one voxel per tick toward its rally, exactly like Vanguard.
var reconMarchWorld = new WorldZone(42);
var reconMarch = reconMarchWorld.DeploySquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
reconMarchWorld.Advance();
Eq(17, reconMarch.X, "slice7 recon marches one voxel toward rally");
// Both doctrines work for Recon: an Advance Recon hunts west
// toward its enemy, not east toward its rally, and fires.
var reconAdvWorld = new WorldZone(42);
var reconAdv = reconAdvWorld.DeploySquad(20, 5, 16, TacticalDoctrine.Advance, SquadKind.Recon)!;
reconAdvWorld.Player.X = 28; reconAdvWorld.Player.Z = 28;
reconAdvWorld.Enemies.Add(new Enemy(12, 5, 16));
reconAdvWorld.Advance();
Eq(15, reconAdv.X, "slice7 advance recon moves toward enemy, not rally");
Eq(13, reconAdvWorld.Enemies[0].X, "slice7 advance recon enemy pursues squad");
Eq(18, reconAdvWorld.Enemies[0].Health, "slice7 advance recon fires 12 in range");
// Economy: one price, total cap. CostForKind is 10 for both
// kinds, so the prompt's COST 10 line stays true; the cap is
// 3 live squads in total, not 3 per kind.
Eq(10, SquadEconomy.CostForKind(SquadKind.Vanguard), "slice7 vanguard price is 10");
Eq(10, SquadEconomy.CostForKind(SquadKind.Recon), "slice7 recon price is 10");
var econWorld = new WorldZone(42);
econWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
var econRecon = econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
Eq(SquadKind.Recon, econRecon.Kind, "slice7 paid recon deploy kind");
Eq(40, econRecon.Health, "slice7 paid recon deploy health");
Eq(30, econWorld.Player.Inventory.Get(BlockType.EmberOre), "slice7 paid recon deploy charges exactly 10");
econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
Eq(3, SquadEconomy.LiveSquadCount(econWorld), "slice7 mixed kinds count toward one total cap");
Check(econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon) == null, "slice7 fourth squad refused at total cap, not per-kind cap");
Eq(10, econWorld.Player.Inventory.Get(BlockType.EmberOre), "slice7 refused over-cap deploy charges nothing");
var econPoorWorld = new WorldZone(42);
Check(econPoorWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon) == null, "slice7 unfunded recon deploy refused");
Eq(0, econPoorWorld.Squads.Count, "slice7 unfunded recon deploy adds no squad");
// Picker: doctrine and kind are independent choices now. A kind
// selection survives a doctrine change, an unknown kind is
// rejected, and a Confirm deploys the selected combination.
var reconPickSim = new Simulation(42);
reconPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
reconPickSim.BeginOrbitalPick();
Check(reconPickSim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice7 picker selects recon kind");
Check(reconPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice7 picker selects doctrine after kind");
Eq(SquadKind.Recon, reconPickSim.OrbitalCommand.Kind, "slice7 doctrine change keeps selected kind");
Check(!reconPickSim.OrbitalCommand.SelectKind((SquadKind)99), "slice7 picker rejects unknown kind");
Eq(SquadKind.Recon, reconPickSim.OrbitalCommand.Kind, "slice7 rejected kind leaves selection unchanged");
reconPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
reconPickSim.SetOrbitalRally(20, 5, 16);
var reconPicked = reconPickSim.ConfirmOrbitalDeploy()!;
Eq(SquadKind.Recon, reconPicked.Kind, "slice7 picker confirm deploys recon");
Eq(TacticalDoctrine.Advance, reconPicked.Doctrine, "slice7 picker confirm deploys advance");
Eq(0, reconPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice7 picker recon deploy charges 10");
// Save/load: the load clamp is per-kind, not Vanguard-shaped.
// A 999-HP Recon loads at 40, a 999-HP Vanguard at 90, and a
// mixed save round-trips both kinds with their own health.
var clampRecon = new TacticalSquad(16, 5, 16, 20, 5, 16, 999, TacticalDoctrine.HoldGround, SquadKind.Recon);
Eq(40, clampRecon.Health, "slice7 recon load clamp is 40, not vanguard 90");
var clampVan = new TacticalSquad(16, 5, 16, 20, 5, 16, 999, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
Eq(90, clampVan.Health, "slice7 vanguard load clamp still 90");
var saveMixedWorld = new WorldZone(42);
saveMixedWorld.DeploySquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
var saveRecon = saveMixedWorld.DeploySquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
saveRecon.TakeDamage(5); // 40 -> 35, still 2 members
var saveMixedLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveMixedWorld));
Eq(2, saveMixedLoaded.Squads.Count, "slice7 mixed save loads both squads");
Eq(SquadKind.Vanguard, saveMixedLoaded.Squads[0].Kind, "slice7 mixed save vanguard kind preserved");
Eq(90, saveMixedLoaded.Squads[0].Health, "slice7 mixed save vanguard health preserved");
Eq(SquadKind.Recon, saveMixedLoaded.Squads[1].Kind, "slice7 mixed save recon kind preserved");
Eq(35, saveMixedLoaded.Squads[1].Health, "slice7 mixed save recon health preserved");
Eq(2, saveMixedLoaded.Squads[1].AliveMembers, "slice7 mixed save recon members preserved");
// Targeting stays kind-blind: an enemy pursues the nearer Recon
// squad, not the farther player, with no kind priority in the rule.
var targetWorld = new WorldZone(42);
targetWorld.DeploySquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
targetWorld.Player.X = 28; targetWorld.Player.Z = 28;
targetWorld.Enemies.Add(new Enemy(20, 5, 16));
targetWorld.Advance();
Eq(19, targetWorld.Enemies[0].X, "slice7 enemy pursues nearer recon squad over farther player");
// Snapshot carries the kind and its shape: a holding Recon
// reads Holding with 2 members, 40 HP, 12 damage.
var snapWorld = new WorldZone(42);
snapWorld.DeploySquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
var snapRecon = OrbitalView.Snapshot(snapWorld)[0];
Eq(SquadKind.Recon, snapRecon.Kind, "slice7 snapshot recon kind");
Eq(2, snapRecon.AliveMembers, "slice7 snapshot recon members");
Eq(40, snapRecon.Health, "slice7 snapshot recon health");
Eq(12, snapRecon.DamagePerTick, "slice7 snapshot recon damage");
Eq(SquadActivity.Holding, snapRecon.Activity, "slice7 snapshot recon at rally is holding");

// ---- Silence instruction (recorded, not a test) ----

Console.WriteLine($"\n{passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
