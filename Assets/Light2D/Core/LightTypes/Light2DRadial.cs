using System.Collections.Generic;
using UnityEngine;

public class Light2DRadial : Light2D
{
    #region Serialized Fields
    [SerializeField]
    protected float lightRadius = 1.0f;
    [SerializeField]
    protected float coneStart = 0.0f;
    [SerializeField]
    protected float coneAngle = 360f;
    #endregion

    #region Properties
    /// <summary>Sets the Radius of the light. Value clamped between 0.001f and Mathf.Infinity</summary>
    public float LightRadius
    {
        get { return lightRadius; }
        set
        {
            lightRadius = Mathf.Clamp(value, 0.001f, Mathf.Infinity);
            flagMeshUpdate = true;
            flagCornerUpdate = true;
        }
    }

    /// <summary>Sets the light cone starting point. Value 0 = Aims Right, Value 90 = Aims Up.</summary>
    public float LightConeStart
    {
        get { return coneStart; }
        set
        {
            coneStart = value;
            flagMeshUpdate = true;
        }
    }

    /// <summary>Sets the light cone size (wedge shape). Value is clamped between 0 and 360.</summary>
    public float LightConeAngle
    {
        get { return coneAngle; }
        set
        {
            coneAngle = Mathf.Clamp(value, 0f, 360f);
            flagShapeUpdate = true;
            flagMeshUpdate = true;
        }
    }
    #endregion

    #region Override Methods
    protected override Collider[] GetColliders()
    {
        return Physics.OverlapSphere(transform.position, lightRadius, shadowLayer);
    }

    protected override void ComputeScanZonesFromCollider(Collider scanCollider, List<Light2DMinMax> scanZonesToAddto)
    {
        var center = scanCollider.bounds.center;
        var extents = scanCollider.bounds.extents;

        var minmax = new Light2DMinMax(float.MaxValue, float.MinValue);
        var backwardsMinMax = new Light2DMinMax(-180.0f, 180.0f);

        // Find the minimum and maximum angles using the center of the light as the origin and each point on the
        // bounding box the angle
        foreach (var sign in BoundSigns)
        {
            var pt = center + Vector3.Scale(extents, sign);
            pt = Quaternion.Euler(0, 0, -coneStart) * transform.InverseTransformPoint(pt);
            var angle = Mathf.Atan2(pt.y, pt.x) * Mathf.Rad2Deg;
            minmax.Expand(angle);
            if (angle > 0 && angle > backwardsMinMax.Min)
                backwardsMinMax.Min = angle;
            else if (angle > 0 && angle < backwardsMinMax.Max)
                backwardsMinMax.Max = angle;
        }

        // If the 'zone' crosses 180 degrees we need to add from that point to the other point instead as this is how
        // we traverse the scan zones, otherwise it would create a gap
        if (minmax.Min < -90.0f && minmax.Max > 90.0f)
        {
            scanZonesToAddto.Add(new Light2DMinMax(-180.0f, backwardsMinMax.Min));
            scanZonesToAddto.Add(new Light2DMinMax(backwardsMinMax.Max, 180.0f));
        }
        else
            scanZonesToAddto.Add(minmax);
    }

    protected override bool DoesLightHaveArea()
    {
        return coneAngle != 0.0f;
    }

    protected override void UpdateBoundingBox()
    {
        boundingBox.BottomLeft = new Vector3(-lightRadius, -lightRadius, 0.0f);
        boundingBox.BottomRight = new Vector3(lightRadius, -lightRadius, 0.0f);
        boundingBox.TopRight = new Vector3(lightRadius, lightRadius, 0.0f);
        boundingBox.TopLeft = new Vector3(-lightRadius, lightRadius, 0.0f);
    }

    protected override void UpdateShape()
    {
        shape.Clear();

        // Divide the angle we have into the number of rays, if we had 20 rays and an angle of 50 we would need 50
        // segments 2.5 degrees apart from eachother starting with the smallest angle first and ranging from
        // -180 to 180 degrees
        var rays = (int)lightDetail;
        for (int i = 0, count = rays + 1; i < count; i++)
        {
            var a = i * (coneAngle / rays) - (coneAngle * 0.5f);
            shape.Add(AngleToVector(a));
        }
    }

