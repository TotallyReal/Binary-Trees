using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform frame;
    [SerializeField]
    private TextMeshProUGUI playButtonText;
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private CameraZoom cameraZoom;

    private bool firstGame = true;


    public void HideMenu()
    {
        if (firstGame)
        {
            gameManager.RandomizeTree(15);
            firstGame = false;
        } else
        {
            gameManager.Resume();
        }
        cameraZoom.enabled = true;
        frame.DOScale(0, 1.0f)
            .SetEase(Ease.InBack)
            .OnComplete(() => { playButtonText.text = "Resume"; });
    }

    public void ShowMenu()
    {
        cameraZoom.enabled = false;
        frame.DOScale(1, 1.0f).SetEase(Ease.OutBack);
    }

    public void OpenHomepage()
    {
        Application.OpenURL("https://prove-me-wrong.com/");
    }

}
