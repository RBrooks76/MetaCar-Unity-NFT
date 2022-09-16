using UnityEngine;
using System.Collections;

namespace RGSK
{

    //WaypointArrow.cs is used to point an arrow in the direction of the next path node
    //Only attach this script to Player vehicles

    public class WaypointArrow : MonoBehaviour
    {

        private Statistics _stats;
        private Transform waypointArrow;
        public float rotateSpeed = 2.0f;
        public bool show = true;

        void Start()
        {
            _stats = GetComponent<Statistics>();

            //find and assign a waypoint arrow
            if (GameObject.Find("WaypointArrow"))
                waypointArrow = GameObject.Find("WaypointArrow").transform;
        }

        void Update()
        {
            if (!waypointArrow || _stats.path.Count <= 0)
                return;


            if (show && RaceManager.instance.raceStarted && !RaceManager.instance.raceCompleted)
            {
                waypointArrow.gameObject.SetActive(true);
                Vector3 direction = _stats.path[_stats.currentNodeNumber].transform.position - waypointArrow.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Vector3 rotation = Quaternion.Lerp(waypointArrow.rotation, targetRotation, Time.deltaTime * rotateSpeed).eulerAngles;
                waypointArrow.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
            else
            {
                waypointArrow.gameObject.SetActive(false);
            }
        }
    }
}