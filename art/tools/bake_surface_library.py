"""Original Foundry surface recipes, Blender 4.5.15 + its bundled NumPy.

Deterministic, periodic scalar fields author matching color/height/roughness/
occlusion/metal maps. No photos, downloaded textures, or copied material graphs.
Color PNGs encode sRGB; packed HROM PNGs contain raw linear data, including alpha.
"""
from pathlib import Path
import hashlib
import json
import math
import bpy
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/"ForgeborneUnity/Assets/Resources/Art/Surfaces"
SIZE=1024
U,V=np.meshgrid(np.arange(SIZE,dtype=np.float32)/SIZE,np.arange(SIZE,dtype=np.float32)/SIZE)


def noise(nx,ny,seed):
    rng=np.random.default_rng(seed)
    grid=rng.random((ny,nx),dtype=np.float32)
    x,y=U*nx,V*ny
    ix,iy=x.astype(np.int32),y.astype(np.int32)
    fx,fy=x-ix,y-iy
    fx=fx*fx*(3-2*fx);fy=fy*fy*(3-2*fy)
    a=grid[iy%ny,ix%nx]*(1-fx)+grid[iy%ny,(ix+1)%nx]*fx
    b=grid[(iy+1)%ny,ix%nx]*(1-fx)+grid[(iy+1)%ny,(ix+1)%nx]*fx
    return a*(1-fy)+b*fy


def smooth(lo,hi,value):
    t=np.clip((value-lo)/(hi-lo),0,1)
    return t*t*(3-2*t)


def srgb(linear):
    return np.where(linear<=.0031308,linear*12.92,1.055*np.maximum(linear,0)**(1/2.4)-.055)


def save(name,data):
    image=bpy.data.images.new(name,width=SIZE,height=SIZE,alpha=True,float_buffer=False,is_data=True)
    image.colorspace_settings.name="Non-Color"
    image.alpha_mode="CHANNEL_PACKED"
    image.pixels.foreach_set(np.asarray(data,dtype=np.float32).ravel())
    path=DEST/(name+".png");image.filepath_raw=str(path);image.file_format="PNG";image.save()
    bpy.data.images.remove(image)
    return {"file":path.name,"bytes":path.stat().st_size,"sha256":hashlib.sha256(path.read_bytes()).hexdigest()}


RECIPES={
    "Graphite":{"color":(.075,.090,.10),"tileMetres":.65,"heightMetres":.00032,"metal":.34,"rough":.49,"seed":101},
    "Steel":{"color":(.35,.40,.42),"tileMetres":.70,"heightMetres":.00013,"metal":.94,"rough":.32,"seed":211},
    "Copper":{"color":(.40,.19,.075),"tileMetres":.75,"heightMetres":.00036,"metal":.91,"rough":.38,"seed":307},
    "Ceramic":{"color":(.53,.58,.55),"tileMetres":1.2,"heightMetres":.00030,"metal":0,"rough":.38,"seed":401},
    "Rubber":{"color":(.025,.030,.033),"tileMetres":.25,"heightMetres":.00022,"metal":0,"rough":.76,"seed":503},
    "Basalt":{"color":(.13,.145,.15),"tileMetres":2.0,"heightMetres":.0022,"metal":0,"rough":.83,"seed":601},
    "Paint":{"color":(.085,.20,.215),"tileMetres":2.0,"heightMetres":.0011,"metal":.02,"rough":.46,"seed":701},
    "Timber":{"color":(.26,.125,.045),"tileMetres":1.0,"heightMetres":.0012,"metal":0,"rough":.77,"seed":809},
}


