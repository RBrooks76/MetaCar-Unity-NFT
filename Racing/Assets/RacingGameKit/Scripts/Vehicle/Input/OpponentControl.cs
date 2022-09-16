
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

//OpponentControl.cs handles AI logic

namespace RGSK
{
    public class OpponentControl : MonoBehaviour
    {

        public enum AiDifficulty { Custom, Easy, Meduim, Hard }


        public enum AiState { FollowingPath, Overtaking }
        public AiDifficulty difficulty = AiDifficulty.Meduim;
        private AiState aiState;
        private Statistics stats;
        private Car_Controller car_controller;
        private Motorbike_Controller bike_controller;


        [Header("Behaviour")]

        [Range(0, 1f)]
        public float accelerationSensitivity = 1.0f;

        [Range(0, 1f)]
        public float accelerationWander = 0.1f; //1 = slower ai, 0 = faster ai

        [Range(0, 1f)]
        private float accelerationFluctationRate = 0.5f;

        [Range(0, 1)]
        public float brakeSensitivity = 1.0f;

        [Range(0, 0.1f)]
        public float steerSensitivity = 0.01f;

        [Range(0, 0.2f)]
        public float avoidanceSensitivity = 0.1f;

        [Range(0, 25)]
        public float maxWanderDistance = 3.0f;

        [Range(0, 0.25f)]
        public float wanderRate = 0.1f;

        [Range(0, 1)]
        public float cautionSpeed = 0.3f;

        [Range(0, 5)]
        public float overtakeStrength = 1.5f;

        [Range(0, 1)]
        public float nitroProbability = 0.5f;

        [Range(0, 180)]
        public float cautionAngle = 45.0f; //angle to treat as cautious. Default set to 45°.
        public float cautionAmount = 30.0f;
        public float maxOvertakeTime = 5.0f; //the maximum time the ai can stay in an overtake state
        public bool randomBehaviour = true;

        [Space(5)]

        [Header("Sensors")]
        public float forwardDistance = 18.0f;
        public float forwardLateralDistance = 1.5f;
        public float sidewayDistance = 2.5f;
        public float angleDistance = 8.0f;
        public float sensorHeight = 0.0f;

        [Header("Misc")]
        public float respawnTime = 10;

        //Private stuff
        private Transform target;
        private Transform overtakeTarget;
        private Vector3 offsetTargetPos;
        private Vector3 steerVector;
        private Vector3 lastStuckPos;
        private float newSteer;
        private float avoidanceSteer;
        private float avoidOffset;
        private float randomValue;
        private float requiredSpeed;
        private float aproachingAngle;
        private float spinningAngle;
        private float cautionRequired;
        private float throttleSensitvity;
        private float throttle;
        private float targetAngle;
        private float stuckTimer;
        private float reverseTimer;
        private float topSpeed;
        private float currentSpeed;
        private float brakeSpeed = 50; //Set a deafult value of 50
        private float overtakeCounter;
        private bool overtakeNow;
        private bool reverse;
        private bool somethingFront;
        private bool somethingLeft;
        private bool somethingRight;
        private bool brakezone;


        void Awake()
        {
            if (GetComponent<Statistics>())
                stats = GetComponent<Statistics>();

            if (GetComponent<Car_Controller>())
                car_controller = GetComponent<Car_Controller>();


            if (GetComponent<Motorbike_Controller>())
                bike_controller = GetComponent<Motorbike_Controller>();

            randomValue = Random.value * 100;
        }

        void Start()
        {
            //Set values based on difficulty
            SetDifficulty(difficulty);

            //Make a few randomizations
            RandomizeAIBehaviour();

			//Invoke the sensor fucntion
			InvokeRepeating ("Sensors", 1, 0.1f);
        }

        void RandomizeAIBehaviour()
        {
            if (!randomBehaviour) return;

            wanderRate = Random.Range(0.01f, 0.25f);

            accelerationFluctationRate = Random.Range(0.1f, 1f);
        }

