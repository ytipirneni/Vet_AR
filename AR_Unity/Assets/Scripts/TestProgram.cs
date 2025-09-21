using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.XREAL.Samples;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using static UnityEngine.Rendering.DebugUI.Table;

public class TestProgram : MonoBehaviour
{
    [Header("Test")]
    public GameObject testPanel;
    public GameObject controlPanel;
    public Button calculateBtn;
    public Button okBtn;
    public bool isCoordinate = false;
    public int currentIndex = 0;
    public int textIndex = 0;
    public TextMeshProUGUI titleText;

    [Header("Control")]
    public Button calculationBtn;
    public Button OkBtn;
    public TextMeshProUGUI TitleText;
    private bool control = false;
    public GameObject setting;
    public GameObject mainProgrammPanel;
   public GameObject StartPractice;
    public GameObject EndPractice;
    public GameObject dog;
    public GameObject dog8;
    public GameObject dog6;

    [Header("ROI Test")]
    public GameObject VisualROI;
    public GameObject VisualROI2;
    public GameObject VisualROI3;
    public GameObject testROIPanel;
    public GameObject testROIButton1;
    public GameObject testROIButton2;
    public GameObject testROIButton3;
    public TextMeshProUGUI ROItestDistance;
    public bool isROI = false;
    public bool ROI1 = false;
    public bool ROI2 = false;
    public bool ROI3 = false;

    [Header("ROI control")]
    public GameObject controlROIPanel;
    public GameObject controlROIButton1;
    public GameObject controlROIButton2;
    public GameObject controlROIButton3;
    public TextMeshProUGUI ROIcontrolDistance;
    bool controlROI = false;
    public TextMeshProUGUI roitest;
    public TextMeshProUGUI roicontrol;

    [Header("Practice")]
    public Button StartTesting;
    public GameObject[] dogObjects;

    [Header("Coordinates")]
    public GameObject[] redCoordinateObjects; // The red coordinates
    public TextMeshProUGUI distanceText; // UI text for displaying distance
    public TextMeshProUGUI ControldistanceText; // UI text for displaying distance
    public objectDrawing drawingScript;
    public GameObject[] objects;
    bool isPracticing = false;
    // analyzing distance 
    private Coroutine hideCoroutine;

    [Header(" Other Refrences ")]
    public objectDrawing Drawing;
    public GameObject[] roivisuals;
    public AnnotationCapture coordinateCapture;
    public AnnotationCapture roi1;
    public AnnotationCapture roi2;
    public AnnotationCapture roi3;
    public GameObject BackButton;
    public GameObject BackButton2;
    // bool's for back button
    bool istestCoordinate = false;
    bool iscontrolCoordinate  = false;
    bool istestROI = false;
    bool iscontrolROI = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // test program buttons 
        okBtn.onClick.AddListener(testOKButton);
        StartTesting.onClick.AddListener(testOKButton);
      
        calculateBtn.onClick.AddListener(AnalyzePoint);
        // control buttons
        calculationBtn.onClick.AddListener(AnalyzePoint);
 
