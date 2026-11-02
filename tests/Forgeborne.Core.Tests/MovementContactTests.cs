using System.Numerics;
using Forgeborne.Core;

internal static class MovementContactTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var profile in new[]{SurfaceProfile.AuthoredFoundryV1,SurfaceProfile.AuthoredFoundryV2})
        {
            var world=new WorldZone(42);AuthoredFoundry.Populate(world,profile);
            void ApproachAndLeave(Vector3 start,Vector3 direction,string label)
            {
                var fps=new FpsController(start.X,start.Y,start.Z);
                fps.UpdateWorld(1f/120,0,0,false,world.Chunk);
                bool contact=false;Vector3 at=default;
                for(int i=0;i<600;i++)
                {
                    var before=Position(fps);
                    fps.UpdateWorld(1f/120,direction.X,direction.Z,false,world.Chunk);
                    at=Position(fps);
                    if(HorizontalDistance(at,before)<.00001f){contact=true;break;}
                }
                check(contact,profile+": "+label+" reaches a real obstruction");
                for(int i=0;i<20;i++)fps.UpdateWorld(1f/120,-direction.X,-direction.Z,false,world.Chunk);
                check(Vector3.Dot(Position(fps)-at,-direction)>.8f,
                    profile+": "+label+" permits backing away after sustained contact");
                check(!world.Chunk.Geometry.OverlapsBody(Position(fps)+Vector3.UnitY*.002f),
                    profile+": "+label+" escape does not tunnel through another physical surface");
            }
            ApproachAndLeave(new Vector3(16.5f,4,16.5f),-Vector3.UnitX,"vessel collar");
            if(profile==SurfaceProfile.AuthoredFoundryV2)
            {
                ApproachAndLeave(new Vector3(10,4,23),-Vector3.UnitX,"inclined formation west contact");
                ApproachAndLeave(new Vector3(10,4,23),-Vector3.UnitZ,"inclined formation south contact");
            }
            // Exact pre-fix coordinates came from real production movement,
            // not a new spawn intentionally placed inside a solid.
            var stuck=new FpsController(6.0127454f,3.9999995f,16.5f);
            for(int i=0;i<20;i++)stuck.UpdateWorld(1f/120,1,0,false,world.Chunk);
            check(stuck.X>6.8f,profile+": a retained contact pose recovers without restarting or teleporting");

            var sliding=new FpsController(16.5f,4,4.5f);
            check(!world.Chunk.Geometry.OverlapsBody(Position(sliding)+Vector3.UnitY*.002f),
                profile+": wall slide starts from free playable space");
            sliding.UpdateWorld(1f/120,0,0,false,world.Chunk);
            float beforeX=sliding.X;
            for(int i=0;i<80;i++)sliding.UpdateWorld(1f/120,1,-1,false,world.Chunk);
            check(sliding.X>beforeX+3.8f && sliding.Z>3 && sliding.Z<3.2f,
                profile+": pressure into the hall wall preserves tangential movement");
        }

        // Independently specified convex box: touching/one-micrometre overlap
        // may separate or slide; approaching and long sweeps remain obstructed.
        var points=new[]{new Vector3(5,4,5),new Vector3(6,4,5),new Vector3(6,6,5),new Vector3(5,6,5),
            new Vector3(5,4,6),new Vector3(6,4,6),new Vector3(6,6,6),new Vector3(5,6,6)};
        var indices=new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2};
        var faces=new List<TerrainFace>();
        for(int i=0;i<indices.Length;i+=3)
        {
            var a=points[indices[i]];var b=points[indices[i+1]];var c=points[indices[i+2]];
            var normal=Vector3.Normalize(Vector3.Cross(b-a,c-a));
            faces.Add(new TerrainFace(a,b,c,normal,normal,normal,5,4,5,BlockType.Stone));
        }
        var box=new TerrainGeometry(faces);
        foreach(float x in new[]{6.3f,6.3f-.000001f})
        {
            var feet=new Vector3(x,4.1f,5.5f);
            check(!box.SweepBody(feet,new Vector3(.05f,0,0)).Hit,"box contact permits separating motion at x="+x);
            check(!box.SweepBody(feet,new Vector3(0,0,.05f)).Hit,"box contact permits tangent motion at x="+x);
            var into=box.SweepBody(feet,new Vector3(-.05f,0,0));
            check(into.Hit && into.Fraction<.0001f,"box contact still blocks motion into the solid at x="+x);
        }
        var fast=box.SweepBody(new Vector3(7.5f,4.1f,5.5f),new Vector3(-10,0,0));
        check(fast.Hit && Math.Abs(fast.Fraction-.12f)<.00001f,"contact recovery preserves independent long-sweep box obstruction");
        check(box.SweepBody(new Vector3(5.5f,4.1f,5.5f),Vector3.UnitX).Fraction==0,
            "deeply contained bodies cannot bypass solid geometry through contact tolerance");
    }

    private static Vector3 Position(FpsController fps)=>new(fps.X,fps.Y,fps.Z);
    private static float HorizontalDistance(Vector3 a,Vector3 b)=>new Vector2(a.X-b.X,a.Z-b.Z).Length();
}
