using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{

    private PlayerInput input;


    private void Awake()
    {
        input = new PlayerInput();
    }

    private void Start()
    {
        input.Player.Zoom.performed += Zoom_performed;
        input.Touch.SecondaryTouchContact.started += TouchZoomStarted;
        input.Touch.SecondaryTouchContact.canceled += TouchZoomEnded;
    }


    #region === Enable \ Disable ===
    private void OnEnable()
    {
        input.Player.Enable();
        input.Touch.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
        input.Touch.Disable();
    }

    #endregion

    private Coroutine zoomCoroutine;

    private void TouchZoomStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        zoomCoroutine = StartCoroutine(ZoomDetection());
    }

    private void TouchZoomEnded(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        StopCoroutine(zoomCoroutine);
    }

    private float CurrentDistance()
    {
        Vector2 primary = input.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
        Vector2 secondary = input.Touch.SecondaryFingerPosition.ReadValue<Vector2>();
        return Mathf.Max(Vector2.Distance(primary, secondary),0.1f);

    }

    IEnumerator ZoomDetection()
    {
        float distance = CurrentDistance();
        float previousDistance = distance;
        while (true)
        {
            distance = CurrentDistance();
            Camera.main.orthographicSize /= (distance / previousDistance);
            previousDistance = distance;
            ClampCamera();
        }
    }



    private void Zoom_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        float zoomValue = input.Player.Zoom.ReadValue<float>();
        Camera.main.orthographicSize *= Mathf.Exp(-zoomValue / 1000);
        ClampCamera();
    }

    private void ClampCamera()
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 10, 20);
    }
}
