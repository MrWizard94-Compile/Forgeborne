#nullable enable
using Forgeborne.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Read-only construction presentation; placement and payment remain in Core.</summary>
public sealed class ConstructionGuide : MonoBehaviour
{
    public FpsInputAdapter? Adapter;
    public bool IsActive { get; private set; }
    public BlockType SelectedType { get; private set; } = BlockType.Wall;
    public bool OutlineVisible => (_outline != null && _outline.enabled) || (_ghostRoot!=null && _ghostRoot.activeSelf);
    public string Label => _label != null ? _label.text : "";
    private static readonly BlockType[] Choices = { BlockType.Wall, BlockType.Workstation, BlockType.KineticMill,
        BlockType.Dynamo, BlockType.Transformer, BlockType.Cable, BlockType.Alternator, BlockType.ShockPylon, BlockType.Conduit };
    private static readonly Vector3[] Corners = { new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,1,0),
        new Vector3(0,1,0),new Vector3(0,0,0),new Vector3(0,0,1),new Vector3(1,0,1),new Vector3(1,0,0),
        new Vector3(1,0,1),new Vector3(1,1,1),new Vector3(1,1,0),new Vector3(1,1,1),new Vector3(0,1,1),
        new Vector3(0,1,0),new Vector3(0,1,1),new Vector3(0,0,1) };
    private readonly Vector3[] _points = new Vector3[Corners.Length];
    private LineRenderer? _outline;
    private Material? _material;
    private GameObject? _ui;
    private Text? _label;
    private BuildPreview _last;
    private bool _hasLast;
    private int _stone, _ore, _wood;
    private Material? _ghostMaterial;
    private GameObject? _ghostRoot;
    private readonly System.Collections.Generic.List<Mesh> _ghostMeshes=new System.Collections.Generic.List<Mesh>();
    private BlockType _ghostType;
    private bool _lastAuthored;

    void Start()
    {
        var resource = Resources.Load<Material>("Materials/RuntimeEmissive");
        if (resource == null) throw new System.InvalidOperationException("Construction material missing.");
        _material = new Material(resource) { name = "Forgeborne construction guide" };
        var ghost=Resources.Load<Material>("Materials/ConstructionGhost");
        if(ghost==null)throw new System.InvalidOperationException("Prepare the original construction ghost material before Play.");
        _ghostMaterial=new Material(ghost){name="Forgeborne authored construction preview"};
        var edges = new GameObject("Construction cell outline");
        edges.transform.SetParent(transform,false);
        _outline = edges.AddComponent<LineRenderer>();
        _outline.sharedMaterial = _material; _outline.positionCount = Corners.Length;
        _outline.useWorldSpace = true; _outline.generateLightingData = true;
        _outline.startWidth = _outline.endWidth = 0.014f;
        _outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _outline.enabled = false;
        _ui = new GameObject("Construction guide",typeof(Canvas),typeof(CanvasScaler));
        _ui.transform.SetParent(transform,false);
        var canvas = _ui.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 19;
        var scaler = _ui.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
        var panel = new GameObject("Build card",typeof(RectTransform)).GetComponent<RectTransform>();
        panel.SetParent(_ui.transform,false); panel.anchorMin=panel.anchorMax=panel.pivot=new Vector2(0.5f,0.5f);
        panel.anchoredPosition = new Vector2(0,-225); panel.sizeDelta = new Vector2(900,104);
        var background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.02f,0.04f,0.055f,0.88f); background.raycastTarget = false;
        var text = new GameObject("Build facts",typeof(RectTransform)).GetComponent<RectTransform>();
        text.SetParent(panel,false); text.anchorMin=Vector2.zero; text.anchorMax=Vector2.one;
        text.offsetMin = new Vector2(15,5); text.offsetMax = new Vector2(-15,-5);
        _label = text.gameObject.AddComponent<Text>();
        _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize = 20; _label.alignment = TextAnchor.MiddleCenter;
        _label.supportRichText = false; _label.raycastTarget = false;
        _ui.SetActive(false);
    }

    public void Toggle()
    {
        if (IsActive) { Cancel(); return; }
        if (Adapter == null || Adapter.InputSuppressed || Adapter.IsOrbitalView || Adapter.HasPendingCommand
            || Adapter.Simulation == null || Adapter.Simulation.IsPlayerDead || Adapter.Simulation.IsObjectiveComplete) return;
        IsActive=true; _hasLast=false;
    }
    public void Select(BlockType type) { if (BuildCost.TryGet(type,out _)) { SelectedType=type; _hasLast=false; } }
    public void Cycle(int direction)
    {
        if (!IsActive || direction==0) return;
        int index = System.Array.IndexOf(Choices,SelectedType);
        Select(Choices[(index+(direction>0 ? 1 : Choices.Length-1))%Choices.Length]);
    }
    public void Cancel()
    {
        IsActive=false; _hasLast=false;
        if(_ghostRoot!=null)_ghostRoot.SetActive(false);
        if (_outline != null) _outline.enabled=false;
        if (_ui != null) _ui.SetActive(false);
        if (_label != null) _label.text="";
    }
    void LateUpdate() => RefreshPreview();
    public void RefreshPreview()
    {
        if (!IsActive || Adapter==null || Adapter.Simulation==null || _outline==null || _ui==null || _material==null || _label==null) return;
        var simulation=Adapter.Simulation;
        if (Adapter.InputSuppressed || Adapter.IsOrbitalView || Adapter.HasPendingCommand
            || simulation.IsPlayerDead || simulation.IsObjectiveComplete) { Cancel(); return; }
        var preview=simulation.InspectBuildAhead(SelectedType);
        bool authored=simulation.World.Chunk.SurfaceProfile==SurfaceProfile.AuthoredFoundryV1;
        _ui.SetActive(true); _outline.enabled=preview.HasCell && !authored;
        if(_ghostRoot!=null)_ghostRoot.SetActive(authored && preview.HasCell);
        var inventory=simulation.World.Player.Inventory;
        int stone=inventory.Get(BlockType.Stone),ore=inventory.Get(BlockType.EmberOre),wood=inventory.Get(BlockType.Wood);
        if (_hasLast && _last==preview && _stone==stone && _ore==ore && _wood==wood && _lastAuthored==authored) return;
        _hasLast=true; _last=preview; _stone=stone; _ore=ore; _wood=wood;_lastAuthored=authored;
        Color color=preview.Result==BuildResult.Success ? new Color(0.05f,0.9f,1)
            : preview.Result==BuildResult.InsufficientResources ? new Color(1,0.6f,0.08f) : new Color(1,0.2f,0.08f);
        _material.color=color; _material.SetColor("_EmissionColor",color*2);
        if (preview.HasCell)
        {
            if(authored)
            {
                EnsureGhost(SelectedType);
                _ghostRoot!.transform.position=new Vector3(preview.X,preview.Y,preview.Z);_ghostRoot.SetActive(true);
                _ghostMaterial!.SetColor("_BaseColor",new Color(color.r,color.g,color.b,.30f));
            }
            else
            {
                var origin=new Vector3(preview.X,preview.Y,preview.Z)-Vector3.one*0.004f;
                for(int i=0;i<Corners.Length;i++) _points[i]=origin+Corners[i]*1.008f;
                _outline.SetPositions(_points);
            }
        }
        string recipe="";
        if(preview.Cost.Stone>0) recipe+="STONE "+stone+"/"+preview.Cost.Stone;
        if(preview.Cost.EmberOre>0) recipe+=(recipe.Length>0 ? " · " : "")+"EMBER "+ore+"/"+preview.Cost.EmberOre;
        if(preview.Cost.Wood>0) recipe+=(recipe.Length>0 ? " · " : "")+"TIMBER "+wood+"/"+preview.Cost.Wood;
        string name=SelectedType==BlockType.KineticMill ? "KINETIC MILL" : SelectedType==BlockType.ShockPylon ? "SHOCK PYLON" : SelectedType.ToString().ToUpperInvariant();
        string reason=preview.Result==BuildResult.Success ? "READY" : preview.Result==BuildResult.InsufficientResources ? "NEED RESOURCES"
            : preview.Result==BuildResult.Occupied ? "SPACE OCCUPIED" : preview.Result==BuildResult.Floating ? "AIM AT A SUPPORTED SURFACE"
            : preview.Result==BuildResult.OutOfBounds ? "OUTSIDE THE YARD" : "SPACE BLOCKED";
        _label.color=color;
        _label.text=name+" · WHEEL SELECT · LMB PLACE · V EXIT\n"+recipe+"\n"+reason;
    }
    private void EnsureGhost(BlockType type)
    {
        if(_ghostRoot!=null && _ghostType==type)return;
        if(_ghostRoot!=null){_ghostRoot.SetActive(false);Destroy(_ghostRoot);}
        foreach(var mesh in _ghostMeshes)Destroy(mesh);_ghostMeshes.Clear();
        _ghostType=type;_ghostRoot=new GameObject("Authored construction preview");
        _ghostRoot.transform.SetParent(Adapter!=null && Adapter.MeshRenderer!=null ? Adapter.MeshRenderer.transform : null,false);
        foreach(var region in AuthoredFoundry.ConstructionTemplate(type))
        {
            var data=region.BuildMesh();var vertices=new Vector3[data.Vertices.Count];
            for(int i=0;i<vertices.Length;i++){var p=data.Vertices[i];vertices[i]=new Vector3(p.Item1,p.Item2,p.Item3);}
            var mesh=new Mesh{name="Construction preview "+region.Label};mesh.vertices=vertices;mesh.triangles=data.Indices.ToArray();mesh.RecalculateBounds();
            _ghostMeshes.Add(mesh);
            var root=new GameObject(region.Label,typeof(MeshFilter),typeof(MeshRenderer));root.transform.SetParent(_ghostRoot.transform,false);
            root.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=root.GetComponent<MeshRenderer>();renderer.sharedMaterial=_ghostMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
    }
    void OnDestroy()
    {
        if (_outline != null) Destroy(_outline.gameObject);
        if (_ui != null) Destroy(_ui);
        if (_material != null) Destroy(_material);
        if (_ghostRoot != null) Destroy(_ghostRoot);
        foreach(var mesh in _ghostMeshes)Destroy(mesh);_ghostMeshes.Clear();
        if (_ghostMaterial != null) Destroy(_ghostMaterial);
    }
}
