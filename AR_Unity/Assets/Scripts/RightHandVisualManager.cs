using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.SubsystemsImplementation;

public class RightHandVisualManager : MonoBehaviour
{
    [Header("Hand Tracking Prefab")]
    [Tooltip("Prefab to show when the right hand is tracked.")]
    public GameObject rightHandPrefab;

    [Tooltip("Parent GameObject where the right hand will be instantiated as child.")]
    public Transform handParent;

    private GameObject rightHandInstance;
    private XRHandSubsystem handSubsystem;

    void Start()
    {
        // Find the hand subsystem (used for hand tracking)
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
        }
        else
        {
            Debug.LogWarning("XRHandSubsystem not found. Hand tracking will not work.");
        }
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
            return;

        XRHand rightHand = handSubsystem.rightHand;

        if (rightHand.isTracked)
        {
            if (rightHandInstance == null && rightHandPrefab != null)
            {
                rightHandInstance = Instantiate(rightHandPrefab, handParent);
            }
        }
        else
        {
            if (rightHandInstance != null)
            {
                Destroy(rightHandInstance);
                rightHandInstance = null;
            }
        }
    }
}
