using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public GameObject objectPrefab;
    public int poolSize = 48;
    private Queue<GameObject> pool = new Queue<GameObject>();
    public List<GameObject> activeObjects = new List<GameObject>(); // Track active objects

    private Transform poolParent;

    void Start()
    {
        poolParent = new GameObject("PoolObjects").transform;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectPrefab, poolParent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }


    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue(); // Get from pool if available
        }
        else
        {
            obj = activeObjects[0]; // Get oldest active object
            activeObjects.RemoveAt(0);
        }

        // Activate and reposition
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        activeObjects.Add(obj); // Track active objects

        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(poolParent); // Reparenting to keep it clean
        if (!pool.Contains(obj)) pool.Enqueue(obj);
        activeObjects.Remove(obj);
    }


    public void ClearAllActiveObjects()
    {
        while (activeObjects.Count > 0)
        {
            ReturnObject(activeObjects[0]); // Return objects to pool
        }
    }

    public void RefillPool()
    {
        while (pool.Count < poolSize)
        {
            GameObject obj = Instantiate(objectPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
