using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{
    public class Motorbike_Controller : MonoBehaviour
    {

        public enum SpeedUnit { MPH, KPH }
        public SpeedUnit _speedUnit = SpeedUnit.MPH;

        //Wheel Transforms
        public Transform frontWheelTransform;
        public Transform rearWheelTransform;
        private List<Transform> wheeltransformList = new List<Transform>();

        //Wheel Colliders
        public WheelCollider frontWheelCollider;
        public WheelCollider rearWheelCollider;
        private List<WheelCollider> wheelcolliderList = new List<WheelCollider>();
        private List<Wheels> wheelsComponent = new List<Wheels>();

        //Wheel Slip settings
        public float forwardSlipLimit = 0.4f;
        public float sidewaySlipLimit = 0.35f;

        //Engine values
        public float engineTorque = 1000.0f; //avoid setting this value too high inorder to not overpower the wheels!
        public float brakeTorque = 1000.0f; //force applied to wheels to stop
        public float maxSteerAngle = 30.0f;
        public float topSpeed = 150.0f;
        public float boost = 2000.0f;
        public float currentSpeed;
        private float currentSteerAngle;
        public float brakeForce = 8000.0f; //force added to stop the bike

        //Gear values
        public int numberOfGears = 6;
        public int currentGear;
        private float[] gearSpeeds;

        //Stability
        private Rigidbody rigid;
        public Vector3 centerOfMass;
        public float downforce = 100.0f;
        public float leanAmount = 60.0f;
        public float maxLeanAngle = 40.0f;
        public float leanDamping = 2.5f;
        [Range(0, 1)]
        public float steerHelper = 0.5f;
        [Range(0, 1)]
        public float traction = 0.5f;
        public float lean = 0.0f;

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
        public GameObject chassis;
        public GameObject handlebars;
        public GameObject brakelightGroup;
        private float speedLimit;
        private Vector3 velocityDir;
        private float currentRotation;
        public float topspeedCache;

        //Slipstream
        public float slipstreamRayHeight = 0f;
        public float slipstreamRayLength = 20.0f;
        [Range(0.1f, 5)]
        public float slipstreamStrength = 2.5f;


        //Nitro
        [Range(0, 10)]
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

        void Start()
        {
            SetupRigidbody();

            SetupWheels();

            SetupGears();

            SetupAudio();

            SetupMisc();
        }

        void SetupRigidbody()
        {
            rigid = GetComponent<Rigidbody>();

            rigid.centerOfMass = centerOfMass;

            rigid.constraints = RigidbodyConstraints.FreezeRotationZ;

            rigid.drag = 0.01f;

            rigid.angularDrag = 0;

            rigid.maxAngularVelocity = 2.0f;
        }

        void SetupWheels()
        {
            wheelcolliderList.Clear();
            wheeltransformList.Clear();

            //Add wheel transfoms/colliders to the lists
            wheeltransformList.Add(frontWheelTransform); wheelcolliderList.Add(frontWheelCollider); wheelsComponent.Add(frontWheelCollider.GetComponent<Wheels>());
            wheeltransformList.Add(rearWheelTransform); wheelcolliderList.Add(rearWheelCollider); wheelsComponent.Add(rearWheelCollider.GetComponent<Wheels>());

            for (int i = 0; i < wheelsComponent.Count; i++)
            {
                wheelsComponent[i].SetupWheelCollider(150, 60000, 4500);
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

                foreach (Transform p in nitroGroup.transform)
                {
                    if (p.GetComponent<ParticleSystem>())
                    {
                        var em = p.GetComponent<ParticleSystem>().emission;
                        em.enabled = false;
                    }
                }
            }

            if (controllable && RaceManager.instance)
                controllable = false;
        }


        void Update()
        {
            ShiftGears();
            WheelSpin();
            WheelAllignment();
            currentSpeed = CalculateSpeed();
        }

        void FixedUpdate()
        {
            Ride();
            SpeedLimiter();
            WheelAllignment();
            ApplyDownforce();
            LeanChassis();
            velocityDir = CalculateVelocityDirection();
        }

        void Ride()
        {
            if (!controllable) return;

            //Power the rear wheel
            if (currentSpeed <= speedLimit)
            {
                if (!reversing)
                {
                    rearWheelCollider.motorTorque = engineTorque * motorInput;
                }
                else
                {
                    rearWheelCollider.motorTorque = engineTorque * -brakeInput;
                }
            }

            
            //Reduce our steer angle depending on how fast the bike is moving
            currentSteerAngle = Mathf.Lerp(maxSteerAngle, (maxSteerAngle / 2), (currentSpeed / (topSpeed * 3.0f)));
            frontWheelCollider.steerAngle = Mathf.Clamp((currentSteerAngle * steerInput), -maxSteerAngle, maxSteerAngle);

            Brake();
            SteerHelper();
            Traction();

            //Reverse
            if (brakeInput > 0 && velocityDir.z <= 0.01f)
            {
                reversing = true;
                speedLimit = 10.0f;
            }
            else
            {
                reversing = false;
                speedLimit = topSpeed;
            }

            //Boost
            if (currentSpeed < speedLimit && motorInput > 0)
            {
                rigid.AddForce(transform.forward * boost);
            }

            //Handle slipstream
            if (enableSlipstream)
            {
                Slipstream();
            }

            //Handle Nito 
            if (enableNitro)
            {
                Nitro();
            }
        }

        void Brake()
        {
            //Footbrake
            if (!reversing && brakeInput > 0.0f)
            {

                //add a backward force to help stop the bike
                rigid.AddForce(-transform.forward * brakeForce);

                rearWheelCollider.brakeTorque = brakeTorque * brakeInput;
                rearWheelCollider.motorTorque = 0;
            }
            else {
                rearWheelCollider.brakeTorque = 0;
                frontWheelCollider.brakeTorque = 0;
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
                brakelightGroup.SetActive(motorInput < 0);
        }

        void SpeedLimiter()
        {
            if (currentSpeed > speedLimit)
            { 
                rigid.velocity = _speedUnit == SpeedUnit.MPH ? (speedLimit / 2.237f) * rigid.velocity.normalized : (speedLimit / 3.6f) * rigid.velocity.normalized;
                rearWheelCollider.motorTorque = 0;       
            }
        }

        void ShiftGears()
        {
            //Shift Down
            if (currentSpeed >= gearSpeeds[currentGear] && rearWheelCollider.rpm > 0)
            {
                currentGear++;
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
            //rotate
            wheeltransformList[0].Rotate(wheelcolliderList[0].rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheeltransformList[1].Rotate(wheelcolliderList[1].rpm / 60 * 360 * Time.deltaTime, 0, 0);

            //steer
            wheeltransformList[0].localEulerAngles = new Vector3(wheeltransformList[0].localEulerAngles.x, wheelcolliderList[0].steerAngle - wheeltransformList[0].localEulerAngles.z, wheeltransformList[0].localEulerAngles.z);

            //set handle bar rotation
            if (handlebars)
                handlebars.transform.rotation = frontWheelCollider.transform.rotation * Quaternion.Euler(0, frontWheelCollider.steerAngle, 0);
        }

        void WheelSpin()
        {
            for (int i = 0; i < wheelsComponent.Count; i++)
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

        void LeanChassis()
        {
            if (!chassis) return;

            //the replay manager / ghost playback will handle setting the lean during replays due to the angularVelocity
            if (ReplayManager.instance && ReplayManager.instance.replayState == ReplayManager.ReplayState.Playing || GetComponent<GhostVehicle>() && GetComponent<GhostVehicle>().play) return;

            WheelHit wheelHit;
            frontWheelCollider.GetGroundHit(out wheelHit);

            float normalAngle = transform.InverseTransformDirection(rigid.velocity).z > 0f ? -1 : 1;

            lean = Mathf.Clamp(Mathf.Lerp(lean, (transform.InverseTransformDirection(rigid.angularVelocity).y * normalAngle) * leanAmount, Time.deltaTime * leanDamping), -maxLeanAngle, maxLeanAngle);

            Quaternion targetAngle = Quaternion.Euler(0, chassis.transform.localEulerAngles.y, lean);

            chassis.transform.localRotation = targetAngle;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);

            rigid.centerOfMass = new Vector3((centerOfMass.x) * transform.localScale.x, (centerOfMass.y) * transform.localScale.y, (centerOfMass.z) * transform.localScale.z);
        }

        void ApplyDownforce()
        {
            rigid.AddForce(-transform.up * downforce * rigid.velocity.magnitude);
        }

        void SteerHelper()
        {

            for (int i = 0; i < 2; i++)
            {
                WheelHit wheelhit;
                wheelcolliderList[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return;
            }

            if (Mathf.Abs(currentRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - currentRotation) * steerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                rigid.velocity = velRotation * rigid.velocity;
            }

            currentRotation = transform.eulerAngles.y;
        }


        void Traction()
        {
            WheelFrictionCurve wheelStiffness = rearWheelCollider.sidewaysFriction;
            wheelStiffness.stiffness = traction + 1.0f;
            rearWheelCollider.sidewaysFriction = wheelStiffness;
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
                        rigid.AddForce(transform.forward * slipstreamStrength, ForceMode.Acceleration);
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
    }
}
