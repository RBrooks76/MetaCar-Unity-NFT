using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//CameraManager.cs handles activating/deactivating race cameras based on their corresponing race states.
//This makes it better to manage race cameras

namespace RGSK
{
    public class CameraManager : MonoBehaviour
    {

        public static CameraManager instance;

        private List<Camera> cameraList = new List<Camera>();

        private AudioListener audioListener;

        [Header("Starting Grid Camera")]
        public Camera startingGridCamera;

        [Header("Player Camera")]
        public Camera playerCamera;

        [Header("Cinematic Camera")]
        public Camera cinematicCamera;

        [Header("MiniMap Camera")]
        public Camera minimapCamera;

        void Awake()
        {
            instance = this;

            CreateAudioListener();

            AddCamerasToCameraList();
        }
        void Update()
        {
            //Make sure the minimap is only enabled in racing state
            if (minimapCamera && RaceManager.instance)
                minimapCamera.enabled = RaceManager.instance._raceState == RaceManager.RaceState.Racing;
        }

        public void ActivatePlayerCamera()
        {
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i] == playerCamera)
                {
                    cameraList[i].enabled = true;
                    SetAudioListerParent(cameraList[i].transform);
                }
                else
                {
                    if (playerCamera != null)
                        cameraList[i].enabled = false;
                }
            }
        }

        public void ActivateStartingGridCamera()
        {
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i] == startingGridCamera)
                {
                    cameraList[i].enabled = true;
                    SetAudioListerParent(cameraList[i].transform);
                }
                else
                {
                    if (startingGridCamera != null)
                        cameraList[i].enabled = false;
                }
            }
        }

        public void ActivateCinematicCamera()
        {
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i] == cinematicCamera)
                {
                    cameraList[i].enabled = true;
                    SetAudioListerParent(cameraList[i].transform);
                }
                else
                {
                    if (cinematicCamera != null)
                        cameraList[i].enabled = false;
                }
            }
        }

        public void SwicthBetweenReplayCameras()
        {
            if (cinematicCamera.enabled)
            {
                ActivatePlayerCamera();
            }
            else if (playerCamera.enabled)
            {
                if (playerCamera.GetComponent<PlayerCamera>())
                    playerCamera.GetComponent<PlayerCamera>().SwitchCameras();
            }
        }

        void SetAudioListerParent(Transform t)
        {
            audioListener.transform.parent = t;
            audioListener.transform.localPosition = Vector3.zero;
            audioListener.transform.localRotation = Quaternion.identity;
        }

        void AddCamerasToCameraList()
        {
            //Add the cameras to the cameraList for easier access later
            if (startingGridCamera && !cameraList.Contains(startingGridCamera))
                cameraList.Add(startingGridCamera);

            if (playerCamera && !cameraList.Contains(playerCamera))
                cameraList.Add(playerCamera);

            if (cinematicCamera && !cameraList.Contains(cinematicCamera))
                cameraList.Add(cinematicCamera);
        }

        void CreateAudioListener()
        {
            //Create an audioListener to make them easier to manage
            audioListener = new GameObject("AudioListener").AddComponent<AudioListener>();

            //Get rid of all other audioListeners so that we dont get that annoying debug message :)
            AudioListener[] allListeners = GameObject.FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
            foreach (AudioListener a in allListeners)
            {
                if (a != audioListener)
                    Destroy(a);
            }
        }
    }
}
