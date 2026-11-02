var view=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
if(view!=null){view.ResetFeedbackPose();view.enabled=SessionState.GetBool("Forgeborne.CarbineCapture.ViewEnabled",true);}
Application.runInBackground=SessionState.GetBool("Forgeborne.CarbineCapture.RunInBackground",false);
return new{presentationReset=true,originalRuntimeBackgroundFlagRestored=true,noCanonicalStateReplacement=true};
