using UnityEngine;
using UnityEngine.UI;

public class ObjectRotator : MonoBehaviour
{
    [Header("Objects to Rotate (Assign in Inspector)")]
    public GameObject[] objectsToRotate; // Array of objects to rotate
    private GameObject selectedObject; // Currently selected object
    private Quaternion originalRotation; // Store initial rotation

    [Header("Rotation Buttons (Assign in Order)")]
    public Button[] rotationButtons; // Array for 8 directional buttons
    public Button resetButton; // Separate reset button

    private void Start()
    {
        if (objectsToRotate.Length > 0)
        {
            selectedObject = objectsToRotate[0]; // Default to the first object
            originalRotation = selectedObject.transform.rotation; // Save its rotation
        }

        // Assign button listeners dynamically
        for (int i = 0; i < rotationButtons.Length; i++)
        {
            int index = i; // Prevent closure issue
            rotationButtons[i].onClick.AddListener(() => Rotate(index));
        }

        resetButton.onClick.AddListener(ResetRotation);
    }

    public void SetSelectedObject(int index)
    {
        if (index >= 0 && index < objectsToRotate.Length)
        {
            selectedObject = objectsToRotate[index]; // Change selected object
            originalRotation = selectedObject.transform.rotation; // Store new rotation
        }
    }

    // Handles all rotation directions based on button index
    private void Rotate(int index)
    {
        if (selectedObject == null) return;

        Vector3[] directions =
        {
            Vector3.up,                 // 0 - Left 
            Vector3.up + Vector3.left,  // 1 - Top-Left 
            Vector3.up + Vector3.left,  // 2 - Bottom-Left 
            Vector3.left,               // 3 - Up 
            Vector3.left,               // 4 - Down 
            Vector3.up,                 // 5 - Right 
            Vector3.up + Vector3.right, // 6 - Top-Right 
            Vector3.up + Vector3.right  // 7 - Bottom-Right 
        };

        float[] angles =
        {
             -45f,  // 0 - Left 
            -45f,  // 1 - Top-Left 
            45f,   // 2 - Bottom-Left
            -45f,  // 3 - Up 
            45f,   // 4 - Down 
            45f,   // 5 - Right 
            -45f,  // 6 - Top-Right 
            45f    // 7 - Bottom-Right 
        };

        // selectedObject.transform.Rotate(directions[index], angles[index], Space.World);

        foreach (var obj in objectsToRotate)
        {
            obj.transform.Rotate(directions[index], angles[index], Space.World);
        }
    }

    private void ResetRotation()
    {
        foreach (var obj in objectsToRotate)
        {
            obj.transform.rotation = originalRotation; // Reset to the stored original rotation
        }
    }


}
