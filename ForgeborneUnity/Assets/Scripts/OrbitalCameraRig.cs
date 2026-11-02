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
        if(Manager.MeshRenderer!=null)Manager.MeshRenderer.SetOverheadCutaway(orbital);
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
        var prefab=Resources.Load<GameObject>("Art/Prefabs/ForgeSuit");
        if(prefab==null) throw new System.InvalidOperationException("Prepare the original ForgeSuit model before Play.");
        _suit=AuthoredSkin.Create(prefab,transform,"Suit position").transform;
        _suit.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (_camera != null) _camera.enabled = false;
        if (FirstPersonCamera != null) { FirstPersonCamera.enabled = true; FirstPersonCamera.tag = "MainCamera"; }
        gameObject.tag = "Untagged";
        IsOrbital = false;
        if(Manager!=null && Manager.MeshRenderer!=null)Manager.MeshRenderer.SetOverheadCutaway(false);
        if (InputAdapter != null) InputAdapter.SetCommandCamera(null);
        if (ViewModel != null) ViewModel.SetModelVisible(true);
        if (FirstPersonReticle != null) FirstPersonReticle.SetActive(true);
        if (FirstPersonControls != null) FirstPersonControls.SetActive(true);
        if (_suit != null) _suit.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (_suit != null) Destroy(_suit.gameObject);
    }
}