        public void SetDifficulty(AiDifficulty _difficulty)
        {
            if (_difficulty == AiDifficulty.Custom) return;

            difficulty = _difficulty;

            switch (_difficulty)
            {
                case AiDifficulty.Easy:

                    accelerationSensitivity = 0.5f;

                    brakeSensitivity = 0.5f;

                    accelerationWander = Random.Range(0.5f, 0.75f);

                    nitroProbability = Random.Range(0.0f, 0.25f);
                    break;

                case AiDifficulty.Meduim:

                    accelerationSensitivity = 0.8f;

                    brakeSensitivity = 0.8f;

                    accelerationWander = Random.Range(0.1f, 0.25f);

                    nitroProbability = Random.Range(0.25f, 0.75f);
                    break;

                case AiDifficulty.Hard:

                    accelerationSensitivity = 1f;

                    brakeSensitivity = 1f;

                    accelerationWander = 0;

                    nitroProbability = Random.Range(0.5f, 1f);
                    break;
            }
        }

        void Update()
        {
            CheckIfStuck();
        }

        void FixedUpdate()
        {
            if (!stats)
                return;

            if (car_controller)
            {
                currentSpeed = car_controller.currentSpeed;

                topSpeed = (!brakezone && !stats.knockedOut) ? car_controller.topSpeed : brakeSpeed;
            }

            if (bike_controller)
            {
                currentSpeed = bike_controller.currentSpeed;

                topSpeed = (!brakezone && !stats.knockedOut) ? bike_controller.topSpeed : brakeSpeed;
            }

            NavigateAI();
        }


        void NavigateAI()
        {

            //Set the target
            target = stats.target;

            //Handle AI Beahviour
            Vector3 fwd = transform.forward;

            requiredSpeed = topSpeed;

            aproachingAngle = Vector3.Angle(target.forward, fwd);

            spinningAngle = GetComponent<Rigidbody>().angularVelocity.magnitude * cautionAmount;

            cautionRequired = Mathf.InverseLerp(0, cautionAngle, Mathf.Max(spinningAngle, aproachingAngle));

            requiredSpeed = Mathf.Lerp(topSpeed, topSpeed * (cautionSpeed), cautionRequired);

            offsetTargetPos = target.position;

            float lineFollow = (aiState == AiState.FollowingPath) ? (Mathf.PerlinNoise(Time.time * wanderRate, randomValue) * 2 - 1) * maxWanderDistance : avoidOffset;

            offsetTargetPos += target.right * lineFollow;

            throttleSensitvity = (requiredSpeed < currentSpeed) ? brakeSensitivity : accelerationSensitivity;

            throttle = Mathf.Clamp((requiredSpeed - currentSpeed) * throttleSensitvity, -1, 1);

            throttle *= (1 - accelerationWander) + (Mathf.PerlinNoise(Time.time * accelerationFluctationRate, randomValue) * accelerationWander);

            steerVector = transform.InverseTransformPoint(offsetTargetPos);

            targetAngle = Mathf.Atan2(steerVector.x, steerVector.z) * Mathf.Rad2Deg;

            newSteer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(currentSpeed);

            //Finaly, feed the input values
            FeedInput(throttle, throttle, newSteer);
        }


        void FeedInput(float motor, float brake, float steer)
        {
            float m = (!reverse) ? motor = Mathf.Clamp(motor, 0, 1) : 0;
            float b = (!reverse) ? brake = -1 * Mathf.Clamp(brake, -1, 0) : 1;
            float s = (!reverse) ? steer : -steer;
            s += avoidanceSteer;

            //Car Input
            if (car_controller)
            {
                //motor value
                car_controller.motorInput = m;

                //brake value
                car_controller.brakeInput = b;

                //handbrake value (always 0 for AI)
                car_controller.handbrakeInput = 0;

                //steer value
                car_controller.steerInput = s + avoidanceSteer;
            }

            //Bike Input
            if (bike_controller)
            {
                //motor value
                bike_controller.motorInput = m;

                //brake value
                bike_controller.brakeInput = b;

                //steer value
                bike_controller.steerInput = s + avoidanceSteer;

            }
        }


