using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomizeUI : MonoBehaviour
{

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private Slider randomizeSlider;
    [SerializeField]
    private TextMeshProUGUI randomizeValue;

    private void Start()
    {
        randomizeValue.text = $"{(int)randomizeSlider.value}";
        RandomizeTree();
    }

    private void OnEnable()
    {
        randomizeSlider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        randomizeSlider.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        randomizeValue.text = $"{(int)value}";
    }

    public void RandomizeTree()
    {
        gameManager.RandomizeTree((int)randomizeSlider.value);
    }
}
