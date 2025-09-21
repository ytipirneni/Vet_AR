

using System.Collections;
using System.Collections.Generic; // XREAL SDK for hand tracking
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

public class objectDrawing : MonoBehaviour
{
    public GameObject[] objects;
    private GameObject selectedObject;
    public Transform colorBar;
    private bool isDrawing = false;
    private float drawTimer = 10000000f;
    private int colorIndex = 0;
    private bool isManualSelection = false;

    [SerializeField] private Button startDrawing;
    [SerializeField] private Button stopDrawing;
    [SerializeField] private float surfaceOffset = 0.07f;

    public Vector3 rotationOffset;
    public float minSpawnDistance = 0.05f;
    private Vector3 lastSpawnPosition = Vector3.zero;
    private bool firstSpawn = true;

    public Slider colorSlider;
    public RawImage gradientImage;
    private Texture2D gradientTexture;

    public LineRenderer lineRenderer;
    public float rayLength = 5f;
    public LayerMask detectionLayer;

    public Vector3 directionOffset = Vector3.zero;
    public HandRaySwitcher switchRayScript;
    public GameObject RHand;
    public GameObject LHand;

    private ObjectPooling objectPool;
    private List<GameObject> drawnAnnotations = new List<GameObject>();

    public static XRHandSubsystem handSubsystem;
    public float pinchThreshold = 0.02f;

    public NoseHighlighter roiSource;
    public NoseHighlighter roiSource2;
    public NoseHighlighter roiSource3;
    public TMP_Text roitestText;
    public TMP_Text roicontrolText;
    public float mmPerPixel = 0.5f;
    public float closeThresholdMM = 5f;

    private List<Vector2> strokePointsXY = new List<Vector2>();
    [Range(1, 8)] public int rasterSupersampling = 2;

    [Range(0f, 1f)] public float posLerp = 0.35f;
    [Range(0f, 1f)] public float normalLerp = 0.35f;

    private Vector3 lastHitPoint;
    private Vector3 lastHitNormal;
    private bool hasLastHit;

    public GameObject LatestAnnotation { get; private set; }

    private Color[] gradientColors = new Color[]
    {
    new Color(0.0f, 0.0f, 0.545f, 0.5f),
    new Color(0.0f, 0.0f, 0.8f, 0.5f),
    new Color(0.0f, 0.0f, 1.0f, 0.5f),
    new Color(0.0f, 0.5f, 1.0f, 0.5f),
    new Color(0.3f, 0.6f, 1.0f, 0.5f),
    new Color(0.678f, 0.847f, 0.902f, 0.5f),
    new Color(0.0f, 1.0f, 0.0f, 0.5f),
    new Color(0.0f, 0.8f, 0.0f, 0.5f),
    new Color(0.6f, 1.0f, 0.6f, 0.5f),
    new Color(1.0f, 1.0f, 0.0f, 0.5f),
    new Color(1.0f, 0.84f, 0.0f, 0.5f),
    new Color(1.0f, 0.647f, 0.0f, 0.5f),
    new Color(1.0f, 0.3f, 0.3f, 0.5f),
    new Color(1.0f, 0.0f, 0.0f, 0.5f),
    new Color(0.8f, 0.0f, 0.0f, 0.5f),
    new Color(0.545f, 0.0f, 0.0f, 0.5f)
    };

    private Collider lastHitCollider;
    public float breakDistance = 0.05f;
    public float fingertipProbe = 0.01f;

    private Color selectedColor;

    [SerializeField] private GameObject rightIndexTipObj;
    [SerializeField] private GameObject leftIndexTipObj;

    public TestProgram testScript;
    private bool _autoClosing = false;
    private float _cooldownUntil = 0f;
    private bool _prevIsROI = false;

    // New state variables
    private bool isArmed = true;
    private bool wasTouchingSurface = false;
    private GameObject lastAnnotation;
    [SerializeField] private float surfaceOFFSet = 0.07f;  
    // For instantiation
    [SerializeField] private GameObject annotationPrefab;

    // ===== NEW / UPDATED FIELDS =====
    [Header("Calculation Path Viz")]
    public Color calcLineColor = Color.cyan;
    public Material calcLineMaterial;
    public bool showCalcLineOnCalculate = true;

    // Fallback offset (used if autoOffsetFromWidth == false)
    [Tooltip("Fallback lift along surface normal (m) if Auto Offset is off.")]
    public float calcLineOffset = 0.003f;

    // How to size the calc line so it matches your brush/stamps
    public enum BrushWidthSource { ManualMM, TipCollider, LatestAnnotation }
    // --- Calc line width controls ---
  

    [Header("Calc Line Width")]
    public BrushWidthSource lineWidthSource = BrushWidthSource.ManualMM;

    // If ManualMM is selected, this is the width in millimeters (e.g., 2 = 2 mm)
    [Range(0.1f, 50f)] public float manualStrokeWidthMM = 2f;

    // Lift the line above the surface by half its width (auto) or a fixed offset
    public bool autoOffsetFromWidth = true;
    [Tooltip("Extra lift added on top of half-width, in millimeters.")]
    [Range(0f, 5f)] public float extraOffsetMM = 0.5f;

    // Fallback fixed offset (meters) if autoOffsetFromWidth == false
    // --- Calc Path Markers ---
    [Header("Calc Path Markers")]
    public GameObject startMarkerPrefab;   // optional; used at the first sample
    public GameObject endMarkerPrefab;     // optional; used at the last sample (falls back to start if null)
    private GameObject _startMarkerInstance;
    private GameObject _endMarkerInstance;

