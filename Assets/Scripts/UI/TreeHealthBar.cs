using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TreeHealthBar : MonoBehaviour
{

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private Slider healthBarSlider;

    private int treeSize = 0;

    public void OnGameStarted(TreeViewer viewer)
    {
        treeSize = viewer.Size();
    }

    public void UpdateImbalance(int imbalance)
    {
        healthBarSlider.value = 1-imbalance/(float)treeSize;
    }

}