        OkBtn.onClick.AddListener(controlOkButton);

    }

    private void Update()
    {
        if(controlROI == true)
        {
            VisualROI.SetActive(true);          // ROI AREA
            VisualROI2.SetActive(true);
            VisualROI3.SetActive(true);
        }    
    }
    // clear the distance text 
    public void ClearText()
    {
        distanceText.text = "";
        ROItestDistance.text = " ";
        ROIcontrolDistance.text = " ";
        ControldistanceText.text = "";
        ROIcontrolDistance.text = " ";
    }
   
  //  program panel
    public void openProgram()
    {
        mainProgrammPanel.SetActive(true);
        setting.SetActive(false);
    }
    public void closeProgram()
    {
        mainProgrammPanel.SetActive(false);
        setting.SetActive(true);
        if (isPracticing == true)
        {
            stopPractice();
        }
       
    }

    // practice
    public void stopPractice()
    {
        drawingScript.StopDrawing();
        StartPractice.SetActive(true);
        EndPractice.SetActive(false);
        dog.SetActive(false);
        isPracticing = false;
    }

    public void startPractice()
    {
        drawingScript.ActivateObject(0);
        drawingScript.objectToDraw(0);
        drawingScript.StartDrawing();
        StartPractice.SetActive(false); // button 
        EndPractice.SetActive(true);
        isPracticing = true;

    }

    #region Test Program
    // Test Panel
    public void activeTestPanel()
    {
        BackButton.SetActive(false);
        isCoordinate = true;
        mainProgrammPanel.SetActive(false);
        testPanel.SetActive(true);
    }

    public void testOKButton()
    {
        istestCoordinate = true;
        if (isPracticing == true)
        {
            drawingScript.StopDrawing();
            StartPractice.SetActive(true);  // refrence for practice button 
            EndPractice.SetActive(false); // button
            dog.SetActive(false);
            isPracticing = false;
            StartCoroutine(CoordinatesDraw());
        }
        else
        {
            StartCoroutine(CoordinatesDraw());
        }

    }

    IEnumerator CoordinatesDraw()
    {

        yield return new WaitForSeconds(0.1f);     
          // currentIndex = 0 
            currentIndex++;  // currentIndex = 1

            if (currentIndex <= 5)
            {
                textIndex++;
                titleText.text = "Test " + currentIndex;
                drawingScript.ActivateObject(currentIndex);
                drawingScript.objectToDraw(currentIndex);
                DisableAllObjects();
                drawingScript.StartDrawing();
            if(currentIndex == 2)
            {
                BackButton.SetActive(true);
            }


            }

            if (currentIndex == 6)
            {
            istestCoordinate = false;
                testPanel.SetActive(false);
                controlPanel.SetActive(true);
                EnableAllObjects();
                currentIndex = 0;
                textIndex = 0;
                StartCoroutine(startControlProgram());

            }
        
    }

    IEnumerator startControlProgram()
    {
        yield return new WaitForSeconds(0.1f);
        controlOkButton();
        iscontrolCoordinate = true;
    }

    void controlOkButton()
    {
        // currentIndex = 0 
        currentIndex++;  // currentIndex = 1
        if (currentIndex <= 5)
        {
            EnableAllObjects();
            textIndex++;

            TitleText.text = "Control " + currentIndex;
            drawingScript.ActivateObject(currentIndex);
            drawingScript.objectToDraw(currentIndex);
        }

        if (currentIndex == 6)
        {
            iscontrolCoordinate = false;
            currentIndex = 0;
            textIndex = 0;
            controlPanel.SetActive(false);
            mainProgrammPanel.SetActive(true);
            dog6.SetActive(false);
            drawingScript.StopDrawing();
            isCoordinate = false;
        }
    }

    //call in the calculation button of test coordinates 
    public void showCoordinates()
    {
        switch (currentIndex)
        {
            case 1:
                SetActiveRedCoordinate(0);
                StartCoroutine(autoExportCoordinates());
            break;
            case 2:
                SetActiveRedCoordinate(1);
                StartCoroutine(autoExportCoordinates());
                break;
            case 3:
                SetActiveRedCoordinate(2);
                StartCoroutine(autoExportCoordinates());
                break;
            case 4:
                SetActiveRedCoordinate(3);
                StartCoroutine(autoExportCoordinates());
                break;
            case 5:
                SetActiveRedCoordinate(4);
                StartCoroutine(autoExportCoordinates());
                break;
        }
    }
    public void hideCoordinates()
    {
        DisableAllObjects();
    }
    IEnumerator autoExportCoordinates()
    {
        yield return new WaitForSeconds(0.5f);
    
        coordinateCapture.TakeAPhoto();

    }
    public void SetActiveRedCoordinate(int index)
    {
        // Safety check: avoid index out of range errors
        if (index < 0 || index >= redCoordinateObjects.Length)
        {
            Debug.LogWarning($"Invalid index {index}. Array length: {redCoordinateObjects.Length}");
            return;
        }

        for (int i = 0; i < redCoordinateObjects.Length; i++)
        {
            // Activate only the one at the given index, deactivate the rest
            redCoordinateObjects[i].SetActive(i == index);
        }
    }




    #endregion


    #region ROI
    // test ROI
    public void activeROIPanel()
    {
        istestROI = true;
        BackButton2.SetActive(false);
        mainProgrammPanel.SetActive(false) ;
        testROIPanel.SetActive(true) ;
        currentIndex = 6;

        drawingScript.ActivateObject(currentIndex);
        drawingScript.objectToDraw(currentIndex);

        VisualROI.SetActive(false);   // ROI AREA
        VisualROI2.SetActive(false);
        VisualROI3.SetActive(false);

        drawingScript.StartDrawing();

        testROIButton1.SetActive(true);  // Calculation Buttons
        testROIButton2.SetActive(false);
        testROIButton3.SetActive(false);

        isROI = true;

        roitest.text = "ROI Test 1";
    }
   

    public void continueTestROI()
    {
       
        currentIndex++;
        if (currentIndex <= 8)
        {
            if (currentIndex == 7)
            {
                roitest.text = "ROI Test 2";
                testROIButton1.SetActive(false);    // Calculation Buttons
                testROIButton2.SetActive(true);
                testROIButton3.SetActive(false);
                BackButton2.SetActive(true);
            }
            if (currentIndex == 8)
            {
                roitest.text = "ROI Test 3";
                testROIButton1.SetActive(false);
                testROIButton2.SetActive(false);    // Calculation Buttons
                testROIButton3.SetActive(true);
            }

            drawingScript.ActivateObject(currentIndex);
            drawingScript.objectToDraw(currentIndex);


        }
        else if (currentIndex == 9)
        {
            istestROI = false;
            controlROI = true;
            currentIndex = 6;
            testROIPanel.SetActive(false);
            controlROIPanel.SetActive(true);

            drawingScript.ActivateObject(currentIndex);
            drawingScript.objectToDraw(currentIndex);
            // control roi calculate buttons 
            controlROIButton1.SetActive(true);            // Calculation Buttons
            controlROIButton2.SetActive(false);
            controlROIButton3.SetActive(false);

           

            roicontrol.text = "ROI control 1";
            iscontrolROI = true;
        }
    }

    public void continueControlROI()
    {
      
        currentIndex++;
        if (currentIndex <= 8)
        {
            if (currentIndex == 7)
            {
              
                roicontrol.text = "ROI control 2";
                controlROIButton1.SetActive(false);
                controlROIButton2.SetActive(true);
                controlROIButton3.SetActive(false);
            }
            if (currentIndex == 8)
            {
              
                roicontrol.text = "ROI control 3";
                controlROIButton1.SetActive(false);
                controlROIButton2.SetActive(false);
                controlROIButton3.SetActive(true);
            }

            drawingScript.ActivateObject(currentIndex);
            drawingScript.objectToDraw(currentIndex);


        }
        else if (currentIndex == 9)
        {
            controlROI = false;
            isROI = false;
            iscontrolCoordinate = false;
            iscontrolROI = false;
            currentIndex =  0;
            controlROIPanel.SetActive(false);
            mainProgrammPanel.SetActive(true);
            drawingScript.StopDrawing();
            dog8.SetActive(false);        
            DisableObjects();
            VisualROI.SetActive(false);
            VisualROI2.SetActive(false);
            VisualROI3.SetActive(false);

        }
       
    }

    public void showROI()
    {
        switch (currentIndex)
        {
            case 6:
                SetActiveROI(0);
                StartCoroutine(autoExportROI1());

                break;
            case 7:
                SetActiveROI(1);
                StartCoroutine(autoExportROI2());
                break;
            case 8:
                SetActiveROI(2);
                StartCoroutine(autoExportROI3());
                break;
        }
    }

    IEnumerator autoExportROI1()
    {
        yield return new WaitForSeconds(0.05f);
        roi1.TakeAPhoto();
    }
    IEnumerator autoExportROI2()
    {
        yield return new WaitForSeconds(0.05f);
        roi2.TakeAPhoto(); 
    }
    IEnumerator autoExportROI3()
    {
        yield return new WaitForSeconds(0.05f);
        roi3.TakeAPhoto();
    }

    public void SetActiveROI(int index)
    {
        // Safety check: avoid index out of range errors
        if (index < 0 || index >= roivisuals.Length)
        {
            Debug.LogWarning($"Invalid index {index}. Array length: {roivisuals.Length}");
            return;
        }

        for (int i = 0; i < roivisuals.Length; i++)
        {
            // Activate only the one at the given index, deactivate the rest
            roivisuals[i].SetActive(i == index);
        }
    }

    public void HideROI()
    {
        foreach (GameObject obj in roivisuals)
        {
            obj.SetActive(false);
        }
    }

    #endregion
    public void exitProgram()
    {
        controlROI = false;
        istestCoordinate = false;
        iscontrolCoordinate = false;
        iscontrolROI = false;
        istestROI = false;
        currentIndex = 0;
        textIndex = 0;
        testPanel.SetActive(false );
        controlPanel.SetActive(false );
        testROIPanel.SetActive(false ) ;
        controlROIPanel.SetActive(false) ;
        drawingScript.StopDrawing();
        mainProgrammPanel.SetActive(true);
        VisualROI.SetActive(false);
        DisableObjects();
        isCoordinate = false;
    }
    
    public void backButton()
    {
        StartCoroutine(BackBtnTimer());
    }
    IEnumerator BackBtnTimer()
    {
        if (istestCoordinate == true)
        {
            if (currentIndex >= 2 && currentIndex <= 5)
            {
                currentIndex--;
                yield return new WaitForSeconds(0.05f);
                titleText.text = "Test " + currentIndex;
                drawingScript.ActivateObject(currentIndex);
                yield return new WaitForSeconds(0.05f);
                drawingScript.objectToDraw(currentIndex);
                if (currentIndex == 1)
                {
                    BackButton.SetActive(false);
                }
               
            }

        }
        if (iscontrolCoordinate == true)
        {
            if (currentIndex == 1)
            {
                DisableAllObjects();
                testPanel.SetActive(true);
                controlPanel.SetActive(false);
                iscontrolCoordinate = false;
                yield return new WaitForSeconds(0.05f);
                titleText.text = "Test 5";
                istestCoordinate = true;
                currentIndex = 5;
                yield return new WaitForSeconds(0.05f);
                drawingScript.ActivateObject(currentIndex);
                drawingScript.objectToDraw(currentIndex);
            }
            else if (currentIndex >= 2 && currentIndex <= 5)
            {

                currentIndex--;
                yield return new WaitForSeconds(0.05f);
                TitleText.text = "Control " + currentIndex;
                drawingScript.ActivateObject(currentIndex);
                yield return new WaitForSeconds(0.05f);
                drawingScript.objectToDraw(currentIndex);
            }

        }

      
    }
    public void backbtn()
    {
        StartCoroutine(backROI());
    }

    IEnumerator backROI()
    {

        if (istestROI == true)
        {
            if (currentIndex == 7 || currentIndex == 8)
            {
                currentIndex--;
                yield return new WaitForSeconds(0.1f);
                if (currentIndex == 6)
                {
                    BackButton2.SetActive(false);
                    roitest.text = "ROI Test 1";
                    testROIButton1.SetActive(true);    // Calculation Buttons
                    testROIButton2.SetActive(false);
                    testROIButton3.SetActive(false);
                }
                if (currentIndex == 7)
                {
                    roitest.text = "ROI Test 2";
                    testROIButton1.SetActive(false);
                    testROIButton2.SetActive(true);    // Calculation Buttons
                    testROIButton3.SetActive(false);
                }
                yield return new WaitForSeconds(0.05f);
                drawingScript.ActivateObject(currentIndex);
                yield return new WaitForSeconds(0.05f);
                drawingScript.objectToDraw(currentIndex);
            }
        }

        if (iscontrolROI == true)
        {
            if (currentIndex == 6)
            {

                currentIndex = 8;
                controlROI = false;
                yield return new WaitForSeconds(0.03f);
              
                testROIPanel.SetActive(true);
                controlROIPanel.SetActive(false);
                iscontrolROI = false;
                HideROI();
                yield return new WaitForSeconds(0.05f);
                istestROI = true;

                drawingScript.ActivateObject(8);
                drawingScript.objectToDraw(8);
                roitest.text = "ROI Test 3 " ;
                testROIButton1.SetActive(false);
                testROIButton2.SetActive(false);    // Calculation Buttons
                testROIButton3.SetActive(true);

            }

          else if (currentIndex == 7 || currentIndex == 8)
            {
                currentIndex--;
                yield return new WaitForSeconds(0.05f);
                if (currentIndex == 6)
                {
                    roicontrol.text = "ROI control 1";
                    controlROIButton1.SetActive(true);            // Calculation Buttons
                    controlROIButton2.SetActive(false);
                    controlROIButton3.SetActive(false);

                    yield return new WaitForSeconds(0.05f);
                    drawingScript.ActivateObject(currentIndex);
                    yield return new WaitForSeconds(0.05f);
                    drawingScript.objectToDraw(currentIndex);
                }
                if (currentIndex == 7)
                {
                    roicontrol.text = "ROI control 2";
                    controlROIButton1.SetActive(false);            // Calculation Buttons
                    controlROIButton2.SetActive(true);
                    controlROIButton3.SetActive(false);

                    yield return new WaitForSeconds(0.05f);
                    drawingScript.ActivateObject(currentIndex);
                    yield return new WaitForSeconds(0.05f);
                    drawingScript.objectToDraw(currentIndex);

                }

            }
        }
    }
    // enable the coordinates 
    public void EnableAllObjects()
    {
        foreach (GameObject obj in redCoordinateObjects)
        {
            obj.SetActive(true);
        }
    }

    // disable the coordinates
    public void DisableAllObjects()
    {
        foreach (GameObject obj in redCoordinateObjects)
        {
            obj.SetActive(false);
        }
    }


    // disable the models 
    public void DisableObjects()
    {
        foreach (GameObject obj in dogObjects)
        {
            obj.SetActive(false);
        }
    }


    // Drawing Logic on the objects 


    #region Analyzing the distance 

    public void AnalyzePoint()
    {
        int pointIndex = currentIndex - 1;
        // 🔄 Replace passed-in index with currentIndex (you define how it's set)
        int index = pointIndex;

        Debug.Log($"Analyzing index: {index}");

        if (index < 0 || index >= redCoordinateObjects.Length)
        {
            Debug.LogWarning("Index out of range. Check your redCoordinateObjects setup.");
            return;
        }


        GameObject latestAnnotation = drawingScript.LatestAnnotation;
        if (latestAnnotation == null)
        {
            Debug.LogWarning("No latest annotation found.");
            return;
        }

        Vector3 start = latestAnnotation.transform.position;

        // ✅ Use 3D collider (BoxCollider or any Collider) on the red coordinate
        Collider collider3D = redCoordinateObjects[index].GetComponent<Collider>();
        if (collider3D == null)
        {
            Debug.LogWarning("No 3D Collider (e.g., BoxCollider) found on red coordinate object.");
            return;
        }

        // Closest point on the collider surface to our annotation (world space)
        Vector3 closestPoint = collider3D.ClosestPoint(start);

        // Distance annotation → collider edge (3D)
        float distanceInMM = Vector3.Distance(start, closestPoint) * 1000f; // 1u = 1m → mm
        Debug.Log($"Latest annotation position: {start}");
        Debug.Log($"Closest point on 3D collider: {closestPoint}");
        Debug.Log($"Calculated distance: {distanceInMM} mm");

        ShowDistance($"{distanceInMM:F1} mm");
    }


    void ShowDistance(string text)
    {
        distanceText.text = text;
        ControldistanceText.text = text;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(10f));
    }
    IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        distanceText.text = "";
        ControldistanceText.text = "";
        Debug.Log("Distance text hidden after delay.");
    }

    #endregion
}
