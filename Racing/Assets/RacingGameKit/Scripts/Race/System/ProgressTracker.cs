using UnityEngine;
using System;
using System.Collections;

namespace RGSK
{
    public class ProgressTracker : MonoBehaviour
    {
        private WaypointCircuit circuit; // A reference to the waypoint-based route we should follow

        private float lookAheadForTargetOffset = 20f;

        private float lookAheadForTargetFactor = 0.1f;

        private float lookAheadForSpeedOffset = 20;

        private float lookAheadForSpeedFactor = 0.5f;

        [HideInInspector]
        public Transform target;

        [HideInInspector]
        private Statistics _statistics;

        [HideInInspector]
        public float progressDistance;

        public float raceCompletion;

        private Vector3 lastPosition;

        private float speed;

        public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        void Awake()
        {
            target = new GameObject("pt").transform;
            circuit = GameObject.FindObjectOfType(typeof(WaypointCircuit)) as WaypointCircuit;
            _statistics = GetComponent<Statistics>();
            _statistics.target = target;
        }

        void Start()
        {
            target.name = _statistics.racerDetails.racerName + " Progress Tracker";
        }

        void Update()
        {
            if (!RaceManager.instance)
                return;

            if (Time.deltaTime > 0)
            {
                speed = Mathf.Lerp(speed, (lastPosition - transform.position).magnitude / Time.deltaTime, Time.deltaTime);
            }

            target.position = circuit.GetRoutePoint(progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed).position;

            target.rotation = Quaternion.LookRotation(circuit.GetRoutePoint(progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed).direction);

            progressPoint = circuit.GetRoutePoint(progressDistance);

            Vector3 progressDelta = progressPoint.position - transform.position;

            if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)         
                progressDistance += progressDelta.magnitude * 0.5f;

            if (Vector3.Dot(progressDelta, progressPoint.direction) > 5.0f)           
                progressDistance -= progressDelta.magnitude * 0.5f;
            

            lastPosition = transform.position;

            if (!_statistics.finishedRace)
            {
                if (!_statistics.knockedOut)
                    raceCompletion = (progressDistance / (circuit.Length * RaceManager.instance.totalLaps)) * 100;
            }
            else
            {
                raceCompletion = 100;
            }

            raceCompletion = Mathf.Round(raceCompletion * 100) / 100;
        }

        void OnDestroy()
        {
            if (target)
                Destroy(target.gameObject);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying && circuit != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(target.position, target.position + target.forward);
            }
        }
#endif
    }
}
