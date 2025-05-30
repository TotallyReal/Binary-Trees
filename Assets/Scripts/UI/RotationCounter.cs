using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RotationCounter : MonoBehaviour
{
    [SerializeField]
    private TreeManager treeManager;

    [SerializeField]
    private TextMeshProUGUI rotationText;

    private int rotationCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        rotationText.text = "Rotations: 0";
    }

    public void AddToRotationCounter(int rotations)
    {
        rotationCount += rotations;
        rotationText.text = $"Rotations: {rotationCount}";
    }


    public void ResetRotationCount()
    {
        rotationCount = 0;
        rotationText.text = $"Rotations: {rotationCount}";
    }



}
