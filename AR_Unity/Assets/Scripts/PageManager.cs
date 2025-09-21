using UnityEngine;

public class PDFPageSwitcher : MonoBehaviour
{
    public GameObject[] pdfPages;  // Each page is a GameObject
    private int currentIndex = 0;

    void Start()
    {
        ShowCurrentPage();
    }

    public void Next()
    {
        // Deactivate current page
        pdfPages[currentIndex].SetActive(false);

        // Move to next page, and wrap around to 0 if we reach the end
        currentIndex = (currentIndex + 1) % pdfPages.Length;

        // Activate next page
        pdfPages[currentIndex].SetActive(true);
    }

    public void Previous()
    {
        // Deactivate current page
        pdfPages[currentIndex].SetActive(false);

        // Move to previous page, and wrap around to the last if we reach the start
        currentIndex = (currentIndex - 1 + pdfPages.Length) % pdfPages.Length;

        // Activate previous page
        pdfPages[currentIndex].SetActive(true);
    }

    private void ShowCurrentPage()
    {
        // Make sure only the first page is active at start
        for (int i = 0; i < pdfPages.Length; i++)
        {
            pdfPages[i].SetActive(i == currentIndex);
        }
    }
}
