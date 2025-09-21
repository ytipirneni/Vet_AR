using UnityEngine;

public class ClearAnnotationPoint : MonoBehaviour
{
    public ObjectPool objectPool; // Reference the pool
    public ObjectPooling objectPooling; // Reference the pool

    public void ClearAnnotations()
    {
        GameObject[] annotationPoints = GameObject.FindGameObjectsWithTag("AnnotationPoint");

        foreach (GameObject point in annotationPoints)
        {
            objectPool.ReturnObject(point); // Return to pool instead of destroying
        }

        // Ensure the pool has the correct number of objects
        objectPool.RefillPool();
        objectPooling.RefillPool();
    }
}

