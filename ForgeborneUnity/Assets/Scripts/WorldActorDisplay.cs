#nullable enable
using System.Collections.Generic;
using Forgeborne.Core;
using UnityEngine;

/// <summary>Renders canonical enemies and the objective. Never advances or clones game state.</summary>
public sealed class WorldActorDisplay : MonoBehaviour
{
    public GameManager? Manager;
    private readonly Dictionary<Enemy, Transform> _drones = new Dictionary<Enemy, Transform>();
    private readonly HashSet<Enemy> _visible = new HashSet<Enemy>();
    private readonly List<Enemy> _removed = new List<Enemy>();
    private readonly List<Material> _materials = new List<Material>();
    private Material? _shell;
    private Material? _steel;
    private Material? _hostile;
    private Material? _energy;
    private Transform? _core;

    Material Surface(string name, Color color, float metallic, float smoothness, bool emission = false)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.5f);
        }
        _materials.Add(material);
        return material;
    }

    void Awake()
    {
        _shell = Surface("Forgeborne graphite shell", new Color(0.055f, 0.08f, 0.10f), 0.75f, 0.45f);
        _steel = Surface("Forgeborne brushed steel", new Color(0.26f, 0.31f, 0.34f), 0.8f, 0.6f);
        _hostile = Surface("Forgeborne hostile amber", new Color(1f, 0.19f, 0.025f), 0.3f, 0.6f, true);
        _energy = Surface("Forgeborne reactor cyan", new Color(0.04f, 0.75f, 1f), 0.2f, 0.6f, true);
    }

    Transform Part(Transform parent, string name, Vector3 position, Vector3 scale, Material? material)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(part.GetComponent<Collider>());
        return part.transform;
    }

    Transform Drone(Enemy enemy)
    {
        var root = new GameObject("Ember Warden").transform;
        root.SetParent(transform, false);
        root.position = new Vector3(enemy.X + 0.5f, enemy.Y + 0.5f, enemy.Z + 0.5f);
        Part(root, "Armoured hull", Vector3.zero, new Vector3(0.68f, 0.42f, 0.62f), _shell);
        Part(root, "Front visor", new Vector3(0, 0.04f, 0.32f), new Vector3(0.48f, 0.10f, 0.035f), _hostile);
        Part(root, "Upper fin", new Vector3(0, 0.27f, -0.06f), new Vector3(0.12f, 0.18f, 0.45f), _steel);
        for (int side = -1; side <= 1; side += 2)
        {
            Part(root, "Outrigger", new Vector3(side * 0.37f, -0.04f, 0), new Vector3(0.13f, 0.12f, 0.72f), _steel);
            Part(root, "Thruster", new Vector3(side * 0.34f, -0.20f, -0.18f), new Vector3(0.11f, 0.09f, 0.22f), _hostile);
        }
        return root;
    }

    void LateUpdate()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null) return;
        var world = simulation.World;
        _visible.Clear();
        foreach (var enemy in world.Enemies)
        {
            if (enemy.IsDead) continue;
            _visible.Add(enemy);
            if (!_drones.TryGetValue(enemy, out var drone))
            {
                drone = Drone(enemy);
                _drones.Add(enemy, drone);
            }
            // Cells remain authoritative, including aim/cover. The silhouette
            // stays in that same cell instead of interpolating a second hitbox.
            drone.position = new Vector3(enemy.X + 0.5f, enemy.Y + 0.5f, enemy.Z + 0.5f);
            var direction = new Vector3(simulation.Fps.X, drone.position.y, simulation.Fps.Z) - drone.position;
            if (direction.sqrMagnitude > 0.001f) drone.rotation = Quaternion.LookRotation(direction);
        }
        _removed.Clear();
        foreach (var pair in _drones)
            if (!_visible.Contains(pair.Key)) _removed.Add(pair.Key);
        foreach (var enemy in _removed)
        {
            Destroy(_drones[enemy].gameObject);
            _drones.Remove(enemy);
        }
        if (_core == null)
        {
            _core = new GameObject("Aether Relay Core").transform;
            _core.SetParent(transform, false);
            Part(_core, "Reactor chamber", Vector3.zero, new Vector3(0.48f, 0.72f, 0.48f), _energy);
            Part(_core, "Crown", new Vector3(0, 0.41f, 0), new Vector3(0.86f, 0.10f, 0.86f), _steel);
            Part(_core, "Base", new Vector3(0, -0.41f, 0), new Vector3(0.86f, 0.10f, 0.86f), _shell);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Part(_core, "Reactor strut", new Vector3(x * 0.36f, 0, z * 0.36f), new Vector3(0.1f, 0.72f, 0.1f), _shell);
        }
        _core.position = new Vector3(world.CoreX + 0.5f, world.CoreY + 0.5f, world.CoreZ + 0.5f);
        _core.gameObject.SetActive(world.CoreHealth > 0);
    }

    void OnDestroy()
    {
        foreach (var material in _materials) Destroy(material);
    }
}