    private LineRenderer _calcLine;
    private readonly List<Vector3> _strokeWorld = new();
    private readonly List<Vector3> _strokeNormals = new();
    private float _lastCalcLineWidthM = 0.0015f;  // remember last applied width


    void Awake()
    {
        // Get the running XRHandSubsystem instance
        List<XRHandSubsystem> handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        if (handSubsystems.Count > 0)
        {
            handSubsystem = handSubsystems[0];
            Debug.Log("XRHandSubsystem initialized.");
        }
        else
        {
            Debug.LogError("XRHandSubsystem not found. Make sure XR Hands package and OpenXR plugin are set up.");
        }
        lineRenderer.enabled = false; // Ensure ray is off when out of range
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        // Read both sphere collider radii once at start
        
    }

    void Start()
    {
        // hand
        List<XRHandSubsystem> handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);
        if (handSubsystems.Count > 0)
        {
            handSubsystem = handSubsystems[0];
            handSubsystem.Start();
        }




        objectPool = FindObjectOfType<ObjectPooling>();  // Find the pool in the scene
        GenerateGradientTexture();
        if (gradientImage != null)
            gradientImage.texture = gradientTexture;

        if (colorSlider != null)
            colorSlider.onValueChanged.AddListener(UpdateSelectedColor);

        startDrawing.onClick.AddListener(StartDrawing);
        stopDrawing.onClick.AddListener(StopDrawing);

