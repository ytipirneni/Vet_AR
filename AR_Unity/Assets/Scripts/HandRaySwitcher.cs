
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandRaySwitcher : MonoBehaviour
{
    public Canvas canvas; // Assign the Canvas in the Inspector
    public Button toggleButton; // Assign the button in the Inspector
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private bool isToggled = false;
    public GameObject RightHand;
    public GameObject LeftHand;
    public Text handSelection;
    public bool rightRay = false;
    public bool leftRay = false;

    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas reference is missing!");
            return;
        }

        LeftHand.SetActive(false);
        RightHand.SetActive(true);
        rightRay = true;
        leftRay = false;

        // Record the initial position and rotation
        // Record the initial local position and rotation
        defaultPosition = canvas.transform.localPosition;
        defaultRotation = canvas.transform.localRotation;


        // Add listener to button
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleCanvasPosition);
    }

    void ToggleCanvasPosition()
    {
        if (canvas == null) return;

        // Get current local position and rotation
        Vector3 currentPos = canvas.transform.localPosition;
        Vector3 currentRot = canvas.transform.localRotation.eulerAngles;

        if (isToggled)
        {
            // Switch back: mirror X and Y relative to current
            canvas.transform.localPosition = new Vector3(-currentPos.x, currentPos.y, currentPos.z);
            canvas.transform.localRotation = Quaternion.Euler(currentRot.x, -currentRot.y, currentRot.z);

            rightRay = true;
            leftRay = false;
            handSelection.text = "R-Hand Selected";
            StartCoroutine(activeRightHand());
        }
        else
        {
            // Switch to other hand: mirror X and Y relative to current
            canvas.transform.localPosition = new Vector3(-currentPos.x, currentPos.y, currentPos.z);
            canvas.transform.localRotation = Quaternion.Euler(currentRot.x, -currentRot.y, currentRot.z);

            rightRay = false;
            leftRay = true;
            handSelection.text = "L-Hand Selected";
            StartCoroutine(activeLeftHand());
        }

        // Toggle state
        isToggled = !isToggled;
    }


    IEnumerator activeRightHand()
    {
        yield return new WaitForSeconds (1);
        LeftHand.SetActive(false);
        RightHand.SetActive(true);

    }
    IEnumerator activeLeftHand()
    {
        yield return new WaitForSeconds(1);
        LeftHand.SetActive(true);
        RightHand.SetActive(false);

    }



}
