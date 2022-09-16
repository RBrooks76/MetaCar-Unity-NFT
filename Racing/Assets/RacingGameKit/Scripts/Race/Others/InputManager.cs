using UnityEngine;
using System.Collections;

namespace RGSK
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance;
        
        public enum InputDevice { Keyboard, XboxController}

        public InputDevice inputDevice;
        public bool autoDetectInputDevice = true;

        [System.Serializable]
        public class KeyboardInput
        {
            public string verticalAxis = "Vertical";
            public string horizontalAxis = "Horizontal";
            public string handbrakeAxis = "Jump";
            public KeyCode nitroKey = KeyCode.LeftShift;           
            public KeyCode respawnKey = KeyCode.Return;
            public KeyCode pauseKey = KeyCode.Escape;

            [Header("Camera Control")]
            public KeyCode switchCamera = KeyCode.V;
            public KeyCode cameraLookLEFT = KeyCode.Q;
            public KeyCode cameraLookRIGHT = KeyCode.E;
            public KeyCode cameraLookBACK = KeyCode.C;
            public string orbitYaxis = "Mouse Y";
            public string orbitXaxis = "Mouse X";
        }

        [System.Serializable]
        public class XboxControllerInput
        {
            public string verticalAxis = "RT";
            public string negativeVerticalAxis = "LT";
            public string horizontalAxis = "LeftAnalogHorizontal";
            public string handbrakeButton = "B";
            public string nitroButton = "A";            
            public string respawnButton = "Y";
            public string pauseButton = "Start";

            [Header("Camera Control")]
            public string switchCamera = "Back";
            public string cameraLookLEFT = "LB";
            public string cameraLookRIGHT = "RB";
            public string cameraLookBACK = "RightAnalogueClick";
            public string orbitYaxis = "RightAnalogVertical";
            public string orbitXaxis = "RightAnalogHorizontal";
        }

        [System.Serializable]
        public class MobileInput
        {
            public UIButton accelerate;
            public UIButton brake;
            public UIButton handBrake;
            public UIButton nitro;
            public UIButton steerRight;
            public UIButton steerLeft;
        }

        [Header("Keyboard Input")]
        public KeyboardInput keyboardInput;

        [Header("Xbox Controller Input")]
        public XboxControllerInput xboxControllerInput;

        [Header("Mobile Input")]
        [Tooltip("These are automatically assigned by the MobileControlManager")]
        public MobileInput mobileInput;

        [Space(10)]
        public bool dontDestroyOnLoad;

        void Awake()
        {
            //Create an instance
            instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        void Update()
        {

            if (!autoDetectInputDevice) return;

            //Check for controller and keyboard presses to automatically set the input device

            if (inputDevice !=InputDevice.XboxController && DetectControllerInput())
            {
                inputDevice = InputDevice.XboxController;
                Debug.Log("Switched to Controller input");
            }

            if (inputDevice != InputDevice.Keyboard && !DetectControllerInput() && Input.anyKeyDown)
            {
                inputDevice = InputDevice.Keyboard;
                Debug.Log("Switched to Keyboard input");
            }
        }

        private bool DetectControllerInput()
        {
            // See if the player presses a controller button / joystick

            if (Input.GetKey(KeyCode.Joystick1Button0) ||
               Input.GetKey(KeyCode.Joystick1Button1) ||
               Input.GetKey(KeyCode.Joystick1Button2) ||
               Input.GetKey(KeyCode.Joystick1Button3) ||
               Input.GetKey(KeyCode.Joystick1Button4) ||
               Input.GetKey(KeyCode.Joystick1Button5) ||
               Input.GetKey(KeyCode.Joystick1Button6) ||
               Input.GetKey(KeyCode.Joystick1Button7) ||
               Input.GetKey(KeyCode.Joystick1Button8) ||
               Input.GetKey(KeyCode.Joystick1Button9) ||
               Input.GetKey(KeyCode.Joystick1Button10) ||
               Input.GetKey(KeyCode.Joystick1Button11) ||
               Input.GetKey(KeyCode.Joystick1Button12) ||
               Input.GetKey(KeyCode.Joystick1Button13) ||
               Input.GetKey(KeyCode.Joystick1Button14) ||
               Input.GetKey(KeyCode.Joystick1Button15) ||
               Input.GetKey(KeyCode.Joystick1Button16) ||
               Input.GetKey(KeyCode.Joystick1Button17) ||
               Input.GetKey(KeyCode.Joystick1Button18) ||
               Input.GetKey(KeyCode.Joystick1Button19))
            {
                return true;
            }

            if (Input.GetAxis("LeftAnalogHorizontal") != 0.0f ||
               Input.GetAxis("LeftAnalogVertical") != 0.0f ||
               Input.GetAxis("Triggers") != 0.0f ||
               Input.GetAxis("RightAnalogHorizontal") != 0.0f ||
               Input.GetAxis("RightAnalogVertical") != 0.0f)
            {
                return true;
            }
            return false;
        }
    }
}
