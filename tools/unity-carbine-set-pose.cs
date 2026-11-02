var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Actual paused FPS Main required.");
const float seconds=__SECONDS__;
if(seconds<0 || seconds>WeaponViewModel.ReloadPresentationSeconds)throw new System.InvalidOperationException("Pose time outside original reload.");
string before=g.SaveToJson();rig.ViewModel.ResetFeedbackPose();rig.ViewModel.ReactReload();rig.ViewModel.AdvancePresentation(seconds);
SessionState.SetInt("Forgeborne.CarbineCapture.SetFrame",Time.frameCount);
SessionState.SetFloat("Forgeborne.CarbineCapture.Time",seconds);
SessionState.SetString("Forgeborne.CarbineCapture.Snapshot",before);
SessionState.SetBool("Forgeborne.CarbineCapture.RunInBackground",Application.runInBackground);
SessionState.SetBool("Forgeborne.CarbineCapture.ViewEnabled",rig.ViewModel.enabled);
SessionState.SetString("Forgeborne.CarbineCapture.Mode","reload");
Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();
return new{setFrame=Time.frameCount,seconds=seconds,requiresLaterRenderedFrame=true};
