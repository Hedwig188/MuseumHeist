using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class Light2DBeam : Light2D
{
    #region Serialized Fields
    [SerializeField]
    protected float beamWidth = defaultWidth;
    [SerializeField]
    protected float beamLength = defaultLength;
    #endregion

    #region Properties
    /// <summary>Sets the width of the light. Value clamped between 0.0f and Mathf.Infinity</summary>
    public float LightBeamWidth
    {
        get { return beamWidth; }
        set
        {
            beamWidth = Mathf.Clamp(value, 0.0f, Mathf.Infinity);
            flagMeshUpdate = true;
            flagCornerUpdate = true;
        }
    }

    /// <summary>Sets the length of the light. Value clamped between 0.0f and Mathf.Infinity</summary>
    public float LightBeamLength
    {
        get { return beamLength; }
        set
        {
            beamLength = Mathf.Clamp(value, 0.0f, Mathf.Infinity);
            flagMeshUpdate = true;
            flagCornerUpdate = true;
        }
    }
    #endregion

    #region Variables
    private List<Vector3> prevColliderPositions;
    private List<Vector3> colliderPositions = new List<Vector3>();
    private static readonly float Light2DSqrt2 = Mathf.Sqrt(2.0f);
    private const float defaultWidth = 0.75f;
    private const float defaultLength = 5.0f;
    #endregion

    #region Override Methods
    protected override Collider[] GetColliders()
    {
        // Huge sphere :( why isn't there an overlapbox?
        var radius = Mathf.Max(beamWidth, beamLength) * Light2DSqrt2;
        return Physics.OverlapSphere(transform.position, radius, shadowLayer);
    }

    protected override void ComputeScanZonesFromCollider(Collider scanCollider, List<Light2DMinMax> compareScanZones)
    {
        var center = scanCollider.bounds.center;
        var extents = scanCollider.bounds.extents;
        var minmaxX = new Light2DMinMax(float.MaxValue, float.MinValue);
        var minmaxY = new Light2DMinMax(float.MaxValue, float.MinValue);

        // Find the minimum and maximum points of the bounding box in our coordinate system
        foreach (var sign in BoundSigns)
        {
            var pt = center + Vector3.Scale(extents, sign);
            pt = transform.InverseTransformPoint(pt);
            minmaxX.Expand(pt.x);
            minmaxY.Expand(pt.y);
        }

        // Check to see if the collision really happens within our rectangle
        if ((minmaxX.Max >= 0.0f && minmaxX.Min <= beamLength)
         && (minmaxY.Max >= -beamWidth * 0.5f && minmaxY.Min <= beamWidth * 0.5f))
        {
            colliderPositions.Add(center);
            compareScanZones.Add(minmaxY);
        }
    }

    protected override bool DoesLightHaveArea()
    {
        return beamWidth * beamLength > 0.0f;
    }

    protected override void UpdateBoundingBox()
    {
        boundingBox.BottomLeft = new Vector3(0.0f, -beamWidth * 0.5f, 0.0f);
        boundingBox.BottomRight = new Vector3(beamLength, -beamWidth * 0.5f, 0.0f);
        boundingBox.TopRight = new Vector3(beamLength, beamWidth * 0.5f, 0.0f);
        boundingBox.TopLeft = new Vector3(0.0f, beamWidth * 0.5f, 0.0f);
    }

    protected override void UpdateShape()
    {
        shape.Clear();

        // The beam is a rectangle which is formed by triangles zig-zagging back and forth which is done by a near and
        // far point
        var rays = (int)lightDetail;
        var seg = -beamWidth * 0.5f;
        var zIncrement = beamWidth / (rays - 1);
        for (var index = 0; index < rays; ++index, seg += zIncrement)
        {
            // close
            shape.Add(new Vector2(0.0f, seg));

            // far
            shape.Add(new Vector2(beamLength, seg));
        }
    }

    protected override void AddTriangles()
    {
        for (var v = 2; v < vertices.Count - 1; v += 2)
        {
            triangles.Add(v);
            triangles.Add(v - 1);
            triangles.Add(v - 2);

            triangles.Add(v + 1);
            triangles.Add(v - 1);
            triangles.Add(v);
        }
    }

    protected override void AddUVs()
    {
        for (var i = 0; i < vertices.Count; i++)
        {
            var uv = new Vector2(vertices[i].x / beamLength, (vertices[i].y / beamWidth) + 0.5f);
            uvs.Add(uv);
        }
    }

    #region Light
    protected override void UpdateLightMeshNoCollision()
    {
        vertices.Add(boundingBox.BottomLeft);
        vertices.Add(boundingBox.BottomRight);
        vertices.Add(boundingBox.TopLeft);
        vertices.Add(boundingBox.TopRight);
    }

    protected override void UpdateLightMeshCollision()
    {
        var wasHit = false;
        var prevPoint = new Light2DLine();
        var prevHitPoint = new Light2DLine();
        var prevNormal = Vector3.zero;
        GameObject prevGameObject = null;
        var dir = transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f));
        var hitPointAdded = false;
        var isPlaying = Application.isPlaying;

        // To do the least work posible, we precalculated "scan areas". These areas are what determines where objects
        // are, so we only have to raytrace if the current y position is in one of these areas. The areas are presorted
        // going from the smallest first, so we don't have to worry about overlap or adding points back in from
        // earlier on.
        for (int index = 0, count = shape.Count - 1; index < count; index += 2)
        {
            var curPoint = new Light2DLine(shape[index], shape[index + 1]);
            GetNextScanZone(curPoint.PointB.y);

            var curHit = false;

            // If we were previously hit, or are in a scan zone, then we need to raycast for objects. We check for the
            // previous hit so we can continue our scan along objects
            if (wasHit || (currentScanZone != null && currentScanZone.IsBetween(curPoint.PointB.y)))
            {
                RaycastHit rhit;
                if (Physics.Raycast(transform.TransformPoint(curPoint.PointA), dir, out rhit, beamLength, shadowLayer))
                {
                    curHit = true;

                    if (isPlaying && useEvents && !unidentifiedObjects.Contains(rhit.transform.gameObject))
                        unidentifiedObjects.Add(rhit.transform.gameObject);

                    var hitPoint = new Light2DLine(curPoint.PointA, transform.InverseTransformPoint(rhit.point));

                    if (!wasHit)
                    {
                        // If this is the first hit, we want to add the previous non-hit point and the hit point, this
                        // way it can create the hit free zone, as well as start the hit zone.
                        AddNearAndFar(prevPoint);
                        AddNearAndFar(hitPoint);
                        wasHit = true;
                        hitPointAdded = true;
                    }
                    else
                    {
                        // If an objects normal has changed, or we changed what object we are hitting, we need to add
                        // the previous point and the new point again, to begin a new zone.
                        var changeInObject = prevNormal != rhit.normal || prevGameObject != rhit.transform.gameObject;
                        if (index == count - 1 || changeInObject)
                        {
                            if (changeInObject && !hitPointAdded)
                                AddNearAndFar(prevHitPoint);

                            AddNearAndFar(hitPoint);
                            hitPointAdded = true;
                        }
                        else
                        {
                            hitPointAdded = false;
                        }
                    }

                    // Update the previous values for later checks
                    prevNormal = rhit.normal;
                    prevGameObject = rhit.transform.gameObject;
                    prevHitPoint = hitPoint;
                }
            }

            // If we were hit then we should add the previous hit point if it wasn't added yet
            if (!curHit)
            {
                if (wasHit || (index == 0 || index == count - 1))
                {
                    // If we were hit then we should add the previous hit point if it wasn't added yet
                    if (wasHit)
                    {
                        if (!hitPointAdded)
                        {
                            AddNearAndFar(prevHitPoint);
                            hitPointAdded = true;
                        }
                        wasHit = false;
                    }
                    AddNearAndFar(curPoint);
                }
            }

            prevPoint = curPoint;
        }
    }
    #endregion

    #region Shadow Caster
    protected override void UpdateShadowMeshNoCollision()
    {
        // Purposely left empty
    }

    protected override void UpdateShadowMeshCollision()
    {
        var wasHit = false;
        var prevHitPoint = new Light2DLine();
        var prevNormal = Vector3.zero;
        GameObject prevGameObject = null;
        var dir = transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f));
        var hitPointAdded = false;
        var isPlaying = Application.isPlaying;

        // Creating the shadow mesh is very similar to creating the light mesh, the only real difference is that
        // instead of forming the mesh around the objects, we only create it behind them.
        for (int index = 0, count = shape.Count - 1; index < count; index += 2)
        {
            var curPoint = new Light2DLine(shape[index], shape[index + 1]);
            GetNextScanZone(curPoint.PointB.y);

            var curHit = false;

            // If we were previously hit, or are in a scan zone, then we need to raycast for objects
            if (wasHit || (currentScanZone != null && currentScanZone.IsBetween(curPoint.PointB.y)))
            {
                RaycastHit rhit;
                if (Physics.Raycast(transform.TransformPoint(curPoint.PointA), dir, out rhit, beamLength, shadowLayer))
                {
                    curHit = true;

                    if (isPlaying && useEvents && !unidentifiedObjects.Contains(rhit.transform.gameObject))
                        unidentifiedObjects.Add(rhit.transform.gameObject);

                    var hitPoint = new Light2DLine(transform.InverseTransformPoint(rhit.point), curPoint.PointB);

                    if (!wasHit)
                    {
                        // The previous values are added to create a degenerate triangle between areas so that the mesh
                        // doesn't appear to be there.
                        if (vertices.Count > 0)
                        {
                            vertices.Add(vertices[vertices.Count - 1]);
                            vertices.Add(hitPoint.PointA);
                        }

                        AddNearAndFar(hitPoint);
                        wasHit = true;
                        hitPointAdded = true;
                    }
                    else
                    {
                        var changeInObject = prevNormal != rhit.normal || prevGameObject != rhit.transform.gameObject;
                        if (index == count - 1 || changeInObject)
                        {
                            if (changeInObject && !hitPointAdded)
                                AddNearAndFar(prevHitPoint);

                            AddNearAndFar(hitPoint);
                            hitPointAdded = true;
                        }
                        else
                        {
                            hitPointAdded = false;
                        }
                    }

                    prevNormal = rhit.normal;
                    prevGameObject = rhit.transform.gameObject;
                    prevHitPoint = hitPoint;
                }
            }

            // If we were hit then we should add the previous hit point if it wasn't added yet
            if (wasHit && !curHit)
            {
                if (!hitPointAdded)
                    AddNearAndFar(prevHitPoint);
                wasHit = false;
            }
        }
    }
    #endregion

    protected override void PreComputeScanZones()
    {
        prevColliderPositions = colliderPositions;
        colliderPositions = new List<Vector3>();
    }

    protected override void PostComputeScanZones()
    {
        colliderPositions.Sort((a, b) => a.y.CompareTo(b.y));

        if (colliderPositions.Count != prevColliderPositions.Count)
            flagMeshUpdate = true;
        else
        {
            for (var index = 0; index < colliderPositions.Count; ++index)
            {
                if (colliderPositions[index] != prevColliderPositions[index])
                {
                    flagMeshUpdate = true;
                    break;
                }
            }
        }
    }

    #endregion

    #region Helpers
    private void AddNearAndFar(Light2DLine nearAndFar)
    {
        vertices.Add(nearAndFar.PointA);
        vertices.Add(nearAndFar.PointB);
    }

    private sealed class Light2DLine
    {
        public Vector3 PointA { get; set; }
        public Vector3 PointB { get; set; }

        public Light2DLine() { }

        public Light2DLine(Vector3 a, Vector3 b)
        {
            PointA = a;
            PointB = b;
        }

        public override string ToString()
        {
            return "[A " + PointA + " B " + PointB + "]";
        }
    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Easy static function for creating 2D lights.
    /// </summary>
    /// <param name="position">Sets the position of the created light</param>
    /// <param name="lightColor">Sets the color of the created light</param>
    /// <param name="beamWidth">Sets the width of the created light</param>
    /// <param name="beamLength">Sets the length of the light</param>
    /// <param name="lightDetail">Sets the detail of the light</param>
    /// <param name="useEvents">If 'TRUE' event messages will be sent.</param>
    /// <param name="isShadow">If 'TRUE' light will be inverted to generate shadow mesh.</param>
    /// <returns>Returns the created Light2D object, NOT the gameobject.</returns>
    public static Light2DBeam Create(
        Vector3 position,
        Color lightColor,
        float beamWidth = defaultWidth,
        float beamLength = defaultLength,
        LightDetailSetting lightDetail = LightDetailSetting.Rays_200,
        bool useEvents = false,
        bool isShadow = false)
    {
        return Create(
            position,
            (Material)Resources.Load("BeamLight"),
            lightColor,
            beamWidth,
            beamLength,
            lightDetail,
            useEvents,
            isShadow);
    }

    /// <summary>
    /// Easy static function for creating 2D lights.
    /// </summary>
    /// <param name="position">Sets the position of the created light</param>
    /// <param name="lightMaterial">Sets the Material of the light</param>
    /// <param name="lightColor">Sets the color of the created light</param>
    /// <param name="beamWidth">Sets the width of the created light</param>
    /// <param name="beamLength">Sets the length of the light</param>
    /// <param name="lightDetail">Sets the detail of the light</param>
    /// <param name="useEvents">If 'TRUE' event messages will be sent.</param>
    /// <param name="isShadow">If 'TRUE' light will be inverted to generate shadow mesh.</param>
    /// <returns>Returns the created Light2D object, NOT the gameobject.</returns>
    public static Light2DBeam Create(
        Vector3 position,
        Material lightMaterial,
        Color lightColor,
        float beamWidth = defaultWidth,
        float beamLength = defaultLength,
        LightDetailSetting lightDetail = LightDetailSetting.Rays_200,
        bool useEvents = false,
        bool isShadow = false)
    {
        var obj = new GameObject("New Beam Light");
        obj.transform.position = position;

        var l2D = obj.AddComponent<Light2DBeam>();
        l2D.LightMaterial = lightMaterial;
        l2D.LightColor = lightColor;
        l2D.LightDetail = lightDetail;
        l2D.LightBeamWidth = beamWidth;
        l2D.LightBeamLength = beamLength;
        l2D.ShadowLayer = -1;
        l2D.EnableEvents = useEvents;
        l2D.IsShadowEmitter = isShadow;

        return l2D;
    }
    #endregion
}
