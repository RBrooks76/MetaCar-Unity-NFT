using UnityEngine;
using System.Collections;

//DriftpointController.cs handles calculating drift points. Drift points are added when the "drifting" bool is set to true
//You can add this script to your player vehicles to tweak the values manually.

namespace RGSK
{
    public class DriftPointController : MonoBehaviour
    {

        private Statistics stats;
        private Car_Controller controller;
        private Rigidbody rigid;
        [HideInInspector]public bool drifting;

        [Header("Drift Conditions")]
        public float minDriftSpeed = 10.0f; //how fast (in mph) the car must be going inorder to begin a drift
        public float driftSuccessTime = 3.0f; //how long after a drift has ended for the points to be added

        [Header("Drift Mutiplier")]
        public float multiplyRate = 5.0f; //how long to add to the drift multiplier
        public int maxMultiply = 10; //the max a drift can be multipied by
        [HideInInspector]public int driftMultiplier = 1;
        private float lastMultiply;

        [Header("Drift Stats")]
        public float totalDriftPoints; //overall drift points
        public float currentDriftPoints; //points earned from the current drift
        public float currentDriftTime; //how long the current drift has lasted
        public float bestDrift; //highest score from a single drift
        public float longestDrift;
        private float driftSuccessCounter;
        private float lastcrash;
        private bool countedLastDrift;
        private bool countedFinalDrift;

       
        void Start()
        {
            if(GetComponent<Car_Controller>()) controller = GetComponent<Car_Controller>();

            stats = GetComponent<Statistics>();

            rigid = GetComponent<Rigidbody>();

            countedLastDrift = true;

            driftMultiplier = 1;

            lastMultiply = multiplyRate;
        }

        void Update()
        {
            if (RaceManager.instance._raceState != RaceManager.RaceState.Racing) return;

            float speed = rigid.velocity.magnitude * 2.237f;

            if (drifting && speed > minDriftSpeed && !stats.goingWrongway)
            {
                if (controller != null && controller.onPenaltySurface) return;

                countedLastDrift = false;
                currentDriftPoints += 500 * Time.deltaTime;
                currentDriftTime += Time.deltaTime;
                driftSuccessCounter = 0;
                CountDriftMultiplier();
            }
            else
            {
                if (driftSuccessCounter >= driftSuccessTime)
                {
                    if (countedLastDrift) return;

                    countedLastDrift = true;
                    SuccessfulDrift();
                }
                else
                {
                    driftSuccessCounter += Time.deltaTime;
                }
            }

            if(stats.finishedRace && currentDriftPoints > 0 && !countedFinalDrift)
            {
                countedFinalDrift = true;
                SuccessfulDrift();
            }
        }

        void SuccessfulDrift()
        {
            if (currentDriftPoints <= 0) return;

            if (currentDriftTime > longestDrift){ longestDrift = currentDriftTime; }
            if (bestDrift < currentDriftPoints) { bestDrift = currentDriftPoints * driftMultiplier;  }

             StartCoroutine(RaceUI.instance.ShowDriftRaceInfo("+ " + (currentDriftPoints * driftMultiplier).ToString("N0"), Color.green));

            totalDriftPoints += (int)currentDriftPoints * driftMultiplier;
            currentDriftPoints = 0;
            currentDriftTime = 0;
            driftMultiplier = 1;
            lastMultiply = multiplyRate;  
        }

        void FailedDrift()
        {
            StartCoroutine(RaceUI.instance.ShowDriftRaceInfo("Drift Failed!", Color.red));
            currentDriftPoints = 0;
            currentDriftTime = 0;
            driftMultiplier = 1;
            lastMultiply = multiplyRate;
        }

        void CountDriftMultiplier()
        {
            if (driftMultiplier >= maxMultiply) return;

            //increment the multiplier every "multiplyRate" seconds
            if (currentDriftTime > lastMultiply)
            {
                lastMultiply += multiplyRate;
                driftMultiplier ++;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            //Fail on crash

            if (Time.time > lastcrash)
            {
                lastcrash = Time.time + 1.0f;

                if (currentDriftTime > 0 && col.relativeVelocity.magnitude >= 5.0f)
                    FailedDrift();
            }
        }

        void OnEnable()
        {
            //Remove the component if added to a motorbike
            if (gameObject.GetComponent<Motorbike_Controller>())
            {
                enabled = false;
            }
        }
    }
}
