/****************************************************************************
* Copyright 2024 XREAL Technology Limited. All rights reserved.
*****************************************************************************/

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.XR.XREAL;

namespace Unity.XR.XREAL.Samples
{
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NativeGalleryDataProvider;
#else
    using GalleryDataProvider = MockGalleryDataProvider;
#endif
    public class AnnotationCapture : MonoBehaviour
    {
        private XREALPhotoCapture m_PhotoCaptureObject;
        private Resolution m_CameraResolution;
        private bool isOnPhotoProcess = false;
        private GalleryDataProvider galleryDataTool;

        public Camera fixedCaptureCamera;

        void Create(Action<XREALPhotoCapture> onCreated)
        {
            if (m_PhotoCaptureObject != null)
            {
                Debug.Log("[AnnotationCapture] PhotoCapture already created.");
                return;
            }

            XREALPhotoCapture.CreateAsync(false, (captureObject) =>
            {
                m_CameraResolution = XREALPhotoCapture.SupportedResolutions
                    .OrderByDescending(res => res.width * res.height).First();

                if (captureObject == null)
                {
                    Debug.LogError("[AnnotationCapture] Failed to create PhotoCapture.");
                    return;
                }

                m_PhotoCaptureObject = captureObject;

                CameraParameters cameraParameters = new CameraParameters
                {
                    cameraResolutionWidth = m_CameraResolution.width,
                    cameraResolutionHeight = m_CameraResolution.height,
                    pixelFormat = CapturePixelFormat.PNG,
                    frameRate = NativeConstants.RECORD_FPS_DEFAULT,
                    blendMode = BlendMode.Blend,
                    cameraType = CameraType.RGB,
                    audioState = AudioState.None,
                    hologramOpacity = 0.0f,
                    captureSide = CaptureSide.Single,
                    backgroundColor = Color.black
                };

                m_PhotoCaptureObject.StartPhotoModeAsync(cameraParameters, (result) =>
                {
                    if (result.success)
                    {
                        onCreated?.Invoke(m_PhotoCaptureObject);
                    }
                    else
                    {
                        isOnPhotoProcess = false;
                        Close();
                        Debug.LogError("[AnnotationCapture] Start PhotoMode failed: " + result.resultType);
                    }
                }, true);
            });
        }

        public void TakeAPhoto()
        {
            if (isOnPhotoProcess)
            {
                Debug.LogWarning("[AnnotationCapture] Already processing a photo.");
                return;
            }

            isOnPhotoProcess = true;

            if (m_PhotoCaptureObject == null)
            {
                Create((capture) => CaptureFromFixedCamera());
            }
            else
            {
                CaptureFromFixedCamera();
            }
        }

        void CaptureFromFixedCamera()
        {
            if (fixedCaptureCamera == null)
            {
                Debug.LogError("[AnnotationCapture] Fixed camera not assigned.");
                return;
            }

            RenderTexture renderTex = new RenderTexture(m_CameraResolution.width, m_CameraResolution.height, 24);
            fixedCaptureCamera.targetTexture = renderTex;
            fixedCaptureCamera.Render();

            RenderTexture.active = renderTex;
            Texture2D targetTexture = new Texture2D(m_CameraResolution.width, m_CameraResolution.height, TextureFormat.RGB24, false);
            targetTexture.ReadPixels(new Rect(0, 0, m_CameraResolution.width, m_CameraResolution.height), 0, 0);
            targetTexture.Apply();

            fixedCaptureCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTex);

            SaveTextureAsPNG(targetTexture);
            SaveTextureToGallery(targetTexture);

            Close();
        }

        void SaveTextureAsPNG(Texture2D texture)
        {
            try
            {
                string filename = $"Xreal_Shot_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";
                string path = Path.Combine(Application.persistentDataPath, "XrealShots");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filePath = Path.Combine(path, filename);
                byte[] bytes = texture.EncodeToPNG();
                Debug.Log($"[AnnotationCapture] Saved {bytes.Length / 1024} KB to {filePath}");

                File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnnotationCapture] Save PNG failed: {e.Message}");
                throw;
            }
        }

        void SaveTextureToGallery(Texture2D texture)
        {
            try
            {
                string filename = $"Xreal_Shot_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";
                byte[] bytes = texture.EncodeToPNG();
                Debug.Log($"[AnnotationCapture] Gallery Save: {bytes.Length / 1024} KB as {filename}");

                if (galleryDataTool == null)
                {
                    galleryDataTool = new GalleryDataProvider();
                }

                galleryDataTool.InsertImage(bytes, filename, "Screenshots");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnnotationCapture] Gallery save failed: {e.Message}");
                throw;
            }
        }

        void Close()
        {
            if (m_PhotoCaptureObject == null)
            {
                Debug.LogWarning("[AnnotationCapture] PhotoCapture is null on close.");
                return;
            }

            m_PhotoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }

        void OnStoppedPhotoMode(XREALPhotoCapture.PhotoCaptureResult result)
        {
            m_PhotoCaptureObject?.Dispose();
            m_PhotoCaptureObject = null;
            isOnPhotoProcess = false;
        }

        void OnDestroy()
        {
            m_PhotoCaptureObject?.Dispose();
            m_PhotoCaptureObject = null;
        }
    }
}
