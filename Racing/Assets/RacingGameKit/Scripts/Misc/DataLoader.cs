//DataLoader.cs simply loads race preferences and assigns them to the RaceManager.
using UnityEngine;
using System.Collections;

namespace RGSK
{
    public class DataLoader : MonoBehaviour
    {

        public string ResourceFolder = "PlayerVehicles/"; //the name of the folder within the Resources folder where your vehicles are stored.

        //called by the RaceManager.
        public void LoadRacePreferences()
        {

            //load player vehicles from the resources folder
            if (PlayerPrefs.HasKey("PlayerVehicle"))
            {
                Debug.Log("Loading Player Vehicle : " + PlayerPrefs.GetString("PlayerVehicle"));
                RaceManager.instance.playerCar = (GameObject)Resources.Load(ResourceFolder + PlayerPrefs.GetString("PlayerVehicle"));
                //RaceManager.instance.playerCar = (GameObject)Resources.Load(ResourceFolder + "Car3");
            }

            //load a player name
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                if (PlayerPrefs.GetString("PlayerName") != "")
                {
                    Debug.Log("Loading Player Name : " + PlayerPrefs.GetString("PlayerName"));
                    RaceManager.instance.playerName = PlayerPrefs.GetString("PlayerName");
                }
            }

            //load laps
            if (PlayerPrefs.HasKey("Laps"))
            {
                Debug.Log("Loading Laps : " + PlayerPrefs.GetInt("Laps"));
                RaceManager.instance.totalLaps = PlayerPrefs.GetInt("Laps");
            }

            //load racers
            if (PlayerPrefs.HasKey("Opponents"))
            {
                Debug.Log("Loading Racers : " + (PlayerPrefs.GetInt("Opponents") + 1));
                RaceManager.instance.totalRacers = PlayerPrefs.GetInt("Opponents") + 1;
            }

            //load AI difficulty
            if (PlayerPrefs.HasKey("AiDifficulty"))
            {
                switch (PlayerPrefs.GetString("AiDifficulty"))
                {
                    case "Easy":
                        Debug.Log("Loading AI Difficulty : " + OpponentControl.AiDifficulty.Easy.ToString());
                        RaceManager.instance.aiDifficulty = OpponentControl.AiDifficulty.Easy;
                        break;

                    case "Medium":
                        Debug.Log("Loading AI Difficulty : " + OpponentControl.AiDifficulty.Meduim.ToString());
                        RaceManager.instance.aiDifficulty = OpponentControl.AiDifficulty.Meduim;
                        break;

                    case "Hard":
                        Debug.Log("Loading AI Difficulty : " + OpponentControl.AiDifficulty.Hard.ToString());
                        RaceManager.instance.aiDifficulty = OpponentControl.AiDifficulty.Hard;
                        break;
                }
            }

            //load race type
            if (PlayerPrefs.HasKey("RaceType"))
            {

                switch (PlayerPrefs.GetString("RaceType"))
                {
                    case "":
                        Debug.Log("Race type was not loaded");
                        break;

                    case "Circuit":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.Circuit);
                        RaceManager.instance._raceType = RaceManager.RaceType.Circuit;
                        break;

                    /*case "Sprint":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.Sprint);
                        RaceManager.instance._raceType = RaceManager.RaceType.Sprint;
                        break;
                    */

                    case "LapKnockout":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.LapKnockout);
                        RaceManager.instance._raceType = RaceManager.RaceType.LapKnockout;
                        break;

                    case "TimeTrial":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.TimeTrial);
                        RaceManager.instance._raceType = RaceManager.RaceType.TimeTrial;
                        break;

                    case "SpeedTrap":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.SpeedTrap);
                        RaceManager.instance._raceType = RaceManager.RaceType.SpeedTrap;
                        break;

                    case "Checkpoints":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.Checkpoints);
                        RaceManager.instance._raceType = RaceManager.RaceType.Checkpoints;
                        break;

                    case "Elimination":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.Elimination);
                        RaceManager.instance._raceType = RaceManager.RaceType.Elimination;
                        break;

                    case "Drift":
                        Debug.Log("Loading Race Type : " + RaceManager.RaceType.Drift);
                        RaceManager.instance._raceType = RaceManager.RaceType.Drift;
                        break;
                }
            }
        }
    }
}
