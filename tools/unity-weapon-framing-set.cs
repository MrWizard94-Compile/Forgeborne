var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Paused actual FPS Main required.");
var view=rig.ViewModel;
const string label="__LABEL__";
const float age=__AGE__;
if(age<0 || age>WeaponViewModel.ReloadPresentationSeconds || label.Any(c=>!char.IsLetterOrDigit(c) && c!='-'))
    throw new System.InvalidOperationException("Invalid framing candidate.");
if(SessionState.GetBool("Forgeborne.Framing.Active",false))
    throw new System.InvalidOperationException("Restore the preceding framing candidate first.");
SessionState.SetFloat("Forgeborne.Framing.X",view.ViewOffset.x);
SessionState.SetFloat("Forgeborne.Framing.Y",view.ViewOffset.y);
SessionState.SetFloat("Forgeborne.Framing.Z",view.ViewOffset.z);
SessionState.SetFloat("Forgeborne.Framing.AngleX",view.ViewAngles.x);
SessionState.SetFloat("Forgeborne.Framing.AngleY",view.ViewAngles.y);
SessionState.SetFloat("Forgeborne.Framing.AngleZ",view.ViewAngles.z);
SessionState.SetBool("Forgeborne.Framing.Background",Application.runInBackground);
SessionState.SetBool("Forgeborne.Framing.Enabled",view.enabled);
SessionState.SetString("Forgeborne.Framing.Snapshot",g.SaveToJson());
SessionState.SetString("Forgeborne.Framing.Label",label);
SessionState.SetInt("Forgeborne.Framing.Frame",Time.frameCount);
SessionState.SetBool("Forgeborne.Framing.Active",true);
view.ViewOffset=new Vector3(__X__,__Y__,__Z__);
view.ViewAngles=new Vector3(0,__YAW__,0);
view.ResetFeedbackPose();
if(__SHOT__)view.ReactShot();else view.ReactReload();
view.AdvancePresentation(age);
view.enabled=false;
Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();
return new{label=label,offset=view.ViewOffset.ToString(),yaw=__YAW__,age=age,setFrame=Time.frameCount,canonicalStateUnchanged=g.SaveToJson()==SessionState.GetString("Forgeborne.Framing.Snapshot","")};
