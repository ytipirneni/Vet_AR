using UnityEngine;
using TMPro;
using System.Collections;

public class ResetPanelPosition : MonoBehaviour
{
    [Header("References")]
    public Transform xrCamera; // Your HMD camera, e.g. Camera.main.transform
    public Transform objectA;
    public Transform objectB;
    public TextMeshProUGUI countdownText;

    [Header("Settings")]
    public float countdownDuration = 3f;

    // Relative poses
    private Vector3 localPosA;
    private Quaternion localRotA;

    private Vector3 localPosB;
    private Quaternion localRotB;

    private bool hasSavedPose = false;

    void Start()
    {
        if (xrCamera == null)
            xrCamera = Camera.main.transform;

        SaveRelativeToCamera();
    }

    public void SaveRelativeToCamera()
    {
        // Store each object’s transform relative to camera's local space
        localPosA = xrCamera.InverseTransformPoint(objectA.position);
        localRotA = Quaternion.Inverse(xrCamera.rotation) * objectA.rotation;

        localPosB = xrCamera.InverseTransformPoint(objectB.position);
        localRotB = Quaternion.Inverse(xrCamera.rotation) * objectB.rotation;

        hasSavedPose = true;
    }

    public void StartResetCountdown()
    {
        if (!hasSavedPose)
        {
            Debug.LogWarning("Pose not saved.");
            return;
        }

        StartCoroutine(CountdownAndReset());
    }

    private IEnumerator CountdownAndReset()
    {
        float timer = countdownDuration;

        while (timer > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timer).ToString();

            yield return null;
            timer -= Time.deltaTime;
        }

        if (countdownText != null)
            countdownText.text = "";

        ResetObjectsToCameraView();
    }

    private void ResetObjectsToCameraView()
    {
        // Reconstruct object transforms relative to camera
        objectA.position = xrCamera.TransformPoint(localPosA);
        objectA.rotation = xrCamera.rotation * localRotA;

        objectB.position = xrCamera.TransformPoint(localPosB);
        objectB.rotation = xrCamera.rotation * localRotB;
    }
}
