#nullable enable
using System;
using System.Collections.Generic;
using Forgeborne.Core;
using UnityEngine;

/// <summary>Authored original actor skins, constrained to canonical cells. No simulation ownership.</summary>
public sealed class WorldActorDisplay : MonoBehaviour
{
    public GameManager? Manager;
    private readonly Dictionary<Enemy,Transform> _drones=new Dictionary<Enemy,Transform>();
    private readonly HashSet<Enemy> _visible=new HashSet<Enemy>();
    private readonly List<Enemy> _removed=new List<Enemy>();
    private GameObject? _wardenPrefab;
    private GameObject? _relayPrefab;
    private Transform? _core;

    void Awake()
    {
        _wardenPrefab=Resources.Load<GameObject>("Art/Prefabs/EmberWarden");
        _relayPrefab=Resources.Load<GameObject>("Art/Prefabs/AetherRelay");
        if(_wardenPrefab==null || _relayPrefab==null)
            throw new InvalidOperationException("Prepare authored warden and relay prefabs before Play.");
    }
    Transform Drone(Enemy enemy)
    {
        var root=AuthoredSkin.Create(_wardenPrefab!,transform,"Ember Warden").transform;
        if(root.GetComponentsInChildren<Collider>(true).Length!=0)
            throw new InvalidOperationException("Actor skins must not create parallel collision.");
        return root;
    }
    void LateUpdate()
    {
        var simulation=Manager!=null ? Manager.Simulation : null;
        if(simulation==null) return;
        var world=simulation.World;
        _visible.Clear();
        foreach(var enemy in world.Enemies)
        {
            if(enemy.IsDead) continue;
            _visible.Add(enemy);
            if(!_drones.TryGetValue(enemy,out var drone))
            {
                drone=Drone(enemy);
                _drones.Add(enemy,drone);
            }
            // Small suspension motion remains wholly inside the authoritative
            // cell. Cell-to-cell movement is still canonical and needs a later
            // continuous-motion contract; this is not a second moving hitbox.
            float suspension=Mathf.Sin(Time.time*3.1f+enemy.X*0.7f+enemy.Z*0.3f)*0.018f;
            drone.position=new Vector3(enemy.X+0.5f,enemy.Y+0.5f+suspension,enemy.Z+0.5f);
            var direction=new Vector3(simulation.Fps.X,drone.position.y,simulation.Fps.Z)-drone.position;
            if(direction.sqrMagnitude>0.001f) drone.rotation=Quaternion.LookRotation(direction);
        }
        _removed.Clear();
        foreach(var pair in _drones) if(!_visible.Contains(pair.Key)) _removed.Add(pair.Key);
        foreach(var enemy in _removed)
        {
            var removed=_drones[enemy].gameObject;
            removed.SetActive(false);
            Destroy(removed);_drones.Remove(enemy);
        }
        if(_core==null)
        {
            _core=AuthoredSkin.Create(_relayPrefab!,transform,"Aether Relay Core").transform;
            if(_core.GetComponentsInChildren<Collider>(true).Length!=0)
                throw new InvalidOperationException("Relay skin must not create collision.");
        }
        _core.position=new Vector3(world.CoreX+0.5f,world.CoreY+0.5f,world.CoreZ+0.5f);
        _core.gameObject.SetActive(world.CoreHealth>0);
    }
    void OnDestroy()
    {
        foreach(var drone in _drones.Values) if(drone!=null) Destroy(drone.gameObject);
        _drones.Clear();
        if(_core!=null) Destroy(_core.gameObject);
    }
}
