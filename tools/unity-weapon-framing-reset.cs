if(SessionState.GetBool("Forgeborne.Framing.Active",false))
{
    var view=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
    if(view!=null)
    {
        view.ViewOffset=new Vector3(SessionState.GetFloat("Forgeborne.Framing.X",0),
            SessionState.GetFloat("Forgeborne.Framing.Y",0),SessionState.GetFloat("Forgeborne.Framing.Z",0));
        view.ViewAngles=new Vector3(SessionState.GetFloat("Forgeborne.Framing.AngleX",0),
            SessionState.GetFloat("Forgeborne.Framing.AngleY",0),SessionState.GetFloat("Forgeborne.Framing.AngleZ",0));
        view.ResetFeedbackPose();
        view.enabled=SessionState.GetBool("Forgeborne.Framing.Enabled",true);
    }
    Application.runInBackground=SessionState.GetBool("Forgeborne.Framing.Background",false);
    SessionState.SetBool("Forgeborne.Framing.Active",false);
}
return new{originalViewOffsetAndRuntimeFlagsRestored=true};
