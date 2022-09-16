//ReplayManager.cs handles recording racer positions and playing them back when viweing a replay
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{
    public class ReplayManager : MonoBehaviour
    {
        [System.Serializable]
        public class Racer
        {
            public Transform racer;
            public List<VehicleState> vehicleState = new List<VehicleState>();
            public Car_Controller carController;
            public Motorbike_Controller motorbikeController;
            public Transform motorbikeChassis;
            public List<MotorbikeChassisState> motorbikeChassisState = new List<MotorbikeChassisState>();
        }

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

        public enum ReplayState { Recording, Playing, NONE }
        public static ReplayManager instance;

        [HideInInspector]
        public ReplayState replayState = ReplayState.NONE;

        [HideInInspector]
        public List<Racer> racers = new List<Racer>(new Racer[100]);

        [HideInInspector]
        public int CurrentFrame;

        [HideInInspector]
        public int TotalFrames;

        [HideInInspector]
        public float ReplayPercent;
        private int PlaybackSpeed = 1;

        void Awake()
        {
            instance = this;

            replayState = ReplayState.NONE;
        }

        void FixedUpdate()
        {
            switch (replayState)
            {
                case ReplayState.Recording:

                    Record();

                    break;

                case ReplayState.Playing:

                    Playback();

                    break;
            }
        }

        void Record()
        {
            for (int i = 0; i < racers.Count; i++)
            {
                //Record
                if (racers[i].carController)
                {
                    racers[i].vehicleState.Add(new VehicleState(racers[i].racer.position, racers[i].racer.rotation, racers[i].racer.GetComponent<Rigidbody>().velocity, racers[i].racer.GetComponent<Rigidbody>().angularVelocity, racers[i].carController.motorInput, racers[i].carController.brakeInput, racers[i].carController.handbrakeInput, racers[i].carController.steerInput));
                }

                if (racers[i].motorbikeController)
                {
                    racers[i].vehicleState.Add(new VehicleState(racers[i].racer.position, racers[i].racer.rotation, racers[i].racer.GetComponent<Rigidbody>().velocity, racers[i].racer.GetComponent<Rigidbody>().angularVelocity, racers[i].motorbikeController.motorInput, racers[i].motorbikeController.brakeInput, 0.0f, racers[i].motorbikeController.steerInput));

                    if (racers[i].motorbikeChassis)
                        racers[i].motorbikeChassisState.Add(new MotorbikeChassisState(racers[i].motorbikeChassis.localRotation));
                }
            }
        }

        void Playback()
        {

            CurrentFrame += 1 * PlaybackSpeed;

            if (CurrentFrame <= 2)
                AdjustPlaybackSpeed(1);


            for (int i = 0; i < racers.Count; i++)
            {
                if (CurrentFrame < racers[0].vehicleState.Count - 1)
                {
                    //Playback Replay
                    SetVehicleStateFromReplayFrame(racers[i].racer.GetComponent<Rigidbody>(), racers[i].vehicleState[CurrentFrame].Position, racers[i].vehicleState[CurrentFrame].Rotation, racers[i].vehicleState[CurrentFrame].Velocity, racers[i].vehicleState[CurrentFrame].AngularVelocity, racers[i].vehicleState[CurrentFrame].Throttle, racers[i].vehicleState[CurrentFrame].Brake, racers[i].vehicleState[CurrentFrame].Handbrake, racers[i].vehicleState[CurrentFrame].Steer, PlaybackSpeed == 1);

                    if (racers[i].motorbikeChassis)
                        SetMotorbikeChassisRotation(racers[i].motorbikeChassis, racers[i].motorbikeChassisState[CurrentFrame].chassisRot);
                }
                else
                {
                    //Restart Replay
                    RestartReplay();
                }
            }

            //Get the replay percentage
            ReplayPercent = (float)CurrentFrame / (float)TotalFrames;
        }

        public void GetRacersAndStartRecording(Statistics[] allRacers)
        {
            //Get the racer's vehicle controllers and start recording the replay

            racers.RemoveRange(allRacers.Length, racers.Count - allRacers.Length);

            for (int i = 0; i < racers.Count; i++)
            {
                racers[i].racer = allRacers[i].transform;

                if (racers[i].racer.GetComponent<Car_Controller>())
                {
                    racers[i].carController = racers[i].racer.GetComponent<Car_Controller>();
                }

                if (racers[i].racer.GetComponent<Motorbike_Controller>())
                {
                    racers[i].motorbikeController = racers[i].racer.GetComponent<Motorbike_Controller>();

                    if (racers[i].motorbikeController.chassis)
                        racers[i].motorbikeChassis = racers[i].motorbikeController.chassis.transform;
                }
            }

            replayState = ReplayState.Recording;
        }

        public void StopRecording()
        {
            replayState = ReplayState.NONE;
            TotalFrames = GetTotalFrames();
        }

        public void ViewReplay()
        {
            if (TotalFrames <= 0) return;

            CameraManager.instance.ActivateCinematicCamera();

            DisableInputs();  
      
            RaceManager.instance.SwitchRaceState(RaceManager.RaceState.Replay);

            replayState = ReplayState.Playing;

            if(CurrentFrame<=2) ResetRaceValues();
        }

        public void AdjustPlaybackSpeed(int s)
        {
            if (PlaybackSpeed == s)
            {
                PlaybackSpeed = 1; //reset PlaybackSpeed on ff & rw buttons
            }
            else
            {
                PlaybackSpeed = s;
            }

            //Reset timescale incase the replay is paused
            if(Time.timeScale < 1)
                Time.timeScale = 1.0f;
        }

        public void PauseReplay()
        {
            PlaybackSpeed = (PlaybackSpeed > 0 || PlaybackSpeed < 0) ? 0 : 1;

            Time.timeScale = PlaybackSpeed;
        }

        public void ExitReplay()
        {
            RaceManager.instance.SwitchRaceState(RaceManager.RaceState.Complete);

            AdjustPlaybackSpeed(1);
        }

        private void RestartReplay()
        {
            RaceUI.instance.StartCoroutine("ScreenFadeOut", 0.3f);

            ResetRaceValues();

            CurrentFrame = 0;
        }

        private int GetTotalFrames()
        {
            return racers[0].vehicleState.Count;
        }

        private void DisableInputs()
        {
            for (int i = 0; i < racers.Count; i++)
            {
                RaceManager.instance.DisableRacerInput(racers[i].racer.gameObject);
            }
        }

        /// <summary>
        /// Reset race values such as skidmarks, vehicle damage, etc
        /// </summary>
        private void ResetRaceValues()
        {
            if (GameObject.FindObjectOfType(typeof(Skidmark)))
            {
                Skidmark skidmarks = GameObject.FindObjectOfType(typeof(Skidmark)) as Skidmark;
                skidmarks.ClearSkidmarks();
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
            if (rigid.transform.GetComponent<Car_Controller>())
            {
                rigid.transform.GetComponent<Car_Controller>().motorInput = Throttle;

                rigid.transform.GetComponent<Car_Controller>().brakeInput = Brake;

                rigid.transform.GetComponent<Car_Controller>().handbrakeInput = Handbrake;

                rigid.transform.GetComponent<Car_Controller>().steerInput = Steer;
            }

            if (rigid.transform.GetComponent<Motorbike_Controller>())
            {
                rigid.transform.GetComponent<Motorbike_Controller>().motorInput = Throttle;

                rigid.transform.GetComponent<Motorbike_Controller>().brakeInput = Brake;

                rigid.transform.GetComponent<Motorbike_Controller>().steerInput = Steer;
            }
        }

        private void SetMotorbikeChassisRotation(Transform chassis, Quaternion rot)
        {
            chassis.localRotation = rot;
        }
    }
}