        void Sensors()
        {
            Vector3 fwd;
            RaycastHit hit;
            avoidanceSteer = 0.0f;
            somethingFront = false;
            somethingLeft = false;
            somethingRight = false;


            //FORWARD RAYS

            //F
            fwd = new Vector3(transform.position.x, transform.position.y + sensorHeight, transform.position.z);
            if (Physics.Raycast(fwd, transform.forward, out hit, forwardDistance))
            {

                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    overtakeTarget = hit.transform;

                    if (aiState != AiState.Overtaking && !somethingLeft && !somethingRight)
                        StartCoroutine(AttemptOvertake());

                    //slow down when too close
                    if (hit.distance <= 5.0f)
                        requiredSpeed *= 0.5f;

                    somethingFront = true;
                }
                else
                {
                    somethingFront = true;
                }
            }

            //R
            fwd += transform.right * forwardLateralDistance;
            if (Physics.Raycast(fwd, transform.forward, out hit, forwardDistance))
            {

                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    //slow down when too close
                    if (hit.distance <= 5.0f)
                        requiredSpeed *= 0.5f;

                    somethingFront = true;
                }
                else
                {
                    somethingFront = true;
                }
            }

            //L
            fwd = new Vector3(transform.position.x, transform.position.y + sensorHeight, transform.position.z);
            fwd -= transform.right * forwardLateralDistance;
            if (Physics.Raycast(fwd, transform.forward, out hit, forwardDistance))
            {

                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    //slow down when too close
                    if (hit.distance <= 5.0f)
                        requiredSpeed *= 0.5f;

                    somethingFront = true;
                }
                else
                {
                    somethingFront = true;
                }
            }

            //---ANGLED RAYS---
            fwd = new Vector3(transform.position.x, transform.position.y + sensorHeight, transform.position.z);

            //RIGHT
            if (Physics.Raycast(fwd, Quaternion.AngleAxis(30, transform.up) * transform.forward, out hit, angleDistance))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    avoidanceSteer = -0.5f * avoidanceSensitivity;

                    somethingRight = true;
                }
                else
                {
                    somethingRight = true;
                }
            }

            //LEFT
            if (Physics.Raycast(fwd, Quaternion.AngleAxis(-30, transform.up) * transform.forward, out hit, angleDistance))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    avoidanceSteer = 0.5f * avoidanceSensitivity;

                    somethingLeft = true;
                }
                else
                {
                    somethingLeft = true;
                }
            }


            //---SIDEWAY RAYS---

            //RIGHT
            if (Physics.Raycast(fwd, Quaternion.AngleAxis(90, transform.up) * transform.forward, out hit, sidewayDistance))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    avoidanceSteer = -0.5f * avoidanceSensitivity;

                    somethingRight = true;
                }
                else
                {
                    somethingRight = true;
                }
            }

            //LEFT
            if (Physics.Raycast(fwd, Quaternion.AngleAxis(-90, transform.up) * transform.forward, out hit, sidewayDistance))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    avoidanceSteer = 0.5f * avoidanceSensitivity;

                    somethingLeft = true;
                }
                else
                {
                    somethingLeft = true;
                }
            }

            //Visualize the rays
#if UNITY_EDITOR
            fwd = new Vector3(transform.position.x, transform.position.y + sensorHeight, transform.position.z);
            Debug.DrawRay(fwd, transform.forward * forwardDistance, Color.yellow);

            fwd += transform.right * forwardLateralDistance;
            Debug.DrawRay(fwd, transform.forward * forwardDistance, Color.yellow);

            fwd = new Vector3(transform.position.x, transform.position.y + sensorHeight, transform.position.z);
            fwd -= transform.right * forwardLateralDistance;
            Debug.DrawRay(fwd, transform.forward * forwardDistance, Color.yellow);

            fwd = new Vector3(transform.position.x, transform.position.y + sensorHeight, transform.position.z);
            Debug.DrawRay(fwd, Quaternion.AngleAxis(30, transform.up) * transform.forward * angleDistance, Color.yellow);
            Debug.DrawRay(fwd, Quaternion.AngleAxis(-30, transform.up) * transform.forward * angleDistance, Color.yellow);

            Debug.DrawRay(fwd, Quaternion.AngleAxis(90, transform.up) * transform.forward * sidewayDistance, Color.yellow);
            Debug.DrawRay(fwd, Quaternion.AngleAxis(-90, transform.up) * transform.forward * sidewayDistance, Color.yellow);
