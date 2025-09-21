using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class DebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Assign this in the Inspector
    private Queue<string> messages = new Queue<string>();
    private const int maxLines = 10;

    public static DebugDisplay Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Log(string message)
    {
        if (messages.Count >= maxLines)
            messages.Dequeue();

        messages.Enqueue(message);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        debugText.text = string.Join("\n", messages);
    }
}
