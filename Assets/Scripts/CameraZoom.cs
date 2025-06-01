using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraZoom : MonoBehaviour
{

    private PlayerInput input;
    [SerializeField] private MouseTypeEvent mouseEvent;

    [SerializeField] private DragAndDrop background;


    private void Awake()
    {
        input = new PlayerInput();

    }

    private void Start()
    {
        AdjustCameraParameters();

        input.Player.Zoom.performed += Zoom_performed;
        input.Touch.SecondaryTouchContact.started += TouchZoomStarted;
        input.Touch.SecondaryTouchContact.canceled += TouchZoomEnded;

        background.OnStartDragging += OnBackgroundStartDragging;
        background.OnDragging += OnBackgroundDragging;
    }

    private Vector3 previousScreenPosition;

    private void OnBackgroundStartDragging(object sender, Vector3 position)
    {
        previousScreenPosition = mouseEvent.ScreenPosition();
    }

    private void OnBackgroundDragging(object sender, Vector3 position)
    {
        Vector3 screenPosition = mouseEvent.ScreenPosition();

        Vector3 diff = Camera.main.ScreenToWorldPoint(screenPosition) - Camera.main.ScreenToWorldPoint(previousScreenPosition);
        Camera.main.transform.position -= diff;
        previousScreenPosition = screenPosition;
        ClampCameraPosition();
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

    [Header("Zoom")]
    [SerializeField] bool flipZoomScroll = false;

    private void Zoom_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        // scroll up   =  120 => zoomValue > 1
        // scroll down = -120 => zoomValue < 1
        float zoomValue = Mathf.Exp(input.Player.Zoom.ReadValue<float>() / 1000);
        if (flipZoomScroll)
            zoomValue = 1 / zoomValue;
        Vector3 position = mouseEvent.WorldPosition();
        ZoomToPoint(position, zoomValue);        
    }

    private void ClampCamera()
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 10, 20);
    }

    private void AdjustCameraParameters()
    {
        if (maxPositionX <= 0)
            maxPositionX = 1;
        if (maxPositionY <= 0)
            maxPositionY = 1;

        // If camera is outsize of allowed range, move it to the center
        Vector2 position = Camera.main.transform.position;
        if (Mathf.Abs(position.x) > maxPositionX || Mathf.Abs(position.y) > maxPositionY)
        {
            position = Vector3.zero;
        }

        float dx = Mathf.Min(position.x - (-maxPositionX), maxPositionX - position.x);
        float dy = Mathf.Min(position.y - (-maxPositionY), maxPositionY - position.y);

        maxOrthoSize = Mathf.Min(dy, dx / Camera.main.aspect, maxOrthoSize);
        Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize, maxOrthoSize);
    }

    [SerializeField] private float minOrthoSize = 10;
    [SerializeField] private float maxOrthoSize = 20;

    /// <summary>
    /// Zoom in to the given point with the given ratio. 
    /// If zoomInRatio > 1 zooms in, and if zoomInRatio < 1 zooms out.    /// 
    /// </summary>
    /// <param name="pointSpace"></param>
    /// <param name="zoomInRatio"></param>
    private void ZoomToPoint(Vector2 pointSpace, float zoomInRatio)
    {
        if (zoomInRatio <= 0)
            return;
        zoomInRatio = Camera.main.orthographicSize / Mathf.Clamp(Camera.main.orthographicSize / zoomInRatio, minOrthoSize, maxOrthoSize);
        Vector3 pointSpace3 = new Vector3(pointSpace.x, pointSpace.y, Camera.main.transform.position.z);
        Camera.main.transform.position = pointSpace3 + (Camera.main.transform.position - pointSpace3) / zoomInRatio;
        Camera.main.orthographicSize /= zoomInRatio;
        ClampCameraPosition();
    }

    [Header("Position")]

    [SerializeField]
    private float maxPositionX = 30f;
    [SerializeField]
    private float maxPositionY = 15f;

    private void ClampCameraPosition()
    {
        float vertExtent = Camera.main.orthographicSize;
        float horizExtent = vertExtent * Camera.main.aspect;

        // World-space limits (centered at 0)
        float minX = -maxPositionX + horizExtent;
        float maxX = maxPositionX - horizExtent;
        float minY = -maxPositionY + vertExtent;
        float maxY = maxPositionY - vertExtent;

        // Clamp the camera position
        Vector3 pos = Camera.main.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        Camera.main.transform.position = pos;
    }
}
