#nullable enable
using System;
using UnityEngine;

/// <summary>Original authored carbine/gauntlet view. Presentation only; Core owns aim, ammo and damage.</summary>
public sealed class WeaponViewModel : MonoBehaviour
{
    public Transform? CameraAnchor;
    public Vector3 ViewOffset = new Vector3(0.22f,-0.22f,0.60f);
    private GameObject? _modelRoot;
    private Transform? _muzzle;
    private Transform? _chargingHandle;
    private Vector3 _chargingRest;
    private GameObject? _muzzleFlash;
    private Light? _muzzleLight;
    private Material? _flashMaterial;
    private Mesh? _flashMesh;
    private float _shotKick, _flashLife;
    private float _reloadAge=1;
    public Vector3 MuzzlePosition => _muzzle != null ? _muzzle.position : transform.position;
    public void SetModelVisible(bool visible) { if(_modelRoot!=null) _modelRoot.SetActive(visible); }
    public void ReactShot() { _shotKick=1; _flashLife=0.055f; }
    public void ReactReload() { _reloadAge=0; }

    void Start()
    {
        if(CameraAnchor==null) throw new InvalidOperationException("Wire the physical FPS camera before loading the carbine.");
        var prefab=Resources.Load<GameObject>("Art/Prefabs/PulseCarbine");
        if(prefab==null) throw new InvalidOperationException("Prepare original PulseCarbine prefab before Play.");
        _modelRoot=AuthoredSkin.Create(prefab,CameraAnchor,"ForgeborneWeaponViewModel");
        foreach(var item in _modelRoot.GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.layer=30; // Exclude camera-local geometry from the environment reflection.
            if(item.name=="MuzzleSocket") _muzzle=item;
            if(item.name=="ChargingHandle") _chargingHandle=item;
        }
        if(_muzzle==null) throw new InvalidOperationException("Authored muzzle landmark missing.");
        if(_chargingHandle!=null) _chargingRest=_chargingHandle.localPosition;
        if(_modelRoot.GetComponentsInChildren<Collider>(true).Length!=0)
            throw new InvalidOperationException("View art must not own collision.");
        var resource=Resources.Load<Material>("Art/Materials/Cyan");
        if(resource==null) throw new InvalidOperationException("Authored energy material missing.");
        _flashMaterial=new Material(resource){name="Forgeborne discharge bloom"};
        _muzzleFlash=new GameObject("Ion discharge");
        _muzzleFlash.layer=30;
        _muzzleFlash.transform.SetParent(_modelRoot.transform,false);
        _muzzleFlash.transform.localPosition=_modelRoot.transform.InverseTransformPoint(_muzzle.position);
        _flashMesh=new Mesh{name="Original radial ion burst"};
        var points=new Vector3[10]; points[0]=new Vector3(0,0,-0.012f); points[1]=new Vector3(0,0,0.15f);
        for(int i=0;i<8;i++)
        {
            float angle=i*Mathf.PI/4;
            float radius=i%2==0 ? 0.029f : 0.018f;
            points[i+2]=new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,0.03f);
        }
        var triangles=new int[48];
        for(int i=0;i<8;i++)
        {
            int next=(i+1)%8+2,at=i*6;
            triangles[at]=0;triangles[at+1]=next;triangles[at+2]=i+2;
            triangles[at+3]=1;triangles[at+4]=i+2;triangles[at+5]=next;
        }
        _flashMesh.vertices=points;_flashMesh.triangles=triangles;_flashMesh.RecalculateNormals();_flashMesh.RecalculateBounds();
        _muzzleFlash.AddComponent<MeshFilter>().sharedMesh=_flashMesh;
        var renderer=_muzzleFlash.AddComponent<MeshRenderer>();renderer.sharedMaterial=_flashMaterial;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        _muzzleLight=_muzzleFlash.AddComponent<Light>();_muzzleLight.type=LightType.Point;
        _muzzleLight.color=new Color(0.05f,0.8f,1);_muzzleLight.range=1.6f;
        _muzzleLight.shadows=LightShadows.None;
        ResetFeedbackPose();
    }
    public void ResetFeedbackPose()
    {
        _shotKick=_flashLife=0;_reloadAge=1;
        if(_modelRoot!=null){_modelRoot.transform.localPosition=ViewOffset;_modelRoot.transform.localRotation=Quaternion.identity;}
        if(_chargingHandle!=null) _chargingHandle.localPosition=_chargingRest;
        if(_muzzleFlash!=null) _muzzleFlash.SetActive(false);
    }
    void LateUpdate()
    {
        if(_modelRoot==null) return;
        float dt=Time.deltaTime;
        _shotKick=Mathf.MoveTowards(_shotKick,0,dt*9);
        _flashLife=Mathf.Max(0,_flashLife-dt);_reloadAge+=dt;
        float reload=Mathf.Sin(Mathf.Clamp01(_reloadAge/0.3f)*Mathf.PI);
        _modelRoot.transform.localPosition=ViewOffset+new Vector3(0,0.006f*_shotKick-0.06f*reload,-0.045f*_shotKick);
        _modelRoot.transform.localRotation=Quaternion.Euler(-7*_shotKick+25*reload,0,-12*reload);
        if(_chargingHandle!=null) _chargingHandle.localPosition=_chargingRest+Vector3.back*(reload*0.045f);
        if(_muzzleFlash!=null) _muzzleFlash.SetActive(_flashLife>0);
        if(_flashMaterial!=null) _flashMaterial.SetColor("_EmissionColor",new Color(0.05f,0.8f,1)*(2+_flashLife*85));
        if(_muzzleLight!=null) _muzzleLight.intensity=_flashLife*28;
    }
    void OnDestroy()
    {
        if(_modelRoot!=null) Destroy(_modelRoot);
        if(_flashMesh!=null) Destroy(_flashMesh);
        if(_flashMaterial!=null) Destroy(_flashMaterial);
    }
}
