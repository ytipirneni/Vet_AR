using UnityEngine;
using UnityEngine.Android;

public class RequestPermissions : MonoBehaviour
{
    void Start()
    {
        RequestAllPermissions();
    }

    void RequestAllPermissions()
    {
        Debug.Log(" Checking and requesting permissions...");

        // Camera Permission
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log(" Camera permission NOT granted. Requesting...");
            Permission.RequestUserPermission(Permission.Camera);
        }
        else
        {
            Debug.Log(" Camera permission already granted.");
        }

        // Microphone Permission
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log(" Microphone permission NOT granted. Requesting...");
            Permission.RequestUserPermission(Permission.Microphone);
        }
        else
        {
            Debug.Log(" Microphone permission already granted.");
        }

        // Storage Permissions (Android 10+)
        CheckAndRequestStoragePermission("android.permission.READ_MEDIA_IMAGES", "Read Media Images");
        CheckAndRequestStoragePermission("android.permission.READ_MEDIA_VIDEO", "Read Media Video");
        CheckAndRequestStoragePermission("android.permission.READ_MEDIA_AUDIO", "Read Media Audio");
        CheckAndRequestStoragePermission("android.permission.WRITE_EXTERNAL_STORAGE", "Write External Storage");
    }

    void CheckAndRequestStoragePermission(string permission, string permissionName)
    {
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Debug.Log($" {permissionName} permission NOT granted. Requesting...");
            Permission.RequestUserPermission(permission);
        }
        else
        {
            Debug.Log($" {permissionName} permission already granted.");
        }
    }
}
