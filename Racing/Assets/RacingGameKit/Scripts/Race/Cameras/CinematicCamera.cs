using UnityEngine;
using System.Collections.Generic;

namespace RGSK
{

    /// <summary>
    /// The CinematicCamera, as the name suggests, handles cinematic camera movement from one position to another
    /// </summary>
    
    public class CinematicCamera : MonoBehaviour
    {
        public Transform target;
        public Transform CameraPositionsParent;
        [HideInInspector]
        public List<Transform> cameraPositions = new List<Transform>();
        private Transform closestPosition = null;

        void Start()
        {
            //Find and assign all the camera positions
            if (!CameraPositionsParent) return;

            Transform[] rcams = CameraPositionsParent.GetComponentsInChildren<Transform>();

            foreach (Transform t in rcams)
            {
                if (t != CameraPositionsParent)
                    cameraPositions.Add(t);
            }
        }

        void Update()
        {
            //Try and find a target
            if (!target && GameObject.FindGameObjectWithTag("Player"))
                target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void LateUpdate()
        {

            if (!target || cameraPositions.Count <= 0) return;

            transform.position = GetClosestCameraPosition().position;
            transform.LookAt(target.position);

            float dist = Vector3.Distance(target.position, transform.position);

            //We use unscaled delta time incase of paused replays
            this.GetComponent<Camera>().fieldOfView = Mathf.Lerp(this.GetComponent<Camera>().fieldOfView, Mathf.Lerp(60, 30, 30.0f / dist), Time.unscaledDeltaTime);

        }

        //Return the closest cinematic camera positon and use it as our next "go to" position
        Transform GetClosestCameraPosition()
        {
            float closestDistanceSqr = Mathf.Infinity;

            foreach (Transform c in cameraPositions)
            {
                float distanceToTarget = (c.position - target.position).sqrMagnitude;

                if (distanceToTarget < closestDistanceSqr)
                {
                    closestPosition = c;
                    closestDistanceSqr = distanceToTarget;
                }
            }

            return closestPosition;
        }
    }
}