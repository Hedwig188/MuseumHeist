using System;
using UnityEngine;

public class Light2DBoundingBox
{
    public Vector3 BottomLeft { get; set; }
    public Vector3 BottomRight { get; set; }
    public Vector3 TopRight { get; set; }
    public Vector3 TopLeft { get; set; }

    public Light2DBoundingBox() {}
    public Light2DBoundingBox(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
    {
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
        TopRight = topRight;
        TopLeft = topLeft;
    }

    public override string ToString()
    {
        const string str = "[BL {0}, BR {1}, TR {2}, TL {3}]";
        return String.Format(str, BottomLeft, BottomRight, TopRight, TopLeft);
    }
}
