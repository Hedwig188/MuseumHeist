using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class Light2DMinMax : System.IComparable<Light2DMinMax>
{
    public float Min { get; set; }
    public float Max { get; set; }

    public Light2DMinMax() { }
    public Light2DMinMax(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public int CompareTo(Light2DMinMax obj)
    {
        return Min.CompareTo(obj.Min);
    }

    public bool IsBetween(float val)
    {
        return val >= Min && val <= Max;
    }

    public bool IsBetween(Light2DMinMax minmax)
    {
        return minmax.Min >= Min && minmax.Max <= Max;
    }

    public void Expand(float val)
    {
        if (Max < val)
            Max = val;
        if (Min > val)
            Min = val;
    }

    public override string ToString()
    {
        return "(" + Min + ", " + Max + ")";
    }
}
