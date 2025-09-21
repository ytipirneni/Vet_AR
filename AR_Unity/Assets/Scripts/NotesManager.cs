using UnityEngine;
using UnityEngine.UI;

public class NotesManager : MonoBehaviour
{
    [SerializeField] private Button[] NotesButton;
    [SerializeField] private GameObject[] NotesPanel;
    [SerializeField] private Button[] closeButtons;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // radiograph
        if (NotesButton.Length > 0)
            NotesButton[0].onClick.AddListener(() => TogglePanel(NotesPanel[0], true));
        if (closeButtons.Length > 0)
            closeButtons[0].onClick.AddListener(() => TogglePanel(NotesPanel[0], false));
        // chart 
        if (NotesButton.Length > 0)
            NotesButton[1].onClick.AddListener(() => TogglePanel(NotesPanel[1], true));
        if (closeButtons.Length > 0)
            closeButtons[1].onClick.AddListener(() => TogglePanel(NotesPanel[1], false));
        // textbook 
        if (NotesButton.Length > 0)
            NotesButton[2].onClick.AddListener(() => TogglePanel(NotesPanel[2], true));
        if (closeButtons.Length > 0)
            closeButtons[2].onClick.AddListener(() => TogglePanel(NotesPanel[2], false));
    }

    // Update is called once per frame
    private void TogglePanel(GameObject panel, bool state)
    {
        if (panel != null)
            panel.SetActive(state);
    }
}
