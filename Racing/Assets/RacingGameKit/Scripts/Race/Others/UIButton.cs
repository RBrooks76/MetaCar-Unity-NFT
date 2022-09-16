using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace RGSK {

    /// <summary>
    /// //UI_button handles UGUI button presses for car movement on mobile devices
    /// </summary>
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum ButtonAction { Accelerate, Brake, Handbrake, SteerLeft, SteerRight, Nitro, SwitchCamera, Respawn, Pause }
        public ButtonAction buttonAction;
        public float inputValue;
        public float inputSensitivity = 1.5f;
        public bool buttonPressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            buttonPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            buttonPressed = false;
        }

        void Update()
        {
            if (buttonPressed)
            {
                inputValue += Time.deltaTime * inputSensitivity;
            }
            else
            {
                inputValue -= Time.deltaTime * inputSensitivity;
            }

            inputValue = Mathf.Clamp(inputValue, 0, 1);
        }
    }
}