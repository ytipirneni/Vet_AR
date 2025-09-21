using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class PinchDetection : MonoBehaviour
{
    [SerializeField] private GameObject pinchCirclePrefab;
    [SerializeField] private Vector3 pinchCircleScale = new Vector3(0.02f, 0.02f, 0.02f);
    [SerializeField] private float pinchThreshold = 0.025f;

    private GameObject rightPinchCircle;
    private GameObject leftPinchCircle;

    private XRHandSubsystem handSubsystem;

    void Start()
    {
        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
            handSubsystem.Start(); // <- Very important
            Debug.Log("XRHandSubsystem initialized and started.");
        }
        else
        {
            Debug.LogError("No XRHandSubsystem found.");
        }
    }


    void Update()
    {
        if (handSubsystem == null) return;
        

        XRHand rightHand = handSubsystem.rightHand;
        XRHand leftHand = handSubsystem.leftHand;

        // === Right Hand ===
        if (rightHand.isTracked && IsPinching(rightHand, out Vector3 rightPinchPos))
        {
            Debug.Log("Detected Right Hand Pinch");
            if (rightPinchCircle == null)
            {
                rightPinchCircle = Instantiate(pinchCirclePrefab, rightPinchPos, Quaternion.identity);
                rightPinchCircle.transform.localScale = pinchCircleScale;
            }
            else
            {
                rightPinchCircle.transform.position = rightPinchPos;
            }
        }
        else
        {
            if (rightPinchCircle != null)
            {
                Destroy(rightPinchCircle);
                rightPinchCircle = null;
            }
        }

        // === Left Hand ===
        if (leftHand.isTracked && IsPinching(leftHand, out Vector3 leftPinchPos))
        {
            Debug.Log("Detected Left Hand Pinch");
            if (leftPinchCircle == null)
            {
                leftPinchCircle = Instantiate(pinchCirclePrefab, leftPinchPos, Quaternion.identity);
                leftPinchCircle.transform.localScale = pinchCircleScale;
            }
            else
            {
                leftPinchCircle.transform.position = leftPinchPos;
            }
        }
        else
        {
            if (leftPinchCircle != null)
            {
                Destroy(leftPinchCircle);
                leftPinchCircle = null;
            }
        }
    }

    private bool IsPinching(XRHand hand, out Vector3 pinchPosition)
    {
        pinchPosition = Vector3.zero;

        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (!thumbTip.TryGetPose(out Pose thumbPose) ||
            !indexTip.TryGetPose(out Pose indexPose))
        {
            return false;
        }

        float pinchDistance = Vector3.Distance(thumbPose.position, indexPose.position);
        bool isPinching = pinchDistance < pinchThreshold;

        if (isPinching)
        {
            pinchPosition = (thumbPose.position + indexPose.position) / 2f;
        }

        return isPinching;
    }
}
