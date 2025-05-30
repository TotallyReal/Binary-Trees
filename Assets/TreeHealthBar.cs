using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeHealthBar : MonoBehaviour
{

    [SerializeField]
    private TreeManager treeManager;

    [SerializeField]
    private Slider healthBarSlider;

    private void Start()
    {
        int imbalance = ComputeBalanceHealth(treeManager);
        //Debug.Log($"imbalance = {imbalance}");
        healthBarSlider.value = 1 - imbalance / 10.0f;
    }

    private void OnEnable()
    {
        treeManager.OnTreeChanged += OnTreeChanged;
    }

    private void OnDisable()
    {
        treeManager.OnTreeChanged -= OnTreeChanged;
    }

    private void OnTreeChanged(object sender, TreeViewer viewer)
    {
        int imbalance = ComputeBalanceHealth(viewer);
        //Debug.Log($"imbalance = {imbalance}");
        healthBarSlider.DOValue(1 - imbalance / 10.0f, 0.5f);
    }


    // height_imbalance_sum, height
    private (int, int) ComputeBalanceHealth(int nodeIndex, TreeViewer viewer)
    {
        NodeData nodeData = viewer.GetNode(nodeIndex);
        if (nodeData.leftChild == -1 && nodeData.rightChild == -1)
            return (0, 0);

        if (nodeData.leftChild == -1)
        {
            (int heightImbalanceSum, int height) = ComputeBalanceHealth(nodeData.rightChild, viewer);
            return (heightImbalanceSum + height, 1 + height);
        }

        if (nodeData.rightChild == -1)
        {
            (int heightImbalanceSum, int height) = ComputeBalanceHealth(nodeData.leftChild, viewer);
            return (heightImbalanceSum + height, 1 + height);
        }

        (int heightImbalanceSumR, int heightR) = ComputeBalanceHealth(nodeData.leftChild, viewer);
        (int heightImbalanceSumL, int heightL) = ComputeBalanceHealth(nodeData.rightChild, viewer);
        int imbalance = Mathf.Max(Mathf.Abs(heightL - heightR) - 1, 0);
        return (heightImbalanceSumL + heightImbalanceSumR + imbalance, 1 + Mathf.Max(heightL, heightR));
    }

    private int ComputeBalanceHealth(TreeViewer viewer)
    {
        return ComputeBalanceHealth(viewer.GetRoot(), viewer).Item1;
    }
}
