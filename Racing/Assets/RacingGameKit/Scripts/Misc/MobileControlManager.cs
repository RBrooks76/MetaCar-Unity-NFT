using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RGSK
{
    /// <summary>
    /// MobileControlManager.cs handles setting the UIButton vars in the PlayerControl, activating the UI panel corresponing to the mobile control type chosen and also sets the steer type value to the player
    /// </summary>
    public class MobileControlManager : MonoBehaviour
    {

        public static MobileControlManager instance;
        private PlayerControl playerControl;
        public GameObject touchPanel;
        public GameObject tiltPanel;


        void Awake()
        {
            //create an instance
            instance = this;

            //sets Touch to be default control type
            if (!PlayerPrefs.HasKey("MobileControlType"))
            {
                PlayerPrefs.SetString("MobileControlType", "Touch");
            }
        }

        public void UpdateControls(PlayerControl control)
        {
            switch (PlayerPrefs.GetString("MobileControlType"))
            {
                case "Touch":
                    touchPanel.SetActive(true);
                    tiltPanel.SetActive(false);
                    control.mobileSteerType = PlayerControl.MobileSteerType.TouchSteer;
                    break;

                case "Tilt":
                    touchPanel.SetActive(false);
                    tiltPanel.SetActive(true);
                    control.mobileSteerType = PlayerControl.MobileSteerType.TiltToSteer;
                    break;
            }

            playerControl = control;

            SetMobileUiButtons();
        }

        /// <summary>
        /// This function sets the mobile UI buttons in the InputManager for you
        /// </summary>
        public void SetMobileUiButtons()
        {

            if (!InputManager.instance) return;

            UIButton[] allUIButtons = transform.GetComponentsInChildren<UIButton>();

            foreach (UIButton b in allUIButtons)
            {
                if (b.buttonAction == UIButton.ButtonAction.Accelerate)
                    InputManager.instance.mobileInput.accelerate = b;

                if (b.buttonAction == UIButton.ButtonAction.Brake)
                    InputManager.instance.mobileInput.brake = b;

                if (b.buttonAction == UIButton.ButtonAction.Handbrake)
                    InputManager.instance.mobileInput.handBrake = b;

                if (b.buttonAction == UIButton.ButtonAction.SteerLeft)
                    InputManager.instance.mobileInput.steerLeft = b;

                if (b.buttonAction == UIButton.ButtonAction.SteerRight)
                    InputManager.instance.mobileInput.steerRight = b;

                if (b.buttonAction == UIButton.ButtonAction.Nitro)
                    InputManager.instance.mobileInput.nitro = b;

                if (b.buttonAction == UIButton.ButtonAction.Respawn)
                    b.GetComponent<Button>().onClick.AddListener(() => playerControl.Respawn());

                if (b.buttonAction == UIButton.ButtonAction.SwitchCamera)
                {
                    if (GameObject.FindObjectOfType(typeof(PlayerCamera)))
                    {
                        PlayerCamera playerCam = GameObject.FindObjectOfType(typeof(PlayerCamera)) as PlayerCamera;
                        b.GetComponent<Button>().onClick.AddListener(() => playerCam.SwitchCameras());
                    }
                }

                if (b.buttonAction == UIButton.ButtonAction.Pause && RaceUI.instance)
                    b.GetComponent<Button>().onClick.AddListener(() => RaceUI.instance.PauseResume());
            }
        }
    }
}