#endif
        }

        IEnumerator AttemptOvertake()
        {
            aiState = AiState.Overtaking;

            overtakeNow = true;

            while (overtakeNow)
            {
                overtakeCounter += Time.deltaTime;

                //Overtake over time
                if (overtakeCounter <= maxOvertakeTime)
                {
                    var otherCarLocalDelta = transform.InverseTransformPoint(overtakeTarget.transform.position);
                    float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
                    avoidOffset = overtakeStrength * -Mathf.Sign(otherCarAngle);
                }
                else
                {
                    overtakeNow = false;
                    EndOvertake();
                }

                //Check if the ai has passed the racer (successful overtake)
                if (overtakeTarget)
                {
                    if (Vector3.Angle(transform.forward, overtakeTarget.transform.position - transform.position) > 90 && PositionDifference(overtakeTarget.position, transform.position) > 5.0f)
                    {
                        overtakeNow = false;
                        EndOvertake();
                    }


                    //Check if the overtake target got out of range (failed overtake)
                    if (PositionDifference(overtakeTarget.position, transform.position) > 20.0f) //threshold of 20m
                    {
                        overtakeNow = false;
                        EndOvertake();
                    }
                }

                yield return null;
            }
        }

        void EndOvertake()
        {

            aiState = AiState.FollowingPath;

            overtakeCounter = 0;
        }
     

        void CheckIfStuck()
        {
            if (!RaceManager.instance)
                return;

            //Add to the respawn timer incase the AI gets stuck
            if (RaceManager.instance.raceStarted && !stats.finishedRace && !stats.knockedOut)
            {
                if (currentSpeed <= 3.0f)
                {
                    stuckTimer += Time.deltaTime;
                }
                else
                {
                    stuckTimer = 0.0f;
                }

                //Reverse after stuck for 3 seconds
                if (stuckTimer >= 3.0f && !reverse)
                {
                    if (somethingFront) StartCoroutine(Reverse());
                }

                //respawn after being stuck for "respawnTime" seconds
                if(stuckTimer >= respawnTime)
                {
                    RespawnRacer();
                }
            }
        }

        IEnumerator Reverse()
        {

            reverse = true;

            while (reverse)
            {

                if (reverseTimer < 3.0f)
                {
                    reverseTimer += Time.deltaTime;
                }
                else
                {
                    reverse = false;
                    reverseTimer = 0;
                }

                yield return null;
            }
        }

        void RespawnRacer()
        {
            stuckTimer = 0.0f;

            RaceManager.instance.RespawnRacer(transform, stats.lastPassedNode, 3.0f);      
        }

        float PositionDifference(Vector3 lastPos, Vector3 currPos)
        {
            return Vector3.Distance(lastPos, currPos);
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.GetComponent<Statistics>())
            {
                //If we crash into a racer that is infront, attempt an overtake
                if (Vector3.Angle(transform.forward, other.transform.position - transform.position) < 90)
                {
                    overtakeTarget = other.transform;

                    if (aiState != AiState.Overtaking)
                        StartCoroutine(AttemptOvertake());
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            //Nitro 
            if (other.tag == "NitroTrigger")
            {
                float random = Random.Range((float)0, (float)1);

                if (random <= nitroProbability)
                {
                    StartCoroutine(UseNitro(5.0f));
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            //Enter Brakezone
            if (other.GetComponent<Brakezone>())
            {
                brakezone = true;
                brakeSpeed = other.GetComponent<Brakezone>().targetSpeed;
            }
        }

        void OnTriggerExit(Collider other)
        {
            //Exit Brakezone
            if (other.GetComponent<Brakezone>())
            {
                brakezone = false;
            }
        }

        IEnumerator UseNitro(float time)
        {
            if (!enabled || GetComponent<Statistics>().knockedOut) yield break;

            if (car_controller)
            {
                if (car_controller.usingNitro)
                    yield break;

                car_controller.usingNitro = !somethingFront;

                yield return new WaitForSeconds(time);

                car_controller.usingNitro = false;
            }

            if (bike_controller)
            {
                if (bike_controller.usingNitro)
                    yield break;

                bike_controller.usingNitro = true;

                yield return new WaitForSeconds(time);

                bike_controller.usingNitro = false;
            }
        }
    }
}
