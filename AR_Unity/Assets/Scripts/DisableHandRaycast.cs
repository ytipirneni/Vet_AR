using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DisableHandRaycast : MonoBehaviour
{
    public enum HandSide { Left, Right }
    public HandSide targetHand = HandSide.Right;

    private XRRayInteractor handRayInteractor;

    private void Start()
    {
        FindHandRayInteractor();
    }

    private void FindHandRayInteractor()
    {
        string targetName = targetHand == HandSide.Right ? "RightHandRay" : "LeftHandRay";

        // Try to find by GameObject name (or use tags/layers if needed)
        GameObject rayObject = GameObject.Find(targetName);
        if (rayObject != null)
        {
            handRayInteractor = rayObject.GetComponent<XRRayInteractor>();
        }

        if (handRayInteractor != null)
        {
            DisableRaycast();
        }
        else
        {
            Debug.LogWarning("XRRayInteractor not found for: " + targetHand);
        }
    }

    public void DisableRaycast()
    {
        if (handRayInteractor != null)
        {
            handRayInteractor.enabled = false;
            Debug.Log(targetHand + " hand ray interactor disabled.");
        }
    }

    public void EnableRaycast()
    {
        if (handRayInteractor != null)
        {
            handRayInteractor.enabled = true;
            Debug.Log(targetHand + " hand ray interactor enabled.");
        }
    }
}
