using System.Numerics;

namespace Forgeborne.Core;

/// <summary>Immutable regional scalar samples, evaluated with the exact tetrahedral split used by contouring.</summary>
internal sealed class TerrainFieldSamples
{
    private readonly float[] _values;
    private readonly int _nx,_ny,_nz;
    private readonly Vector3 _origin;
    private readonly float _step;
    internal TerrainFieldSamples(float[] ownedValues,int nx,int ny,int nz,Vector3 origin,float step)
    {_values=ownedValues;_nx=nx;_ny=ny;_nz=nz;_origin=origin;_step=step;}
    internal bool Contains(Vector3 point)
    {
        var p=(point-_origin)/_step;
        int x=(int)Math.Floor(p.X),y=(int)Math.Floor(p.Y),z=(int)Math.Floor(p.Z);
        if(x<0||y<0||z<0||x>=_nx-1||y>=_ny-1||z>=_nz-1)return false;
        float fx=p.X-x,fy=p.Y-y,fz=p.Z-z;
        int a,b,c;
        if(fx>=fy){if(fy>=fz){a=0;b=1;c=2;}else if(fx>=fz){a=0;b=2;c=1;}else{a=2;b=0;c=1;}}
        else {if(fx>=fz){a=1;b=0;c=2;}else if(fy>=fz){a=1;b=2;c=0;}else{a=2;b=1;c=0;}}
        float Fraction(int axis)=>axis==0 ? fx : axis==1 ? fy : fz;
        float Value(int corner)=>_values[((y+((corner>>1)&1))*_nz+z+((corner>>2)&1))*_nx+x+(corner&1)];
        float maximum=Fraction(a),middle=Fraction(b),minimum=Fraction(c);
        float density=Value(0)*(1-maximum)+Value(1<<a)*(maximum-middle)
            +Value((1<<a)|(1<<b))*(middle-minimum)+Value(7)*minimum;
        return density>0.0000001f;
    }
}
