using UnityEngine;
using System.Collections;

namespace RGSK
{
    public class MenuCamera : MonoBehaviour
    {

        public Transform target;

        [Header("Settings")]
        public float zoomMultiplier = 8.0f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;
        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;
        public float distanceMin = .5f;
        public float distanceMax = 15f;
        private float newDistance;
        private bool orbit;
        private bool allowTouchOrbit;
        private Touch touch;
        [HideInInspector]
        public bool canOrbit;

        [Header("Current Values")]
        public float distance = 5.0f;
        public float x = 0.0f;
        public float y = 0.0f;
        public float velX;
        public float velY;

        void Start()
        {
            newDistance = distance;
            canOrbit = true;

            //preset values
            SetValues(-4f, 18.0f);

            //automatically enable touch orbit on mobile platforms
#if UNITY_ANDROID || UNITY_IOS || UNITY_WP_8
            allowTouchOrbit = true;
#else
            allowTouchOrbit = false;
#endif

        }


        void Update()
        {
            orbit = (!allowTouchOrbit) ? Input.GetButton("Fire2") : Input.touchCount == 1;
        }

        void LateUpdate()
        {
            if (target)
            {
                if (canOrbit && orbit)
                {

                    if (!allowTouchOrbit)
                    {
                        velX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                        velY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    }
                    else
                    {
                        if (Input.GetTouch(0).phase == TouchPhase.Moved)
                        {
                            touch = Input.GetTouch(0);
                            velX += touch.deltaPosition.x * xSpeed * 0.02f;
                            velY -= touch.deltaPosition.y * ySpeed * 0.02f;
                        }
                    }
                }

                //Lerp values for smoooth rotation
                y = Mathf.Lerp(y, velY, Time.deltaTime);
                x = Mathf.Lerp(x, velX, Time.deltaTime);

                //Clamp the angles
                y = ClampAngle(y, yMinLimit, yMaxLimit);
                velY = ClampAngle(velY, yMinLimit, yMaxLimit);
                Quaternion rotation = Quaternion.Euler(y, x, 0);

                //Calculate distance from mouse scroll wheel
                newDistance = Mathf.Clamp(newDistance, distanceMin, distanceMax);
                newDistance += Input.GetAxis("Mouse ScrollWheel") * -zoomMultiplier;
                distance = Mathf.Lerp(distance, newDistance, Time.deltaTime);
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + target.position;

                //set the camera rot & pos
                transform.rotation = rotation;
                transform.position = position;
            }
        }

        public void MouseOverRectTransform(bool isOver)
        {
            canOrbit = isOver;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

        public void SetValues(float xVal, float yVal)
        {
            velX = xVal;
            velY = yVal;
        }
    }
}
