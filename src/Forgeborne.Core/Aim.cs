namespace Forgeborne.Core;

/// <summary>What an aim ray first reaches: a solid block, a live
/// enemy, the objective core, or nothing within reach.</summary>
public enum AimTargetKind { None, Solid, Enemy, Core }

/// <summary>
/// One aim-ray result. <see cref="Cell"/> is the hit cell (solid,
/// enemy, or core cell); <see cref="PrevCell"/> is the last air
/// cell the ray was in before entering it, which is the
/// face-adjacent build cell for a solid hit. <see cref="LeftBounds"/>
/// is true only when the ray left the chunk before reaching
/// anything, so callers can tell "nothing aimed" (reach ran out
/// in open air) from "aimed out of the world".
/// </summary>
public readonly record struct AimHit(
    AimTargetKind Kind,
    int X, int Y, int Z,
    int PrevX, int PrevY, int PrevZ,
    Enemy? Enemy,
    bool LeftBounds,
    float? Distance = null);

/// <summary>
/// Slice 8 Step 2 — the one aim ray, shared by gather, build, and
/// fire, so the three actions cannot drift into three different
/// aim rules. It walks from the eye point along the core Look
/// forward vector in 0.05 m steps (small enough that a cell
/// change is always face-by-face, never diagonal, so the previous
/// cell is always face-adjacent to the hit solid), up to the
/// caller's reach. Actors (enemies, the core) are air cells, so
/// when <paramref name="includeActors"/> is false the ray passes
/// through them to the first solid — a gather or build is not
/// blocked by standing in front of an enemy — and when true the
/// first actor cell wins over any solid behind it, because the
/// walk reaches it first. A solid always stops the ray: gather
/// cannot strip a block through a wall, and fire cannot hit an
/// enemy behind one. There is no fallback anywhere in this
/// class: no solid, no actor, and no bounds within reach is
/// Kind None, and callers must fail from that, never quietly
/// act on the old fixed +Z cell.
/// </summary>
public static class AimRay
{
    public const float StepMetres = 0.05f;

    public static AimHit Trace(
        VoxelChunk chunk,
        IReadOnlyList<Enemy> enemies,
        int coreX, int coreY, int coreZ,
        float originX, float originY, float originZ,
        (float X, float Y, float Z) forward,
        float reach,
        bool includeActors)
    {
        if(chunk.SurfaceProfile!=SurfaceProfile.LegacyBlocks)
            return TraceContinuous(chunk,enemies,coreX,coreY,coreZ,originX,originY,originZ,forward,reach,includeActors);
        int prevX = (int)Math.Floor(originX);
        int prevY = (int)Math.Floor(originY);
        int prevZ = (int)Math.Floor(originZ);
        if (reach <= 0) return new AimHit(AimTargetKind.None, prevX, prevY, prevZ, prevX, prevY, prevZ, null, false);
        for (float t = StepMetres; t <= reach; t += StepMetres)
        {
            float px = originX + forward.X * t;
            float py = originY + forward.Y * t;
            float pz = originZ + forward.Z * t;
            int x = (int)Math.Floor(px);
            int y = (int)Math.Floor(py);
            int z = (int)Math.Floor(pz);
            if (x == prevX && y == prevY && z == prevZ) continue;
            if (!VoxelChunk.InBounds(x, y, z))
                return new AimHit(AimTargetKind.None, x, y, z, prevX, prevY, prevZ, null, true);
            if (includeActors)
            {
                foreach (var e in enemies)
                {
                    if (!e.IsDead && e.X == x && e.Y == y && e.Z == z)
                        return new AimHit(AimTargetKind.Enemy, x, y, z, prevX, prevY, prevZ, e, false);
                }
                if (x == coreX && y == coreY && z == coreZ)
                    return new AimHit(AimTargetKind.Core, x, y, z, prevX, prevY, prevZ, null, false);
            }
            if (BlockInfo.IsSolid(chunk.Get(x, y, z)))
                return new AimHit(AimTargetKind.Solid, x, y, z, prevX, prevY, prevZ, null, false);
            prevX = x; prevY = y; prevZ = z;
        }
        return new AimHit(AimTargetKind.None, prevX, prevY, prevZ, prevX, prevY, prevZ, null, false);
    }

