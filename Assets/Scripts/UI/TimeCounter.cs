using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeCounter : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI timeText;

    [SerializeField]
    private GameManager gameManager;

    void Update()
    {
        timeText.text = $"Time: {gameManager.GetTime().ToString("F1")}";
    }
}
