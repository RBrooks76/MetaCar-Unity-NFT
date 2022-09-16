//PlayerControl.cs handles user input

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RGSK
{
    public class PlayerControl : MonoBehaviour
    {
        public enum InputTypes { Desktop, Mobile, Automatic }
        public enum MobileSteerType { TiltToSteer, TouchSteer }

        [Header("Input Platform")]
        public InputTypes inputType = InputTypes.Automatic;

        private Car_Controller car_controller;
        private Motorbike_Controller bike_controller;

        [Header("Mobile Controls")]
        public MobileSteerType mobileSteerType;

        [Header("Other")]
        public bool autoAcceleration;

        void Awake()
        {
            if (GetComponent<Car_Controller>())
                car_controller = GetComponent<Car_Controller>();

            if (GetComponent<Motorbike_Controller>())
                bike_controller = GetComponent<Motorbike_Controller>();
        }

        void Start()
        {

            if (!InputManager.instance)
            {
                Debug.LogError("No Input Manager Found! Please Create an Input Manager by going to Window/RacingGameStarterKit/RaceComponents");
                enabled = false;
                return;
            }

            autoAcceleration = (PlayerPrefs.GetString("AutoAcceleration") == "True");

            if (inputType == InputTypes.Mobile || inputType == InputTypes.Automatic)
            {
                if (MobileControlManager.instance)
                {
                    MobileControlManager.instance.UpdateControls(this);
                }
            }
        }

        void Update()
        {
            switch (inputType)
            {
                case InputTypes.Desktop:
                    DesktopControl();
                    break;

                case InputTypes.Mobile:
                    MobileControl();
                    break;

                case InputTypes.Automatic:
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
                    DesktopControl();
#else
					MobileControl();
#endif
                    break;
            }
        }

        void DesktopControl()
        {

            switch (InputManager.instance.inputDevice)
            {
                case InputManager.InputDevice.Keyboard:

                    //Get input values
                    float accel = (!autoAcceleration) ? Mathf.Clamp01(Input.GetAxis(InputManager.instance.keyboardInput.verticalAxis)) : 1.0f;
                    float brake = Mathf.Clamp01(-Input.GetAxis(InputManager.instance.keyboardInput.verticalAxis));
                    float handbrake = Mathf.Clamp01(Input.GetAxis(InputManager.instance.keyboardInput.handbrakeAxis));
                    float steer = Mathf.Clamp(Input.GetAxis(InputManager.instance.keyboardInput.horizontalAxis), -1, 1);
                    bool nitro = Input.GetKey(InputManager.instance.keyboardInput.nitroKey);

                    //Send inputs
                    SendInputs(accel, brake, steer, handbrake, nitro);

                    //Pause
                    if (Input.GetKeyDown(InputManager.instance.keyboardInput.pauseKey))
                    {
                        if (RaceManager.instance)
                            RaceManager.instance.PauseRace();
                    }

                    //Respawn
                    if (Input.GetKeyDown(InputManager.instance.keyboardInput.respawnKey))
                    {
                        Respawn();
                    }

                    break;

                case InputManager.InputDevice.XboxController:

                    //Get input values
                    float _accel = (!autoAcceleration) ? Mathf.Clamp01(Input.GetAxis(InputManager.instance.xboxControllerInput.verticalAxis)) : 1.0f;
                    float _brake = Mathf.Clamp01(Input.GetAxis(InputManager.instance.xboxControllerInput.negativeVerticalAxis));
                    float _steer = Mathf.Clamp(Input.GetAxis(InputManager.instance.xboxControllerInput.horizontalAxis), -1, 1);
                    float _handbrake = Input.GetButton(InputManager.instance.xboxControllerInput.handbrakeButton) ? 1 : 0;
                    bool _nitro = Input.GetButton(InputManager.instance.xboxControllerInput.nitroButton);

                    //Send inputs
                    SendInputs(_accel, _brake, _steer, _handbrake, _nitro);

                    //Pause
                    if (Input.GetButtonDown(InputManager.instance.xboxControllerInput.pauseButton))
                    {
                        if (RaceManager.instance)
                            RaceManager.instance.PauseRace();
                    }

                    //Respawn
                    if (Input.GetButton(InputManager.instance.xboxControllerInput.respawnButton))
                    {
                        Respawn();
                    }
                 
                    break;
            }
        }

        void MobileControl()
        {

            float steer = 0;

            if (mobileSteerType == MobileSteerType.TiltToSteer)
            {
                //steer according to the device tilt amount
                steer = Input.acceleration.x;
            }
            else
            {
                //steer with the on-screen ui buttons
                if (InputManager.instance.mobileInput.steerRight != null && InputManager.instance.mobileInput.steerLeft != null)
                {
                    steer = InputManager.instance.mobileInput.steerRight.inputValue + (-InputManager.instance.mobileInput.steerLeft.inputValue);
                }
            }

            //send inputs
            float _accel = (!autoAcceleration) ? InputManager.instance.mobileInput.accelerate.inputValue : 1.0f;
            float _brake = InputManager.instance.mobileInput.brake.inputValue;
            float _handbrake = (InputManager.instance.mobileInput.handBrake) ? InputManager.instance.mobileInput.handBrake.inputValue : 0;
            bool _nitro = (InputManager.instance.mobileInput.nitro) ? InputManager.instance.mobileInput.nitro.buttonPressed : false;

            SendInputs(_accel, _brake, steer, _handbrake, _nitro);
        }

        void SendInputs(float accel, float brake, float steer, float handbrake, bool nitro)
        {
            if (car_controller)
            {
                car_controller.motorInput = (brake <= 0) ? accel : 0;
                car_controller.brakeInput = brake;
                car_controller.steerInput = steer;
                car_controller.handbrakeInput = handbrake;
                car_controller.usingNitro = nitro;
            }

            if (bike_controller)
            {
                bike_controller.motorInput = (brake <= 0) ? accel : 0;
                bike_controller.brakeInput = brake;
                bike_controller.steerInput = steer;
                bike_controller.usingNitro = nitro;
            }
        }

        public void Respawn()
        {
            if (RaceManager.instance)
                RaceManager.instance.RespawnRacer(transform, GetComponent<Statistics>().lastPassedNode, 3.0f);
        }
    }
}
