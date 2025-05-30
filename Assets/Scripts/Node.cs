using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{

    [SerializeField]
    private Edge edgeToParent;
    [SerializeField]
    private TextMeshPro label;
    [SerializeField]
    private int index;

    internal void SetIndex(int index)
    {
        this.index = index;
    }

    internal int GetIndex()
    {
        return index;
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal Edge GetEdgeToParent()
    {
        return edgeToParent;
    }

    void UpdatePosition()
    {
        //float angle = 2 * Mathf.PI * (posInDepth) / (1 << radius);
        //transform.localPosition = new Vector2(this.radius * r * Mathf.Sin(angle), this.radius * r * Mathf.Cos(angle));
    }

    internal void SetLabel(string text)
    {
        label.text = text;
    }
}
