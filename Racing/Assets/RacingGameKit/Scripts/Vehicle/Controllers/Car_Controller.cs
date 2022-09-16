using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{
    public class Car_Controller : MonoBehaviour
    {

        public enum Propulsion { RWD, FWD }
        public enum VehicleTyres { Race, Drift}
        public enum SpeedUnit { MPH, KPH }

        public Propulsion _propulsion = Propulsion.RWD;
        public SpeedUnit _speedUnit = SpeedUnit.MPH;

        //Wheel Transforms
        public Transform FL_Wheel;
        public Transform FR_Wheel;
        public Transform RL_Wheel;
        public Transform RR_Wheel;
        private List<Transform> wheeltransformList = new List<Transform>();
       
        //Wheel Colliders
        public WheelCollider FL_WheelCollider;
        public WheelCollider FR_WheelCollider;
        public WheelCollider RL_WheelCollider;
        public WheelCollider RR_WheelCollider;
        private List<WheelCollider> wheelcolliderList = new List<WheelCollider>();
        private List<Wheels> wheelsComponent = new List<Wheels>();

        //Engine values
        public float engineTorque = 800.0f; //avoid setting this value too high inorder to not overpower the wheels!
        public float brakeTorque = 2000.0f;
        public float handbrakeTourque = 6000.0f;
        public float maxSteerAngle = 30.0f;
        public float topSpeed = 150.0f;
        public float boost = 100.0f;
        public float currentSpeed;
        public float maxReverseSpeed = 75.0f;
        private float currentSteerAngle;
        public float brakeForce = 15000.0f;

        //Gear values
        public int numberOfGears = 6;
        public int currentGear;
        private float[] gearSpeeds;

        //Stability
        private Rigidbody rigid;
        public Vector3 centerOfMass;
        public float antiRollAmount = 8000.0f;
        public float downforce = 50.0f;
        [Range(0, 1)]
        public float steerHelper = 0.8f;
        [Range(0, 1)]
        public float traction = 0.5f;

        //Wheel Slip settings
        public float forwardSlipLimit = 0.4f;
        public float sidewaySlipLimit = 0.35f;
        
        //Sounds
        public AudioSource engineAudioSource;
        public AudioSource nitroAudioSource;
        public AudioClip engineSound;
        public AudioClip nitroSound;
        public List<AudioClip> crashSounds = new List<AudioClip>();

        //Bools
        public bool controllable;
        public bool reversing;
        public bool enableSlipstream;
        public bool enableNitro;
        public bool usingNitro;
        public bool onPenaltySurface;

        //Misc
        public GameObject brakelightGroup;
        public Transform steeringWheel;
        private float speedLimit;
        private Vector3 velocityDir;
        private float currentRotation;
        public float topspeedCache;

        //Slipstream
        public float slipstreamRayHeight = 0f;
        public float slipstreamRayLength = 20.0f;
        [Range(0.1f, 5)]public float slipstreamStrength = 2.5f;
        
          
        //Nitro
        [Range(0, 100)]
        public float nitroStrength = 5f;
        [Range(0, 50)]
        public float addedNitroSpeed = 25;
        public float nitroRegenerationRate = 0.1f;
        public float nitroDepletionRate = 0.25f;
        public float nitroCapacity = 1.0f;
        public float nitroAddedSpeed = 20.0f;
        private float nitroTopSpeed;
        public GameObject nitroGroup;

        //Input values
        public float motorInput;
        public float brakeInput;
        public float steerInput;
        public float handbrakeInput;

        public GameObject m_BulletPrefab;
        public Transform m_BulletPos;

        public GameObject m_MineSprite;
        public GameObject m_BulletSprite;
        public GameObject m_RocketSprite;

        private int m_MineCount = 0;
        private int m_BulletCount = 0;
        private int m_RocketCount = 0;

        public bool m_isPlayer = false;

        public int m_Health = 100;

        private ECprojectileActor m_shooter;

        void Start()
        {
            SetupRigidbody();

            SetupWheels();

            SetupGears();

            SetupAudio();

            SetupMisc();

            m_isPlayer = this.gameObject.tag == "Player";

            if (m_isPlayer)
            {
                m_MineSprite.SetActive(false);
                m_BulletSprite.SetActive(false);
                m_RocketSprite.SetActive(false);

                m_shooter = GetComponentInChildren<ECprojectileActor>();
            }
        }

        void SetupRigidbody()
        {
            rigid = GetComponent<Rigidbody>();

            rigid.centerOfMass = centerOfMass;

            rigid.drag = 0.01f;

            rigid.angularDrag = 0;
        }

        void SetupWheels()
        {
            wheeltransformList.Clear();
            wheelcolliderList.Clear();
            wheelsComponent.Clear();

            wheeltransformList.Add(FL_Wheel); wheelcolliderList.Add(FL_WheelCollider); wheelsComponent.Add(FL_WheelCollider.GetComponent<Wheels>());
            wheeltransformList.Add(FR_Wheel); wheelcolliderList.Add(FR_WheelCollider); wheelsComponent.Add(FR_WheelCollider.GetComponent<Wheels>());
            wheeltransformList.Add(RL_Wheel); wheelcolliderList.Add(RL_WheelCollider); wheelsComponent.Add(RL_WheelCollider.GetComponent<Wheels>());
            wheeltransformList.Add(RR_Wheel); wheelcolliderList.Add(RR_WheelCollider); wheelsComponent.Add(RR_WheelCollider.GetComponent<Wheels>());

            for (int i = 0; i < wheelsComponent.Count; i++)
            {
                wheelsComponent[i].SetupWheelCollider(50, 25000, 2000);
            }

            //Set no traction for the drift race type
            if (RaceManager.instance && RaceManager.instance._raceType == RaceManager.RaceType.Drift)
            {
                traction = 0;

                steerHelper = 0;

                if (_propulsion == Propulsion.FWD)
                {
                    _propulsion = Propulsion.RWD;
                    Debug.Log("Setting propulsion to RWD for the drift race");
                }
            }
        }

        void SetupAudio()
        {
            if (engineAudioSource)
            {
                engineAudioSource.clip = engineSound;
                engineAudioSource.loop = true;
                engineAudioSource.spatialBlend = 1.0f;
                engineAudioSource.minDistance = 3.0f;
                engineAudioSource.Play();
            }

            if (nitroAudioSource)
            {
                nitroAudioSource.clip = nitroSound;
                nitroAudioSource.loop = true;
                nitroAudioSource.spatialBlend = 1.0f;
                nitroAudioSource.minDistance = 3.0f;
                nitroAudioSource.Stop();
            }
        }

        void SetupGears()
        {
            //Calculate gearSpeeds
            gearSpeeds = new float[numberOfGears];

            for (int i = 0; i < numberOfGears; i++)
            {
                gearSpeeds[i] = Mathf.Lerp(0, topSpeed, ((float)i / (float)(numberOfGears)));
            }

            gearSpeeds[numberOfGears - 1] = topSpeed + 60.0f; //ensure the last gear doesn't exceed topSpeed!

            currentGear = 1;
        }

        void SetupMisc()
        {
            topspeedCache = topSpeed;
            nitroTopSpeed = topSpeed + nitroAddedSpeed;
            speedLimit = topspeedCache;

            if (brakelightGroup)
                brakelightGroup.SetActive(false);

            if (nitroGroup)
            {
                nitroGroup.SetActive(true);

                foreach(Transform p in nitroGroup.transform)
                {
                    if (p.GetComponent<ParticleSystem>())
                    {
                        var em = p.GetComponent<ParticleSystem>().emission;
                        em.enabled = false;
                    }                       
                }
            }

            if (controllable && RaceManager.instance)
            {
                controllable = false;
            }
        }

        void Update()
        {
            ShiftGears();
            WheelAllignment();
            WheelSpin();
            DriftController();
            onPenaltySurface = CheckForPenalty();
            currentSpeed = CalculateSpeed();

            UseItem();
        }

        void FixedUpdate()
        {
            Drive();
            SpeedLimiter();                     
            ApplyDownforce();
            StabilizerBars();
            velocityDir = CalculateVelocityDirection();
        }

        void Shoot(int type)
        {
            m_shooter.bombType = type;
            m_shooter.Fire();
        }

        void SpeedLimiter()
        {
            if(currentSpeed > speedLimit)
            {
                switch (_propulsion)
                {
                    case Propulsion.FWD:

                        rigid.velocity = _speedUnit == SpeedUnit.MPH ? (speedLimit / 2.237f) * rigid.velocity.normalized : (speedLimit / 3.6f) * rigid.velocity.normalized;
                        FL_WheelCollider.motorTorque = 0;
                        FR_WheelCollider.motorTorque = 0;

                        break;

                    case Propulsion.RWD:

                        rigid.velocity = _speedUnit == SpeedUnit.MPH ? (speedLimit / 2.237f) * rigid.velocity.normalized : (speedLimit / 3.6f) * rigid.velocity.normalized;
                        RL_WheelCollider.motorTorque = 0;
                        RR_WheelCollider.motorTorque = 0;

                        break;
                }
            }
        }

        void Drive()
        {
            if (!controllable)
                return;

            switch (_propulsion)
            {

                //Rear wheel drive
                case Propulsion.RWD:

                    if (currentSpeed <= speedLimit)
                    {
                        if (!reversing)
                        {
                            RL_WheelCollider.motorTorque = engineTorque * motorInput;
                            RR_WheelCollider.motorTorque = engineTorque * motorInput;
                        }
                        else {
                            RL_WheelCollider.motorTorque = engineTorque * -brakeInput;
                            RR_WheelCollider.motorTorque = engineTorque * -brakeInput;
                        }
                    }
                    break;

                //Front wheel drive
                case Propulsion.FWD:

                    if (currentSpeed <= speedLimit)
                    {
                        if (!reversing)
                        {
                            FL_WheelCollider.motorTorque = engineTorque * motorInput;
                            FR_WheelCollider.motorTorque = engineTorque * motorInput;
                        }
                        else {
                            FL_WheelCollider.motorTorque = engineTorque * -brakeInput;
                            FR_WheelCollider.motorTorque = engineTorque * -brakeInput;
                        }
                    }
                    break;
            }

            Brake();
            SteerHelper();
            Traction();

            //Steering
            currentSteerAngle = Mathf.Lerp(maxSteerAngle, (maxSteerAngle / 2), (currentSpeed / (topSpeed * 2.0f)));
            FL_WheelCollider.steerAngle = Mathf.Clamp((currentSteerAngle * steerInput), -maxSteerAngle, maxSteerAngle) * 0.5f;
            FR_WheelCollider.steerAngle = Mathf.Clamp((currentSteerAngle * steerInput), -maxSteerAngle, maxSteerAngle) * 0.5f;


            //Debug.Log("steer=" + steerInput.ToString() + ": curSpeed=" + currentSpeed.ToString());
            if ((steerInput == 1 || steerInput == -1) && currentSpeed > 0)
            {
                //rigid.AddForce(-transform.forward * brakeForce * 2);
                //currentSpeed = 50;
                //rigid.AddForce(-transform.up * downforce * rigid.velocity.magnitude * 10);
                //rigid.AddForce(-transform.forward * brakeForce);
            }

            //Reverse
            if (brakeInput > 0 && velocityDir.z <= 0.01f)
            {
                reversing = true;
                speedLimit = maxReverseSpeed;
            }
            else
            {
                reversing = false;
                speedLimit = topSpeed;
            }


            //Handle steering wheel rotation
            if (steeringWheel)
            {
                Quaternion currentRotation = steeringWheel.rotation;
                Quaternion targetRotation = transform.rotation * Quaternion.Euler(10, 0, (FL_WheelCollider.steerAngle) * -2.0f);
                steeringWheel.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * 5f);
            }

            //Boost
            if (currentSpeed < speedLimit && motorInput > 0 /*&& steerInput == 0*/)
            {
                //rigid.AddForce(transform.forward * boost);
            }

            //Slipstream
            if (enableSlipstream)
            {               
                Slipstream();
            }

            //Nitro 
            if (enableNitro)
            {
                Nitro();
            }
        }

        void Brake()
        {
            //Footbrake
            if (!reversing && brakeInput > 0.0f && handbrakeInput < 0.1f)
            {
                //add a backward force to help stop the car
                rigid.AddForce(-transform.forward * brakeForce);

                if (_propulsion == Propulsion.RWD)
                {
                    RL_WheelCollider.brakeTorque = brakeTorque * brakeInput;
                    RR_WheelCollider.brakeTorque = brakeTorque * brakeInput;
                    RL_WheelCollider.motorTorque = 0;
                    RR_WheelCollider.motorTorque = 0;
                }
                else
                {
                    FL_WheelCollider.brakeTorque = brakeTorque * brakeInput;
                    FR_WheelCollider.brakeTorque = brakeTorque * brakeInput;
                    FL_WheelCollider.motorTorque = 0;
                    FR_WheelCollider.motorTorque = 0;
                }
            }

            else
            {
                RL_WheelCollider.brakeTorque = 0;
                RR_WheelCollider.brakeTorque = 0;
                FL_WheelCollider.brakeTorque = 0;
                FR_WheelCollider.brakeTorque = 0;
            }

            //Handbrake
            if (handbrakeInput > 0)
            {
                RL_WheelCollider.brakeTorque = handbrakeTourque * handbrakeInput;
                RR_WheelCollider.brakeTorque = handbrakeTourque * handbrakeInput;              
            }

            //Decelerate
            if (motorInput == 0 && brakeInput == 0 && rigid.velocity.magnitude > 1.0f)
            {
                if (velocityDir.z >= 0.01f)
                    rigid.AddForce(-transform.forward * 250);
                else
                    rigid.AddForce(transform.forward * 250);
            }

            //Activate brake lights if braking
            if (brakelightGroup)
            {
                brakelightGroup.SetActive(brakeInput > 0.3f);
            }
        }

        void ShiftGears()
        {
            //Shift Down
            if(currentSpeed >= gearSpeeds[currentGear] && FL_WheelCollider.rpm > 0)
            {
                currentGear ++;
            }

            //Shift Up
            if (currentSpeed < gearSpeeds[currentGear - 1])
            {
                currentGear--;
            }

            EngineSound();     
        }

        void EngineSound()
        {
            engineAudioSource.pitch = Mathf.MoveTowards(engineAudioSource.pitch, currentSpeed / gearSpeeds[currentGear] + 1, 0.05f);

            engineAudioSource.volume = Mathf.MoveTowards(engineAudioSource.volume, 0.75f + Mathf.Abs(motorInput), 0.05f);
        }

        void WheelAllignment()
        {

            for (int i = 0; i < wheelcolliderList.Count; i++)
            {

                Quaternion rot;
                Vector3 pos;

                wheelcolliderList[i].GetWorldPose(out pos, out rot);

                //Set rotation & position of the wheels
                wheeltransformList[i].rotation = rot;
                wheeltransformList[i].position = pos;
            }
        }

        void WheelSpin()
        {
            for(int i = 0; i < wheelsComponent.Count; i++)
            {
                WheelHit wheelHit;
                wheelcolliderList[i].GetGroundHit(out wheelHit);

                if (Mathf.Abs(wheelHit.forwardSlip) >= forwardSlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= sidewaySlipLimit)
                {
                    wheelsComponent[i].shouldEmit = true;
                }
                else
                {
                    wheelsComponent[i].shouldEmit = false;
                }

                //Handle drift points in a sideways slip
                if (GetComponent<DriftPointController>())
                {
                    GetComponent<DriftPointController>().drifting = Mathf.Abs(wheelHit.sidewaysSlip) >= .2f;
                }
            }
        }

        void DriftController()
        {
            DriftPointController driftController = GetComponent<DriftPointController>();

            if (driftController != null)
            {
                WheelHit rearWheelHit;

                RL_WheelCollider.GetGroundHit(out rearWheelHit);

                driftController.drifting = Mathf.Abs(rearWheelHit.sidewaysSlip) > sidewaySlipLimit;
            }
        }

        public void StabilizerBars()
        {

            WheelHit FrontWheelHit;

            float travelFL = 1.0f;
            float travelFR = 1.0f;

            bool groundedFL = FL_WheelCollider.GetGroundHit(out FrontWheelHit);

            if (groundedFL)
                travelFL = (-FL_WheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FL_WheelCollider.radius) / FL_WheelCollider.suspensionDistance;

            bool groundedFR = FR_WheelCollider.GetGroundHit(out FrontWheelHit);

            if (groundedFR)
                travelFR = (-FR_WheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FR_WheelCollider.radius) / FR_WheelCollider.suspensionDistance;

            float antiRollForceFront = (travelFL - travelFR) * antiRollAmount;

            if (groundedFL)
                rigid.AddForceAtPosition(FL_WheelCollider.transform.up * -antiRollForceFront, FL_WheelCollider.transform.position);
            if (groundedFR)
                rigid.AddForceAtPosition(FR_WheelCollider.transform.up * antiRollForceFront, FR_WheelCollider.transform.position);

            WheelHit RearWheelHit;

            float travelRL = 1.0f;
            float travelRR = 1.0f;

            bool groundedRL = RL_WheelCollider.GetGroundHit(out RearWheelHit);

            if (groundedRL)
                travelRL = (-RL_WheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RL_WheelCollider.radius) / RL_WheelCollider.suspensionDistance;

            bool groundedRR = RR_WheelCollider.GetGroundHit(out RearWheelHit);

            if (groundedRR)
                travelRR = (-RR_WheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RR_WheelCollider.radius) / RR_WheelCollider.suspensionDistance;

            float antiRollForceRear = (travelRL - travelRR) * antiRollAmount;

            if (groundedRL)
                rigid.AddForceAtPosition(RL_WheelCollider.transform.up * -antiRollForceRear, RL_WheelCollider.transform.position);
            if (groundedRR)
                rigid.AddForceAtPosition(RR_WheelCollider.transform.up * antiRollForceRear, RR_WheelCollider.transform.position);

            if (groundedRR && groundedRL && currentSpeed > 5.0f)
                rigid.AddRelativeTorque((Vector3.up * (steerInput * motorInput)) * 5000f);
        }


        void ApplyDownforce()
        {
            rigid.AddForce(-transform.up * downforce * rigid.velocity.magnitude);
        }


        void Traction()
        {
            switch (_propulsion)
            {
                case Propulsion.RWD:
                    WheelFrictionCurve rearWheelStiffness = RL_WheelCollider.sidewaysFriction;
                    rearWheelStiffness.stiffness = traction + 1.0f;

                    RL_WheelCollider.sidewaysFriction = rearWheelStiffness;
                    RR_WheelCollider.sidewaysFriction = rearWheelStiffness;
                    break;

                case Propulsion.FWD:
                    WheelFrictionCurve frontWheelStiffness = FL_WheelCollider.sidewaysFriction;
                    frontWheelStiffness.stiffness = traction + 1.0f;
               
                    FL_WheelCollider.sidewaysFriction = frontWheelStiffness;
                    FR_WheelCollider.sidewaysFriction = frontWheelStiffness;
                    break;
            }
        }

        void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                wheelcolliderList[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return;
            }

            if (Mathf.Abs(currentRotation - transform.eulerAngles.y) < 10f)
            {
                //float turnadjust = (transform.eulerAngles.y - currentRotation) * (steerHelper / 2);
                float turnadjust = (transform.eulerAngles.y - currentRotation) * (steerHelper);
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                rigid.velocity = velRotation * rigid.velocity;
            }

            currentRotation = transform.eulerAngles.y;
        }

        void Slipstream()
        {
            Vector3 fwd;
            RaycastHit hit;

            fwd = new Vector3(transform.position.x, transform.position.y + slipstreamRayHeight, transform.position.z);

            if (Physics.Raycast(fwd, transform.forward, out hit, slipstreamRayLength))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreCollision") && hit.transform.root.GetComponent<Statistics>())
                {
                    if (currentSpeed > 40 && motorInput > 0)
                        rigid.AddForce(transform.forward * slipstreamStrength,ForceMode.Acceleration);
                }
            }

            #if UNITY_EDITOR
            fwd = new Vector3(transform.position.x, transform.position.y + slipstreamRayHeight, transform.position.z);
            Debug.DrawRay(fwd, transform.forward * slipstreamRayLength, Color.cyan);
            #endif
        }

        void Nitro()
        {
            if (usingNitro && nitroCapacity > 0f && motorInput > 0)
            {

                //increase top speed
                topSpeed = nitroTopSpeed;

                //deplete nitro
                nitroCapacity = Mathf.MoveTowards(nitroCapacity, 0, nitroDepletionRate * Time.deltaTime);

                //add nitro boost
                rigid.AddForce(transform.forward * nitroStrength,ForceMode.Acceleration);

                //handle sound
                if (nitroAudioSource)
                {
                    if (!nitroAudioSource.isPlaying)
                    {
                        nitroAudioSource.Play();
                    }

                    nitroAudioSource.volume = Mathf.Lerp(nitroAudioSource.volume, 1.0f, Time.deltaTime * 2f);
                    nitroAudioSource.pitch = Mathf.Lerp(nitroAudioSource.pitch, 1.5f, Time.deltaTime * 2f);
                }

                //activate nitro
                if (nitroGroup)
                {
                    foreach (Transform p in nitroGroup.transform)
                    {
                        if (p.GetComponent<ParticleSystem>())
                        {
                            p.GetComponent<ParticleSystem>().Emit(1);
                        }
                    }
                }
            }
            else
            {
                //handle sound
                if (nitroAudioSource)
                {
                    nitroAudioSource.volume = Mathf.Lerp(nitroAudioSource.volume, 0.0f, Time.deltaTime * 2f);
                    nitroAudioSource.pitch = Mathf.Lerp(nitroAudioSource.pitch, 1.0f, Time.deltaTime * 2f);
                }

                //reset top speed
                topSpeed = topspeedCache;

                //recharge nitro
                if (!usingNitro && nitroRegenerationRate > 0)
                    nitroCapacity = Mathf.MoveTowards(nitroCapacity, 1, nitroRegenerationRate * Time.deltaTime);
            }
        }

        void OnCollisionEnter(Collision col)
        {

            if (col.relativeVelocity.magnitude < 5.0f) return;

            //Get volume based on velocity
            float hitVol = Mathf.InverseLerp(5, 100, currentSpeed);

            foreach (ContactPoint contact in col.contacts)
            {     
                //Play a sound at the contact point
                if (SoundManager.instance && crashSounds.Count > 0)
                {
                    SoundManager.instance.PlayClip(crashSounds[Random.Range(0, crashSounds.Count)], contact.point, hitVol, 1.0f);
                }
            }

            float damagevalue = Mathf.InverseLerp(30, 150, currentSpeed) * 10;

            //Debug.Log("damage = " + damagevalue.ToString() + ", speed = " + currentSpeed.ToString());

            m_Health -= (int)(damagevalue);
            if (m_Health <= 0)
            {
                Statistics stats = GetComponent<Statistics>();
                RaceManager.instance.RespawnRacer(transform, stats.lastPassedNode, 3.0f);
                m_Health = 100;
            }
        }

        private bool CheckForPenalty()
        {
            return wheelsComponent[3].wheelOnPenaltySurface;
        }

        private float CalculateSpeed()
        {
            //Calculate currentSpeed(MPH)
            currentSpeed = (_speedUnit == SpeedUnit.MPH) ? rigid.velocity.magnitude * 2.237f : rigid.velocity.magnitude * 3.6f;

            //Round currentSpeed
            currentSpeed = Mathf.Round(currentSpeed);

            //Never return a negative value
            return Mathf.Abs(currentSpeed);
        }

        private Vector3 CalculateVelocityDirection()
        {
            return transform.InverseTransformDirection(rigid.velocity);
        }

        public void GetItems(int type)
        {
            if (m_isPlayer == false)
                return;

            if (type == 1) m_BulletCount++;
            if (type == 2) m_RocketCount++;
            if (type == 3) m_MineCount++;

            if (m_BulletCount > 0) m_BulletSprite.SetActive(true);
            if (m_RocketCount > 0) m_RocketSprite.SetActive(true);
            if (m_MineCount > 0) m_MineSprite.SetActive(true);
        }

        private void UseItem()
        {
            if (m_isPlayer == false)
                return;

            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (m_BulletCount > 0)
                {
                    Shoot(0);
                    m_BulletCount--;
                    if (m_BulletCount <= 0)
                        m_BulletSprite.SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (m_RocketCount > 0)
                {
                    Shoot(1);
                    m_RocketCount--;
                    if (m_RocketCount <= 0)
                        m_RocketSprite.SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (m_MineCount > 0)
                {
                    DropMine();
                    m_MineCount--;
                    if (m_MineCount <= 0)
                        m_MineSprite.SetActive(false);
                }
            }
        }

        private void DropMine()
        {
            Instantiate(Resources.Load("item/Mine", typeof(GameObject)), transform.position, transform.rotation);
        }

        public void Damage(int _damage)
        {
            m_Health -= _damage;

            if (m_Health <= 0)
            {
                Statistics stats = GetComponent<Statistics>();
                RaceManager.instance.RespawnRacer(transform, stats.lastPassedNode, 3.0f);
                m_Health = 100;
            }
        }
    }
}
