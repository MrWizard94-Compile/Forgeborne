#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Original authored carbine/gauntlet view. Presentation only; Core owns aim, ammo and damage.</summary>
public sealed class WeaponViewModel : MonoBehaviour
{
    public Transform? CameraAnchor;
    public Vector3 ViewOffset = new Vector3(0.19f,-0.10f,0.55f);
    public Vector3 ViewAngles = new Vector3(0,-12,0);
    private GameObject? _modelRoot;
    private Transform? _muzzle;
    private Transform? _chargingHandle;
    private Transform? _magazine,_leftForearm,_leftPalm;
    private Vector3 _chargingRest,_magazineRest,_leftElbowRest,_leftWristRest,_leftGripRest;
    private Vector3 _chargingLocalRest,_magazineLocalRest;
    private Quaternion _leftForearmRotation,_leftPalmRotation;
    private Vector3 _leftThumbOppositionAxis;
    private readonly List<BoneRest> _boneRest=new List<BoneRest>();
    private readonly List<FingerJoint> _leftFingers=new List<FingerJoint>(),_rightIndex=new List<FingerJoint>();
    private sealed class BoneRest
    {
        public Transform Bone=null!;
        public Vector3 Position,Scale;
        public Quaternion Rotation;
        public void Restore(){Bone.localPosition=Position;Bone.localRotation=Rotation;Bone.localScale=Scale;}
    }
    private sealed class FingerJoint
    {
        public Transform Bone=null!;
        public Vector3 BendAxis;
        public void Flex(float degrees){Bone.localRotation*=Quaternion.AngleAxis(degrees,BendAxis);}
    }
    private GameObject? _muzzleFlash;
    private Light? _muzzleLight;
    private Light? _viewFill;
    private Material? _flashMaterial;
    private Mesh? _flashMesh;
    private float _shotKick, _flashLife;
    public const float ReloadPresentationSeconds=2.1f;
    private double _reloadAge=ReloadPresentationSeconds;
    public Vector3 MuzzlePosition => _muzzle != null ? _muzzle.position : transform.position;
    public void SetModelVisible(bool visible)
    {
        if(_modelRoot==null)return;
        ResetFeedbackPose();_modelRoot.SetActive(visible);
        if(_viewFill!=null)_viewFill.enabled=visible;
    }
    public void ReactShot() { _shotKick=1; _flashLife=0.055f; _reloadAge=ReloadPresentationSeconds; }
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
            if(item.name=="Magazine") _magazine=item;
            if(item.name=="LeftForearm") _leftForearm=item;
            if(item.name=="LeftPalm") _leftPalm=item;
        }
        if(_muzzle==null) throw new InvalidOperationException("Authored muzzle landmark missing.");
        if(_chargingHandle==null || _magazine==null || _leftForearm==null || _leftPalm==null)
            throw new InvalidOperationException("Original carbine moving assemblies/glove rig missing.");
        _chargingRest=_modelRoot.transform.InverseTransformPoint(_chargingHandle.position);
        _magazineRest=_modelRoot.transform.InverseTransformPoint(_magazine.position);
        _chargingLocalRest=_chargingHandle.localPosition;_magazineLocalRest=_magazine.localPosition;
        _leftElbowRest=_modelRoot.transform.InverseTransformPoint(_leftForearm.position);
        _leftWristRest=_modelRoot.transform.InverseTransformPoint(_leftPalm.position);
        Transform? contact=null;
        foreach(var item in _modelRoot.GetComponentsInChildren<Transform>(true))if(item.name=="LeftPalmContact")contact=item;
        if(contact==null)throw new InvalidOperationException("Authored support-hand contact landmark missing.");
        _leftGripRest=_modelRoot.transform.InverseTransformPoint(contact.position);
        _leftForearmRotation=Quaternion.Inverse(_modelRoot.transform.rotation)*_leftForearm.rotation;
        _leftPalmRotation=Quaternion.Inverse(_modelRoot.transform.rotation)*_leftPalm.rotation;
        var bones=new HashSet<Transform>();
        foreach(var skin in _modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            foreach(var bone in skin.bones)
                if(bones.Add(bone))_boneRest.Add(new BoneRest{Bone=bone,Position=bone.localPosition,Rotation=bone.localRotation,Scale=bone.localScale});
        if(_boneRest.Count!=34)throw new InvalidOperationException("Both original seventeen-joint rigs must reach the weapon view.");
        foreach(string digit in new[]{"Index","Middle","Ring","Little","Thumb"})CacheFinger("Left"+digit,_leftFingers);
        CacheFinger("RightIndex",_rightIndex);
        _leftThumbOppositionAxis=_leftFingers[12].Bone.InverseTransformDirection(_modelRoot.transform.forward).normalized;
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
        var fill=new GameObject("Weapon diffuse fill");
        fill.transform.SetParent(CameraAnchor,false);fill.transform.localPosition=new Vector3(-.35f,.16f,.05f);
        _viewFill=fill.AddComponent<Light>();_viewFill.type=LightType.Point;
        _viewFill.color=new Color(.82f,.86f,.90f);_viewFill.intensity=.45f;_viewFill.range=1.8f;
        _viewFill.shadows=LightShadows.None;_viewFill.cullingMask=1<<30;
        ResetFeedbackPose();
    }
    public void ResetFeedbackPose()
    {
        _shotKick=_flashLife=0;_reloadAge=ReloadPresentationSeconds;
        if(_modelRoot!=null){_modelRoot.transform.localPosition=ViewOffset;_modelRoot.transform.localRotation=Quaternion.Euler(ViewAngles);}
        foreach(var bone in _boneRest)bone.Restore();
        if(_chargingHandle!=null) _chargingHandle.localPosition=_chargingLocalRest;
        if(_magazine!=null) _magazine.localPosition=_magazineLocalRest;
        if(_muzzleFlash!=null) _muzzleFlash.SetActive(false);
        if(_muzzleLight!=null)_muzzleLight.intensity=0;
    }
    private void CacheFinger(string prefix,List<FingerJoint> result)
    {
        if(_modelRoot==null)return;
        var joints=new Transform[3];
        foreach(var bone in _boneRest)
            for(int i=0;i<3;i++)if(bone.Bone.name==prefix+i)joints[i]=bone.Bone;
        foreach(var joint in joints)if(joint==null)throw new InvalidOperationException("Original finger chain missing: "+prefix);
        var normal=Vector3.Cross(joints[1].position-joints[0].position,joints[2].position-joints[1].position).normalized;
        if(normal.sqrMagnitude<.5f)throw new InvalidOperationException("Original finger bend plane is undefined: "+prefix);
        foreach(var joint in joints)result.Add(new FingerJoint{Bone=joint,BendAxis=joint.InverseTransformDirection(normal).normalized});
    }
    private static float Ease(float start,float end,float time)
    {
        float t=Mathf.InverseLerp(start,end,time);
        return t*t*t*(t*(t*6-15)+10); // Zero velocity and acceleration at each hold.
    }
    private static float ClearanceArc(float time)
    {
        float t=time*(1-time);
        return 64*t*t*t;
    }
    void LateUpdate()=>AdvancePresentation(Time.deltaTime);
    public void AdvancePresentation(float deltaTime)
    {
        if(float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime<0)throw new ArgumentOutOfRangeException(nameof(deltaTime));
        if(_modelRoot==null || !_modelRoot.activeInHierarchy) return;
        _shotKick=Mathf.MoveTowards(_shotKick,0,deltaTime*9);
        _flashLife=Mathf.Max(0,_flashLife-deltaTime);_reloadAge=Math.Min(ReloadPresentationSeconds,_reloadAge+deltaTime);
        float time=(float)(_reloadAge/ReloadPresentationSeconds);
        float reload=Ease(0,.22f,time)*(1-Ease(.75f,1,time));
        _modelRoot.transform.localPosition=ViewOffset+new Vector3(0,.006f*_shotKick-.025f*reload,-.045f*_shotKick);
        _modelRoot.transform.localRotation=Quaternion.Euler(ViewAngles)*Quaternion.Euler(-7*_shotKick+12*reload,0,-6*reload);
        foreach(var bone in _boneRest)bone.Restore();
        float reach=Ease(.02f,.18f,time);
        float draw=Ease(.24f,.34f,time)*(1-Ease(.38f,.48f,time));
        float chargingReach=Ease(.50f,.68f,time);
        float returnToGrip=Ease(.84f,1,time);
        float charging=Ease(.70f,.76f,time)*(1-Ease(.79f,.84f,time));
        var magazineTravel=new Vector3(0,-.13f*draw,-.012f*draw);
        var chargingTravel=Vector3.back*(charging*.035f);
        if(_magazine!=null)_magazine.position=_modelRoot.transform.TransformPoint(_magazineRest+magazineTravel);
        if(_chargingHandle!=null)_chargingHandle.position=_modelRoot.transform.TransformPoint(_chargingRest+chargingTravel);
        float open=-18*Mathf.Sin(reach*Mathf.PI)*(1-chargingReach)
            -12*Mathf.Sin(chargingReach*Mathf.PI)*(1-returnToGrip)
            -12*Mathf.Sin(returnToGrip*Mathf.PI);
        float hookGrip=chargingReach*(1-returnToGrip);
        for(int i=0;i<_leftFingers.Count;i++)
        {
            int digit=i/3,joint=i%3;
            // The index hooks the slide; the remaining fingers close into the
            // palm instead of extending across the receiver with the index.
            float curl=digit==4 ? (joint==0 ? 0 : joint==1 ? 20 : 15)
                : digit==0 ? (joint==0 ? 15 : joint==1 ? 25 : 20)
                : (joint==0 ? 40 : joint==1 ? 40 : 25);
            _leftFingers[i].Flex(open+curl*hookGrip);
        }
        _leftFingers[12].Bone.localRotation*=Quaternion.AngleAxis(-35*hookGrip,_leftThumbOppositionAxis);
        // Fold the refitted thumb toward the index after opposition.
        _leftFingers[12].Flex(-65*hookGrip);
        foreach(var finger in _rightIndex)finger.Flex(6*_shotKick);
        if(_leftForearm!=null && _leftPalm!=null)
        {
            var grasp=Quaternion.Euler(-70,0,0);
            var chargingGrasp=Quaternion.Euler(-70,-25,0);
            var magazineWrist=_magazineRest+new Vector3(-.047f,-.035f,.002f)+magazineTravel
                -grasp*(_leftGripRest-_leftWristRest);
            // The distal index joint hooks outside the slide. Its actual rig
            // offset, not a detached palm marker, follows the moving assembly.
            var hookOffset=_modelRoot.transform.InverseTransformPoint(_leftFingers[2].Bone.position)-_leftWristRest;
            var chargingWrist=_chargingRest+new Vector3(-.025f,-.002f,.027f)+chargingTravel-chargingGrasp*hookOffset;
            var wrist=Vector3.Lerp(_leftWristRest,magazineWrist,reach);
            wrist=Vector3.Lerp(wrist,chargingWrist,chargingReach);
            wrist=Vector3.Lerp(wrist,_leftWristRest,returnToGrip);
            // Lift away from the receiver between contacts instead of cutting
            // directly through it. Both arcs are zero at the held contacts.
            wrist+=new Vector3(-.070f,.025f,-.035f)*ClearanceArc(chargingReach)*(1-returnToGrip)
                +new Vector3(-.065f,-.045f,.015f)*ClearanceArc(returnToGrip);
            var turn=Quaternion.Slerp(Quaternion.identity,grasp,reach);
            turn=Quaternion.Slerp(turn,chargingGrasp,chargingReach);
            turn=Quaternion.Slerp(turn,Quaternion.identity,returnToGrip);
            var preferredElbow=_leftElbowRest+new Vector3(-.025f*chargingReach,-.08f*reach,-.15f*reach)*(1-returnToGrip);
            var direction=(wrist-preferredElbow).normalized;
            float length=Vector3.Distance(_leftWristRest,_leftElbowRest);
            var elbow=wrist-direction*length;
            _leftForearm.position=_modelRoot.transform.TransformPoint(elbow);
            _leftForearm.rotation=_modelRoot.transform.rotation*Quaternion.FromToRotation(_leftWristRest-_leftElbowRest,direction)*_leftForearmRotation;
            _leftPalm.rotation=_modelRoot.transform.rotation*turn*_leftPalmRotation;
        }
        if(_muzzleFlash!=null) _muzzleFlash.SetActive(_flashLife>0);
        if(_flashMaterial!=null) _flashMaterial.SetColor("_EmissionColor",(new Color(0.05f,0.8f,1)*(2+_flashLife*85)).gamma);
        if(_muzzleLight!=null) _muzzleLight.intensity=_flashLife*28;
    }
    void OnDestroy()
    {
        if(_modelRoot!=null) Destroy(_modelRoot);
        if(_flashMesh!=null) Destroy(_flashMesh);
        if(_flashMaterial!=null) Destroy(_flashMaterial);
        if(_viewFill!=null) Destroy(_viewFill.gameObject);
    }
}
