using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PolarPosition
{
    public float radius;
    public float angle;

    public Vector2 ToVector()
    {
        return new Vector2(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle));
    }

    public static PolarPosition FromVector(Vector3 v)
    {
        return new PolarPosition()
        {
            angle = -Vector2.SignedAngle(Vector2.up, v) * Mathf.PI / 180,
            radius = v.magnitude
        };
    }

    public PolarPosition Child(bool left)
    {
        return new PolarPosition()
        {
            radius = radius + 1,
            angle = angle + (left ? -1 : 1) * 0.5f * Mathf.PI / Mathf.Pow(2, radius)
        };
    }

    public PolarPosition LeftChild()
    {
        return new PolarPosition()
        {
            radius = radius + 1,
            angle = angle - 0.5f * Mathf.PI / Mathf.Pow(2, radius)
        };
    }

    public PolarPosition RightChild()
    {
        return new PolarPosition()
        {
            radius = radius + 1,
            angle = angle + 0.5f * Mathf.PI / Mathf.Pow(2, radius)
        };
    }
}
