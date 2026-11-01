"""Record final 0.124 bytes and evidence classes without changing old manifests."""
from pathlib import Path
import hashlib
import json

ROOT=Path(__file__).resolve().parents[2]
EVIDENCE=ROOT/"docs/evidence/2026-11-01"


def result(name):
    report=json.loads((EVIDENCE/name).read_text(encoding="utf-8-sig"))
    if not report.get("success"):
        raise ValueError("Consumer check failed: "+name)
    return report["data"]["result"]


def record(path):
    data=path.read_bytes()
    return {"path":path.relative_to(ROOT).as_posix(),"bytes":len(data),"sha256":hashlib.sha256(data).hexdigest()}


main=result("unity-authored-main-status.json")
world=result("unity-authored-foundry-main-smoke.json")
construction=result("unity-authored-construction-smoke.json")
power=result("unity-authored-power-smoke.json")
geometry=record(ROOT/"src/Forgeborne.Core/Content/CinderExchangeV1.json")
installed=record(ROOT/"ForgeborneUnity/Assets/Plugins/Forgeborne.Core.dll")
built=record(ROOT/"src/Forgeborne.Core/bin/Release/netstandard2.1/Forgeborne.Core.dll")
assert geometry["sha256"]==main["contentSha256"]==world["contentSha256"]
assert installed["sha256"]==built["sha256"]
assert main["actualEntryIsAuthored"] and main["legacyBlockRootEmpty"] and main["allLinkedAdaptersShareOneSimulation"]
assert construction["nineOriginalChoices"]==9 and construction["noCubeOutline"] and power["powerSourceRemovalRestoresTint"]
files=[ROOT/p for p in (
    ".gitattributes","art/tools/build_foundry_environment.py","art/source/CinderExchange.blend",
    "art/tools/build_visual_assets.py","art/source/PulseCarbine.blend","art/source/PulseCarbine.json",
    "ForgeborneUnity/Assets/Resources/Art/Models/PulseCarbine.fbx",
    "ForgeborneUnity/Assets/Resources/Art/Prefabs/PulseCarbine.prefab",
    "ForgeborneUnity/Assets/Resources/Art/Materials/Timber.mat",
    "ForgeborneUnity/Assets/Resources/Materials/ConstructionGhost.mat",
    "ForgeborneUnity/Assets/Scenes/Main.unity","ForgeborneUnity/ProjectSettings/ProjectSettings.asset",
    "ForgeborneUnity/Assets/Scripts/VoxelMeshRenderer.cs","ForgeborneUnity/Assets/Scripts/ConstructionGuide.cs",
    "ForgeborneUnity/Assets/Scripts/GameManager.cs","ForgeborneUnity/Assets/Scripts/FoundryAtmosphere.cs",
    "ForgeborneUnity/Assets/Scripts/PlayerFlow.cs","ForgeborneUnity/Assets/Editor/ForgeborneArtAssets.cs",
    "src/Forgeborne.Core/AuthoredFoundry.cs","src/Forgeborne.Core/TerrainGeometry.cs",
    "src/Forgeborne.Core/VoxelChunk.cs","src/Forgeborne.Core/SaveSystem.cs","src/Forgeborne.Core/Aim.cs",
    "src/Forgeborne.Core/World.cs","src/Forgeborne.Core/Orbital.cs","src/Forgeborne.Core/SurfaceProfile.cs",
    "tests/Forgeborne.Core.Tests/AuthoredSurfaceTests.cs",
    "tools/unity-authored-foundry-prepare.cs","tools/unity-authored-foundry-smoke.cs",
    "tools/unity-authored-construction-smoke.cs","tools/unity-authored-power-smoke.cs","tools/unity-authored-main-status.cs")]
files += list(EVIDENCE.glob("unity-authored-*.json"))
files += [EVIDENCE/"authored-foundry-fps-diagnostic.png",EVIDENCE/"authored-foundry-orbital-diagnostic.png",Path(__file__)]
report={
    "version":"0.124.0","scope":"Actual Main entry and shared original authored geometry; full game incomplete",
    "frozenLaw":"63d99cdc72a1c18b5aa91b9336d02fd30126407b",
    "sourceQA":{"configuration":"Release","warnings":0,"errors":0,"passed":5535,"failed":0},
    "geometry":geometry,"installedCore":installed,"buildMatchesInstalled":True,
    "sourceParts":world["sourcePieces"],"openingTriangles":world["triangles"],"openingRegions":world["activeRegions"],
    "runtimeEvidence":"Actual paused Unity Main, scripted production consumers, exact recovery, 300 collider rays; no native input claim",
    "artOrigin":"Original Blender meshes and existing original shaders/materials; no imported art",
    "reproduction":"Two final geometry generations match exact UTF-8/LF bytes. Blender/FBX container reproducibility not claimed. Other text hashes use raw workspace line endings.",
    "fullAAAPlusAcceptance":"FAIL","nativeArtPackage":"NOT BUILT","representativePerformance":"OPEN","heardAudioAndFun":"OPEN",
    "userSaveSlots":"NOT WRITTEN OR CONVERTED","files":[record(p) for p in sorted(set(files))]}
destination=EVIDENCE/"authored-0124-manifest.json"
destination.write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
print(json.dumps({"manifest":str(destination),"files":len(report["files"]),"coreSha256":installed["sha256"],"geometrySha256":geometry["sha256"]}))
