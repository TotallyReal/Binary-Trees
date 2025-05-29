using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    [SerializeField]
    private Transform fromPosition;
    [SerializeField]
    private Transform toPosition;
    [SerializeField]
    private LineRenderer lineRenderer;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateEdge()
    {
        lineRenderer.SetPosition(0, fromPosition.position);
        lineRenderer.SetPosition(1, toPosition.position);
    }

    public enum Side{
        FROM_POSITION = 0,
        TO_POSITION = 1
    }

    public void SetFollowPosition(Side side, Transform node)
    {
        if (side == Side.FROM_POSITION)
        {
            fromPosition = node;
        }
        else
        {
            toPosition = node;
        }
    }

    public Vector3 GetPosition(Side side)
    {
        return (side == Side.FROM_POSITION) ? fromPosition.position : toPosition.position;
    }

    public void SetColor(Side side, Color color)
    {
        if (side == Side.FROM_POSITION)
        {
            lineRenderer.startColor = color;
        }
        else
        {
            lineRenderer.endColor = color;
        }
    }
}
