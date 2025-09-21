using UnityEngine;

public class AnnotationFollow : MonoBehaviour
{
    private GameObject targetObject;
    private Vector3 localOffset;
    private Quaternion localRotationOffset;

    public void AttachToObject(GameObject target, Vector3 worldPosition, Quaternion worldRotation)
    {
        targetObject = target;
        localOffset = target.transform.InverseTransformPoint(worldPosition);
        localRotationOffset = Quaternion.Inverse(target.transform.rotation) * worldRotation;
    }

    private void LateUpdate()
    {
        if (targetObject == null || !targetObject.activeInHierarchy)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }

        transform.position = targetObject.transform.TransformPoint(localOffset);
        transform.rotation = targetObject.transform.rotation * localRotationOffset;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}
