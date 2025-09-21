using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL.Samples
{
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NativeGalleryDataProvider;
#else
    using GalleryDataProvider = MockGalleryDataProvider;
#endif

    public class PhotoCaptureExample : MonoBehaviour
    {
        public enum ResolutionLevel
        {
            High,
            Middle,
            Low,
        }

        [SerializeField] private Button m_PhotoButton;
        [SerializeField] private Dropdown m_QualityDropDown;
        [SerializeField] private Dropdown m_RenderModeDropDown;
        [SerializeField] private Dropdown m_CaptureSideDropDown;
        [SerializeField] private Toggle m_UseGreenBGToggle;

        public UnityEngine.UI.Text timerText;
        private float countdownTime = 10f; // 10-second timer
        public GameObject countdownPanel;

        List<string> _ResolutionOptions = new List<string>() {
            ResolutionLevel.High.ToString(),
            ResolutionLevel.Middle.ToString(),
            ResolutionLevel.Low.ToString()
        };
        List<string> _RendermodeOptions = new List<string>() {
            BlendMode.Blend.ToString(),
            BlendMode.CameraOnly.ToString(),
            BlendMode.VirtualOnly.ToString()
        };
        List<string> _CaptureSideOptions = new List<string>() {
            CaptureSide.Single.ToString(),
            CaptureSide.Both.ToString()
        };

        public BlendMode blendMode = BlendMode.Blend;
        public ResolutionLevel resolutionLevel = ResolutionLevel.High;
        public CaptureSide captureside = CaptureSide.Single;
        public bool useGreenBackGround = false;

        GalleryDataProvider galleryDataTool;

        private XREALPhotoCapture m_PhotoCapture;
        private Resolution m_CameraResolution;
        private bool isOnPhotoProcess = false;

        void Awake()
        {
            m_QualityDropDown.options.Clear();
            m_QualityDropDown.AddOptions(_ResolutionOptions);
            m_QualityDropDown.value = _ResolutionOptions.IndexOf(resolutionLevel.ToString());
            m_QualityDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse(_ResolutionOptions[index], out resolutionLevel);
            });

            m_RenderModeDropDown.options.Clear();
            m_RenderModeDropDown.AddOptions(_RendermodeOptions);
            m_RenderModeDropDown.value = _RendermodeOptions.IndexOf(blendMode.ToString());
            m_RenderModeDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse(_RendermodeOptions[index], out blendMode);
            });

            m_CaptureSideDropDown.options.Clear();
            m_CaptureSideDropDown.AddOptions(_CaptureSideOptions);
            m_CaptureSideDropDown.value = _CaptureSideOptions.IndexOf(captureside.ToString());
            m_CaptureSideDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse(_CaptureSideOptions[index], out captureside);
            });

            m_UseGreenBGToggle.isOn = useGreenBackGround;
            m_UseGreenBGToggle.onValueChanged.AddListener((val) => useGreenBackGround = val);

            m_PhotoButton.onClick.AddListener(TakeAPhoto);
        }

        void CreatePhotoCapture(Action<XREALPhotoCapture> onCreated)
        {
            if (m_PhotoCapture != null)
            {
                Debug.Log("[TakePicture] Already created.");
                return;
            }

            XREALPhotoCapture.CreateAsync(false, (captureObject) =>
            {
                m_CameraResolution = XREALPhotoCapture.SupportedResolutions
                    .OrderByDescending(res => res.width * res.height).First();

                if (captureObject == null)
                {
                    Debug.LogError("PhotoCapture creation failed.");
                    return;
                }

                m_PhotoCapture = captureObject;

                CameraParameters cameraParameters = new CameraParameters
                {
                    cameraType = CameraType.RGB,
                    hologramOpacity = 0.0f,
                    frameRate = NativeConstants.RECORD_FPS_DEFAULT,
                    cameraResolutionWidth = m_CameraResolution.width,
                    cameraResolutionHeight = m_CameraResolution.height,
                    pixelFormat = CapturePixelFormat.PNG,
                    blendMode = blendMode,
                    audioState = AudioState.None,
                    captureSide = captureside,
                    backgroundColor = useGreenBackGround ? Color.green : Color.black
                };

                m_PhotoCapture.StartPhotoModeAsync(cameraParameters, (result) =>
                {
                    if (result.success)
                        onCreated?.Invoke(m_PhotoCapture);
                    else
                    {
                        isOnPhotoProcess = false;
                        ClosePhotoCapture();
                        Debug.LogError("[TakePicture] Start PhotoMode failed.");
                    }
                }, true);
            });
        }
        public void CaptureScreen()
        {
            StartCoroutine(StartScreenshotTimer());
        }

        public IEnumerator StartScreenshotTimer()
        {
            float timer = countdownTime;
            while (timer > 0)
            {
                countdownPanel.SetActive(true);
                timerText.text = Mathf.Ceil(timer).ToString() + "s"; // Update UI text
                yield return new WaitForSeconds(1f);
                timer -= 1f;
                countdownPanel.SetActive(false);

            }


            TakeAPhoto();
        }
        void TakeAPhoto()
        {
            if (isOnPhotoProcess)
            {
                Debug.LogWarning("[TakePicture] Already capturing.");
                return;
            }

            isOnPhotoProcess = true;

            if (m_PhotoCapture == null)
                CreatePhotoCapture(c => c.TakePhotoAsync(OnCapturedPhotoToMemory));
            else
                m_PhotoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);
        }

        void OnCapturedPhotoToMemory(XREALPhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame frame)
        {
            if (!result.success)
            {
                Debug.LogError("[TakePicture] Capture failed.");
                ClosePhotoCapture();
                return;
            }

            var texture = new Texture2D(m_CameraResolution.width, m_CameraResolution.height);
            frame.UploadImageDataToTexture(texture);

            SaveTextureAsPNG(frame);
            SaveTextureToGallery(frame);
            ClosePhotoCapture();
        }

        void SaveTextureAsPNG(PhotoCaptureFrame frame)
        {
            if (frame.TextureData == null)
            {
                Debug.LogError("[TakePicture] TextureData is null.");
                return;
            }

            try
            {
                string filename = $"Xreal_Shot_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";
                string path = Path.Combine(Application.persistentDataPath, "XrealShots");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                File.WriteAllBytes(Path.Combine(path, filename), frame.TextureData);
                Debug.Log($"[TakePicture] Saved: {filename}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TakePicture] Save failed: {e.Message}");
            }
        }

        void SaveTextureToGallery(PhotoCaptureFrame frame)
        {
            if (frame.TextureData == null) return;

            try
            {
                string filename = $"Xreal_Shot_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";
                if (galleryDataTool == null)
                    galleryDataTool = new GalleryDataProvider();

                galleryDataTool.InsertImage(frame.TextureData, filename, "Screenshots");
                Debug.Log($"[TakePicture] Inserted to gallery: {filename}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TakePicture] Gallery insert failed: {e.Message}");
            }
        }

        void ClosePhotoCapture()
        {
            if (m_PhotoCapture == null) return;

            m_PhotoCapture.StopPhotoModeAsync((result) =>
            {
                m_PhotoCapture.Dispose();
                m_PhotoCapture = null;
                isOnPhotoProcess = false;
            });
        }

        void OnDestroy()
        {
            m_PhotoCapture?.Dispose();
            m_PhotoCapture = null;
        }
    }
}
