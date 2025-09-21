using UnityEngine;
using UnityEngine.UI;

public class GradientColorPicker : MonoBehaviour
{
    public Slider colorSlider; // The slider to control the gradient
    private Color[] gradientColors = new Color[]
 {
     new Color(0.0f, 0.0f, 0.545f), // Dark Blue
     Color.blue,
     new Color(0.678f, 0.847f, 0.902f), // Light Sky Blue
     Color.green,
     Color.yellow,
     new Color(1.0f, 0.647f, 0.0f),     // Orange
     Color.red,
     new Color(0.545f, 0.0f, 0.0f)      // Dark Red


 };

    public RawImage gradientImage; // The RawImage to display the gradient
    public GameObject targetObject; // The object that will change color

    private Texture2D gradientTexture;
    private Renderer targetRenderer;

    void Start()
    {
        GenerateGradientTexture();
        if (gradientImage != null)
            gradientImage.texture = gradientTexture;

        if (targetObject != null)
            targetRenderer = targetObject.GetComponent<Renderer>();

        if (colorSlider != null)
            colorSlider.onValueChanged.AddListener(UpdateColor);
    }

    void GenerateGradientTexture()
    {
        if (gradientColors.Length < 2)
            return;

        int width = 1;  // Single column for vertical gradient
        int height = 256;  // Full range of steps
        gradientTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        gradientTexture.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < height; i++)
        {
            float t = (float)i / (height - 1); // Normalize between 0 and 1
            float scaledValue = t * (gradientColors.Length - 1);
            int index = Mathf.FloorToInt(scaledValue);
            float lerpFactor = scaledValue - index;

            // Use only the colors from gradientColors array
            Color color = Color.Lerp(gradientColors[index],
                                     gradientColors[Mathf.Clamp(index + 1, 0, gradientColors.Length - 1)],
                                     lerpFactor);

            for (int x = 0; x < width; x++)  // Fill the column
            {
                gradientTexture.SetPixel(x, i, color);
            }
        }

        gradientTexture.Apply();
        gradientImage.texture = gradientTexture; // Assign texture
        gradientImage.color = Color.white; // Ensure visibility
    }




    void UpdateColor(float value)
    {
        if (gradientColors.Length < 2 || targetRenderer == null)
            return;

        // Calculate the position in the gradient
        float scaledValue = value * (gradientColors.Length - 1);
        int index = Mathf.FloorToInt(scaledValue);
        float t = scaledValue - index;

        Color selectedColor = Color.Lerp(gradientColors[index], gradientColors[Mathf.Clamp(index + 1, 0, gradientColors.Length - 1)], t);

        // Apply color to object
        targetRenderer.material.color = selectedColor;
    }
}