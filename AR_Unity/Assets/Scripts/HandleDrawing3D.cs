
using System.Collections;
using System.Collections.Generic; // XREAL SDK for hand tracking
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

public class HandleDrawing3D : MonoBehaviour
{
   
    public Transform colorBar;
    public float drawDistance = 0.5f;
    public float minDistanceBetweenPoints = 0.02f;
    public float drawDuration = 60f;
    private bool isDrawing = false;
    private bool wasPinching = false;
    private float drawTimer;
    private Vector3 lastDrawPosition;

    public ObjectPool objectPool;  // Assign via Inspecto
    private List<GameObject> activeObjects = new List<GameObject>(); // Track active objects

    [SerializeField] private Button startDrawing;
    [SerializeField] private Button stopDrawing;
    public Quaternion customRotation = Quaternion.identity; // Default to no rotation
    public Vector3 rotationOffset = Vector3.zero; // Default to no offset

    // Hue Gradient Color Picker
    public Slider colorSlider;
    public RawImage gradientImage;
    public HandRaySwitcher switchRayScript;
    private Texture2D gradientTexture;

    private Color[] gradientColors = new Color[]
  {
    new Color(0.0f, 0.0f, 0.545f, 0.5f), // Dark Blue
    new Color(0.0f, 0.0f, 0.8f, 0.5f),   // Medium Blue
    new Color(0.0f, 0.0f, 1.0f, 0.5f),   // Standard Blue
    new Color(0.0f, 0.5f, 1.0f, 0.5f),   // Deep Sky Blue
    new Color(0.3f, 0.6f, 1.0f, 0.5f),   // Soft Blue
    new Color(0.678f, 0.847f, 0.902f, 0.5f), // Light Sky Blue
    new Color(0.0f, 1.0f, 0.0f, 0.5f),   // Green
    new Color(0.0f, 0.8f, 0.0f, 0.5f),   // Bright Green
    new Color(0.6f, 1.0f, 0.6f, 0.5f),   // Light Green
    new Color(1.0f, 1.0f, 0.0f, 0.5f),   // Yellow
    new Color(1.0f, 0.84f, 0.0f, 0.5f),  // Golden Yellow
    new Color(1.0f, 0.647f, 0.0f, 0.5f), // Orange
    new Color(1.0f, 0.3f, 0.3f, 0.5f),   // Light Red
    new Color(1.0f, 0.0f, 0.0f, 0.5f),   // Red
    new Color(0.8f, 0.0f, 0.0f, 0.5f),   // Deep Red
    new Color(0.545f, 0.0f, 0.0f, 0.5f)  // Dark Red
  };
    private XRHandSubsystem handSubsystem;

    private Color selectedColor;

    void Start()
    {
        DebugDisplay.Instance?.Log("Start");

        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
            handSubsystem.Start();
            Debug.Log("XRHandSubsystem started in HandleDrawing3D");
        }
        else
        {
            Debug.LogError("No XRHandSubsystem found in HandleDrawing3D");
        }

       
        GenerateGradientTexture();
        if (gradientImage != null)
            gradientImage.texture = gradientTexture;

        if (colorSlider != null)
            colorSlider.onValueChanged.AddListener(UpdateSelectedColor);

        startDrawing.onClick.AddListener(StartDrawing);
        stopDrawing.onClick.AddListener(StopDrawing);
        drawTimer = drawDuration;
        colorBar.gameObject.SetActive(false);
    }

    private int colorIndex = 0;  // Index to track the color sequence

    void Update()
    {
        if (isDrawing)
        {
            if (isDrawing)
            {
                XRHand activeHand;

                if (switchRayScript.leftRay)
                {
                    activeHand = handSubsystem.leftHand;
                }
                else
                {
                    activeHand = handSubsystem.rightHand; // Default to right hand
                }

                if (activeHand == null || !activeHand.isTracked) return;

                // Get joint poses
                XRHandJoint thumbTip = activeHand.GetJoint(XRHandJointID.ThumbTip);
                XRHandJoint indexTip = activeHand.GetJoint(XRHandJointID.IndexTip);

                Pose thumbPose, indexPose;

                if (!thumbTip.TryGetPose(out thumbPose) || !indexTip.TryGetPose(out indexPose))
                    return;

                // Calculate pinch distance and pinch state
                float pinchDistance = Vector3.Distance(thumbPose.position, indexPose.position);
                bool isPinching = pinchDistance < 0.025f;

                // Midpoint between thumb and index = pinch position
                Vector3 pinchPosition = (thumbPose.position + indexPose.position) / 2f;

                if (isPinching && !wasPinching)
                {
                    lastDrawPosition = pinchPosition;
                }
                else if (isPinching && wasPinching)
                {
                    if (Vector3.Distance(lastDrawPosition, pinchPosition) > minDistanceBetweenPoints)
                    {

                      
                        DrawInSpace(pinchPosition); // Instantiate prefab at pinch position
                        Debug.Log("pinching at position: " + pinchPosition);
                        lastDrawPosition = pinchPosition;
                    }
                }

                wasPinching = isPinching;
            }

        }
    }

    private void DrawInSpace(Vector3 pinchPosition)
    {
       // if (IsPointerOverUI()) return;

       
        Color currentColor = gradientColors[colorIndex];

        Quaternion annotationRotation = customRotation * Quaternion.Euler(rotationOffset);
        GameObject annotation = objectPool.GetObject(pinchPosition, annotationRotation);

        Debug.Log("Drawing at position: " + pinchPosition);


        // Apply color
        annotation.GetComponent<Renderer>().material.color = currentColor;

        if (objectPool == null)
        {
           
            return;
        }

        // If we exceed max objects, return the oldest one to the pool
        if (objectPool.activeObjects.Count > objectPool.poolSize)
        {
            GameObject oldestObject = objectPool.activeObjects[0]; // First object is the oldest
            objectPool.activeObjects.RemoveAt(0);
            objectPool.ReturnObject(oldestObject);
           

        }

        // Cycle through colors
        colorIndex = (colorIndex + 1) % gradientColors.Length;
    }




    public void StartDrawing()
    {
        isDrawing = true;
        drawTimer = drawDuration;
        colorBar.gameObject.SetActive(true);
        startDrawing.gameObject.SetActive(false);
        stopDrawing.gameObject.SetActive(true);
    }

    public void StopDrawing()
    {
        isDrawing = false;
        colorBar.gameObject.SetActive(false);
        startDrawing.gameObject.SetActive(true);
        stopDrawing.gameObject.SetActive(false);
    }

   
    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void GenerateGradientTexture()
    {
        if (gradientColors.Length < 2)
            return;

        int width = 1;
        int height = 1024;  // Full range of steps

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

        Debug.Log("Selected Color: " + selectedColor);
    }
}
