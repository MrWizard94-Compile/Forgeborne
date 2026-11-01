#nullable enable
// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// This script ONLY assigns MeshBuilder buffers to a Unity Mesh. It contains
// no voxel logic, no gather/build rules, and no second chunk representation.
// Canonical state is Forgeborne.Core.WorldZone, via Simulation.
using UnityEngine;
using Forgeborne.Core;
using System.Collections.Generic;

// MeshCollider added in 0.15.0 so the Orbital rally picker's
// crosshair raycast can hit the voxel mesh: without a collider
// the raycast would pass through the rendered chunk and never
// set a rally. The collider shares the same mesh buffers, so it
// adds no second chunk representation.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelMeshRenderer : MonoBehaviour
{
    public Simulation? Simulation;
    // Slice 16 Step 1: the chunk material, decided 0.68.0
    // (rationale in DECISIONS.md). Inspector-wired when a
    // scene author has built a vertex-colour material in
    // the editor — that material wins, and the runtime
    // fallback below never overwrites it, so a deliberate
    // Shader Graph material is not replaced by a stock
    // shader at Play. Unwired, Awake assigns a runtime
    // material from the URP Lit shader, falling back to
    // Standard — the same shader pair SquadMarkerDisplay
    // already uses, and the project already depends on
    // URP 17.0.0 (Packages/manifest.json), so no new
    // shader asset, texture, or imported material rides
    // along. The material is assigned in Awake, before
    // GameManager.Start calls the first RefreshMesh, so
    // the first mesh never renders one frame under an
    // unwired material. Honest limit, not softened by
    // this step: assigning a material is not proving it
    // multiplies vertex colours in Unity 6000 URP 17.0.0
    // — playtest-gate item 26 remains the observation
    // that settles the tint, and item 30 is the
    // observation that settles which shader was
    // actually assigned and whether untinted stone, ore,
    // and wood still render exactly as before. If
    // neither shader is found, the Inspector material
    // is left untouched and a warning logs, rather than
    // a null material silently rendering pink.
    public Material? ChunkMaterial;
    private Mesh? _mesh;
    private MeshCollider? _collider;
    private Material? _runtimeMaterial;
    private MeshRenderer? _renderer;
    private sealed class RegionView
    {
        public GameObject Root=null!;
        public Mesh Mesh=null!;
        public MeshCollider Collider=null!;
        public TerrainRegion? Region;
        public MeshRenderer Renderer=null!;
        public MaterialPropertyBlock Properties=new MaterialPropertyBlock();
        public Light? Practical;
        public BlockType[] Types=System.Array.Empty<BlockType>();
    }
    private readonly Dictionary<int,RegionView> _regions=new Dictionary<int,RegionView>();
    private readonly Dictionary<string,Material> _authoredMaterials=new Dictionary<string,Material>();
    private readonly HashSet<int> _seenRegions=new HashSet<int>();
    private bool _continuousVisible;
    public int LastRegionsRebuilt { get; private set; }

    void Awake()
    {
        _mesh = new Mesh { name = "ForgeborneChunk" };
        GetComponent<MeshFilter>().mesh = _mesh;
        _collider = GetComponent<MeshCollider>();
        var renderer = GetComponent<MeshRenderer>();
        _renderer=renderer;
        if (ChunkMaterial != null)
        {
            renderer.sharedMaterial = ChunkMaterial;
        }
        else
        {
            var shader = Shader.Find("Forgeborne/VoxelSurface") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                _runtimeMaterial = new Material(shader);
                renderer.sharedMaterial = _runtimeMaterial;
            }
            else Debug.LogWarning("Forgeborne chunk material: neither Universal Render Pipeline/Lit nor Standard shader found; chunk renders with its Inspector material, and vertex tints may not show — see playtest-gate items 26 and 30.");
        }
    }

    void OnDestroy()
    {
        foreach(var region in _regions.Values)
        {
            if(region.Collider!=null)region.Collider.sharedMesh=null;
            if(region.Root!=null)region.Root.SetActive(false);
            if(region.Mesh!=null)Destroy(region.Mesh);
            if(region.Root!=null)Destroy(region.Root);
        }
        _regions.Clear();
        if (_collider != null) _collider.sharedMesh = null;
        if (_mesh != null) Destroy(_mesh);
        if (_runtimeMaterial != null) Destroy(_runtimeMaterial);
    }

    // Call this when the chunk changes (gather / build / destruction).
    // In Slice 1, re-meshing the whole 32^3 chunk is acceptable; chunk
    // dirty-region meshing is a later optimisation, not a Slice 1 gate.
    // Slice 13 Step 3: this refresh is now the tinted refresh, not
    // an untinted one — it builds with WorldZone.PowerState, so
    // every quad carries its cell's power tint, and the mesh's
    // vertex colours carry it to the renderer. No second mesh:
    // the colours are one more buffer on the same MeshData.
    // Honest material limit, not hidden, and not closed by
    // Slice 16 Step 1: mesh.colors only shows if the chunk
    // material multiplies vertex colours. Awake now assigns
    // one (Inspector ChunkMaterial, else runtime URP Lit /
    // Standard fallback), so the scene no longer depends
    // on an unwritten Inspector step — but with a material
    // that ignores vertex colours the mesh still renders
    // untinted, and playtest-gate item 26 is the
    // observation that settles whether the assigned
    // material multiplies them, with item 30 recording
    // which shader was actually assigned. Untinted
    // vertices are white, so a vertex-colour material
    // leaves ordinary stone, ore, and wood exactly as
    // before.
    public void RefreshMesh()
    {
        if (Simulation == null || _mesh == null) return;
        bool continuous=Simulation.World.Chunk.SurfaceProfile!=SurfaceProfile.LegacyBlocks;
        if(continuous){RefreshRegions();return;}
        LastRegionsRebuilt=0;
        if(_continuousVisible)
        {
            foreach(var region in _regions.Values)
            {
                region.Root.SetActive(false);region.Collider.sharedMesh=null;region.Mesh.Clear();
                region.Region=null;region.Types=System.Array.Empty<BlockType>();
            }
            _continuousVisible=false;
        }
        if(_renderer!=null)_renderer.enabled=true;
        var data=MeshBuilder.BuildFromChunk(Simulation.World.Chunk,Simulation.World.PowerState);
        Upload(_mesh,_collider,data);
    }

    private void RefreshRegions()
    {
        if(Simulation==null || _mesh==null || _renderer==null)return;
        var matrix=transform.localToWorldMatrix;
        for(int row=0;row<4;row++)for(int column=0;column<4;column++)
            if(Mathf.Abs(matrix[row,column]-(row==column ? 1 : 0))>0.000001f)
                throw new System.InvalidOperationException("Current canonical chunk surfaces require an identity world transform.");
        if(!_continuousVisible)
        {
            if(_collider!=null)_collider.sharedMesh=null;
            _mesh.Clear();_renderer.enabled=false;_continuousVisible=true;
        }
        LastRegionsRebuilt=0;
        var geometry=Simulation.World.Chunk.Geometry;
        _seenRegions.Clear();
        foreach(var region in geometry.Regions)
        {
            _seenRegions.Add(region.Id);
            _regions.TryGetValue(region.Id,out var view);
            if(region.TriangleCount==0)
            {
                if(view!=null && view.Root.activeSelf)
                {view.Root.SetActive(false);view.Collider.sharedMesh=null;view.Mesh.Clear();view.Region=null;view.Types=System.Array.Empty<BlockType>();}
                continue;
            }
            if(view==null)
            {
                var root=new GameObject("Surface region "+region.Id,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
                root.transform.SetParent(transform,false);
                var surface=root.GetComponent<MeshRenderer>();surface.sharedMaterial=_renderer.sharedMaterial;
                var properties=new MaterialPropertyBlock();properties.SetFloat("_GridStrength",0);surface.SetPropertyBlock(properties);
                view=new RegionView{Root=root,Mesh=new Mesh{name="Forgeborne surface "+region.Id},Collider=root.GetComponent<MeshCollider>(),Renderer=surface};
                root.GetComponent<MeshFilter>().sharedMesh=view.Mesh;_regions.Add(region.Id,view);
            }
            view.Root.SetActive(true);
            if(!object.ReferenceEquals(view.Region,region))
            {
                view.Root.name=region.Label==null ? "Surface region "+region.Id : "Authored "+region.Label+" ["+region.Id+"]";
                Material? material=_renderer.sharedMaterial;
                if(region.MaterialRole!=null)
                {
                    if(!_authoredMaterials.TryGetValue(region.MaterialRole,out var authored))
                    {
                        authored=Resources.Load<Material>("Art/Materials/"+region.MaterialRole);
                        if(authored==null)throw new System.InvalidOperationException("Prepare original authored material "+region.MaterialRole+" before Play.");
                        _authoredMaterials.Add(region.MaterialRole,authored);
                    }
                    material=authored;
                }
                view.Renderer.sharedMaterial=material;
                view.Properties.Clear();view.Properties.SetFloat("_GridStrength",0);
                view.Properties.SetFloat("_WorldWeather",1);view.Renderer.SetPropertyBlock(view.Properties);
                var data=region.HasPowerSensitiveMaterials ? region.BuildMesh(Simulation.World.PowerState) : region.BuildMesh();
                Upload(view.Mesh,view.Collider,data);view.Types=data.BlockTypes.ToArray();view.Region=region;LastRegionsRebuilt++;
                if(region.MaterialRole=="Warm")
                {
                    if(view.Practical==null)
                    {
                        var lightRoot=new GameObject("Authored practical light");lightRoot.transform.SetParent(view.Root.transform,false);
                        view.Practical=lightRoot.AddComponent<Light>();view.Practical.type=LightType.Point;
                        view.Practical.color=new Color(1,0.56f,0.28f);view.Practical.intensity=12;view.Practical.range=10;
                        view.Practical.shadows=LightShadows.None;
                    }
                    view.Practical.transform.localPosition=view.Mesh.bounds.center+Vector3.down*0.10f;
                    view.Practical.enabled=true;
                }
                else if(view.Practical!=null)view.Practical.enabled=false;
            }
            else if(region.HasPowerSensitiveMaterials)
            {
                var colours=region.BuildColors(Simulation.World.PowerState);var result=new Color[colours.Length];
                for(int i=0;i<result.Length;i++)
                {var c=colours[i];result[i]=SurfaceColor(view.Types[i])*new Color(c.Item1,c.Item2,c.Item3,c.Item4);}
                view.Mesh.colors=result;
            }
            if(region.MaterialRole!=null && region.HasPowerSensitiveMaterials)
            {
                var colours=region.BuildColors(Simulation.World.PowerState);
                var tint=colours[0];var material=view.Renderer.sharedMaterial;
                view.Properties.SetColor("_BaseColor",material.GetColor("_BaseColor")*new Color(tint.Item1,tint.Item2,tint.Item3,tint.Item4));
                view.Renderer.SetPropertyBlock(view.Properties);
            }
        }
        // Authored deposits and constructed components disappear as a unit.
        // They do not occupy the field mesher's fixed 729 region slots.
        foreach(var pair in _regions)
            if(!_seenRegions.Contains(pair.Key))
            {
                var view=pair.Value;view.Root.SetActive(false);view.Collider.sharedMesh=null;view.Mesh.Clear();
                view.Region=null;view.Types=System.Array.Empty<BlockType>();
            }
    }

    private static void Upload(Mesh mesh,MeshCollider? collider,MeshData data)
    {
        var verts = new Vector3[data.Vertices.Count];
        var norms = new Vector3[data.Normals.Count];
        var uvs = new Vector2[data.Uvs.Count];
        var cols = new Color[data.Colors.Count];
        var types = new Vector2[data.BlockTypes.Count];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new Vector3(data.Vertices[i].Item1, data.Vertices[i].Item2, data.Vertices[i].Item3);
            norms[i] = new Vector3(data.Normals[i].Item1, data.Normals[i].Item2, data.Normals[i].Item3);
            uvs[i] = new Vector2(data.Uvs[i].Item1, data.Uvs[i].Item2);
            var tint = new Color(data.Colors[i].Item1, data.Colors[i].Item2, data.Colors[i].Item3, data.Colors[i].Item4);
            cols[i] = SurfaceColor(data.BlockTypes[i]) * tint;
            types[i] = new Vector2((int)data.BlockTypes[i], 0);
        }
        if (collider != null) collider.sharedMesh = null;
        mesh.Clear();
        mesh.indexFormat=verts.Length>65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = verts;mesh.normals = norms;mesh.uv = uvs;mesh.colors = cols;mesh.uv2 = types;
        mesh.triangles = data.Indices.ToArray();mesh.RecalculateBounds();
        // Re-attach after the clear/rebuild, so the raycast collider
        // matches the freshly built mesh instead of the cleared one.
        if (collider != null) collider.sharedMesh = mesh;
    }

    private static Color SurfaceColor(BlockType block)
    {
        switch (block)
        {
            case BlockType.Stone: return new Color(0.22f, 0.27f, 0.30f);
            case BlockType.EmberOre: return new Color(0.86f, 0.29f, 0.07f);
            case BlockType.Wood: return new Color(0.34f, 0.17f, 0.075f);
            case BlockType.Wall: return new Color(0.10f, 0.16f, 0.20f);
            case BlockType.Workstation: return new Color(0.09f, 0.40f, 0.46f);
            case BlockType.Bedrock: return new Color(0.055f, 0.065f, 0.08f);
            default: return new Color(0.22f, 0.32f, 0.38f);
        }
    }
}
