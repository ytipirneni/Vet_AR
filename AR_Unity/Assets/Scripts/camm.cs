using UnityEngine;

public class camm : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Awake()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
