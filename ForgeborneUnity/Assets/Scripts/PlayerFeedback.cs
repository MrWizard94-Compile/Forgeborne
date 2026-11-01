#nullable enable
using System.Collections.Generic;
using Forgeborne.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Original audiovisual presentation of resolved Core actions. Never applies rules or advances time.</summary>
public sealed class PlayerFeedback : MonoBehaviour
{
    public GameManager? Manager;
    public FpsInputAdapter? InputAdapter;
    public WeaponViewModel? ViewModel;
    public Text? Reticle;
    public string Status => _message != null ? _message.text : "";
    public bool TraceVisible => _trace != null && _trace.enabled;
    private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
    private AudioSource? _effects;
    private AudioSource? _ambience;
    private Text? _target;
    private GameObject? _uiRoot;
    private Text? _message;
    private Image? _hurt;
    private LineRenderer? _trace;
    private GameObject? _impact;
    private Material? _signal;
    private Vector3 _impactPoint;
    private float _traceLife, _impactLife, _messageLife, _hitLife, _hurtLife;
    private WorldZone? _seenWorld;
    private int _health, _shields;
    private Vector3 _lastFeet;
    private float _stride;

    void Start()
    {
        _effects = gameObject.AddComponent<AudioSource>();
        _effects.playOnAwake = false;
        _effects.spatialBlend = 0; // Suit-local mix; the only AudioListener remains on the physical FPS camera.
        _ambience = gameObject.AddComponent<AudioSource>();
        _ambience.playOnAwake = false;
        _ambience.loop = true;
        _ambience.volume = 0;
        _ambience.clip = Clip("FoundryBed");
        _ambience.Play();
        var surface = Resources.Load<Material>("Materials/RuntimeEmissive");
        if (surface == null) throw new System.InvalidOperationException("Feedback material resource missing.");
        _signal = new Material(surface) { name = "Forgeborne pulse signal", color = new Color(0.05f,0.9f,1) };
        _signal.SetColor("_EmissionColor", new Color(0.05f,0.9f,1) * 3);
        var traceObject = new GameObject("Pulse trace");
        traceObject.transform.SetParent(transform, false);
        _trace = traceObject.AddComponent<LineRenderer>();
        _trace.sharedMaterial = _signal;
        _trace.positionCount = 2;
        _trace.useWorldSpace = true;
        _trace.generateLightingData = true;
        _trace.startWidth = 0.018f;
        _trace.endWidth = 0.006f;
        _trace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trace.enabled = false;
        _impact = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _impact.name = "Action signal";
        _impact.transform.SetParent(transform, false);
        _impact.GetComponent<Renderer>().sharedMaterial = _signal;
        Destroy(_impact.GetComponent<Collider>());
        _impact.SetActive(false);
        BuildUi();
    }

    AudioClip Clip(string name)
    {
        if (_clips.TryGetValue(name, out var clip)) return clip;
        clip = Resources.Load<AudioClip>("Audio/" + name);
        if (clip == null) throw new System.InvalidOperationException("Original audio resource missing: " + name);
        _clips.Add(name, clip);
        return clip;
    }

    void Sound(string name, float volume = 0.35f)
    {
        if (_effects != null) _effects.PlayOneShot(Clip(name), volume);
    }