    private static AimHit TraceContinuous(VoxelChunk chunk,IReadOnlyList<Enemy> enemies,int coreX,int coreY,int coreZ,
        float originX,float originY,float originZ,(float X,float Y,float Z) forward,float reach,bool includeActors)
    {
        var origin=new System.Numerics.Vector3(originX,originY,originZ);
        var direction=new System.Numerics.Vector3(forward.X,forward.Y,forward.Z);
        int ox=(int)Math.Floor(originX),oy=(int)Math.Floor(originY),oz=(int)Math.Floor(originZ);
        var none=new AimHit(AimTargetKind.None,ox,oy,oz,ox,oy,oz,null,false);
        if(reach<=0 || direction.LengthSquared()<1e-12f)return none;
        direction=System.Numerics.Vector3.Normalize(direction);
        bool solid=chunk.Geometry.Raycast(origin,direction,reach,out var surface);
        float nearest=solid ? surface.Distance : reach;
        Enemy? target=null;bool core=false;
        // Actor shape/motion is still an explicit migration gap. Terrain never
        // falls back to occupied-cell occlusion in this surface profile.
        if(includeActors)
        {
            foreach(var enemy in enemies)
                if(!enemy.IsDead && ActorDistance(enemy.X,enemy.Y,enemy.Z,origin,direction,out float distance)
                    && distance<=nearest){nearest=distance;target=enemy;core=false;}
            if(ActorDistance(coreX,coreY,coreZ,origin,direction,out float distanceToCore) && distanceToCore<=nearest)
            {nearest=distanceToCore;target=null;core=true;}
        }
        if(target!=null || core)
        {
            var previous=origin+direction*Math.Max(0,nearest-0.001f);
            return new AimHit(core ? AimTargetKind.Core : AimTargetKind.Enemy,
                core ? coreX : target!.X,core ? coreY : target!.Y,core ? coreZ : target!.Z,
                (int)Math.Floor(previous.X),(int)Math.Floor(previous.Y),(int)Math.Floor(previous.Z),target,false,nearest);
        }
        if(solid)
        {
            int px=surface.X,py=surface.Y,pz=surface.Z;
            var normal=System.Numerics.Vector3.Abs(surface.Normal);
            if(normal.X>=normal.Y && normal.X>=normal.Z)px+=Math.Sign(surface.Normal.X);
            else if(normal.Y>=normal.Z)py+=Math.Sign(surface.Normal.Y);
            else pz+=Math.Sign(surface.Normal.Z);
            if(AuthoredFoundry.IsAuthored(chunk.SurfaceProfile))
            {
                var outside=surface.Position+surface.Normal*0.02f;
                px=(int)Math.Floor(outside.X);py=(int)Math.Floor(outside.Y);pz=(int)Math.Floor(outside.Z);
            }
            return new AimHit(AimTargetKind.Solid,surface.X,surface.Y,surface.Z,px,py,pz,null,false,surface.Distance);
        }
        var end=origin+direction*reach;
        return none with{LeftBounds=!VoxelChunk.InBounds((int)Math.Floor(end.X),(int)Math.Floor(end.Y),(int)Math.Floor(end.Z))};
    }
    private static bool ActorDistance(int x,int y,int z,System.Numerics.Vector3 origin,System.Numerics.Vector3 direction,out float distance)
    {
        float entry=0,exit=float.PositiveInfinity;
        for(int axis=0;axis<3;axis++)
        {
            float p=axis==0 ? origin.X : axis==1 ? origin.Y : origin.Z;
            float d=axis==0 ? direction.X : axis==1 ? direction.Y : direction.Z;
            int low=axis==0 ? x : axis==1 ? y : z;
            if(Math.Abs(d)<1e-9f){if(p<low || p>low+1){distance=0;return false;}continue;}
            float a=(low-p)/d,b=(low+1-p)/d;
            if(a>b){float swap=a;a=b;b=swap;}
            entry=Math.Max(entry,a);exit=Math.Min(exit,b);
            if(entry>exit){distance=0;return false;}
        }
        distance=entry;return exit>=0;
    }
}
