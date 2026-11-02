var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Actual paused FPS Main required.");
const float yaw=__YAW__;
var view=rig.ViewModel;string before=g.SaveToJson();view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(0);
SessionState.SetBool("Forgeborne.CarbineCapture.ViewEnabled",view.enabled);view.enabled=false;
var field=typeof(WeaponViewModel).GetField("_modelRoot",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
var root=(GameObject)field.GetValue(view);root.transform.localPosition=new Vector3(0,0,1.0f);root.transform.localRotation=Quaternion.Euler(0,yaw,-10);
SessionState.SetInt("Forgeborne.CarbineCapture.SetFrame",Time.frameCount);SessionState.SetFloat("Forgeborne.CarbineCapture.Time",0);
SessionState.SetString("Forgeborne.CarbineCapture.Snapshot",before);
SessionState.SetString("Forgeborne.CarbineCapture.Mode",yaw>0 ? "inspection-right" : "inspection-left");
SessionState.SetBool("Forgeborne.CarbineCapture.RunInBackground",Application.runInBackground);
Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();
return new{setFrame=Time.frameCount,diagnosticModelRotation=true,yaw=yaw,notNormalGameplayFraming=true};
