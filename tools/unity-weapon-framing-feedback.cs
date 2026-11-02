var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var feedback=UnityEngine.Object.FindFirstObjectByType<PlayerFeedback>();
var view=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || feedback==null || view==null)
    throw new System.InvalidOperationException("Actual paused Main required.");
var simulation=g.Simulation;string snapshot=g.SaveToJson();
var camera=g.FpsAdapter.GetComponent<Camera>();var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
try
{
    simulation.Fps.Teleport(16.5f,4,7.5f);simulation.AdvanceFrame(0,0,0,false);
    simulation.World.Enemies.Clear();var enemy=new Forgeborne.Core.Enemy(16,5,10);simulation.World.Enemies.Add(enemy);
    var eye=new Vector3(simulation.Fps.X,simulation.Fps.Y+Forgeborne.Core.Simulation.EyeHeight,simulation.Fps.Z);
    var direction=(new Vector3(16.5f,5.5f,10.5f)-eye).normalized;
    float yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
    float pitch=Mathf.Asin(direction.y)*Mathf.Rad2Deg;
    simulation.ApplyLook(yaw-simulation.Look.Yaw,pitch-simulation.Look.Pitch);
    camera.transform.SetPositionAndRotation(eye,Quaternion.Euler(-pitch,yaw,0));
    var hit=simulation.InspectAim();float analyticalDistance;
    var bounds=new Bounds(new Vector3(16.5f,5.5f,10.5f),Vector3.one);
    if(hit.Kind!=Forgeborne.Core.AimTargetKind.Enemy || !hit.Distance.HasValue
        || !bounds.IntersectRay(new Ray(eye,direction),out analyticalDistance)
        || Mathf.Abs(hit.Distance.Value-analyticalDistance)>.00002f)
        throw new System.InvalidOperationException("The current-world shot fixture is obstructed or aim disagrees with its independent box oracle.");
    int ammo=simulation.Weapon.Mag;long tick=simulation.World.Tick;
    typeof(FpsInputAdapter).GetMethod("FireWithFeedback",flags).Invoke(g.FpsAdapter,null);
    var trace=(LineRenderer)typeof(PlayerFeedback).GetField("_trace",flags).GetValue(feedback);
    if(enemy.Health!=30-simulation.EffectiveDamage || simulation.Weapon.Mag!=ammo-1 || !feedback.TraceVisible
        || Vector3.Distance(trace.GetPosition(0),view.MuzzlePosition)>.000002f
        || Vector3.Distance(trace.GetPosition(1),eye+direction*analyticalDistance)>.00002f)
        throw new System.InvalidOperationException("Production shot lost damage, ammo, authored muzzle or canonical hit endpoint.");
    string afterShot=g.SaveToJson();view.AdvancePresentation(0);g.FpsAdapter.ResetInteraction(false);
    var root=(GameObject)typeof(WeaponViewModel).GetField("_modelRoot",flags).GetValue(view);
    if(g.SaveToJson()!=afterShot || simulation.World.Tick!=tick || feedback.TraceVisible
        || Quaternion.Angle(root.transform.localRotation,Quaternion.Euler(view.ViewAngles))>.001f
        || root.transform.localPosition!=view.ViewOffset)
        throw new System.InvalidOperationException("Presentation/reset changed rules or lost the selected rest framing.");
    return new{productionFire=true,independentCanonicalHitDistanceMetres=analyticalDistance,
        traceStartsAtActualAuthoredMuzzle=true,traceEndsAtCanonicalHit=true,ammoAndDamagePreserved=true,
        selectedRestPoseRestored=true,presentationDoesNotAdvanceWorld=true,
        scope="Scripted existing production action and real line renderer, not native input, heard mix or fun acceptance."};
}
finally
{
    g.FpsAdapter.ResetInteraction(false);
    if(!g.TryLoadFromJson(snapshot) || !object.ReferenceEquals(simulation,g.Simulation) || g.SaveToJson()!=snapshot)
        throw new System.InvalidOperationException("Framing action probe failed to restore the original canonical state in place.");
    camera.transform.SetPositionAndRotation(oldPosition,oldRotation);
}