        colorBar.gameObject.SetActive(false);

    }


    void Update()
    {
        // pick active hand + show the correct hand rig
        XRHand activeHandState;
        if (switchRayScript.leftRay)
        {
            activeHandState = handSubsystem.leftHand;
            RHand.SetActive(false);
            LHand.SetActive(true);
        }
        else
        {
            activeHandState = handSubsystem.rightHand;
            RHand.SetActive(true);
            LHand.SetActive(false);
        }

        if (activeHandState == null || !activeHandState.isTracked)
            return;

        // ROI edge: when ROI toggles ON, hard reset gates so next loop can auto-close again
        

        // pinch state (kept if you use it elsewhere)
        XRHandJoint thumbTip = activeHandState.GetJoint(XRHandJointID.ThumbTip);
        XRHandJoint indexTip = activeHandState.GetJoint(XRHandJointID.IndexTip);

        Pose thumbPose, indexTipPose;
        bool isPinching = false;
        Vector3 handPosition = Vector3.zero;

        if (thumbTip.TryGetPose(out thumbPose) && indexTip.TryGetPose(out indexTipPose))
        {
            float pinchDistance = Vector3.Distance(thumbPose.position, indexTipPose.position);
            isPinching = pinchDistance < pinchThreshold;
            handPosition = indexTipPose.position;
        }

        if (!isDrawing)
            return;

        // timer
        drawTimer -= Time.deltaTime;
        if (drawTimer <= 0f)
        {
            StopDrawing();
            return;
        }

        if(testScript.isCoordinate == true)
        {
            DrawCoordinate(activeHandState);
        }
        else
        {
            DrawAtFingertipSurface(activeHandState);
        }
           


        lineRenderer.enabled = false;
    }





    public void objectToDraw(int index)
    {
        if (index >= 0 && index < objects.Length)
        {
            selectedObject = objects[index];
            Debug.Log("Object selected: " + selectedObject.name);

            foreach (GameObject annotation in drawnAnnotations)
            {
                annotation.transform.SetParent(selectedObject.transform);

            }

        }
    }



    // Call this from Update() when isDrawing == true
    // REPLACE the old DrawAtFingertipSurface method with this sphere-driven one
    // ===== DRAW: only sample when you actually stamp, and use SMOOTHED contact =====
    private void DrawAtFingertipSurface(XRHand hand)
    {
        if (hand == null || !hand.isTracked) return;

        XRHandJoint tipJ = hand.GetJoint(XRHandJointID.IndexTip);
        if (!tipJ.TryGetPose(out var tipPose)) return;

        Vector3 tipPos = tipPose.position;

        GameObject activeTipObj = switchRayScript.leftRay ? leftIndexTipObj : rightIndexTipObj;
        SphereCollider tipCol = activeTipObj ? activeTipObj.GetComponent<SphereCollider>() : null;
        if (tipCol == null) return;

        Vector3 tipWorldCenter = tipCol.transform.TransformPoint(tipCol.center);
        float scale = Mathf.Max(activeTipObj.transform.lossyScale.x,
                                activeTipObj.transform.lossyScale.y,
                                activeTipObj.transform.lossyScale.z);
        float tipWorldRadius = tipCol.radius * scale;

        Collider[] hits = Physics.OverlapSphere(tipWorldCenter, tipWorldRadius, detectionLayer, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            hasLastHit = false;
            firstSpawn = true;
            return;
        }

        MeshCollider meshCol = null;
        Collider chosen = null;
        float bestSqr = float.MaxValue;

        foreach (var c in hits)
        {
            if (!IsDetectable(c.gameObject)) continue;
            var mc = c as MeshCollider;
            if (mc == null) continue;

            Vector3 cp = mc.ClosestPoint(tipPos);
            float sq = (cp - tipPos).sqrMagnitude;
            if (sq < bestSqr)
            {
                bestSqr = sq;
                meshCol = mc;
                chosen = c;
            }
        }

        if (meshCol == null)
        {
            hasLastHit = false;
            firstSpawn = true;
            return;
        }

        Vector3 closestPoint = meshCol.ClosestPoint(tipPos);
        Vector3 approxOut = (tipPos - closestPoint).sqrMagnitude > 1e-8f
            ? (tipPos - closestPoint).normalized
            : Vector3.up;

        RaycastHit rh;
        Vector3 hitPoint = closestPoint;
        Vector3 hitNormal;

        bool gotNormal = meshCol.Raycast(new Ray(closestPoint + approxOut * fingertipProbe, -approxOut), out rh, fingertipProbe * 2f);
        hitNormal = gotNormal ? rh.normal : approxOut;

        bool snap = !hasLastHit || chosen != lastHitCollider || Vector3.Distance(hitPoint, lastHitPoint) > breakDistance;

        Vector3 smPoint = snap ? hitPoint : Vector3.Lerp(lastHitPoint, hitPoint, posLerp);
        Vector3 smNormal = snap ? hitNormal : Vector3.Slerp(lastHitNormal, hitNormal, normalLerp);

        hasLastHit = true;
        lastHitPoint = smPoint;
        lastHitNormal = smNormal;
        lastHitCollider = chosen;

        // Use SMOOTHED contact for both visuals and sampling; offset is only for the spawned annotation prefab
        Vector3 spawnPosition = smPoint + smNormal * surfaceOffset;

        // spacing gate: only stamp + record when far enough from last stamp
        if (!firstSpawn && Vector3.Distance(spawnPosition, lastSpawnPosition) < minSpawnDistance)
            return;

        firstSpawn = false;
        lastSpawnPosition = spawnPosition;

        Quaternion annotationRotation = Quaternion.LookRotation(smNormal) * Quaternion.Euler(rotationOffset);

        if (objectPool.activeObjects.Count > objectPool.poolSize)
        {
            var oldest = objectPool.activeObjects[0];
            objectPool.activeObjects.RemoveAt(0);
            objectPool.ReturnObject(oldest);
        }

        selectedObject = chosen.gameObject;
        GameObject annotation = objectPool.GetObject(spawnPosition, annotationRotation);
        annotation.transform.SetParent(selectedObject.transform);
        drawnAnnotations.Add(annotation);
        LatestAnnotation = annotation;

        // 1) area polygon (2D) from SMOOTHED point
        strokePointsXY.Add(new Vector2(smPoint.x, smPoint.y));
        // 2) calc-line uses EXACTLY the same SMOOTHED samples (1->2->... sequence)
        CalcLine_AddSample(smPoint, smNormal);

        annotation.GetComponent<Renderer>().material.color =
            isManualSelection ? selectedColor : gradientColors[colorIndex % gradientColors.Length];

        colorIndex++;
        isManualSelection = false;
    }



    private void DrawCoordinate(XRHand hand)
    {
        if (hand == null || !hand.isTracked) return;

        XRHandJoint tipJ = hand.GetJoint(XRHandJointID.IndexTip);
        if (!tipJ.TryGetPose(out var tipPose)) return;

        Vector3 tipPos = tipPose.position;

        GameObject activeTipObj = switchRayScript.leftRay ? leftIndexTipObj : rightIndexTipObj;
        SphereCollider tipCol = activeTipObj.GetComponent<SphereCollider>();
        if (tipCol == null) return;

        Vector3 tipWorldCenter = tipCol.transform.TransformPoint(tipCol.center);
        float scale = Mathf.Max(activeTipObj.transform.lossyScale.x,
                                activeTipObj.transform.lossyScale.y,
                                activeTipObj.transform.lossyScale.z);
        float tipWorldRadius = tipCol.radius * scale;

        Collider[] hits = Physics.OverlapSphere(tipWorldCenter,
                                                tipWorldRadius,
                                                detectionLayer,
                                                QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            wasTouchingSurface = false;
            return;
        }

        MeshCollider meshCol = null;
        Collider chosen = null;
        float bestSqr = float.MaxValue;

        foreach (var c in hits)
        {
            if (!IsDetectable(c.gameObject)) continue;
            var mc = c as MeshCollider;
            if (mc == null) continue;

            Vector3 cp = mc.ClosestPoint(tipPos);
            float sq = (cp - tipPos).sqrMagnitude;
            if (sq < bestSqr)
            {
                bestSqr = sq;
                meshCol = mc;
                chosen = c;
            }
        }

        if (meshCol == null)
        {
            wasTouchingSurface = false;
            return;
        }

        Vector3 closestPoint = meshCol.ClosestPoint(tipPos);
        Vector3 approxOut = (tipPos - closestPoint).sqrMagnitude > 1e-8f
            ? (tipPos - closestPoint).normalized
            : Vector3.up;

        RaycastHit rh;
        Vector3 hitPoint = closestPoint;
        Vector3 hitNormal;

        bool gotNormal = meshCol.Raycast(
            new Ray(closestPoint + approxOut * fingertipProbe, -approxOut),
            out rh,
            fingertipProbe * 2f
        );

        hitNormal = gotNormal ? rh.normal : approxOut;

        // smoothing logic reused
        Vector3 smPoint = !wasTouchingSurface ? hitPoint : Vector3.Lerp(lastHitPoint, hitPoint, posLerp);
        Vector3 smNormal = !wasTouchingSurface ? hitNormal : Vector3.Slerp(lastHitNormal, hitNormal, normalLerp);

        bool touchingThisFrame = true; // because we got a valid mesh

        // Placement only on touch begin and if armed
        if (isArmed && !wasTouchingSurface && touchingThisFrame)
        {
            Vector3 spawnPosition = smPoint + smNormal * surfaceOFFSet;
            Quaternion annotationRotation = Quaternion.LookRotation(smNormal) * Quaternion.Euler(rotationOffset);

            GameObject annotation = Instantiate(annotationPrefab, spawnPosition, annotationRotation);
            annotation.transform.SetParent(chosen.gameObject.transform);

            annotation.GetComponent<Renderer>().material.color = gradientColors[0];

            lastAnnotation = annotation;
            LatestAnnotation = annotation;

            isArmed = false;
        }

        // update for next frame
        wasTouchingSurface = touchingThisFrame;
        lastHitPoint = smPoint;
        lastHitNormal = smNormal;
        lastHitCollider = chosen;
    }

    public void RedoCoordinate()
    {
        if (lastAnnotation != null)
        {
            Destroy(lastAnnotation);
            lastAnnotation = null;
        }
        isArmed = true;
    }






    // caclulations for the 3 ROI
    public void CalculateCoverage()
    {
        if (roiSource == null || strokePointsXY.Count < 3)
        {
            Debug.LogWarning("ROI or drawing not ready.");
            return;
        }

        // --- Build polygons in XY ---
        List<Vector3> roiWorld = roiSource.GenerateClosedCatmullRomSplinePoints();
        var roiXY = new List<Vector2>(roiWorld.Count);
        foreach (var p in roiWorld) roiXY.Add(new Vector2(p.x, p.y));

        var roiClosed = new List<Vector2>(roiXY);
        var strokeClosed = new List<Vector2>(strokePointsXY);

        // Ensure closed loops (unit: meters; 0.0001 ~ 0.1 mm; closeThresholdMM is mm)
        PolygonUtils.EnsureClosed(roiClosed, 0.0001f);
        PolygonUtils.EnsureClosed(strokeClosed, closeThresholdMM / 1000f);

        if (roiClosed.Count < 3 || strokeClosed.Count < 3)
        {
            Debug.LogWarning("Invalid polygon(s).");
            return;
        }

        // --- Areas (mm²) ---
        float Z = roiSource.CalculateAreaInSquareMillimeters();                     // ROI area (mm²)
        float strokeAreaMM2 = PolygonUtils.PolygonArea(strokeClosed) * 1_000_000f;   // stroke area (mm²)

        // --- Fast containment tests to enforce invariants before rasterizing ---
        // centroid helpers
        Vector2 CentroidAvg(List<Vector2> poly)
        {
            // average-of-vertices centroid is sufficient for our containment tests
            Vector2 c = Vector2.zero;
            for (int i = 0; i < poly.Count; i++) c += poly[i];
            return c / Mathf.Max(1, poly.Count);
        }

        bool MostPointsInside(List<Vector2> query, List<Vector2> container, float passRatio = 0.98f)
        {
            int inside = 0;
            for (int i = 0; i < query.Count; i++)
            {
                if (NoseHighlighter.IsPointInPolygon(query[i], container)) inside++;
            }
            return inside >= Mathf.CeilToInt(passRatio * query.Count);
        }

        Vector2 roiCentroid = CentroidAvg(roiClosed);
        Vector2 strokeCentroid = CentroidAvg(strokeClosed);

        bool roiCentroidInStroke = NoseHighlighter.IsPointInPolygon(roiCentroid, strokeClosed);
        bool strokeCentroidInRoi = NoseHighlighter.IsPointInPolygon(strokeCentroid, roiClosed);
        bool roiMostlyInStroke = MostPointsInside(roiClosed, strokeClosed); // ROI enclosed by stroke
        bool strokeMostlyInRoi = MostPointsInside(strokeClosed, roiClosed);   // stroke enclosed by ROI

        float Y; // overlap mm²

        // Case 1: Drawing fully covers ROI -> overlap must equal ROI area
        if (roiCentroidInStroke && roiMostlyInStroke)
        {
            Y = Z; // fully covered
        }
        // Case 2: Drawing fully inside ROI -> overlap equals stroke area, non-overlap is zero
        else if (strokeCentroidInRoi && strokeMostlyInRoi)
        {
            Y = strokeAreaMM2;
        }
        else
        {
            // --- Partial overlap: fallback to rasterization (supersampled) ---
            Vector2 min = roiClosed[0], max = roiClosed[0];
            for (int i = 1; i < roiClosed.Count; i++)
            {
                min = Vector2.Min(min, roiClosed[i]);
                max = Vector2.Max(max, roiClosed[i]);
            }

            int ss = Mathf.Max(1, rasterSupersampling);
            float mmPerPxEff = mmPerPixel / ss;

            int widthPx = Mathf.CeilToInt((max.x - min.x) * 1000f / mmPerPxEff);
            int heightPx = Mathf.CeilToInt((max.y - min.y) * 1000f / mmPerPxEff);

            if (widthPx <= 0 || heightPx <= 0)
            {
                Debug.LogWarning("Invalid ROI size.");
                return;
            }

            bool[,] roiMask = PolygonUtils.RasterizePolygon(roiClosed, min, mmPerPxEff, widthPx, heightPx);
            bool[,] drawMask = PolygonUtils.RasterizePolygon(strokeClosed, min, mmPerPxEff, widthPx, heightPx);

            int overlapCount = 0;
            for (int y = 0; y < heightPx; y++)
                for (int x = 0; x < widthPx; x++)
                    if (roiMask[x, y] && drawMask[x, y]) overlapCount++;

            Y = overlapCount * (mmPerPxEff * mmPerPxEff); // mm²
        }

        // --- Final clamps to guarantee invariants ---
        // Y cannot exceed either ROI or stroke area due to geometry
        Y = Mathf.Clamp(Y, 0f, Mathf.Min(Z, strokeAreaMM2));

        float X = Mathf.Max(0f, strokeAreaMM2 - Y); // non-overlap (outside ROI)
        float denomDraw = X + Y;

        float A = denomDraw > 0f ? (X / denomDraw) * 100f : 0f;  // % of drawing outside ROI
        float B = denomDraw > 0f ? (Y / denomDraw) * 100f : 0f;  // % of drawing inside ROI
        float C = Z > 0f ? (Y / Z) * 100f : 0f;                  // % of ROI covered

        string displayText =
     $"non-overlap: {X:F1} mm² ({A:F1}%)\n" +
     $"overlap: {Y:F1} mm² ({B:F1}%)\n" +
     $"ROI: {Z:F1} mm² ({C:F1}%)";

        if (roitestText) roitestText.text = displayText;
        if (roicontrolText) roicontrolText.text = displayText;
        ShowCalculationPath();

    }

    public void CalculateCoverage2()
    {
        if (roiSource2 == null || strokePointsXY.Count < 3)
        {
            Debug.LogWarning("ROI or drawing not ready.");
            return;
        }

        // --- Build polygons in XY ---
        List<Vector3> roiWorld = roiSource2.GenerateClosedCatmullRomSplinePoints();
        var roiXY = new List<Vector2>(roiWorld.Count);
        foreach (var p in roiWorld) roiXY.Add(new Vector2(p.x, p.y));

        var roiClosed = new List<Vector2>(roiXY);
        var strokeClosed = new List<Vector2>(strokePointsXY);

        // Ensure closed loops (unit: meters; 0.0001 ~ 0.1 mm; closeThresholdMM is mm)
        PolygonUtils.EnsureClosed(roiClosed, 0.0001f);
        PolygonUtils.EnsureClosed(strokeClosed, closeThresholdMM / 1000f);

        if (roiClosed.Count < 3 || strokeClosed.Count < 3)
        {
            Debug.LogWarning("Invalid polygon(s).");
            return;
        }

        // --- Areas (mm²) ---
        float Z = roiSource2.CalculateAreaInSquareMillimeters();                     // ROI area (mm²)
        float strokeAreaMM2 = PolygonUtils.PolygonArea(strokeClosed) * 1_000_000f;   // stroke area (mm²)

        // --- Fast containment tests to enforce invariants before rasterizing ---
        // centroid helpers
        Vector2 CentroidAvg(List<Vector2> poly)
        {
            // average-of-vertices centroid is sufficient for our containment tests
            Vector2 c = Vector2.zero;
            for (int i = 0; i < poly.Count; i++) c += poly[i];
            return c / Mathf.Max(1, poly.Count);
        }

        bool MostPointsInside(List<Vector2> query, List<Vector2> container, float passRatio = 0.98f)
        {
            int inside = 0;
            for (int i = 0; i < query.Count; i++)
            {
                if (NoseHighlighter.IsPointInPolygon(query[i], container)) inside++;
            }
            return inside >= Mathf.CeilToInt(passRatio * query.Count);
        }

        Vector2 roiCentroid = CentroidAvg(roiClosed);
        Vector2 strokeCentroid = CentroidAvg(strokeClosed);

        bool roiCentroidInStroke = NoseHighlighter.IsPointInPolygon(roiCentroid, strokeClosed);
        bool strokeCentroidInRoi = NoseHighlighter.IsPointInPolygon(strokeCentroid, roiClosed);
        bool roiMostlyInStroke = MostPointsInside(roiClosed, strokeClosed); // ROI enclosed by stroke
        bool strokeMostlyInRoi = MostPointsInside(strokeClosed, roiClosed);   // stroke enclosed by ROI

        float Y; // overlap mm²

        // Case 1: Drawing fully covers ROI -> overlap must equal ROI area
        if (roiCentroidInStroke && roiMostlyInStroke)
        {
            Y = Z; // fully covered
        }
        // Case 2: Drawing fully inside ROI -> overlap equals stroke area, non-overlap is zero
        else if (strokeCentroidInRoi && strokeMostlyInRoi)
        {
            Y = strokeAreaMM2;
        }
        else
        {
            // --- Partial overlap: fallback to rasterization (supersampled) ---
            Vector2 min = roiClosed[0], max = roiClosed[0];
            for (int i = 1; i < roiClosed.Count; i++)
            {
                min = Vector2.Min(min, roiClosed[i]);
                max = Vector2.Max(max, roiClosed[i]);
            }

            int ss = Mathf.Max(1, rasterSupersampling);
            float mmPerPxEff = mmPerPixel / ss;

            int widthPx = Mathf.CeilToInt((max.x - min.x) * 1000f / mmPerPxEff);
            int heightPx = Mathf.CeilToInt((max.y - min.y) * 1000f / mmPerPxEff);

            if (widthPx <= 0 || heightPx <= 0)
            {
                Debug.LogWarning("Invalid ROI size.");
                return;
            }

            bool[,] roiMask = PolygonUtils.RasterizePolygon(roiClosed, min, mmPerPxEff, widthPx, heightPx);
            bool[,] drawMask = PolygonUtils.RasterizePolygon(strokeClosed, min, mmPerPxEff, widthPx, heightPx);

            int overlapCount = 0;
            for (int y = 0; y < heightPx; y++)
                for (int x = 0; x < widthPx; x++)
                    if (roiMask[x, y] && drawMask[x, y]) overlapCount++;

            Y = overlapCount * (mmPerPxEff * mmPerPxEff); // mm²
        }

        // --- Final clamps to guarantee invariants ---
        // Y cannot exceed either ROI or stroke area due to geometry
        Y = Mathf.Clamp(Y, 0f, Mathf.Min(Z, strokeAreaMM2));

        float X = Mathf.Max(0f, strokeAreaMM2 - Y); // non-overlap (outside ROI)
        float denomDraw = X + Y;

        float A = denomDraw > 0f ? (X / denomDraw) * 100f : 0f;  // % of drawing outside ROI
        float B = denomDraw > 0f ? (Y / denomDraw) * 100f : 0f;  // % of drawing inside ROI
        float C = Z > 0f ? (Y / Z) * 100f : 0f;                  // % of ROI covered

        string displayText =
     $"non-overlap: {X:F1} mm² ({A:F1}%)\n" +
     $"overlap: {Y:F1} mm² ({B:F1}%)\n" +
     $"ROI: {Z:F1} mm² ({C:F1}%)";

        if (roitestText) roitestText.text = displayText;
        if (roicontrolText) roicontrolText.text = displayText;
        ShowCalculationPath();
    }

    public void CalculateCoverage3()
    {
        if (roiSource3 == null || strokePointsXY.Count < 3)
        {
            Debug.LogWarning("ROI or drawing not ready.");
            return;
        }

        // --- Build polygons in XY ---
        List<Vector3> roiWorld = roiSource3.GenerateClosedCatmullRomSplinePoints();
        var roiXY = new List<Vector2>(roiWorld.Count);
        foreach (var p in roiWorld) roiXY.Add(new Vector2(p.x, p.y));

        var roiClosed = new List<Vector2>(roiXY);
        var strokeClosed = new List<Vector2>(strokePointsXY);

        // Ensure closed loops (unit: meters; 0.0001 ~ 0.1 mm; closeThresholdMM is mm)
        PolygonUtils.EnsureClosed(roiClosed, 0.0001f);
        PolygonUtils.EnsureClosed(strokeClosed, closeThresholdMM / 1000f);

        if (roiClosed.Count < 3 || strokeClosed.Count < 3)
        {
            Debug.LogWarning("Invalid polygon(s).");
            return;
        }

        // --- Areas (mm²) ---
        float Z = roiSource3.CalculateAreaInSquareMillimeters();                     // ROI area (mm²)
        float strokeAreaMM2 = PolygonUtils.PolygonArea(strokeClosed) * 1_000_000f;   // stroke area (mm²)

        // --- Fast containment tests to enforce invariants before rasterizing ---
        // centroid helpers
        Vector2 CentroidAvg(List<Vector2> poly)
        {
            // average-of-vertices centroid is sufficient for our containment tests
            Vector2 c = Vector2.zero;
            for (int i = 0; i < poly.Count; i++) c += poly[i];
            return c / Mathf.Max(1, poly.Count);
        }

        bool MostPointsInside(List<Vector2> query, List<Vector2> container, float passRatio = 0.98f)
        {
            int inside = 0;
            for (int i = 0; i < query.Count; i++)
            {
                if (NoseHighlighter.IsPointInPolygon(query[i], container)) inside++;
            }
            return inside >= Mathf.CeilToInt(passRatio * query.Count);
        }

        Vector2 roiCentroid = CentroidAvg(roiClosed);
        Vector2 strokeCentroid = CentroidAvg(strokeClosed);

        bool roiCentroidInStroke = NoseHighlighter.IsPointInPolygon(roiCentroid, strokeClosed);
        bool strokeCentroidInRoi = NoseHighlighter.IsPointInPolygon(strokeCentroid, roiClosed);
        bool roiMostlyInStroke = MostPointsInside(roiClosed, strokeClosed); // ROI enclosed by stroke
        bool strokeMostlyInRoi = MostPointsInside(strokeClosed, roiClosed);   // stroke enclosed by ROI

        float Y; // overlap mm²

        // Case 1: Drawing fully covers ROI -> overlap must equal ROI area
        if (roiCentroidInStroke && roiMostlyInStroke)
        {
            Y = Z; // fully covered
        }
        // Case 2: Drawing fully inside ROI -> overlap equals stroke area, non-overlap is zero
        else if (strokeCentroidInRoi && strokeMostlyInRoi)
        {
            Y = strokeAreaMM2;
        }
        else
        {
            // --- Partial overlap: fallback to rasterization (supersampled) ---
            Vector2 min = roiClosed[0], max = roiClosed[0];
            for (int i = 1; i < roiClosed.Count; i++)
            {
                min = Vector2.Min(min, roiClosed[i]);
                max = Vector2.Max(max, roiClosed[i]);
            }

            int ss = Mathf.Max(1, rasterSupersampling);
            float mmPerPxEff = mmPerPixel / ss;

            int widthPx = Mathf.CeilToInt((max.x - min.x) * 1000f / mmPerPxEff);
            int heightPx = Mathf.CeilToInt((max.y - min.y) * 1000f / mmPerPxEff);

            if (widthPx <= 0 || heightPx <= 0)
            {
                Debug.LogWarning("Invalid ROI size.");
                return;
            }

            bool[,] roiMask = PolygonUtils.RasterizePolygon(roiClosed, min, mmPerPxEff, widthPx, heightPx);
            bool[,] drawMask = PolygonUtils.RasterizePolygon(strokeClosed, min, mmPerPxEff, widthPx, heightPx);

            int overlapCount = 0;
            for (int y = 0; y < heightPx; y++)
                for (int x = 0; x < widthPx; x++)
                    if (roiMask[x, y] && drawMask[x, y]) overlapCount++;

            Y = overlapCount * (mmPerPxEff * mmPerPxEff); // mm²
        }

        // --- Final clamps to guarantee invariants ---
        // Y cannot exceed either ROI or stroke area due to geometry
        Y = Mathf.Clamp(Y, 0f, Mathf.Min(Z, strokeAreaMM2));

        float X = Mathf.Max(0f, strokeAreaMM2 - Y); // non-overlap (outside ROI)
        float denomDraw = X + Y;

        float A = denomDraw > 0f ? (X / denomDraw) * 100f : 0f;  // % of drawing outside ROI
        float B = denomDraw > 0f ? (Y / denomDraw) * 100f : 0f;  // % of drawing inside ROI
        float C = Z > 0f ? (Y / Z) * 100f : 0f;                  // % of ROI covered

        string displayText =
     $"non-overlap: {X:F1} mm² ({A:F1}%)\n" +
     $"overlap: {Y:F1} mm² ({B:F1}%)\n" +
     $"ROI: {Z:F1} mm² ({C:F1}%)";

        if (roitestText) roitestText.text = displayText;
        if (roicontrolText) roicontrolText.text = displayText;

        ShowCalculationPath();
    }

    // ===== RESET: always clear stroke + UI, then calc-line buffers =====
    public void ResetDrawingData()
    {
        // clear area stroke
        strokePointsXY.Clear();
        if (roitestText) roitestText.text = " ";
        if (roicontrolText) roicontrolText.text = " ";

        // clear calc path buffers
        _strokeWorld.Clear();
        _strokeNormals.Clear();

        if (_calcLine)
        {
            _calcLine.positionCount = 0;
            _calcLine.enabled = false;
        }
        if (_startMarkerInstance) { Destroy(_startMarkerInstance); _startMarkerInstance = null; }
        if (_endMarkerInstance) { Destroy(_endMarkerInstance); _endMarkerInstance = null; }
        // gates
        firstSpawn = true;
        hasLastHit = false;
    }


    // ===== HELPER: decide line width in meters based on your chosen source =====
    private float DetermineCalcLineWidthMeters()
    {
        float widthM = Mathf.Max(0.0002f, manualStrokeWidthMM / 1000f); // default to ManualMM

        switch (lineWidthSource)
        {
            case BrushWidthSource.ManualMM:
                // already set above
                break;

            case BrushWidthSource.TipCollider:
                {
                    GameObject activeTipObj = switchRayScript.leftRay ? leftIndexTipObj : rightIndexTipObj;
                    if (activeTipObj)
                    {
                        var tipCol = activeTipObj.GetComponent<SphereCollider>();
                        if (tipCol)
                        {
                            float scale = Mathf.Max(activeTipObj.transform.lossyScale.x,
                                                    activeTipObj.transform.lossyScale.y,
                                                    activeTipObj.transform.lossyScale.z);
                            float tipWorldRadius = tipCol.radius * scale;
                            widthM = Mathf.Max(0.0002f, tipWorldRadius * 2f);
                        }
                    }
                    break;
                }

            case BrushWidthSource.LatestAnnotation:
                {
                    if (LatestAnnotation)
                    {
                        var r = LatestAnnotation.GetComponent<Renderer>();
                        if (r)
                        {
                            // Use the max footprint axis as a simple diameter proxy
                            var size = r.bounds.size;
                            widthM = Mathf.Max(0.0002f, Mathf.Max(size.x, size.y));
                        }
                    }
                    break;
                }
        }

        return widthM;
    }


    // ===== ENSURE the line exists; material/color set; width will be set per-show =====
    private void EnsureCalcLine()
    {
        if (_calcLine) return;

        var go = new GameObject("CalculationPath");
        go.transform.SetParent(transform, false);
        _calcLine = go.AddComponent<LineRenderer>();
        _calcLine.useWorldSpace = true;
        _calcLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _calcLine.receiveShadows = false;
        _calcLine.numCornerVertices = 2;
        _calcLine.numCapVertices = 2;
        _calcLine.material = calcLineMaterial ? calcLineMaterial : new Material(Shader.Find("Sprites/Default"));
        _calcLine.startColor = _calcLine.endColor = calcLineColor;
        _calcLine.textureMode = LineTextureMode.Stretch;
        _calcLine.enabled = false;
    }



    private void UpdateRayPosition()
    {
        XRHand activeHandState;
        if (switchRayScript.rightRay || (!switchRayScript.leftRay && !switchRayScript.rightRay)) // Default to right hand
        {
            activeHandState = handSubsystem.rightHand;
        }
        else if (switchRayScript.leftRay)
        {
            activeHandState = handSubsystem.leftHand;
        }
        else
        {
            return;
        }

        if (activeHandState == null) return;

        // Get relevant joints
        XRHandJoint indexTip = activeHandState.GetJoint(XRHandJointID.IndexTip);
        XRHandJoint indexBase = activeHandState.GetJoint(XRHandJointID.IndexProximal);

        Pose indexTipPose, indexBasePose;

        if (!indexTip.TryGetPose(out indexTipPose) || !indexBase.TryGetPose(out indexBasePose))
        {
            return;
        }

        // ✅ Ray origin is the index finger tip
        Vector3 rayOrigin = indexTipPose.position;

        // ✅ Ray direction is from base to tip (how your finger points)
        Vector3 rayDirection = (indexTipPose.position - indexBasePose.position).normalized + directionOffset;
        rayDirection.Normalize();

        Vector3 endPoint = rayOrigin + rayDirection * rayLength;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength, detectionLayer))
        {
            if (IsDetectable(hit.collider.gameObject))
            {
                endPoint = hit.point;
            }
        }

        // ✅ Draw the finger-aimed ray
        DrawRay(rayOrigin, endPoint);
    }
    // ===== SHOW: connect 1->2->...->N->1; width from brush; offset above annotations =====
    public void ShowCalculationPath()
    {
        if (!showCalcLineOnCalculate || _strokeWorld.Count < 3) return;
        EnsureCalcLine();

        // Width (meters) chosen by your selected source
        float widthM = DetermineCalcLineWidthMeters();
        _calcLine.widthMultiplier = widthM;

        // Offset: either half the width + extra (in mm) or fixed fallback
        float effectiveOffset = autoOffsetFromWidth
            ? (widthM * 0.5f + (extraOffsetMM / 1000f))
            : calcLineOffset;

        int n = _strokeWorld.Count;
        _calcLine.positionCount = n + 1;

        for (int i = 0; i < n; i++)
            _calcLine.SetPosition(i, _strokeWorld[i] + _strokeNormals[i] * effectiveOffset);

        // close the loop (N -> 1)
        _calcLine.SetPosition(n, _strokeWorld[0] + _strokeNormals[0] * effectiveOffset);

        _calcLine.startColor = _calcLine.endColor = calcLineColor;
        _calcLine.enabled = true;

        // --- Start/End markers ---
        // positions lifted to match the line
        Vector3 p0 = _strokeWorld[0] + _strokeNormals[0] * effectiveOffset;
        Vector3 pLast = _strokeWorld[n - 1] + _strokeNormals[n - 1] * effectiveOffset;

        // tangents for orientation (forward along the path), with normals as up
        Vector3 tStart = (n >= 2) ? (_strokeWorld[1] - _strokeWorld[0]).normalized : Vector3.forward;
        Vector3 tEnd = (n >= 2) ? (_strokeWorld[n - 1] - _strokeWorld[n - 2]).normalized : Vector3.forward;

        Quaternion rStart = Quaternion.LookRotation(tStart, _strokeNormals[0]);
        Quaternion rEnd = Quaternion.LookRotation(tEnd, _strokeNormals[n - 1]);

        // instantiate or move
        var endPrefab = endMarkerPrefab ? endMarkerPrefab : startMarkerPrefab;

        if (startMarkerPrefab)
        {
            if (_startMarkerInstance == null)
                _startMarkerInstance = Instantiate(startMarkerPrefab, p0, rStart, _calcLine.transform);
            else
            {
                _startMarkerInstance.transform.SetPositionAndRotation(p0, rStart);
                _startMarkerInstance.transform.SetParent(_calcLine.transform, worldPositionStays: false);
            }
        }

        if (endPrefab)
        {
            if (_endMarkerInstance == null)
                _endMarkerInstance = Instantiate(endPrefab, pLast, rEnd, _calcLine.transform);
            else
            {
                _endMarkerInstance.transform.SetPositionAndRotation(pLast, rEnd);
                _endMarkerInstance.transform.SetParent(_calcLine.transform, worldPositionStays: false);
            }
        }
    }


    // ===== SAMPLE appender (called when a stamp happens, already wired above) =====
    private void CalcLine_AddSample(Vector3 surfacePoint, Vector3 surfaceNormal)
    {
        EnsureCalcLine();
        _strokeWorld.Add(surfacePoint);
        _strokeNormals.Add(surfaceNormal);
    }


    void DrawRay(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    bool IsDetectable(GameObject obj)
    {
        foreach (GameObject detectable in objects)
        {
            if (obj == detectable) return true;
        }
        return false;
    }

    public IEnumerator stop()
    {
        isDrawing = false;

        yield return new WaitForSeconds(1.5f);
        StartDrawing();
    }
    public void ActivateObject(int index)
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


    public void StartDrawing()
    {
        ResetDrawingData();
        if (selectedObject != null)
        {
           
            isDrawing = true;
            drawTimer = 10000000f; // Reset timer when starting
            colorBar.gameObject.SetActive(true);
            firstSpawn = true;
           
        }
        startDrawing.gameObject.SetActive(false);
        stopDrawing.gameObject.SetActive(true);

    }

    public void StopDrawing()
    {
        startDrawing.gameObject.SetActive(true);
        stopDrawing.gameObject.SetActive(false);
        isDrawing = false;
     
        lineRenderer.enabled = false;
        colorBar.gameObject.SetActive(false);
        if (switchRayScript.rightRay)
        {

            RHand.SetActive(true);
        }
        else if (switchRayScript.leftRay)
        {
            LHand.SetActive(true);
        }
    }


    void GenerateGradientTexture()
    {
        if (gradientColors.Length < 2)
            return;

        int width = 1;
        int height = 1024;

        gradientTexture = new Texture2D(width, height, TextureFormat.RGBA32, true);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        gradientTexture.filterMode = FilterMode.Trilinear;

        for (int i = 0; i < height; i++)
        {
            float t = (float)i / (height - 1);
            float scaledValue = t * (gradientColors.Length - 1);
            int index = Mathf.FloorToInt(scaledValue);
            float lerpFactor = scaledValue - index;

            Color color = Color.Lerp(gradientColors[index],
                                     gradientColors[Mathf.Clamp(index + 1, 0, gradientColors.Length - 1)],
                                     lerpFactor);

            for (int x = 0; x < width; x++)
            {
                gradientTexture.SetPixel(x, i, color);
            }
        }


        gradientTexture.Apply();
        gradientImage.texture = gradientTexture;
        gradientImage.color = Color.white;
    }

    void UpdateSelectedColor(float sliderValue)
    {
        float scaledValue = sliderValue * (gradientColors.Length - 1);
        int index = Mathf.FloorToInt(scaledValue);
        float lerpFactor = scaledValue - index;

        selectedColor = Color.Lerp(gradientColors[index],
                                   gradientColors[Mathf.Clamp(index + 1, 0, gradientColors.Length - 1)],
                                   lerpFactor);

        isManualSelection = true;

        Debug.Log("Selected Color: " + selectedColor);
    }




}


