var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || rig==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0
    || g.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2)
    throw new System.InvalidOperationException("Actual paused v2 Main required.");
var s=g.Simulation;var geometry=s.World.Chunk.Geometry;string saved=g.SaveToJson();bool originalMode=rig.IsOrbital;
try
{
    rig.SetOrbital(false);
    if((g.FpsAdapter.GetComponent<Camera>().cullingMask&(1<<VoxelMeshRenderer.OverheadLayer))==0)
        throw new System.InvalidOperationException("FPS camera excludes its physical roof layer.");
    var roofs=g.MeshRenderer.GetComponentsInChildren<MeshCollider>().Where(c=>c.sharedMesh!=null && c.gameObject.layer==VoxelMeshRenderer.OverheadLayer).ToArray();
    if(roofs.Length!=6 || roofs.Any(c=>!c.enabled || c.GetComponent<MeshRenderer>().shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.On))
        throw new System.InvalidOperationException("FPS roof rendering/collision is incomplete.");
    rig.SetOrbital(true);var camera=rig.GetComponent<Camera>();
    if(camera.orthographic || roofs.Any(c=>!c.enabled || c.GetComponent<MeshRenderer>().shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly))
        throw new System.InvalidOperationException("Perspective orbital cutaway must preserve actual roof collisions and shadows.");
    bool found=false;RaycastHit physical=new RaycastHit(),visible=new RaycastHit();Vector3 target=Vector3.zero;
    for(int z=10;z<=28 && !found;z+=2)for(int x=20;x<=28 && !found;x+=2)
    {
        target=new Vector3(x,4,z);var ray=new Ray(camera.transform.position,target-camera.transform.position);
        if(!Physics.Raycast(ray,out physical,120,Physics.DefaultRaycastLayers) || physical.collider.gameObject.layer!=VoxelMeshRenderer.OverheadLayer)continue;
        if(!g.FpsAdapter.TryGetCommandSurface(ray,out visible) || Mathf.Abs(visible.point.y-4)>.001f)continue;
        var origin=ray.origin;var direction=ray.direction;Forgeborne.Core.SurfaceRayHit canonical;
        if(!geometry.Raycast(new System.Numerics.Vector3(origin.x,origin.y,origin.z),new System.Numerics.Vector3(direction.x,direction.y,direction.z),120,out canonical)
            || Mathf.Abs(canonical.Distance-physical.distance)>.0001f)
            throw new System.InvalidOperationException("Cutaway changed or bypassed canonical physical roof geometry.");
        found=true;
    }
    if(!found)throw new System.InvalidOperationException("Actual production command picking could not reach a visible floor beneath the hidden roof.");
    rig.SetOrbital(false);
    if(roofs.Any(c=>c.GetComponent<MeshRenderer>().shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.On)
        || (g.FpsAdapter.CommandRaycastMask&(1<<VoxelMeshRenderer.OverheadLayer))==0
        || g.SaveToJson()!=saved || !object.ReferenceEquals(geometry,s.World.Chunk.Geometry))
        throw new System.InvalidOperationException("FPS restoration changed canonical world state, roof visibility or physical picking.");
    return new{roofPieces=roofs.Length,perspectiveCutaway=true,roofCollidersRetained=true,roofShadowsRetained=true,
        actualProductionCommandPickerReachesVisibleFloor=true,canonicalRoofRayUnchanged=true,fpsRoofRestored=true,worldAndSimulationUnchanged=true,
        target=new[]{target.x,target.y,target.z},physicalRoofHitDistance=physical.distance,commandFloorHit=new[]{visible.point.x,visible.point.y,visible.point.z},
        scope="Actual paused production camera/render/picker consumers; no native keyboard, representative performance or fun acceptance."};
}
finally{rig.SetOrbital(originalMode);}
