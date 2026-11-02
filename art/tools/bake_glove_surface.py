"""Bake only the original glove pair; never overwrite the retained eight pairs."""
from pathlib import Path
import sys,json
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bake_surface_library as library

assert library.bpy.app.version[:3]==(4,5,15)
library.bpy.ops.wm.read_factory_settings(use_empty=True)
library.DEST.mkdir(parents=True,exist_ok=True)
report={"authoring":"Original Forgeborne leather grain, no external art","blender":library.bpy.app.version_string,
        "numpy":library.np.__version__,"resolution":library.SIZE,"source":"art/tools/bake_glove_surface.py",
        "sharedFieldsSource":"art/tools/bake_surface_library.py","materials":{}}
recipes={"GloveFabric":{"color":(.16,.105,.061),"tileMetres":.12,"heightMetres":.00018,"metal":0,"rough":.61,"seed":919},
         "SleeveFabric":{"color":(.12,.155,.16),"tileMetres":.16,"heightMetres":.00013,"metal":0,"rough":.78,"seed":1021}}
for name,recipe in recipes.items():report["materials"][name]=library.bake(name,recipe)
destination=library.ROOT/"art/source/GloveSurfaces.json"
destination.write_bytes((json.dumps(report,indent=2)+"\n").encode("utf-8"))
print(json.dumps({"manifest":str(destination),"maps":4,"existingEightPairsRewritten":False}))