    void Update()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null) return;
        bool suppressed = InputAdapter == null || InputAdapter.InputSuppressed;
        bool orbital = InputAdapter != null && InputAdapter.IsOrbitalView;
        float dt = Time.deltaTime;
        var feet = new Vector3(simulation.Fps.X, simulation.Fps.Y, simulation.Fps.Z);
        if (!ReferenceEquals(_seenWorld, simulation.World))
        {
            _seenWorld = simulation.World;
            _health = simulation.World.Player.Health;
            _shields = simulation.World.Player.Shields;
            _lastFeet = feet;
            _stride = 0;
            ResetSignals();
        }
        else
        {
            int health = simulation.World.Player.Health, shields = simulation.World.Player.Shields;
            if (health < _health || shields < _shields)
            {
                _hurtLife = 0.25f;
                Sound(health == 0 && _health > 0 ? "SuitBreach" : "SuitImpact", 0.4f);
            }
            _health = health;
            _shields = shields;
        }
        if (!suppressed && !orbital && simulation.Fps.IsGrounded)
        {
            var travel = feet - _lastFeet;
            travel.y = 0;
            _stride += travel.magnitude;
            if (_stride >= 1.65f) { _stride %= 1.65f; Sound("Footfall", 0.18f); }
        }
        _lastFeet = feet;
        if (_ambience != null) _ambience.volume = Mathf.MoveTowards(_ambience.volume,
            suppressed ? 0.025f : 0.065f, Time.unscaledDeltaTime * 0.12f);
        _traceLife = Mathf.Max(0, _traceLife - dt);
        _impactLife = Mathf.Max(0, _impactLife - dt);
        _messageLife = Mathf.Max(0, _messageLife - dt);
        _hitLife = Mathf.Max(0, _hitLife - dt);
        _hurtLife = Mathf.Max(0, _hurtLife - dt);
        if (_trace != null) _trace.enabled = _traceLife > 0 && !orbital;
        if (_impact != null)
        {
            _impact.SetActive(_impactLife > 0);
            _impact.transform.position = _impactPoint;
            _impact.transform.localScale = Vector3.one * (0.025f + _impactLife * 0.8f);
            _impact.transform.Rotate(90*dt,150*dt,60*dt,Space.Self);
        }
        if (_hurt != null) _hurt.color = new Color(0.95f,0.18f,0.04f,_hurtLife * 0.4f);
        if (_message != null) _message.enabled = _messageLife > 0;
        if (Reticle != null)
        {
            Reticle.color = _hitLife > 0 ? new Color(1,0.45f,0.1f) : Color.white;
            Reticle.fontSize = _hitLife > 0 ? 29 : 22;
        }
        if (_target != null)
        {
            _target.enabled = !suppressed && !orbital && !simulation.IsPlayerDead && !simulation.IsObjectiveComplete;
            if (_target.enabled)
            {
                var hit = simulation.InspectAim();
                _target.text = hit.Kind == AimTargetKind.Enemy && hit.Enemy != null ? "WARDEN · " + hit.Enemy.Health + " HP"
                    : hit.Kind == AimTargetKind.Core ? "RELAY · " + simulation.World.CoreHealth + " HP"
                    : hit.Kind == AimTargetKind.Solid ? Name(simulation.World.Chunk.Get(hit.X,hit.Y,hit.Z))
                    : "NO TARGET · " + Simulation.AimReach.ToString("0") + "M REACH";
            }
        }
    }

    public void Shot(ShotOutcome result)
    {
        if (!result.Fired) { Message("MAGAZINE EMPTY · R TO RELOAD", true); return; }
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null) return;
        Sound("PulseDischarge",0.4f);
        if (ViewModel != null) ViewModel.ReactShot();
        var direction = new Vector3(simulation.Look.ForwardX,simulation.Look.ForwardY,simulation.Look.ForwardZ);
        var origin = new Vector3(simulation.Fps.X,simulation.Fps.Y + Simulation.EyeHeight,simulation.Fps.Z);
        var end = origin + direction * Simulation.AimReach;
        if (result.Target != AimTargetKind.None)
        {
            if(result.HitDistance.HasValue) end=origin+direction*result.HitDistance.Value;
            else
            {
                var bounds = new Bounds(new Vector3(result.X+0.5f,result.Y+0.5f,result.Z+0.5f),Vector3.one);
                if (bounds.IntersectRay(new Ray(origin,direction),out float distance)) end = origin + direction * Mathf.Max(0,distance-0.02f);
            }
            Signal(end);
        }
        if (_trace != null)
        {
            _trace.SetPosition(0,ViewModel != null ? ViewModel.MuzzlePosition : origin);
            _trace.SetPosition(1,end);
            _traceLife = 0.055f;
            _trace.enabled = true;
        }
        if (result.DamageApplied > 0)
        {
            _hitLife = 0.14f;
            Sound("Impact",0.2f);
            Message(result.Destroyed ? result.Target == AimTargetKind.Core ? "RELAY DISABLED" : "WARDEN DISABLED"
                : result.DamageApplied + " DAMAGE",false);
            if (result.Destroyed && result.Target == AimTargetKind.Core) Sound("RelayCollapse",0.4f);
        }
    }

    public void Gathered(GatherResult result, BlockType type, int received, Vector3 cell)
    {
        if (result == GatherResult.Success)
        {
            Sound("StoneChisel"); Signal(cell);
            Message(received > 0 ? "+" + received + " " + Name(type) : "RESOURCE SALVAGED",false);
        }
        else Message(result == GatherResult.NotGatherable
            ? BlockInfo.IsGatherable(type) ? "PACK FULL · SURFACE KEPT" : "THIS SURFACE CANNOT BE SALVAGED"
            : "AIM AT STONE, EMBER ORE OR TIMBER",true);
    }

    public void Built(BuildResult result, BlockType type, Vector3 cell)
    {
        if (result == BuildResult.Success) { Sound("ConstructionLatch"); Signal(cell+Vector3.up*0.6f); Message(Name(type) + " PLACED",false); }
        else Message(result == BuildResult.InsufficientResources ? "INSUFFICIENT BUILD RESOURCES"
            : result == BuildResult.Occupied ? "BUILD SPACE OCCUPIED · AIM ELSEWHERE"
            : result == BuildResult.NotAir ? "BUILD CELL OCCUPIED" : result == BuildResult.Floating ? "AIM AT A SUPPORTED SURFACE"
            : "BUILD LOCATION UNAVAILABLE",true);
    }

    public void Reloaded(bool changed, bool full)
    {
        if (changed) { Sound("MagazineLatch"); if (ViewModel != null) ViewModel.ReactReload(); Message("MAGAZINE RELOADED",false); }
        else Message(full ? "MAGAZINE FULL" : "NO RESERVE AMMUNITION",true);
    }

    public void ResetSignals()
    {
        _traceLife = _impactLife = _messageLife = _hitLife = _hurtLife = 0;
        if (_trace != null) _trace.enabled = false;
        if (_impact != null) _impact.SetActive(false);
        if (_message != null) { _message.text = ""; _message.enabled = false; }
        if (_target != null) _target.enabled = false;
        if (_hurt != null) _hurt.color = Color.clear;
        if (Reticle != null) { Reticle.color = Color.white; Reticle.fontSize = 22; }
        if (_effects != null) _effects.Stop();
        if (ViewModel != null) ViewModel.ResetFeedbackPose();
    }

    void Signal(Vector3 point) { _impactPoint = point; _impactLife = 0.2f; }
    void Message(string text, bool refused)
    {
        if (_message != null) { _message.text = text; _message.color = refused ? new Color(1,0.65f,0.3f) : new Color(0.4f,0.95f,1); }
        _messageLife = 1.6f;
        if (refused) Sound("ActionRefused",0.16f);
    }

    static string Name(BlockType type) => type == BlockType.EmberOre ? "EMBER ORE" : type == BlockType.Wood ? "TIMBER"
        : type == BlockType.Stone ? "STONE" : type == BlockType.Bedrock ? "FOUNDATION"
        : type == BlockType.KineticMill ? "KINETIC MILL" : type == BlockType.ShockPylon ? "SHOCK PYLON" : type.ToString().ToUpperInvariant();

    Text Label(Transform parent, string name, Vector2 position, int size)
    {
        var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f,0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = new Vector2(850,50);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter; text.color = new Color(0.6f,0.9f,0.96f);
        text.supportRichText = false; text.raycastTarget = false;
        return text;
    }

    void BuildUi()
    {
        var root = new GameObject("Action feedback",typeof(Canvas),typeof(CanvasScaler));
        _uiRoot = root;
        root.transform.SetParent(transform,false);
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
        var hurtRect = new GameObject("Suit impact",typeof(RectTransform)).GetComponent<RectTransform>();
        hurtRect.SetParent(root.transform,false); hurtRect.anchorMin = Vector2.zero; hurtRect.anchorMax = Vector2.one;
        hurtRect.offsetMin = hurtRect.offsetMax = Vector2.zero; _hurt = hurtRect.gameObject.AddComponent<Image>(); _hurt.raycastTarget = false;
        _hurt.color = Color.clear;
        _target = Label(root.transform,"Aim target",new Vector2(0,-55),18);
        _message = Label(root.transform,"Action result",new Vector2(0,-110),22);
    }

    void OnDestroy()
    {
        if (_ambience != null) { _ambience.Stop(); Destroy(_ambience); }
        if (_effects != null) { _effects.Stop(); Destroy(_effects); }
        if (_trace != null) Destroy(_trace.gameObject);
        if (_impact != null) Destroy(_impact);
        if (_uiRoot != null) Destroy(_uiRoot);
        if (_signal != null) Destroy(_signal);
    }
}