    protected override void AddTriangles()
    {
        if (isShadowCaster)
        {
            // Shadow casters don't come from a single point in the center, so instead we zig-zag their coordinates
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
        else
        {
            // Circles all start from the 0th point
            for (var v = 1; v < vertices.Count - 1; v++)
            {
                triangles.Add(0);
                triangles.Add(v + 1);
                triangles.Add(v);
            }
        }
    }

    protected override void AddUVs()
    {
        for (var i = 0; i < vertices.Count; i++)
        {
            Vector2 uv = Quaternion.Euler(0, 0, -coneStart) *
                         new Vector2((vertices[i].x * 0.5f) / lightRadius, (vertices[i].y * 0.5f) / lightRadius);
            uvs.Add(new Vector2(uv.x + 0.5f, uv.y + 0.5f));
        }
    }

    #region Light
    protected override void UpdateLightMeshNoCollision()
    {
        // Optimize for when no mesh's are colliding
        var quatEul = Quaternion.Euler(0, 0, coneStart);
        if (coneAngle == 360.0f)
        {
            // If the angle is 360 then this is a perfect circle so we can actually create a box to represent it
            vertices.Add(quatEul * boundingBox.BottomLeft);
            vertices.Add(quatEul * boundingBox.BottomRight);
            vertices.Add(quatEul * boundingBox.TopRight);
            vertices.Add(quatEul * boundingBox.TopLeft);
        }
        else
        {
            // If this isn't a perfect circle let's break it up into a maximum of four triangles starting at the origin
            vertices.Add(Vector3.zero);

            // Calculate the positions of the points around the circle and only use them if we need to
            var ang = coneAngle * 0.5f;
            var cc90 = ((coneAngle >= 90.0f) ? 1.0f : AngleToPos(ang)) * lightRadius;
            var cc270 = ((coneAngle >= 270.0f) ? -1.0f : AngleToPos(90.0f - ang)) * lightRadius;
            var cc360 = AngleToPos(180.0f - ang) * lightRadius;

            // Because everything is semetrical, this check will be done twice, once for the negative position
            if (coneAngle > 270.0f)
                vertices.Add(quatEul * new Vector3(-lightRadius, -cc360, 0.0f));
            if (coneAngle > 90.0f)
                vertices.Add(quatEul * new Vector3(cc270, -lightRadius, 0.0f));

            vertices.Add(quatEul * new Vector3(lightRadius, -cc90, 0.0f));
            vertices.Add(quatEul * new Vector3(lightRadius, cc90, 0.0f));

            // and once for the positive. This also rotates around a circle, so we must check the largets negative,
            // then rotate around to the largest positive.
            if (coneAngle > 90.0f)
                vertices.Add(quatEul * new Vector3(cc270, lightRadius, 0.0f));
            if (coneAngle > 270.0f)
                vertices.Add(quatEul * new Vector3(-lightRadius, cc360, 0.0f));
        }
    }

    protected override void UpdateLightMeshCollision()
    {
        vertices.Add(Vector3.zero);

        var quatEul = Quaternion.Euler(0, 0, coneStart);
        var wasHit = false;
        var prevA = 0.0f;
        var prevPoint = Vector3.zero;
        var prevHitPoint = Vector3.zero;
        var prevNormal = Vector3.zero;
        var hitPointAdded = false;
        var rays = (int)lightDetail;
        var isPlaying = Application.isPlaying;

        GameObject prevGameObject = null;

        // To do the least work posible, we precalculated "scan areas". These areas are what determines where objects
        // are, so we only have to raytrace if the current angle is in one of these areas. The areas are presorted
        // going from the smallest first, so we don't have to worry about overlap or adding points back in from
        // earlier on.
        for (var i = 0; i < shape.Count; i++)
        {
            var a = i * (coneAngle / rays) - (coneAngle * 0.5f);

            // Find the next scan area to work with (if any)
            GetNextScanZone(a);

            var dir = quatEul * shape[i];
            var curPoint = dir * lightRadius;
            var curHit = false;

            // If we were previously hit, or are in a scan zone, then we need to raycast for objects. We check for the
            // previous hit so we can continue our scan along objects
            if (wasHit || (currentScanZone != null && currentScanZone.IsBetween(a)))
            {
                RaycastHit rhit;
                if (Physics.Raycast(
                    transform.position,
                    transform.TransformDirection(dir),
                    out rhit,
                    lightRadius,
                    shadowLayer))
                {
                    // Mark that the current point has been 'hit'
                    curHit = true;

                    if (isPlaying && useEvents && !unidentifiedObjects.Contains(rhit.transform.gameObject))
                        unidentifiedObjects.Add(rhit.transform.gameObject);

                    var hitPoint = transform.InverseTransformPoint(rhit.point);

                    if (!wasHit)
                    {
                        // If this is the first hit, we want to add the previous non-hit point and the hit point, this
                        // way it can create the hit free zone, as well as start the hit zone.
                        vertices.Add(prevPoint);
                        vertices.Add(hitPoint);
                        wasHit = true;
                        hitPointAdded = true;
                    }
                    else
                    {
                        // If an objects normal has changed, or we changed what object we are hitting, we need to add
                        // the previous point and the new point again, to begin a new zone.
                        var changeInObject = prevNormal != rhit.normal || prevGameObject != rhit.transform.gameObject;
                        if (i == rays || changeInObject)
                        {
                            if (changeInObject && !hitPointAdded)
                                vertices.Add(prevHitPoint);

                            vertices.Add(hitPoint);
                            hitPointAdded = true;
                        }
                        else
                            hitPointAdded = false;
                    }

                    // Update the previous values for later checks
                    prevNormal = rhit.normal;
                    prevGameObject = rhit.transform.gameObject;
                    prevHitPoint = hitPoint;
                }
            }

            if (!curHit)
            {
                // Add the corner points if we passed through them
                AddBetweenCornerAngles(prevA, a, quatEul);

                if (wasHit // Add if we previously hit something
                    || (i == 0 || i == rays) // First or Last
                    || IsCornerAngle(a))
                {
                    // If we were hit then we should add the previous hit point if it wasn't added yet
                    if (wasHit)
                    {
                        if (!hitPointAdded)
                        {
                            vertices.Add(prevHitPoint);
                            hitPointAdded = true;
                        }
                        wasHit = false;
                    }
                    vertices.Add(curPoint);
                }
            }

            // Update the previous values for later checks
            prevPoint = curPoint;
            prevA = a;
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
        var prevA = 0.0f;
        var wasHit = false;
        var hitPointAdded = false;
        var prevPoint = Vector3.zero;
        var prevHitPoint = Vector3.zero;
        var prevNormal = Vector3.zero;
        GameObject prevGameObject = null;
        var rays = (int) lightDetail;
        var isPlaying = Application.isPlaying;
        var quatEul = Quaternion.Euler(0, 0, coneStart);

        // Same concept as in UpdateLightMeshCollision, however this time instead of capturing the points between
        // collisions, we only want the collision points to form the shadow areas.
        for (var i = 0; i < shape.Count; i++)
        {
            var a = i * (coneAngle / rays) - (coneAngle * 0.5f);
            GetNextScanZone(a);

            var dir = quatEul * shape[i];
            var curPoint = dir * lightRadius;
            var curHit = false;

            // If we were previously hit, or are in a scan zone, then we need to raycast for objects
            if (wasHit || (currentScanZone != null && currentScanZone.IsBetween(a)))
            {
                RaycastHit rhit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(dir), out rhit, lightRadius,
                    shadowLayer))
                {
                    AddBetweenCornerAngles(prevA, a, quatEul);

                    curHit = true;

                    if (isPlaying && useEvents && !unidentifiedObjects.Contains(rhit.transform.gameObject))
                        unidentifiedObjects.Add(rhit.transform.gameObject);

                    var hitPoint = transform.InverseTransformPoint(rhit.point);

                    if (!wasHit)
                    {
                        // The previous values are added to create a degenerate triangle between areas so that the mesh
                        // doesn't appear to be there.
                        if (vertices.Count > 0)
                        {
                            vertices.Add(vertices[vertices.Count - 1]);
                            vertices.Add(hitPoint);
                        }

                        vertices.Add(hitPoint);
                        vertices.Add(curPoint);
                        wasHit = true;
                        hitPointAdded = true;
                    }
                    else
                    {
                        var changeInObject =
                            prevNormal != rhit.normal
                            || prevGameObject != rhit.transform.gameObject
                            || IsCornerAngle(a);

                        if (i == rays || changeInObject)
                        {
                            if (changeInObject && !hitPointAdded)
                            {
                                vertices.Add(prevHitPoint);
                                vertices.Add(prevPoint);
                            }

                            vertices.Add(hitPoint);
                            vertices.Add(curPoint);
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

            if (wasHit && !curHit)
            {
                if (!hitPointAdded)
                {
                    vertices.Add(prevHitPoint);
                    vertices.Add(prevPoint);
                }
                wasHit = false;
            }

            prevPoint = curPoint;
            prevA = a;
        }
    }
    #endregion

    #endregion

    #region Helpers

    private void AddShadowAnglePosition(Vector3 dir)
    {
        RaycastHit rhit;
        if (Physics.Raycast(
            transform.position,
            transform.TransformDirection(dir),
            out rhit,
            lightRadius,
            shadowLayer))
        {
            var hitPoint = transform.InverseTransformPoint(rhit.point);
            vertices.Add(hitPoint);
        }
    }

    private void AddBetweenCornerAngles(float prevA, float a, Quaternion quatEul)
    {
        if (prevA < -135 && a > -135)
        {
            if (isShadowCaster)
                AddShadowAnglePosition(quatEul * boundingBox.BottomLeft);
            vertices.Add(quatEul * boundingBox.BottomLeft);
        }

        if (prevA < -45 && a > -45)
        {
            if (isShadowCaster)
                AddShadowAnglePosition(quatEul * boundingBox.BottomRight);
            vertices.Add(quatEul * boundingBox.BottomRight);
        }

        if (prevA < 45 && a > 45)
        {
            if (isShadowCaster)
                AddShadowAnglePosition(quatEul * boundingBox.TopRight);
            vertices.Add(quatEul * boundingBox.TopRight);
        }

        if (prevA < 135 && a > 135)
        {
            if (isShadowCaster)
                AddShadowAnglePosition(quatEul * boundingBox.TopLeft);
            vertices.Add(quatEul * boundingBox.TopLeft);
        }
    }

    private static bool IsCornerAngle(float angle)
    {
        return angle == 45.0 || angle == -45.0f || angle == 135.0f || angle == -135.0f;
    }

    private static Vector2 AngleToVector(float a)
    {
        if (a <= 45 && a >= -45.0f)    // RIGHT SIDE
            return new Vector2(1.0f, AngleToPos(a));
        if (a > 45 && a < 135)      // TOP SIDE
            return new Vector2(AngleToPos(90.0f - a), 1.0f);
        if (a < -45.0f && a > -135.0f)     // BOTTOM SIDE
            return new Vector2(AngleToPos(a + 90), -1.0f);
        // LEFT SIDE
        return new Vector2(-1.0f, AngleToPos(180.0f - a));
    }

    private static float AngleToPos(float a)
    {
        a *= Mathf.Deg2Rad;
        var sn = Mathf.Sin(a);
        var cs = Mathf.Cos(a);

        return cs == 0.0f ? 1.0f : (sn / cs);
    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Easy static function for creating 2D lights.
    /// </summary>
    /// <param name="position">Sets the position of the created light</param>
    /// <param name="lightColor">Sets the color of the created light</param>
    /// <param name="lightRadius">Sets the radius of the created light</param>
    /// <param name="lightConeAngle">Sets the cone angle of the light</param>
    /// <param name="lightDetail">Sets the detail of the light</param>
    /// <param name="useEvents">If 'TRUE' event messages will be sent.</param>
    /// <param name="isShadow">If 'TRUE' light will be inverted to generate shadow mesh.</param>
    /// <returns>Returns the created Light2D object, NOT the gameobject.</returns>
    public static Light2DRadial Create(
        Vector3 position,
        Color lightColor,
        float lightRadius = 1,
        int lightConeAngle = 360,
        LightDetailSetting lightDetail = LightDetailSetting.Rays_500,
        bool useEvents = false,
        bool isShadow = false,
        bool allowLightsToHide = true)
    {
        return Create(
            position,
            (Material)Resources.Load("RadialLight"),
            lightColor,
            lightRadius,
            lightConeAngle,
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
    /// <param name="lightRadius">Sets the radius of the created light</param>
    /// <param name="lightConeAngle">Sets the cone angle of the light</param>
    /// <param name="lightDetail">Sets the detail of the light</param>
    /// <param name="useEvents">If 'TRUE' event messages will be sent.</param>
    /// <param name="isShadow">If 'TRUE' light will be inverted to generate shadow mesh.</param>
    /// <returns>Returns the created Light2D object, NOT the gameobject.</returns>
    public static Light2DRadial Create(
        Vector3 position,
        Material lightMaterial,
        Color lightColor,
        float lightRadius = 1,
        int lightConeAngle = 360,
        LightDetailSetting lightDetail = LightDetailSetting.Rays_500,
        bool useEvents = false,
        bool isShadow = false)
    {
        var obj = new GameObject("New Radial Light");
        obj.transform.position = position;

        var l2D = obj.AddComponent<Light2DRadial>();
        l2D.LightMaterial = lightMaterial;
        l2D.LightColor = lightColor;
        l2D.LightDetail = lightDetail;
        l2D.LightRadius = lightRadius;
        l2D.LightConeAngle = lightConeAngle;
        l2D.ShadowLayer = -1;
        l2D.EnableEvents = useEvents;
        l2D.IsShadowEmitter = isShadow;

        return l2D;
    }
    #endregion
}
