#nullable enable
using Forgeborne.Core;
using UnityEngine;

/// <summary>Perspective command view of the existing world. FpsInputAdapter remains the sole frame driver.</summary>
[DefaultExecutionOrder(-50), RequireComponent(typeof(Camera))]
public sealed class OrbitalCameraRig : MonoBehaviour
{
    public GameManager? Manager;
    public FpsInputAdapter? InputAdapter;
    public Camera? FirstPersonCamera;
    public WeaponViewModel? ViewModel;
    public GameObject? FirstPersonReticle;
    public GameObject? FirstPersonControls;
    public bool IsOrbital { get; private set; }
    public Vector3 FocusPoint => _focus;
    public float ViewDistance => _distance;
    private Camera? _camera;
    private Transform? _suit;
    private Material? _shell;
    private Material? _signal;
    private WorldZone? _seenWorld;
    private Vector3 _focus = new Vector3(16, 4.5f, 16);
    private float _distance = 35;
    private float _azimuth;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        if (FirstPersonCamera != null) _camera.CopyFrom(FirstPersonCamera);
        _camera.fieldOfView = 52;
        _camera.nearClipPlane = 0.1f;
        _camera.enabled = false;
        gameObject.tag = "Untagged";
    }

    void Start()
    {
        if (Manager == null || InputAdapter == null || FirstPersonCamera == null)
        {
            Debug.LogError("Orbital camera requires the existing manager, FPS adapter and camera.");
            enabled = false;
            return;
        }
        BuildSuitView();
    }

    void Update()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null || InputAdapter == null || InputAdapter.InputSuppressed || !Application.isFocused
            || simulation.IsPlayerDead || simulation.IsObjectiveComplete) return;
        if (Input.GetKeyDown(KeyCode.Tab)) SetOrbital(!IsOrbital);
        if (!IsOrbital) return;
        if (Input.GetKeyDown(KeyCode.F)) FocusPlayer();
        float x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
        float z = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        var pan = Vector3.ClampMagnitude(new Vector3(x, 0, z), 1);
        _focus += Quaternion.Euler(0, _azimuth, 0) * pan * (_distance * 0.35f * Time.deltaTime);
        _focus.x = Mathf.Clamp(_focus.x, 0, VoxelChunk.Size - 1);
        _focus.z = Mathf.Clamp(_focus.z, 0, VoxelChunk.Size - 1);
        _distance = Mathf.Clamp(_distance - Input.GetAxis("Mouse ScrollWheel") * 25, 16, 55);
        _azimuth += ((Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0)) * 55 * Time.deltaTime;
    }

    void LateUpdate()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null) return;
        if (!ReferenceEquals(_seenWorld, simulation.World))
        {
            _seenWorld = simulation.World;
            FocusPlayer();
        }
        if (!IsOrbital) return;
        ApplyView();
    }

    void ApplyView()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null) return;
        transform.rotation = Quaternion.Euler(55, _azimuth, 0);
        transform.position = _focus - transform.forward * _distance;
        if (_suit != null)
        {
            _suit.position = new Vector3(simulation.Fps.X, simulation.Fps.Y, simulation.Fps.Z);
            _suit.rotation = Quaternion.Euler(0, simulation.Look.Yaw, 0);
        }
    }

    public void FocusPlayer()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation != null) _focus = new Vector3(simulation.Fps.X, simulation.Fps.Y + 0.9f, simulation.Fps.Z);
    }

    public void SetOrbital(bool orbital)
    {
        if (_camera == null || FirstPersonCamera == null || InputAdapter == null
            || Manager == null || Manager.Simulation == null || IsOrbital == orbital) return;
        InputAdapter.ResetInteraction(false); // Switching view cancels free pending commands without spending resources.
        IsOrbital = orbital;
        FirstPersonCamera.enabled = !orbital;
        FirstPersonCamera.tag = orbital ? "Untagged" : "MainCamera";
        _camera.enabled = orbital;
        gameObject.tag = orbital ? "MainCamera" : "Untagged";
        if (orbital) FocusPlayer();
        InputAdapter.SetCommandCamera(orbital ? _camera : null);
        if (!orbital && !InputAdapter.InputSuppressed) InputAdapter.CapturePointer();
        if (ViewModel != null) ViewModel.SetModelVisible(!orbital);
        if (FirstPersonReticle != null) FirstPersonReticle.SetActive(!orbital);
        if (FirstPersonControls != null) FirstPersonControls.SetActive(!orbital);
        if (_suit != null) _suit.gameObject.SetActive(orbital);
        if (orbital) ApplyView();
    }

    void BuildSuitView()
    {
        var baseSurface = Resources.Load<Material>("Materials/RuntimeOpaque");
        var glowSurface = Resources.Load<Material>("Materials/RuntimeEmissive");
        if (baseSurface == null || glowSurface == null) throw new System.InvalidOperationException("Suit surface resources are missing.");
        _shell = new Material(baseSurface) { color = new Color(0.12f, 0.25f, 0.28f), name = "Forgeborne suit shell" };
        _signal = new Material(glowSurface) { color = new Color(0.05f, 0.9f, 1), name = "Forgeborne suit signal" };
        _signal.SetColor("_EmissionColor", new Color(0.05f, 0.9f, 1) * 1.5f);
        _suit = new GameObject("Suit position").transform;
        _suit.SetParent(transform, false);
        Part("Torso", new Vector3(0,1,0), new Vector3(0.56f,0.65f,0.35f), _shell);
        Part("Helmet", new Vector3(0,1.55f,0), new Vector3(0.4f,0.36f,0.38f), _shell);
        Part("Visor", new Vector3(0,1.57f,0.2f), new Vector3(0.32f,0.12f,0.025f), _signal);
        Part("Left leg", new Vector3(-0.17f,0.35f,0), new Vector3(0.22f,0.7f,0.3f), _shell);
        Part("Right leg", new Vector3(0.17f,0.35f,0), new Vector3(0.22f,0.7f,0.3f), _shell);
        Part("Beacon", new Vector3(0,2,0), new Vector3(0.12f,0.12f,0.12f), _signal);
        _suit.gameObject.SetActive(false);
    }

    void Part(string label, Vector3 position, Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = label;
        part.transform.SetParent(_suit, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(part.GetComponent<Collider>()); // View only; the canonical controller remains the player's collision owner.
    }

    void OnDisable()
    {
        if (_camera != null) _camera.enabled = false;
        if (FirstPersonCamera != null) { FirstPersonCamera.enabled = true; FirstPersonCamera.tag = "MainCamera"; }
        gameObject.tag = "Untagged";
        IsOrbital = false;
        if (InputAdapter != null) InputAdapter.SetCommandCamera(null);
        if (ViewModel != null) ViewModel.SetModelVisible(true);
        if (FirstPersonReticle != null) FirstPersonReticle.SetActive(true);
        if (FirstPersonControls != null) FirstPersonControls.SetActive(true);
        if (_suit != null) _suit.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (_shell != null) Destroy(_shell);
        if (_signal != null) Destroy(_signal);
    }
}
