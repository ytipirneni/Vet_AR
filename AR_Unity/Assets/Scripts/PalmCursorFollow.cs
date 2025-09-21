using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.EventSystems;
using TMPro;

public class PalmCursorFollow : MonoBehaviour
{
    [Header("Hand Tracking")]
    public XRHandSubsystem handSubsystem;
    public XRNode handNode = XRNode.RightHand;
    public float rayLength = 5f;

    [Header("Cursor")]
    public GameObject cursorPrefab;
    private GameObject cursorInstance;

    [Header("UI Layer")]
    public LayerMask uiLayer;

    [Header("Smoothing")]
    public float smoothSpeed = 20f;

    [Header("Debug Output")]
    public TextMeshProUGUI debugText;

    [Header("Debug Ray Line")]
    public bool showDebugRay = true;
    public LineRenderer debugLineRenderer;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Initialize XR Hand Subsystem
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);
        if (handSubsystems.Count > 0)
        {
            handSubsystem = handSubsystems[0];
            DebugLog("Hand subsystem initialized.");
        }
        else
        {
            DebugLog("No XRHandSubsystem found.");
        }

        // Instantiate Cursor
        if (cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab);
            cursorInstance.SetActive(false);
        }

        // Setup LineRenderer
        if (debugLineRenderer != null)
        {
            debugLineRenderer.positionCount = 2;
            debugLineRenderer.enabled = showDebugRay;
        }
    }

    void Update()
    {
        if (handSubsystem == null)
        {
            DebugLog("Hand subsystem is null.");
            return;
        }

        XRHand hand = handNode == XRNode.LeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;

        if (!hand.isTracked)
        {
            DebugLog("Hand not tracked.");
            DisableCursorAndLine();
            return;
        }

        if (IsPalmTracked(hand, out Pose palmPose))
        {
            DebugLog("Palm tracked.");

            Ray ray = new Ray(palmPose.position, palmPose.forward);

            // LineRenderer
            if (debugLineRenderer != null && showDebugRay)
            {
                debugLineRenderer.enabled = true;
                debugLineRenderer.SetPosition(0, ray.origin);
                debugLineRenderer.SetPosition(1, ray.origin + ray.direction * rayLength);
            }

            // UI Raycast
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mainCamera.WorldToScreenPoint(ray.origin + ray.direction * rayLength)
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                var hit = results[0];
                DebugLog("UI Hit: " + hit.gameObject.name);

                if (cursorInstance != null)
                {
                    cursorInstance.SetActive(true);
                    Vector3 targetPos = hit.worldPosition;
                    cursorInstance.transform.position = Vector3.Lerp(cursorInstance.transform.position, targetPos, Time.deltaTime * smoothSpeed);
                }
            }
            else
            {
                DebugLog("No UI hit.");
                if (cursorInstance != null) cursorInstance.SetActive(false);
            }
        }
        else
        {
            DebugLog("Palm joint not tracked.");
            DisableCursorAndLine();
        }
    }

    private bool IsPalmTracked(XRHand hand, out Pose palmPose)
    {
        return hand.GetJoint(XRHandJointID.Palm).TryGetPose(out palmPose);
    }

    private void DisableCursorAndLine()
    {
        if (cursorInstance != null)
            cursorInstance.SetActive(false);

        if (debugLineRenderer != null)
            debugLineRenderer.enabled = false;
    }

    private void DebugLog(string msg)
    {
        if (debugText != null)
            debugText.text = msg;

        Debug.Log(msg);
    }
}
