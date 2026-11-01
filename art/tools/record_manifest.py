"""Record exact owned source/runtime art bytes; no game acceptance is inferred."""
from pathlib import Path
import hashlib
import json

ROOT = Path(__file__).resolve().parents[2]
NAMES = ("PulseCarbine", "EmberWarden", "AetherRelay", "ForgeSuit", "RelayFoundry")
ART = ROOT / "ForgeborneUnity/Assets/Resources/Art"
files = [ROOT / "art/tools/build_visual_assets.py", Path(__file__)]
for name in NAMES:
    for path in (ROOT / f"art/source/{name}.blend", ROOT / f"art/source/{name}.json",
                 ART / f"Models/{name}.fbx", ART / f"Prefabs/{name}.prefab"):
        if not path.is_file():
            raise FileNotFoundError(path)
        files.append(path)
files += list(ART.rglob("*.mat")) + list(ART.rglob("*.meta"))
for name in ("IndustrialSurface", "FoundrySky", "VoxelSurface"):
    files.append(ROOT / f"ForgeborneUnity/Assets/Shaders/{name}.shader")
for name in ("ForgebornePipeline", "ForgeborneRenderer", "FoundryGrade"):
    files.append(ROOT / f"ForgeborneUnity/Assets/Rendering/{name}.asset")
files.append(ROOT / "ForgeborneUnity/Assets/Scenes/Main.unity")
for name in ("AuthoredSkin", "WeaponViewModel", "WorldActorDisplay", "OrbitalCameraRig", "FoundryAtmosphere"):
    files.append(ROOT / f"ForgeborneUnity/Assets/Scripts/{name}.cs")
    files.append(ROOT / f"ForgeborneUnity/Assets/Scripts/{name}.cs.meta")
files.append(ROOT / "ForgeborneUnity/Assets/Editor/ForgeborneArtAssets.cs")
for name in ("ProjectSettings", "GraphicsSettings", "TagManager"):
    files.append(ROOT / f"ForgeborneUnity/ProjectSettings/{name}.asset")

def digest(path):
    data = path.read_bytes()
    return {"path": path.relative_to(ROOT).as_posix(), "bytes": len(data),
            "sha256": hashlib.sha256(data).hexdigest()}

manifest = {
    "checkpoint": "0.122.0 authored art technical checkpoint",
    "artOrigin": "Original Forgeborne geometry and shader authoring; no imported art",
    "blender": "4.5.15 LTS",
    "blenderArchiveSha256": "52dd02fd74877fa08e74b67d7854cf32468a232c02aaa19b6844c91285574f8d",
    "runtimeCore": digest(ROOT / "ForgeborneUnity/Assets/Plugins/Forgeborne.Core.dll"),
    "models": NAMES,
    "hashConvention": "Raw current workspace bytes; Git checkout line endings may change text hashes without changing source semantics",
    "files": [digest(path) for path in sorted(set(files))],
    "reproduction": "Container bytes may vary; validate geometry counts/bounds, roles, signed axes, metres and muzzle in Unity",
    "nativeArtPackage": "NOT BUILT",
    "worldVisualAcceptance": "FAIL: director rejected cubic world; geometry migration required",
    "performanceAndFunAcceptance": "NOT ESTABLISHED",
}
destination = ROOT / "docs/evidence/2026-11-01/authored-0122-assets.json"
destination.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
print(f"Recorded {len(manifest['files'])} files and {len(NAMES)} models: {destination}")
