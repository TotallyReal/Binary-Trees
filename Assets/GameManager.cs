using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private TreeManager treeManager;


    private void Start()
    {
        //OnTreeChanged(null, treeManager);
    }

    public event EventHandler<TreeViewer> OnGameStarted;
    public UnityEvent<TreeViewer> UOnGameStarted;

    public void RandomizeTree(int size)
    {
        System.Random rng = new System.Random();
        int[] indices = Enumerable.Range(1, size)
                         .OrderBy(_ => rng.Next())
                         .ToArray();
        treeManager.ClearTree();
        treeManager.AddOrdered(indices);
    }

    #region ======================= imblanace count =======================

    private int imbalanceCount = 0;
    private bool solved = true;

    public event EventHandler<TreeViewer> OnImbalanceSolved;
    public UnityEvent<TreeViewer> UOnImbalanceSolved;

    public event EventHandler<int> OnImbalanceChanged;
    public UnityEvent<int> UOnImbalanceChanged;


    private void OnTreeRotated(TreeViewer viewer)
    {
        int newImbalanceCount = ComputeImbalanceCount(viewer);
        if (newImbalanceCount != imbalanceCount)
        {
            imbalanceCount = newImbalanceCount;
            OnImbalanceChanged?.Invoke(this, imbalanceCount);
            UOnImbalanceChanged?.Invoke(imbalanceCount);
        }

        if (!solved && imbalanceCount == 0)
        {
            solved = true;
            PauseTime();
            OnImbalanceSolved?.Invoke(this, treeManager);
            UOnImbalanceSolved?.Invoke(treeManager);
        }
    }

    private void OnTreeChanged(TreeViewer viewer)
    {
        OnGameStarted?.Invoke(this, treeManager);
        UOnGameStarted?.Invoke(treeManager);

        imbalanceCount = ComputeImbalanceCount(viewer);
        OnImbalanceChanged?.Invoke(this, imbalanceCount);
        UOnImbalanceChanged?.Invoke(imbalanceCount);
        // don't invoke solved event if the new tree is already solved
        solved = (imbalanceCount == 0);
        RestartTime();
        Resume();
    }

    Color imbalanceColor = new Color(1, 0.5f, 0.5f);

    // imbalance count, height
    private (int, int) ComputeImbalanceCount(int nodeIndex, TreeViewer viewer)
    {
        NodeData nodeData = viewer.GetNodeData(nodeIndex);
        Node node = viewer.GetNode(nodeIndex);
        node.MarkColor(Color.white);

        if (nodeData.leftChild == -1 && nodeData.rightChild == -1)
            return (0, 0);

        if (nodeData.leftChild == -1)
        {
            (int subImbalanceCount, int height) = ComputeImbalanceCount(nodeData.rightChild, viewer);
            if (height > 0)
            {
                node.MarkColor(imbalanceColor);
                subImbalanceCount += 1;
            }
            return (subImbalanceCount, 1 + height);
        }

        if (nodeData.rightChild == -1)
        {
            (int subImbalanceCount, int height) = ComputeImbalanceCount(nodeData.leftChild, viewer);
            if (height > 0)
            {
                node.MarkColor(imbalanceColor);
                subImbalanceCount += 1;
            }
            return (subImbalanceCount, 1 + height);
        }

        (int subImbalanceCountR, int heightR) = ComputeImbalanceCount(nodeData.leftChild, viewer);
        (int subImbalanceCountL, int heightL) = ComputeImbalanceCount(nodeData.rightChild, viewer);
        int imbalance = Mathf.Max(Mathf.Abs(heightL - heightR) - 1, 0);
        if (imbalance > 0)
        {
            node.MarkColor(imbalanceColor);
            subImbalanceCountR += 1;
        }
        return (subImbalanceCountR + subImbalanceCountL, 1 + Mathf.Max(heightL, heightR));
    }


    private int ComputeImbalanceCount(TreeViewer viewer)
    {
        return ComputeImbalanceCount(viewer.GetRoot(), viewer).Item1;
    }

    #endregion

    #region ======================= timing =======================

    [SerializeField]
    private float time;
    [SerializeField]
    private bool paused = false;


    public void PauseTime()
    {
        paused = true;
    }

    public void Resume()
    {
        if (!solved)
        {
            paused = false;
        }        
    }

    public void RestartTime()
    {
        time = 0;
    }


    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            time += Time.deltaTime;
        }
    }

    public float GetTime()
    {
        return time;
    }

    #endregion
}