def bake(name,recipe):
    seed=recipe["seed"]
    broad=noise(5,5,seed);medium=noise(37,37,seed+1);micro=noise(311,311,seed+2)
    rng=np.random.default_rng(seed+3);grain=rng.random((SIZE,SIZE),dtype=np.float32)
    base=np.array(recipe["color"],dtype=np.float32)
    color=np.broadcast_to(base,(SIZE,SIZE,3)).copy()*(.85+.25*broad[...,None]+.09*(micro[...,None]-.5))
    rough=np.full((SIZE,SIZE),recipe["rough"],dtype=np.float32)+.10*(micro-.5)
    metal=np.full((SIZE,SIZE),recipe["metal"],dtype=np.float32)
    height=.5+.12*(medium-.5)+.06*(grain-.5)
    ao=np.ones((SIZE,SIZE),dtype=np.float32)
    if name in ("Steel","Graphite"):
        brush=noise(512,11,seed+4)
        long=noise(733,3,seed+5)
        scratches=smooth(.79,.92,long)*smooth(.35,.64,noise(17,7,seed+6))
        pits=smooth(.82,.96,noise(151,151,seed+7))
        height+=.13*(brush-.5)-.22*scratches-.22*pits
        rough+=.16*(brush-.5)+.20*scratches+.1*pits
        color*=1-.20*scratches[...,None]-.24*pits[...,None]
        ao-=.15*pits
        if name=="Graphite":
            chipped=smooth(.78,.90,medium+.13*(micro-.5))*.32
            color=color*(1-chipped[...,None])+np.array((.28,.31,.32))*chipped[...,None]
            metal=np.clip(metal+chipped*.6,0,1)
    elif name=="Paint":
        # Ragged loss of pigment exposes one continuous substrate, including
        # its metalness/roughness/relief. Random dark noise is not painted rust.
        chips=smooth(.88,.96,medium+.18*(micro-.5))
        under=smooth(.83,.92,medium+.16*(micro-.5))
        rust=smooth(.43,.69,broad)*chips
        substrate=(np.zeros_like(color)+np.array((.27,.29,.285)))*(.85+.24*micro[...,None])
        substrate=substrate*(1-rust[...,None])+np.array((.19,.065,.023))*rust[...,None]
        color=color*(1-under[...,None])+np.array((.12,.15,.15))*under[...,None]
        color=color*(1-chips[...,None])+substrate*chips[...,None]
        grime=smooth(.65,.88,noise(53,3,seed+8))*smooth(.34,.70,broad)
        color*=1-.28*grime[...,None]
        height+=.08*(micro-.5)-.42*chips-.12*under
        rough+=.26*chips+.13*grime;metal=chips*(1-rust)*.95;ao-=.12*under
    elif name=="Copper":
        tarnish=smooth(.47,.79,broad*.6+medium*.4)
        oxide=np.array((.065,.13,.105))
        color=color*(1-tarnish[...,None]*.63)+oxide*tarnish[...,None]*.63
        height+=.12*(micro-.5)+.15*tarnish;rough+=.24*tarnish;metal-=.22*tarnish
    elif name=="Ceramic":
        craze=np.maximum(0,1-np.abs(np.sin((U*8+noise(5,5,seed+9)*.15)*math.pi*2))/.025)
        craze*=smooth(.45,.70,noise(5,7,seed+10))
        color*=1-.27*craze[...,None];height-=.18*craze;rough+=.1*craze
    elif name=="Rubber":
        weave=(np.sin(U*math.pi*512)*np.sin(V*math.pi*512))*.5+.5
        height+=.18*(weave-.5)+.04*(micro-.5)
        color*=.90+.14*weave[...,None];rough+=.08*(weave-.5)
    elif name=="Basalt":
        aggregate=noise(127,127,seed+11)
        pits=smooth(.77,.93,aggregate+.17*(grain-.5))
        veins=smooth(.46,.50,medium)-smooth(.50,.54,medium)
        height+=.20*(aggregate-.5)-.34*pits+.07*veins
        color*=.70+.52*aggregate[...,None]+.15*grain[...,None]
        color=color*(1-veins[...,None]*.10)+np.array((.28,.275,.24))*veins[...,None]*.10
        ao-=.32*pits;rough+=.13*pits
    elif name=="Timber":
        fibre=noise(401,7,seed+12)
        early=noise(43,5,seed+13)
        rings=(np.sin((U*21+noise(7,7,seed+14)*.6)*math.pi*2)*.5+.5)
        pores=smooth(.79,.95,fibre)*smooth(.25,.60,early)
        height+=.18*(rings-.5)+.16*(fibre-.5)-.33*pores
        color*=.65+.43*early[...,None]+.19*rings[...,None]
        color*=1-.35*pores[...,None];ao-=.20*pores;rough+=.12*pores
    pigment=np.concatenate((srgb(np.clip(color,0,1)),np.ones((SIZE,SIZE,1))),axis=2)
    packed=np.stack((np.clip(height,0,1),np.clip(rough,.12,.98),np.clip(ao,.45,1),np.clip(metal,0,1)),axis=2)
    coordinates=((17,29),(513,619),(899,731))
    probes=[{"pixel":[x,y],"colorLinear":np.clip(color[y,x],0,1).tolist(),"surface":packed[y,x].tolist()} for x,y in coordinates]
    return {"recipe":recipe,"color":save(name+"_Color",pigment),"surface":save(name+"_HROM",packed),"gpuProbeSamples":probes,
        "linearColorMean":color.mean(axis=(0,1)).tolist(),"roughnessRange":[float(rough.min()),float(rough.max())],
        "packedContract":"R=height, G=roughness, B=occlusion, A=metalness; raw linear; no alpha premultiplication"}


assert bpy.app.version[:3]==(4,5,15)
bpy.ops.wm.read_factory_settings(use_empty=True)
DEST.mkdir(parents=True,exist_ok=True)
report={"authoring":"Original Forgeborne procedural surface fields; no external art",
        "blender":bpy.app.version_string,"numpy":np.__version__,"resolution":SIZE,"source":"art/tools/bake_surface_library.py","materials":{}}
for name,recipe in RECIPES.items():
    report["materials"][name]=bake(name,recipe)
    print("BAKED",name,flush=True)
manifest=ROOT/"art/source/FoundrySurfaces.json"
manifest.write_bytes((json.dumps(report,indent=2)+"\n").encode("utf-8"))
print(json.dumps({"manifest":str(manifest),"materials":len(RECIPES),"maps":len(RECIPES)*2}))
