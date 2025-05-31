using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSounds : MonoBehaviour
{

    private void Start()
    {

    }

    public void PlayRotation()
    {
        SoundManager.PlaySound(SoundType.ROTATE);
    }

    public void PlayWin()
    {
        SoundManager.PlaySound(SoundType.WIN);
    }

    public void PlaySound(SoundType soundType)
    {

    }
}
