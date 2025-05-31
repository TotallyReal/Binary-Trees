using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiEffect : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem confettiSystem;

    [SerializeField]
    private TreeHealthBar healthBar;

    private void OnEnable()
    {
        //healthBar.OnImbalanceSolved += OnImbalanceSolved;
    }

    private void OnDisable()
    {
        //healthBar.OnImbalanceSolved -= OnImbalanceSolved;
    }

    public void RunConfetti()
    {

    }

    private void OnImbalanceSolved(object sender, TreeViewer e)
    {
        PlayEffect();
    }

    public void PlayEffect()
    {
        confettiSystem.Play();
    }
}
