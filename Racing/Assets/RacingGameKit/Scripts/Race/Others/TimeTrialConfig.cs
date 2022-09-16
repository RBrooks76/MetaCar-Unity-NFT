//Simple class that initializes a time trial race.
using UnityEngine;
using System.Collections;

namespace RGSK
{
    public class TimeTrialConfig : MonoBehaviour
    {

		private bool runningRoutine;

        void Start()
        {
        
        	RaceManager.instance.SwitchRaceState(RaceManager.RaceState.Racing);
            CameraManager.instance.ActivatePlayerCamera();

            //Set AI to drive to the starting point
            if (RaceManager.instance.timeTrialAutoDrive)
            {
                GetComponent<Statistics>().AIMode();

                RaceUI.instance.ShowRaceInfo("Auto Drive...", 5.0f, Color.white);
            }
            else
            {
                GetComponent<Statistics>().PlayerMode();
            }

            if (GetComponent<Car_Controller>())
                GetComponent<Car_Controller>().controllable = true;

            if (GetComponent<Motorbike_Controller>())
                GetComponent<Motorbike_Controller>().controllable = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "FinishLine" || other.tag == "Finish")
            {
                if(!runningRoutine)
                    StartCoroutine(StartTimeTrial());
            }
        }

        IEnumerator StartTimeTrial()
        {
			
			runningRoutine = true;
			
            //Handle UI
            RaceUI.instance.SetCountDownText("GO!");

            SoundManager.instance.PlayDefaultSound(SoundManager.instance.defaultSounds.startRaceSound);

            RaceManager.instance.StartRace();

            //Begin recording the ghost vehicle
            if (GetComponent<GhostVehicle>())
            {
                 GetComponent<GhostVehicle>().record = true;
            }

            //Enable player input and get rid of AI
            GetComponent<Statistics>().PlayerMode();

            yield return new WaitForSeconds(1);

            RaceUI.instance.SetCountDownText(string.Empty);

            Destroy(this);
        }
    }
}
