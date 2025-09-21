using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PanelBtnManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject[] Panels;
    [SerializeField] private GameObject MainPanel;



    [Header("Buttons")]
    [SerializeField] private Button[] mainButtons; // Select, Rotate, Draw, Capture, Notes, Settings
    [SerializeField] private Button[] selectPanelButtons; // One, Two, Three, Four, Five, None
    [SerializeField] private Button closeRotationPanelBtn;
    [SerializeField] private Button closeSelectPanelBtn;
    [SerializeField] private Button closeDrawingPanel;
    [SerializeField] private Button closeCapturePanel;
    [SerializeField] private Button closeNotesPanel;
    [SerializeField] private Button closeSettingsPanel;

    [Header("Objects")]
    [SerializeField] private GameObject[] objects; // Objects to activate/deactivate

    public ObjectRotator rotatorScript; // Reference to ObjectRotator
    public objectDrawing drawingScript;
 
    private void Awake()
    {
        // Select Panel 
        if (mainButtons.Length > 0)

            mainButtons[0].onClick.AddListener(() => {
                TogglePanel(Panels[0], true);
                MainPanel.SetActive(false); 
            });



        if (closeSelectPanelBtn != null)
            closeSelectPanelBtn.onClick.AddListener(() =>
            {
                TogglePanel(Panels[0], false);
                MainPanel.SetActive(true); 
            });
            
              


        // Rotation Panel
        if (mainButtons.Length > 0)
            mainButtons[1].onClick.AddListener(() => {
                TogglePanel(Panels[1], true);
                MainPanel.SetActive(false);
            });
        if (closeRotationPanelBtn != null)
            closeRotationPanelBtn.onClick.AddListener(() => {
                TogglePanel(Panels[1], false);
                MainPanel.SetActive(true);
            });

        // Drawing Panel
        if (mainButtons.Length > 0)
            mainButtons[2].onClick.AddListener(() => {
                TogglePanel(Panels[2], true);
                MainPanel.SetActive(false);
            });
        if (closeDrawingPanel != null)
            closeDrawingPanel.onClick.AddListener(() => {
                TogglePanel(Panels[2], false);
                MainPanel.SetActive(true);
            });

        // Capture Panel
        if (mainButtons.Length > 0)
            mainButtons[3].onClick.AddListener(() => {
                TogglePanel(Panels[3], true);
                MainPanel.SetActive(false);
            });
        if (closeCapturePanel != null)
            closeCapturePanel.onClick.AddListener(() => {
                TogglePanel(Panels[3], false);
                MainPanel.SetActive(true);
            });

        // notes panel
        if (mainButtons.Length > 0)
            mainButtons[4].onClick.AddListener(() => {
                TogglePanel(Panels[4], true);
                MainPanel.SetActive(false);
            });
        if (closeNotesPanel != null)
            closeNotesPanel.onClick.AddListener(() => {
                TogglePanel(Panels[4], false);
                MainPanel.SetActive(true);
            });
        // Settings Panel
        if (mainButtons.Length > 0)
            mainButtons[5].onClick.AddListener(() => {
                TogglePanel(Panels[5], true);
                MainPanel.SetActive(false);
            });
        if (closeSettingsPanel != null)
            closeSettingsPanel.onClick.AddListener(() => {
                TogglePanel(Panels[5], false);
                MainPanel.SetActive(true);
            });



       
    }

    private bool[] isSelected; // Track selection state

    private void Start()
    {
        if (selectPanelButtons == null || objects == null)
        {
            Debug.LogError("Panel buttons or objects array is not assigned in the Inspector!");
            return;
        }

        isSelected = new bool[selectPanelButtons.Length]; // Initialize selection states
        AssignSelectPanelButtons();
        }

    private void AssignSelectPanelButtons()
    {
        if (selectPanelButtons.Length != objects.Length)
        {
            Debug.LogError("Mismatch: Number of buttons and objects should be the same!");
            return;
        }

        for (int i = 0; i < selectPanelButtons.Length; i++)
        {
            int index = i;

            if (selectPanelButtons[index] == null)
            {
                Debug.LogError($"Button at index {index} is null!");
                continue;
            }

            selectPanelButtons[index].onClick.AddListener(() => ToggleSelection(index));

            Text buttonText = selectPanelButtons[index].GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Select"; // Default text
            else
                Debug.LogError($"TMP_Text missing on button at index {index}!");
        }
    }




    private void TogglePanel(GameObject panel, bool state)
    {
        if (panel != null)
            panel.SetActive(state);
    }




    private void ToggleSelection(int index)
    {
        // Validate the index
        if (index < 0 || index >= objects.Length)
        {
            Debug.LogError($"Index {index} is out of bounds!");
            return;
        }

        if (selectPanelButtons[index] == null)
        {
            Debug.LogError($"Button at index {index} is null!");
            return;
        }

        Text buttonText = selectPanelButtons[index].GetComponentInChildren<Text>();
        if (buttonText == null)
        {
            Debug.LogError($"TMP_Text component is missing in button at index {index}!");
            return;
        }

        // Deselect all other buttons and reset their text
        for (int i = 0; i < selectPanelButtons.Length; i++)
        {
            if (i != index)
            {
                isSelected[i] = false;
                Text otherButtonText = selectPanelButtons[i].GetComponentInChildren<Text>();
                if (otherButtonText != null)
                    otherButtonText.text = (i + 1).ToString();  // Reset text
                DeactivateObject(i);  // Deactivate other objects
            }
        }

        // Toggle selection of the current button
        isSelected[index] = !isSelected[index];

        if (isSelected[index])
        {
            buttonText.text = "Deselect";  // Change button text to "Deselect"
            ActivateObject(index);  // Activate the selected object

            // Call related functions
            if (rotatorScript != null)
                rotatorScript.SetSelectedObject(index);
            else
                Debug.LogWarning("rotatorScript is null!");

            if (drawingScript != null)
                drawingScript.objectToDraw(index);
            else
                Debug.LogWarning("drawingScript is null!");
        }
        else
        {
            buttonText.text = (index + 1).ToString();  // Reset text when deselected
            DeactivateObject(index);  // Deactivate the selected object
        }
    }


    private void ActivateObject(int index)
    {
        if (objects == null || index >= objects.Length || objects[index] == null)
        {
            Debug.LogError($"Object at index {index} is null or out of bounds!");
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].SetActive(i == index);
        }
    }

    private void DeactivateObject(int index)
    {
        if (objects == null || index >= objects.Length || objects[index] == null)
        {
            Debug.LogError($"Object at index {index} is null or out of bounds!");
            return;
        }

        objects[index].SetActive(false);
    }
}
