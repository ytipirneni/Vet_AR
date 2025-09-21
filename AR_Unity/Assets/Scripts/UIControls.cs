using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.Hands
{

   
    public class UIControls : MonoBehaviour
    {
        [Header("RightHand Controls")]
        public GameObject touch;
       

        public GameObject pinch;
        public GameObject handRay;
        // hover 
        public GameObject rightHandHover;
        public CurveVisualController RightHandHoverScript;
        public GameObject FillImage;
        public ReleaseThresholdButtonReader touchScript;
        public CurveVisualController rayScript;
        [Header("References")]
        public GameObject setting;
        public GameObject control;
        public HandRaySwitcher handSwitchScript;
        [Header("LeftHand Controls")]
        public GameObject leftHandTouch;
        public GameObject leftHandPinch;
        public GameObject leftHandRay;
        [Header("Selected Control Text")]
        public Text pinchText;
        public Text hoverText;
        public Text touchText;
    
        // hover 
        public GameObject leftHandHover;
        public GameObject fillImage;
        public ReleaseThresholdButtonReader leftHandtouchScript;
        public CurveVisualController leftHandRayScript;
        public CurveVisualController LeftHandHoverScript;

        public Button HandswitchBTN;
        public AutoClickRayUI autoClickScript;
        public GameObject autoClick;

        [SerializeField] private RectTransform targetPanel; // Drag your panel here in Inspector

     
        public bool isPinch;
         bool isTouch;
        private bool isHover;

        private void Awake()
        {
           
           
        }
        private void Start()
        {
            HandswitchBTN.onClick.AddListener(CheckHand);
            touch.SetActive(false);
            pinch.SetActive(true);
            rightHandHover.SetActive(false);
            RightHandHoverScript.enabled = false;
            touchScript.enabled = false;
            isPinch = true;
            isTouch = false;
            isHover = false;
        }

        private void Update()
        {

            selectedControlText();


        }

        void selectedControlText()
        {
            if (isPinch)
            {
                pinchText.text = "Selected";
            }
            else
            {
                pinchText.text = "Pinch";
            }
             if (isHover)
            {
                hoverText.text = "Selected";
            }
            else
            {
                hoverText.text = "Hover";
            }
            if (isTouch)
            {
                touchText.text = "Selected";
            }
            else
            {
                touchText.text = "Touch";
            }

        }
        public void pinchControl()

        {
          
            if (handSwitchScript.rightRay == true)
            {
                if (isTouch || isHover)
                {
                    DeactivateAllWithTag("RightHandHoverEnd");
                    SetPanelTransform(new Vector3(1.826f, -0.002f, -3.8f), new Vector3(0.001172729f, 0.001172729f, 0.001172729f));
                    touch.SetActive(false);
                    pinch.SetActive(true);
                    handRay.SetActive(true);
                    rightHandHover.SetActive(false);
                    touchScript.enabled = false;
                    rayScript.enabled = true;
                    RightHandHoverScript.enabled = false;
                    isPinch = true;
                    isTouch = false;
                    isHover = false;
                    autoClickScript.enabled = false;
                    autoClick.SetActive(false);
                    FillImage.SetActive(false);
                  
                    ActivateAllWithTag("RightLaserEndPoint");
                    // panel size 
                    // Example: Move to bottom and make it smaller
                   
                  

                }
            }
            if (handSwitchScript.leftRay == true)
            {
                if (isTouch || isHover)
                {
                    DeactivateAllWithTag("HoverEnd");
                    SetPanelTransform(new Vector3(-1.826f, -0.002f, -3.8f), new Vector3(0.001172729f, 0.001172729f, 0.001172729f));
                    leftHandTouch.SetActive(false);
                    leftHandPinch.SetActive(true);
                    leftHandRay.SetActive(true);
                    leftHandHover.SetActive(false);
                    leftHandtouchScript.enabled = false;
                    leftHandRayScript.enabled = true;
                    LeftHandHoverScript.enabled = false;
                    isPinch = true;
                    isTouch = false;
                    isHover = false;
                    autoClickScript.enabled = false;
                    autoClick.SetActive(false);
                    fillImage.SetActive(false);
                    ActivateAllWithTag("LaserEnd");
                   

                   
                  
                }
            }


        }

        public void touchControl()
        {

          
            if (handSwitchScript.rightRay == true)
            {
               
                if (isPinch || isHover)
                {
                    SetPanelTransform(new Vector3(0.38f, -0.25f, -4.6f), new Vector3(0.0008514012f, 0.0008514012f, 0.0008514012f));
                 
                    touchScript.enabled = true;
                    rayScript.enabled = false;
                    touch.SetActive(true);
                    pinch.SetActive(false);
                    handRay.SetActive(false);
                    rightHandHover.SetActive(false);
                    RightHandHoverScript.enabled = false;
                    isPinch = false;
                    isTouch = true;
                    isHover = false;
                    FillImage.SetActive(false);
                    autoClickScript.enabled = false;
                    autoClick.SetActive(false);


                    DeactivateAllWithTag("RightHandHoverEnd");

                    DeactivateAllWithTag("RightLaserEndPoint");


                  
                }
            }
            if (handSwitchScript.leftRay == true)
            {
                if (isPinch || isHover)
                {
                    SetPanelTransform(new Vector3(-0.38f, -0.25f, -4.6f), new Vector3(0.0008514012f, 0.0008514012f, 0.0008514012f));
           
                    leftHandtouchScript.enabled = true;
                    leftHandRayScript.enabled = false;
                    leftHandTouch.SetActive(true);
                    leftHandPinch.SetActive(false);
                    leftHandRay.SetActive(false);
                    leftHandHover.SetActive(false);
                    LeftHandHoverScript.enabled = false;
                    isPinch = false;
                    isTouch = true;
                    isHover = false;
                    fillImage.SetActive(false);
                    autoClickScript.enabled = false;
                    autoClick.SetActive(false);

                    DeactivateAllWithTag("LaserEnd");
                    DeactivateAllWithTag("HoverEnd");

                   
                }
            }


        }

        public void hoverControl()
        {
     
            if (handSwitchScript.rightRay == true)
            {
                if (isPinch || isTouch)
                {
                    DeactivateAllWithTag("RightLaserEndPoint"); ;
                    SetPanelTransform(new Vector3(1.826f, -0.002f, -3.8f), new Vector3(0.001172729f, 0.001172729f, 0.001172729f));
                    autoClickScript.enabled = true;
                    autoClick.SetActive(true);
                    touchScript.enabled = false;
                    rayScript.enabled = false;
                    touch.SetActive(false);
                    pinch.SetActive(false);
                    handRay.SetActive(false);
                    rightHandHover.SetActive(true);
                    RightHandHoverScript.enabled = true;
                    isPinch = false;
                    isTouch = false;
                    isHover = true;
                    FillImage.SetActive(true);
                    ActivateAllWithTag("RightHandHoverEnd");
                  


                 
                   
                }
            }
            if (handSwitchScript.leftRay == true)
            {
                if (isPinch || isTouch)
                {
                    DeactivateAllWithTag("LaserEnd");
                    SetPanelTransform(new Vector3(-1.826f, -0.002f, -3.8f), new Vector3(0.001172729f, 0.001172729f, 0.001172729f));
                    autoClickScript.enabled = true;
                    autoClick.SetActive(true);
                    leftHandtouchScript.enabled = false;
                    leftHandRayScript.enabled = false;
                    leftHandTouch.SetActive(false);
                    leftHandPinch.SetActive(false);
                    leftHandRay.SetActive(false);
                    leftHandHover.SetActive(true);
                    LeftHandHoverScript.enabled = true;
                    isPinch = false;
                    isTouch = false;
                    isHover = true;                
                    fillImage.SetActive(true);
                   
                    ActivateAllWithTag(" HoverEnd");

                  
                    
                }
            }

        }

        public void CheckHand()
        {
            StartCoroutine(startControlSwitch());
        }

        IEnumerator startControlSwitch()
        {
            yield return new WaitForSeconds(0.3f);

            if (handSwitchScript.rightRay == true)
            {
                StartCoroutine(pause());
                rightHandSwitch();
                DeactivateAllWithTag("HoverEnd");
                DeactivateAllWithTag("LaserEnd");

            }
            else if (handSwitchScript.leftRay == true)
            {
                leftHandSwitch();
                DeactivateAllWithTag("RightHandHoverEnd");

                DeactivateAllWithTag("RightLaserEndPoint");
                StartCoroutine(pause());


            }

        }

        IEnumerator pause()
        {
            autoClickScript.enabled = false;
            autoClick.SetActive(false);
            yield return new WaitForSeconds(1);
            autoClickScript.enabled = true;
            autoClick.SetActive(true);
        }

        // this ensure the current control mode to the left or right hand when siwitching
        public void leftHandSwitch()
        {
           
            if (isPinch)
            {
                leftHandTouch.SetActive(false);
                leftHandPinch.SetActive(true);
                leftHandRay.SetActive(true);
                leftHandHover.SetActive(false);
                leftHandtouchScript.enabled = false;
                rayScript.enabled = true;
                LeftHandHoverScript.enabled = false;
                isPinch = true;
                isTouch = false;
                isHover = false;
                fillImage.SetActive(false);
                ActivateAllWithTag("LaserEnd");
                DeactivateAllWithTag("HoverEnd");

              
            }
            else if(isTouch)
            {
                leftHandtouchScript.enabled = true;
                leftHandRayScript.enabled = false;
                leftHandTouch.SetActive(true);
                leftHandPinch.SetActive(false);
                leftHandRay.SetActive(false);
                leftHandHover.SetActive(false);
                LeftHandHoverScript.enabled = false;
                isPinch = false;
                isTouch = true;
                isHover = false;
                fillImage.SetActive(false);

                DeactivateAllWithTag("LaserEnd");
                DeactivateAllWithTag("HoverEnd");
            }
            else if(isHover)
            {
                leftHandtouchScript.enabled = false;
                leftHandRayScript.enabled = false;
                leftHandTouch.SetActive(false);
                leftHandPinch.SetActive(false);
                leftHandRay.SetActive(false);
                leftHandHover.SetActive(true);
                LeftHandHoverScript.enabled = true;
                isPinch = false;
                isTouch = false;
                isHover = true;
                fillImage.SetActive(true);
                DeactivateAllWithTag("LaserEnd");
                ActivateAllWithTag(" HoverEnd");
               
            }
        }

        public void rightHandSwitch()
        {
          
            if (isPinch)
            {
                touch.SetActive(false);
                pinch.SetActive(true);
                handRay.SetActive(true);
                rightHandHover.SetActive(false);
                touchScript.enabled = false;
                rayScript.enabled = true;
                RightHandHoverScript.enabled = false;
                isPinch = true;
                isTouch = false;
                isHover = false;
                FillImage.SetActive(false);
                DeactivateAllWithTag("RightHandHoverEnd");

                DeactivateAllWithTag("RightLaserEndPoint");
            }
            else if (isTouch)
            {
                touchScript.enabled = true;
                rayScript.enabled = false;
                touch.SetActive(true);
                pinch.SetActive(false);
                handRay.SetActive(false);
                rightHandHover.SetActive(false);
                RightHandHoverScript.enabled = false;
                isPinch = false;
                isTouch = true;
                isHover = false;
                FillImage.SetActive(false);
                DeactivateAllWithTag("RightHandHoverEnd");

                DeactivateAllWithTag("RightLaserEndPoint");
            }
            else if (isHover)
            {
                touchScript.enabled = false;
                rayScript.enabled = false;
                touch.SetActive(false);
                pinch.SetActive(false);
                handRay.SetActive(false);
                rightHandHover.SetActive(true);
                RightHandHoverScript.enabled = true;
                isPinch = false;
                isTouch = false;
                isHover = true;
                FillImage.SetActive(true);
                DeactivateAllWithTag("RightHandHoverEnd");

                DeactivateAllWithTag("RightLaserEndPoint");
            }
        }

        public void openControls()
        {
            setting.SetActive(false);
            control.SetActive(true);
        }

        public void closeControls()
        {
            setting.SetActive(true);
            control.SetActive(false);
        }

        void DeactivateAllWithTag(string tag)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in taggedObjects)
            {
                obj.SetActive(false);
            }
        }
        void ActivateAllWithTag(string tag)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in taggedObjects)
            {
                obj.SetActive(true);
            }
        }

       

        // Method to set position & scale at runtime
        public void SetPanelTransform(Vector3 position, Vector3 scale)
        {
            if (targetPanel == null)
            {
                Debug.LogError("Target Panel reference not set!");
                return;
            }

            targetPanel.localPosition = position;
            targetPanel.localScale = new Vector3(scale.x, scale.y, scale.z);
        }

    }
}
