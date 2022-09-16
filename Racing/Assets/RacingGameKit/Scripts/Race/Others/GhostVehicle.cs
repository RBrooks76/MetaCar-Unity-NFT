//GhostVehicle.cs handles ghost behaviour - recording positions & playing them back.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{
    public class GhostVehicle : MonoBehaviour
    {
        [System.Serializable]
        public class VehicleState
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;
            public float Throttle;
            public float Brake;
            public float Handbrake;
            public float Steer;

            public VehicleState(Vector3 p, Quaternion r, Vector3 v, Vector3 av, float t, float b, float hb, float s)
            {
                Position = p;
                Rotation = r;
                Velocity = v;
                AngularVelocity = av;
                Throttle = t;
                Brake = b;
                Handbrake = hb;
                Steer = s;
            }
        }

        [System.Serializable]
        public class MotorbikeChassisState
        {
            public Quaternion chassisRot;

            public MotorbikeChassisState(Quaternion r)
            {
                chassisRot = r;
            }
        }

        [HideInInspector]
        public List<VehicleState> vehicleState = new List<VehicleState>();

        [HideInInspector]
        public List<VehicleState> vehicleStateCache = new List<VehicleState>();

        [HideInInspector]
        public List<MotorbikeChassisState> motorbikeChassisState = new List<MotorbikeChassisState>();

        [HideInInspector]
        public bool record = false;

        [HideInInspector]
        public bool play = false;

        private Car_Controller carController;
        private Motorbike_Controller motorbikeController;
        private Transform motorbikeChassis;

        private int CurrentFrame = 0;
        

        void Start()
        {
            if (GetComponent<Car_Controller>())
            {
                carController = GetComponent<Car_Controller>();
            }

            if (GetComponent<Motorbike_Controller>())
            {
                motorbikeController = GetComponent<Motorbike_Controller>();

                if (motorbikeController.chassis)
                    motorbikeChassis = motorbikeController.chassis.transform;

            }

            CurrentFrame = 0;
        }


        void FixedUpdate()
        {
            if (record)
            {
                Record();
            }

            if (play)
            {
                Playback();
            }
        }

        void Record()
        {
            //Record

            if (carController)
            {
                vehicleState.Add(new VehicleState(transform.position, transform.rotation, transform.GetComponent<Rigidbody>().velocity, transform.GetComponent<Rigidbody>().angularVelocity, carController.motorInput, carController.brakeInput, carController.handbrakeInput, carController.steerInput));
            }

            if (motorbikeController)
            {
                vehicleState.Add(new VehicleState(transform.position, transform.rotation, transform.GetComponent<Rigidbody>().velocity, transform.GetComponent<Rigidbody>().angularVelocity, motorbikeController.motorInput, motorbikeController.brakeInput, 0.0f, motorbikeController.steerInput));

                if (motorbikeChassis)
                    motorbikeChassisState.Add(new MotorbikeChassisState(motorbikeChassis.localRotation));
            }
        }

        void Playback()
        {
            //Playback

            CurrentFrame += 1;

            if (CurrentFrame < vehicleState.Count - 1)
            {               
                SetVehicleStateFromReplayFrame(transform.GetComponent<Rigidbody>(), vehicleState[CurrentFrame].Position, vehicleState[CurrentFrame].Rotation, vehicleState[CurrentFrame].Velocity, vehicleState[CurrentFrame].AngularVelocity, vehicleState[CurrentFrame].Throttle, vehicleState[CurrentFrame].Brake, vehicleState[CurrentFrame].Handbrake, vehicleState[CurrentFrame].Steer, true);

                if (motorbikeChassis)
                    SetMotorbikeChassisRotation(motorbikeChassis, motorbikeChassisState[CurrentFrame].chassisRot);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        
        public void CacheValues()
        {
            //Cache Values

            vehicleStateCache.Clear();

            for (int i = 0; i < vehicleState.Count; i++)
            {
                vehicleStateCache.Add(vehicleState[i]);
            }
        }

        public void UseCachedValues()
        {
            //Retrieve Cached Values

            if (vehicleStateCache.Count <= 0)
                return;

            vehicleState.Clear();

            for (int i = 0; i < vehicleStateCache.Count; i++)
            {
                vehicleState.Add(vehicleStateCache[i]);
            }
        }

        public void ClearValues()
        {
            vehicleState.Clear();
        }

        public void StartGhost()
        {
            //Enter play and disable uneccesary components from the ghost

            record = false;

            play = true;

            Transform[] c = transform.GetComponentsInChildren<Transform>();

            foreach(Transform t in c)
            {
                //Get rid of these because we dont want to track anything on a ghost vehicle
                if (t.GetComponent<Statistics>())
                {
                    Destroy(t.GetComponent<Statistics>());
                }

                if (t.GetComponent<ProgressTracker>())
                {
                    Destroy(t.GetComponent<ProgressTracker>());
                }

                //Disable other components that aren't needed by the ghost
                if (t.GetComponent<AudioSource>())
                {
                    t.GetComponent<AudioSource>().mute = true;
                }

                if (t.GetComponent<ParticleSystem>())
                {
                    t.GetComponent<ParticleSystem>().gameObject.SetActive(false);
                }
               
                if (t.GetComponent<WaypointArrow>())
                {
                    t.GetComponent<WaypointArrow>().enabled = false;
                }

                if (t.GetComponent<Wheels>())
                {
                    t.GetComponent<Wheels>().enabled = false;
                }

                //Get rid of the brake lights for the ghost vehicle
                if (t.GetComponent<Car_Controller>())
                {
                    if(t.GetComponent<Car_Controller>().brakelightGroup != null)
                    {
                        Destroy(t.GetComponent<Car_Controller>().brakelightGroup);
                        t.GetComponent<Car_Controller>().brakelightGroup = null;
                    }
                }

                if (t.GetComponent<Motorbike_Controller>())
                {
                    if (t.GetComponent<Motorbike_Controller>().brakelightGroup != null)
                    {
                        Destroy(t.GetComponent<Motorbike_Controller>().brakelightGroup);
                        t.GetComponent<Motorbike_Controller>().brakelightGroup = null;
                    }
                }
            }
        }

        private void SetVehicleStateFromReplayFrame(Rigidbody rigid, Vector3 Pos, Quaternion Rot, Vector3 Vel, Vector3 aVel, float Throttle, float Brake, float Handbrake, float Steer, bool normalSpeed)
        {
            if (normalSpeed)
            {
                rigid.MovePosition(Pos);

                rigid.MoveRotation(Rot);
            }
            else
            {
                rigid.transform.position = Pos;

                rigid.transform.rotation = Rot;
            }

            rigid.velocity = Vel;
            rigid.angularVelocity = aVel;

            rigid.isKinematic = !normalSpeed;

            //Handle input playback 
            if (carController)
            {
                carController.controllable = true;

                carController.motorInput = Throttle;

                carController.brakeInput = Brake;

                carController.handbrakeInput = Handbrake;

                carController.steerInput = Steer;
            }

            if (motorbikeController)
            {
                motorbikeController.controllable = true;

                motorbikeController.motorInput = Throttle;

                motorbikeController.brakeInput = Brake;

                motorbikeController.steerInput = Steer;
            }
        }

        private void SetMotorbikeChassisRotation(Transform chassis, Quaternion rot)
        {
            chassis.localRotation = rot;
        }

        /* Saving and Loading ghost vehicles (WIP)
        public void SaveValues(string raceTrack)
         {
             BinaryFormatter bf = new BinaryFormatter();

             // Construct a SurrogateSelector object

             SurrogateSelector ss = new SurrogateSelector();
             Vectro3SerializationSurrogate v3ss = new Vectro3SerializationSurrogate();
             QuaternionSerializationSurrogate qss = new QuaternionSerializationSurrogate();
             ss.AddSurrogate(typeof(Vector3),new StreamingContext(StreamingContextStates.All),v3ss);
             ss.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), qss);

             // Have the formatter use our surrogate selector
             bf.SurrogateSelector = ss;

             FileStream file = File.Create(Application.dataPath + raceTrack + ".ghostData");
             bf.Serialize(file, vehiclePositionCache);
             file.Close();
             Debug.Log("Saved ghost");
         }

         public void LoadValues(string raceTrack)
         {
             if (File.Exists(Application.dataPath + raceTrack + ".ghostData"))
             {
                 BinaryFormatter bf = new BinaryFormatter();

                 // Construct a SurrogateSelector object
                 SurrogateSelector ss = new SurrogateSelector();
                 Vectro3SerializationSurrogate v3ss = new Vectro3SerializationSurrogate();
                 QuaternionSerializationSurrogate qss = new QuaternionSerializationSurrogate();
                 ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);
                 ss.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), qss);

                 // Have the formatter use our surrogate selector
                 bf.SurrogateSelector = ss;

                 FileStream file = File.Open(Application.dataPath + raceTrack + ".ghostData", FileMode.Open);
                 List <VehiclePositions> vp = (List<VehiclePositions>)bf.Deserialize(file);
                 vehiclePosition = vp;
                 file.Close();

                 //Find the vehicle that was used
                 GameObject[] allVehicles = Resources.LoadAll<GameObject>("PlayerVehicles");
                 for(int i = 0; i < allVehicles.Length; i++)
                 {
                     if (allVehicles[i].name == vehiclePosition[0].vehicleResourceName)
                     {
                         Debug.Log(allVehicles[i].name + " was the vehicle used");
                         RaceManager.instance.CreateGhostVehicle(allVehicles[i]);s
                     }
                 }
                 Debug.Log("Loaded ghost");
             }
         }
         */
    }
}