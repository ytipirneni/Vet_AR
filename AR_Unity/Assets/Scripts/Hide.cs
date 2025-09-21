using UnityEngine;

public class Hide : MonoBehaviour
{
    public GameObject[] panels;  // Array for 6 UI Panels
    public GameObject MainPanel;
    public GameObject ShowBtn;
    public GameObject[] objects; // Array for 6 GameObjects

    private bool[] activePanels;  // Tracks which panels were active
    private bool[] activeObjects; // Tracks which objects were active

    void Start()
    {
        // Initialize the arrays to store active states
        activePanels = new bool[panels.Length];
        activeObjects = new bool[objects.Length];
    }

    public void HideActiveElements()
    {
       
        // Loop through panels and store active states
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                activePanels[i] = panels[i].activeSelf; // Store if it's active
                panels[i].SetActive(false); // Hide it
            }
        }

        // Loop through objects and store active states
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                activeObjects[i] = objects[i].activeSelf; // Store if it's active
                objects[i].SetActive(false); // Hide it
            }
        }
        ShowBtn.SetActive(true);
    }

    public void UnhideElements()
    {
        // Loop through panels and reactivate those that were active
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(activePanels[i]); // Restore previous state
            }
        }

        // Loop through objects and reactivate those that were active
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(activeObjects[i]); // Restore previous state
            }
        }
       
        ShowBtn.SetActive(false);
    }
}
