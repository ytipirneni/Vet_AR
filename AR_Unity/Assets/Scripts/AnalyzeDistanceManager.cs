using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnalyzeDistanceManager : MonoBehaviour
{
    public GameObject[] redCoordinateObjects; // The red coordinates
    public TextMeshProUGUI distanceText; // UI text for displaying distance
    public GameObject analyzePanel;
    public GameObject SettingPanel;
    public objectDrawing drawerScript;  // Assign via Inspector or GetComponent
    public TestProgram testProgram;

    private Coroutine hideCoroutine;

    public void AnalyzePoint(int index)
    {
        Debug.Log($"Analyzing index: {index}");

        if (index < 0 || index >= redCoordinateObjects.Length)
        {
            Debug.LogWarning("Index out of range. Check your redCoordinateObjects setup.");
            return;
        }

        GameObject latestAnnotation = drawerScript.LatestAnnotation;
      
       
        if (latestAnnotation == null)
        {
            Debug.LogWarning("No latest annotation found.");
            return;
        }

        Vector3 start = latestAnnotation.transform.position;
        Vector3 end = redCoordinateObjects[index].transform.position;

        Debug.Log($"Latest annotation position: {start}");
        Debug.Log($"Red coordinate position: {end}");

        float distanceInMM = Vector3.Distance(start, end) * 1000f; // Convert to mm
        Debug.Log($"Calculated distance: {distanceInMM} mm");

        ShowDistance($"{distanceInMM:F1} mm");
    }

    void ShowDistance(string text)
    {
        distanceText.text = text;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(10f));
    }

    public void HideDistance()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        distanceText.text = "";
        Debug.Log("Distance hidden manually.");
    }

    IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        distanceText.text = "";
        Debug.Log("Distance text hidden after delay.");
    }

    public void OpenPanel()
    {
        analyzePanel.SetActive(true);
        SettingPanel.SetActive(false);
       
    }

    public void closePanel()
    {
        analyzePanel.SetActive(false);
        SettingPanel.SetActive(true);
       
    }

   

  

}
