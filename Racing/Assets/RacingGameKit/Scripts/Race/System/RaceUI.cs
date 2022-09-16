//Race_UI.cs handles displaying all UI in the race.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RGSK
{
    public class RaceUI : MonoBehaviour
    {

        #region Grouped UI Classes
        [System.Serializable]
        public class RacerInfoUI
        {
            public Text position;
            public Text name;
            public Text vehicleName;
            public Text bestLapTime;
            public Text totalTime;
        }

        [System.Serializable]
        public class RacingUI
        {
            public Text rank;
            public Text lap;
            public Text currentLapTime;
            public Text previousLapTime;
            public Text bestLapTime;
            public Text totalTime;
            public Text countdown;
            public Text raceInfo;
            public Text finishedText;

            [Header("In Race Standings")]
            public List<RacerInfoUI> inRaceStandings = new List<RacerInfoUI>();
            public Color playerColor = Color.green;
            public Color normalColor = Color.white;

            [Header("Wrongway Indication")]
            public Text wrongwayText;
            public Image wrongwayImage;
        }

        [System.Serializable]
        public class DriftingUI
        {
            public GameObject driftPanel;
            public Text totalDriftPoints;
            public Text currentDriftPoints;
            public Text driftMultiplier;
            public Text driftStatus;
            public Text goldPoints, silverPoints, bronzePoints;
        }

        [System.Serializable]
        public class DriftResults
        {
            public Text totalPoints;
            public Text driftRaceTime;
            public Text bestDrift;
            public Text longestDrift;
            public Image gold, silver, bronze;
        }

        [System.Serializable]
        public class VehicleUI
        {
            public Text currentSpeed;
            public Text currentGear;
            public Image nitroBar;
            public TextMesh speedText3D, gearText3D;
            private string speedUnit;

            [Header("Speedometer")]
            public RectTransform needle;
            public float minNeedleAngle = -20.0f;
            public float maxNeedleAngle = 220.0f;
            public float rotationMultiplier = 0.85f;
            [HideInInspector]
            public float needleRotation;
        }

        [System.Serializable]
        public class Rewards
        {
            public Text rewardCurrency;
            public Text rewardVehicle;
            public Text rewardTrack;
        }
        #endregion

        public static RaceUI instance;
        private Statistics player;
        private DriftPointController driftpointcontroller;

        [Header("Starting Grid UI")]
        public GameObject startingGridPanel;
        public List<RacerInfoUI> startingGrid = new List<RacerInfoUI>();

        [Header("Racing UI")]
        public GameObject racePanel;
        public GameObject pausePanel;
        public RacingUI racingUI;
        public DriftingUI driftUI;
        public VehicleUI vehicleUI;

        [Header("Fail Race UI")]
        public GameObject failRacePanel;
        public Text failTitle;
        public Text failReason;

        [Header("Race Finished UI")]
        public GameObject raceCompletePanel;
        public GameObject raceResultsPanel, driftResultsPanel;
        public List<RacerInfoUI> raceResults = new List<RacerInfoUI>();
        public DriftResults driftResults;
        public Rewards rewardTexts;
        public Slider m_Healthbar;

        [Header("Replay UI")]
        public GameObject replayPanel;
        public Image progressBar;

        [Header("ScreenFade")]
        public Image screenFade;
        public float fadeSpeed = 0.5f;
        public bool fadeOnStart = true;
        public bool fadeOnExit = true;

        [Header("Scene Ref")]
        public string menuScene = "Menu";

        [HideInInspector]
        public List<string> raceInfos = new List<string>();

        Car_Controller m_Car_Ctrl;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (fadeOnStart && screenFade) StartCoroutine(ScreenFadeOut(fadeSpeed));

            ClearUI();

            ConfigureUiBasedOnRaceType();

            UpdateUIPanels();
        }

        void ClearUI()
        {
            //Clear Starting Grid
            if (startingGrid.Count > 0)
            {
                for (int i = 0; i < startingGrid.Count; i++)
                {
                    startingGrid[i].position.text = string.Empty;
                    startingGrid[i].name.text = string.Empty;
                    startingGrid[i].vehicleName.text = string.Empty;
                }
            }

            //Clear In Race Standings
            if (racingUI.inRaceStandings.Count > 0)
            {
                for (int i = 0; i < racingUI.inRaceStandings.Count; i++)
                {
                    racingUI.inRaceStandings[i].position.text = (i + 1).ToString();

                    //Disable the parent if one exists so we can activate it later based on how many racers there are
                    if (racingUI.inRaceStandings[i].position.transform.parent)
                        racingUI.inRaceStandings[i].position.transform.parent.gameObject.SetActive(false);
                }
            }

            //Clear Race Reults
            if (raceResults.Count > 0)
            {
                for (int i = 0; i < raceResults.Count; i++)
                {
                    if (raceResults[i].position) raceResults[i].position.text = string.Empty;
                    if (raceResults[i].name) raceResults[i].name.text = string.Empty;
                    if (raceResults[i].totalTime) raceResults[i].totalTime.text = string.Empty;
                    if (raceResults[i].vehicleName) raceResults[i].vehicleName.text = string.Empty;
                    if (raceResults[i].bestLapTime) raceResults[i].bestLapTime.text = string.Empty;
                }
            }

            //Clear other texts
            if (racingUI.raceInfo) racingUI.raceInfo.text = string.Empty;
            if (racingUI.countdown) racingUI.countdown.text = string.Empty;
            if (racingUI.finishedText) racingUI.finishedText.text = string.Empty;
            if (rewardTexts.rewardCurrency) rewardTexts.rewardCurrency.text = string.Empty;
            if (rewardTexts.rewardVehicle) rewardTexts.rewardVehicle.text = string.Empty;
            if (rewardTexts.rewardTrack) rewardTexts.rewardTrack.text = string.Empty;
        }


        void ConfigureUiBasedOnRaceType()
        {
            if (!RaceManager.instance) return;

            if (driftUI.driftPanel) driftUI.driftPanel.SetActive(RaceManager.instance._raceType == RaceManager.RaceType.Drift);
            if (raceResultsPanel) raceResultsPanel.SetActive(RaceManager.instance._raceType != RaceManager.RaceType.Drift);
            if (driftResultsPanel) driftResultsPanel.SetActive(RaceManager.instance._raceType == RaceManager.RaceType.Drift);

            if (RaceManager.instance._raceType == RaceManager.RaceType.Drift)
            {
                if (driftUI.goldPoints) driftUI.goldPoints.text = RaceManager.instance.goldDriftPoints.ToString("N0");
                if (driftUI.silverPoints) driftUI.silverPoints.text = RaceManager.instance.silverDriftPoints.ToString("N0");
                if (driftUI.bronzePoints) driftUI.bronzePoints.text = RaceManager.instance.bronzeDriftPoints.ToString("N0");
            }
        }

        void Update()
        {
            if (!player)
            {
                if (GameObject.FindGameObjectWithTag("Player"))
                {
                    player = GameObject.FindGameObjectWithTag("Player").GetComponent<Statistics>();

                    if (player && player.GetComponent<DriftPointController>())
                        driftpointcontroller = player.GetComponent<DriftPointController>();
                }
            }
            else
            {
                UpdateUI();
                VehicleGUI();
            }
        }

        void UpdateUI()
        {
            if (!RaceManager.instance) return;

            switch (RaceManager.instance._raceType)
            {

                case RaceManager.RaceType.Circuit:
                    DefaultUI();
                    break;

                /*case RaceManager.RaceType.Sprint:
                    DefaultUI();
                    break;
                */

                case RaceManager.RaceType.LapKnockout:
                    DefaultUI();
                    break;

                case RaceManager.RaceType.TimeTrial:
                    TimeTrialUI();
                    break;

                case RaceManager.RaceType.SpeedTrap:
                    DefaultUI();
                    break;

                case RaceManager.RaceType.Checkpoints:
                    CheckpointRaceUI();
                    break;

                case RaceManager.RaceType.Elimination:
                    EliminationRaceUI();
                    break;

                case RaceManager.RaceType.Drift:
                    DriftRaceUI();
                    break;
            }

            switch (RaceManager.instance._raceState)
            {
                case RaceManager.RaceState.StartingGrid:
                    ShowStartingGrid();
                    break;

                case RaceManager.RaceState.Racing:
                    ShowInRaceStandings();
                    WrongwayUI();
                    break;

                case RaceManager.RaceState.Complete:
                    if (RaceManager.instance._raceType != RaceManager.RaceType.Drift)
                    {
                        ShowRaceResults();
                    }
                    else
                    {
                        ShowDriftResults();
                    }
                    break;

                case RaceManager.RaceState.Replay:
                    ShowReplayUI();
                    break;
            }
        }


        #region RaceTypes UI

        void DefaultUI()
        {
            //POS
            if (racingUI.rank)
                racingUI.rank.text = "Pos " + player.rank + "/" + RankManager.instance.currentRacers;

            //LAP
            if (racingUI.lap)
                racingUI.lap.text = "Lap " + player.lap + "/" + RaceManager.instance.totalLaps;

            //LAP TIME
            if (racingUI.currentLapTime)
                racingUI.currentLapTime.text = "Current " + player.currentLapTime;

            //TOTAL TIME
            if (racingUI.totalTime)
                racingUI.totalTime.text = "Total " + player.totalRaceTime;

            //LAST LAP TIME
            if (racingUI.previousLapTime)
                racingUI.previousLapTime.text = GetPrevLapTime();

            //BEST LAP TIME
            if (racingUI.bestLapTime)
                racingUI.bestLapTime.text = GetBestLapTime();
        }


        void TimeTrialUI()
        {
            //POS
            if (racingUI.rank)
                racingUI.rank.text = "Pos " + player.GetComponent<Statistics>().rank + "/" + RankManager.instance.currentRacers;

            //LAP
            if (racingUI.lap)
                racingUI.lap.text = "Lap " + player.lap;

            //LAP TIME
            if (racingUI.currentLapTime)
                racingUI.currentLapTime.text = "Current " + player.currentLapTime;

            //TOTAL TIME
            if (racingUI.totalTime)
                racingUI.totalTime.text = "Total " + player.totalRaceTime;

            //LAST LAP TIME
            if (racingUI.previousLapTime)
                racingUI.previousLapTime.text = GetPrevLapTime();

            //BEST LAP TIME
            if (racingUI.bestLapTime)
                racingUI.bestLapTime.text = GetBestLapTime();

        }


        void CheckpointRaceUI()
        {
            //POS
            if (racingUI.rank)
                racingUI.rank.text = "Pos " + player.GetComponent<Statistics>().rank + "/" + RankManager.instance.currentRacers;

            //CHECKPOINTS
            if (racingUI.lap)
                racingUI.lap.text = "CP " + player.checkpoint + "/" + player.checkpoints.Count * RaceManager.instance.totalLaps;

            //TIMER
            if (racingUI.currentLapTime)
                racingUI.currentLapTime.text = "Time : " + player.currentLapTime;

            //BEST LAP TIME
            if (racingUI.bestLapTime)
                racingUI.bestLapTime.text = GetBestLapTime();

            //EMPTY strings
            if (racingUI.previousLapTime)
                racingUI.previousLapTime.text = "";

            if (racingUI.totalTime)
                racingUI.totalTime.text = "";
        }

        void EliminationRaceUI()
        {
            //POS
            if (racingUI.rank)
                racingUI.rank.text = "Pos " + player.GetComponent<Statistics>().rank + "/" + RankManager.instance.currentRacers;

            //LAP
            if (racingUI.lap)
                racingUI.lap.text = "Lap " + player.lap + "/" + RaceManager.instance.totalLaps;

            //TIMER
            if (racingUI.currentLapTime)
                racingUI.currentLapTime.text = "Time : " + RaceManager.instance.FormatTime(RaceManager.instance.eliminationCounter);

            //TOTAL TIME
            if (racingUI.totalTime)
                racingUI.totalTime.text = "Total " + player.totalRaceTime;

            //LAST LAP
            if (racingUI.previousLapTime)
                racingUI.previousLapTime.text = GetPrevLapTime();

            //BEST LAP
            if (racingUI.bestLapTime)
                racingUI.bestLapTime.text = GetBestLapTime();
        }

        void DriftRaceUI()
        {
            //DRIFT UI
            if (driftUI.totalDriftPoints)
                driftUI.totalDriftPoints.text = player.GetComponent<DriftPointController>().totalDriftPoints.ToString("N0") + " Pts";

            if (driftUI.currentDriftPoints)
                driftUI.currentDriftPoints.text = driftpointcontroller.currentDriftPoints > 0 ? "+ " + player.GetComponent<DriftPointController>().currentDriftPoints.ToString("N0") + " Pts" : string.Empty;

            if (driftUI.driftMultiplier)
                driftUI.driftMultiplier.text = driftpointcontroller.driftMultiplier > 1 ? "x " + driftpointcontroller.driftMultiplier : string.Empty;

            //POS
            if (racingUI.rank)
                racingUI.rank.text = string.Empty;

            //LAP
            if (racingUI.lap)
                racingUI.lap.text = "Lap " + player.lap + "/" + RaceManager.instance.totalLaps;

            //LAP TIME
            if (racingUI.currentLapTime)
                racingUI.currentLapTime.text = "Time " + player.currentLapTime;

            //TOTAL TIME
            if (racingUI.totalTime)
                racingUI.totalTime.text = "Total " + player.totalRaceTime;

            //LAST LAP TIME
            if (racingUI.previousLapTime)
                racingUI.previousLapTime.text = GetPrevLapTime();

            //BEST LAP TIME
            if (racingUI.bestLapTime)
                racingUI.bestLapTime.text = GetBestLapTime();
        }

        #endregion

        void VehicleGUI()
        {
            if (m_Car_Ctrl == null)
            {
                if (player.GetComponent<Car_Controller>())
                    m_Car_Ctrl = player.GetComponent<Car_Controller>();
            }


            //Speed
            if (vehicleUI.currentSpeed)
            {
                if (m_Car_Ctrl)
                    vehicleUI.currentSpeed.text = m_Car_Ctrl.currentSpeed + player.GetComponent<Car_Controller>()._speedUnit.ToString();

                if (player.GetComponent<Motorbike_Controller>())
                    vehicleUI.currentSpeed.text = player.GetComponent<Motorbike_Controller>().currentSpeed + player.GetComponent<Motorbike_Controller>()._speedUnit.ToString();
            }

            //Gear
            if (vehicleUI.currentGear)
            {
                if (m_Car_Ctrl)
                    vehicleUI.currentGear.text = m_Car_Ctrl.currentGear.ToString();

                if (player.GetComponent<Motorbike_Controller>())
                    vehicleUI.currentGear.text = player.GetComponent<Motorbike_Controller>().currentGear.ToString();
            }

            //Speedometer
            if (vehicleUI.needle)
            {
                float fraction = 0;

                if (m_Car_Ctrl)
                {
                    fraction = m_Car_Ctrl.currentSpeed / vehicleUI.maxNeedleAngle;
                }

                if (player.GetComponent<Motorbike_Controller>())
                {
                    fraction = player.GetComponent<Motorbike_Controller>().currentSpeed / vehicleUI.maxNeedleAngle;
                }

                vehicleUI.needleRotation = Mathf.Lerp(vehicleUI.minNeedleAngle, vehicleUI.maxNeedleAngle, (fraction * vehicleUI.rotationMultiplier));
                vehicleUI.needle.transform.eulerAngles = new Vector3(vehicleUI.needle.transform.eulerAngles.x, vehicleUI.needle.transform.eulerAngles.y, -vehicleUI.needleRotation);
            }

            //Nitro Bar
            if (vehicleUI.nitroBar)
            {
                if (m_Car_Ctrl)
                    vehicleUI.nitroBar.fillAmount = m_Car_Ctrl.nitroCapacity;

                if (player.GetComponent<Motorbike_Controller>())
                    vehicleUI.nitroBar.fillAmount = player.GetComponent<Motorbike_Controller>().nitroCapacity;

            }

            //3D text mesh
            if (!vehicleUI.speedText3D && GameObject.Find("3DSpeedText"))
                vehicleUI.speedText3D = GameObject.Find("3DSpeedText").GetComponent<TextMesh>();

            if (!vehicleUI.gearText3D && GameObject.Find("3DGearText"))
                vehicleUI.gearText3D = GameObject.Find("3DGearText").GetComponent<TextMesh>();

            if (vehicleUI.speedText3D)
            {
                if (m_Car_Ctrl)
                    vehicleUI.speedText3D.text = m_Car_Ctrl.currentSpeed + m_Car_Ctrl._speedUnit.ToString();

                if (player.GetComponent<Motorbike_Controller>())
                    vehicleUI.speedText3D.text = player.GetComponent<Motorbike_Controller>().currentSpeed + player.GetComponent<Motorbike_Controller>()._speedUnit.ToString();
            }

            if (vehicleUI.gearText3D)
            {
                if (m_Car_Ctrl)
                    vehicleUI.gearText3D.text = m_Car_Ctrl.currentGear.ToString();

                if (player.GetComponent<Motorbike_Controller>())
                    vehicleUI.gearText3D.text = player.GetComponent<Motorbike_Controller>().currentGear.ToString();
            }

            if (m_Car_Ctrl)
            {
                //Health
                m_Healthbar.value = m_Car_Ctrl.m_Health;
            }
        }

        public void UpdateUIPanels()
        {
            if (!RaceManager.instance) return;

            switch (RaceManager.instance._raceState)
            {

                //if starting grid, set all other panels active to false except from the starting panel
                case RaceManager.RaceState.StartingGrid:
                    if (startingGridPanel) startingGridPanel.SetActive(true);

                    if (racePanel) racePanel.SetActive(false);

                    if (pausePanel) pausePanel.SetActive(false);

                    if (failRacePanel) failRacePanel.SetActive(false);

                    if (raceCompletePanel) raceCompletePanel.SetActive(false);

                    if (replayPanel) replayPanel.SetActive(false);

                    break;

                //if racing, set all other panels active to false except from the racing panel
                case RaceManager.RaceState.Racing:
                    if (startingGridPanel) startingGridPanel.SetActive(false);

                    if (racePanel) racePanel.SetActive(true);

                    if (pausePanel) pausePanel.SetActive(false);

                    if (failRacePanel) failRacePanel.SetActive(false);

                    if (raceCompletePanel) raceCompletePanel.SetActive(false);

                    if (replayPanel) replayPanel.SetActive(false);

                    break;

                //if paused, set all other panels active to false except from the pause panel
                case RaceManager.RaceState.Paused:
                    if (startingGridPanel) startingGridPanel.SetActive(false);

                    if (racePanel) racePanel.SetActive(false);

                    if (pausePanel) pausePanel.SetActive(true);

                    if (failRacePanel) failRacePanel.SetActive(false);

                    if (raceCompletePanel) raceCompletePanel.SetActive(false);

                    if (replayPanel) replayPanel.SetActive(false);
                    break;

                //if the race is complete, set all other panels active to false except from the completion panel
                case RaceManager.RaceState.Complete:
                    if (startingGridPanel) startingGridPanel.SetActive(false);

                    if (racePanel) racePanel.SetActive(false);

                    if (pausePanel) pausePanel.SetActive(false);

                    if (failRacePanel) failRacePanel.SetActive(false);

                    if (raceCompletePanel) raceCompletePanel.SetActive(true);

                    if (replayPanel) replayPanel.SetActive(false);
                    break;

                //if the player is knocked out, set all other panels active to false except from the ko panel
                case RaceManager.RaceState.KnockedOut:
                    if (startingGridPanel) startingGridPanel.SetActive(false);

                    if (racePanel) racePanel.SetActive(false);

                    if (pausePanel) pausePanel.SetActive(false);

                    if (failRacePanel) failRacePanel.SetActive(true);

                    if (raceCompletePanel) raceCompletePanel.SetActive(false);

                    if (replayPanel) replayPanel.SetActive(false);

                    break;

                case RaceManager.RaceState.Replay:
                    if (startingGridPanel) startingGridPanel.SetActive(false);

                    if (racePanel) racePanel.SetActive(false);

                    if (pausePanel) pausePanel.SetActive(false);

                    if (failRacePanel) failRacePanel.SetActive(false);

                    if (raceCompletePanel) raceCompletePanel.SetActive(false);

                    if (replayPanel) replayPanel.SetActive(true);
                    break;

            }
        }

        void ShowStartingGrid()
        {
            //loop through the total number of cars & show their race standings
            if (startingGrid.Count > 0)
            {
                for (int i = 0; i < RankManager.instance.totalRacers; i++)
                {
                    Statistics _statistics = RankManager.instance.racerRanks[i].racer.GetComponent<Statistics>();

                    if (_statistics == null) return;

                    //Position
                    if (startingGrid[i].position) startingGrid[i].position.text = _statistics.rank.ToString();

                    //Name
                    if (startingGrid[i].name) startingGrid[i].name.text = _statistics.racerDetails.racerName;

                    //Vehicle name
                    if (startingGrid[i].vehicleName) startingGrid[i].vehicleName.text = _statistics.racerDetails.vehicleName;
                }
            }
        }
        void ShowInRaceStandings()
        {
            if (racingUI.inRaceStandings.Count <= 0 || RankManager.instance.totalRacers <= 1)
                return;

            //in race standings
            for (int i = 0; i < RankManager.instance.totalRacers; i++)
            {
                if (i < racingUI.inRaceStandings.Count)
                {
                    Statistics _statistics = RankManager.instance.racerRanks[i].racer.GetComponent<Statistics>();

                    if (_statistics == null) return;

                    //Name
                    if (racingUI.inRaceStandings[i].name) racingUI.inRaceStandings[i].name.text = (RaceManager.instance._raceType != RaceManager.RaceType.SpeedTrap) ? _statistics.racerDetails.racerName
                     : _statistics.racerDetails.racerName + " [" + RankManager.instance.racerRanks[i].speedRecord + " mph]";


                    //Colors
                    if (player == _statistics)
                    {
                        racingUI.inRaceStandings[i].position.color = racingUI.playerColor;
                        racingUI.inRaceStandings[i].name.color = racingUI.playerColor;
                    }
                    else
                    {
                        racingUI.inRaceStandings[i].position.color = racingUI.normalColor;
                        racingUI.inRaceStandings[i].name.color = racingUI.normalColor;
                    }

                }
            }
        }

        public void RefreshInRaceStandings()
        {
            if (RankManager.instance.totalRacers <= 1) return;

            for (int i = 0; i < racingUI.inRaceStandings.Count; i++)
            {
                if (i < RankManager.instance.totalRacers)
                {
                    if (racingUI.inRaceStandings[i].position.transform.parent)
                        racingUI.inRaceStandings[i].position.transform.parent.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Loops through the total number of racers and shows their standings
        /// This function is called for non drift races because of different UI setup
        /// </summary>
        void ShowRaceResults()
        {
            if (raceResults.Count > 0)
            {
                for (int i = 0; i < RankManager.instance.totalRacers; i++)
                {
                    Statistics _statistics = RankManager.instance.racerRanks[i].racer.GetComponent<Statistics>();

                    if (_statistics == null) return;

                    //Position
                    if (raceResults[i].position) raceResults[i].position.text = _statistics.rank.ToString();


                    //Name
                    if (raceResults[i].name)
                    {
                        if (RaceManager.instance._raceType != RaceManager.RaceType.SpeedTrap)
                        {
                            raceResults[i].name.text = _statistics.racerDetails.racerName;
                        }
                        else {
                            raceResults[i].name.text = _statistics.racerDetails.racerName + " [" + RankManager.instance.racerRanks[i].speedRecord + " mph]";
                        }
                    }

                    //Total Race Time
                    if (raceResults[i].totalTime)
                    {
                        if (_statistics.finishedRace && !_statistics.knockedOut)
                        {
                            raceResults[i].totalTime.text = _statistics.totalRaceTime;
                        }
                        else if (_statistics.knockedOut)
                        {
                            raceResults[i].totalTime.text = "Knocked Out";
                        }
                        else
                        {
                            raceResults[i].totalTime.text = "Running...";
                        }
                    }

                    //Best Lap Time
                    if (raceResults[i].bestLapTime)
                    {
                        raceResults[i].bestLapTime.text = (_statistics.bestLapTime == string.Empty) ? "--:--:--" : _statistics.bestLapTime;
                    }

                    //Vehicle Name
                    if (raceResults[i].vehicleName)
                    {
                        raceResults[i].vehicleName.text = _statistics.racerDetails.vehicleName;
                    }
                }
            }
        }

        /// <summary>
        /// Gets drift information from the driftpointcontroller and displays them
        /// This function is only called for drift races because of different UI setup
        /// </summary>
        void ShowDriftResults()
        {

            if (driftpointcontroller)
            {
                if (driftResults.totalPoints)
                    driftResults.totalPoints.text = "Total Points : " + driftpointcontroller.totalDriftPoints.ToString("N0");

                if (driftResults.driftRaceTime)
                    driftResults.driftRaceTime.text = "Time : " + driftpointcontroller.GetComponent<Statistics>().totalRaceTime;

                if (driftResults.bestDrift)
                    driftResults.bestDrift.text = "Best Drift : " + driftpointcontroller.bestDrift.ToString("N0") + " pts";

                if (driftResults.longestDrift)
                    driftResults.longestDrift.text = "Longest Drift : " + driftpointcontroller.longestDrift.ToString("0.00") + " s";

                if (driftResults.gold)
                    driftResults.gold.gameObject.SetActive(driftpointcontroller.GetComponent<Statistics>().rank == 1);

                if (driftResults.silver)
                    driftResults.silver.gameObject.SetActive(driftpointcontroller.GetComponent<Statistics>().rank == 2);

                if (driftResults.bronze)
                    driftResults.bronze.gameObject.SetActive(driftpointcontroller.GetComponent<Statistics>().rank > 2);
            }
        }

        public void ShowReplayUI()
        {
            //Display the replay progress bar
            if (progressBar)
                progressBar.fillAmount = ReplayManager.instance.ReplayPercent;
        }

        //Used to show useful race info
        public void ShowRaceInfo(string info, float time, Color c)
        {
            StartCoroutine(RaceInfo(info, time, c));
        }

        IEnumerator RaceInfo(string info, float time, Color c)
        {
            if (!racingUI.raceInfo)
                yield break;

            if (racingUI.raceInfo.text == "")
            {
                racingUI.raceInfo.text = info;

                Color col = c;
                col.a = 1.0f;
                racingUI.raceInfo.color = col;

                yield return new WaitForSeconds(time);

                //Do Fade Out
                while (col.a > 0.0f)
                {
                    col.a -= Time.deltaTime * 2.0f;
                    racingUI.raceInfo.color = col;
                    yield return null;
                }

                if (col.a <= 0.01f)
                {
                    racingUI.raceInfo.text = string.Empty;
                }

                //Check if there are any other race infos that need to be displayed
                CheckRaceInfoList();
            }
            else
            {
                raceInfos.Add(info);
            }
        }

        public IEnumerator ShowDriftRaceInfo(string info, Color c)
        {
            if (!driftUI.driftStatus) yield break;

            driftUI.driftStatus.text = info;
            driftUI.driftStatus.color = c;

            yield return new WaitForSeconds(2.0f);

            driftUI.driftStatus.text = string.Empty;
        }

        public void CheckRaceInfoList()
        {
            if (raceInfos.Count > 0)
            {
                ShowRaceInfo(raceInfos[raceInfos.Count - 1], 2.0f, Color.white);
                raceInfos.RemoveAt(raceInfos.Count - 1);
            }
        }

        void WrongwayUI()
        {
            //Wrong way indication
            if (racingUI.wrongwayText)
            {
                if (player.GetComponent<Statistics>().goingWrongway)
                {
                    racingUI.wrongwayText.text = "Wrong Way!";
                }
                else
                {
                    racingUI.wrongwayText.text = string.Empty;
                }
            }

            if (racingUI.wrongwayImage)
            {
                if (player.GetComponent<Statistics>().goingWrongway)
                {
                    racingUI.wrongwayImage.enabled = true;
                }
                else
                {
                    racingUI.wrongwayImage.enabled = false;
                }
            }
        }

        string GetPrevLapTime()
        {
            if (player.prevLapTime != "")
            {
                return "Last " + player.prevLapTime;
            }
            else
            {
                return "Last --:--:--";
            }
        }

        string GetBestLapTime()
        {
            if (PlayerPrefs.HasKey("BestTime" + SceneManager.GetActiveScene().name))
            {
                return "Best " + PlayerPrefs.GetString("BestTime" + SceneManager.GetActiveScene().name);
            }
            else
            {
                return "Best --:--:--";
            }
        }

        public void SetCountDownText(string value)
        {
            if (!racingUI.countdown) return;

            racingUI.countdown.text = value;
        }

        public void SetFailRace(string title, string reason)
        {
            if (failTitle) failTitle.text = title;

            if (failReason) failReason.text = reason;
        }

        /// <summary>
        /// Gets rid of all other UI apart from the FinishedText to show the "Race Completed" text in the End Race Rountine
        /// </summary>
        public void DisableRacePanelChildren()
        {
            if (!racingUI.finishedText) return;

            RectTransform[] rectTransforms = racePanel.GetComponentsInChildren<RectTransform>();

            foreach (RectTransform t in rectTransforms)
            {
                if (t != racePanel.GetComponent<RectTransform>() && t != racingUI.finishedText.GetComponent<RectTransform>())
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        public void SetFinishedText(string word)
        {
            if (racingUI.finishedText)
                racingUI.finishedText.text = word;
        }

        public void SetRewardText(string currency, string vehicleUnlock, string trackUnlock)
        {
            if (currency != "" && rewardTexts.rewardCurrency)
                rewardTexts.rewardCurrency.text = "You won : " + currency + " Cr";

            if (vehicleUnlock != "" && rewardTexts.rewardVehicle)
                rewardTexts.rewardVehicle.text = "You Unlocked : " + vehicleUnlock;

            if (trackUnlock != "" && rewardTexts.rewardTrack)
                rewardTexts.rewardTrack.text = "You Unlocked : " + trackUnlock;
        }

        #region Screen Fade
        public IEnumerator ScreenFadeOut(float speed)
        {
            //Get the color
            Color col = screenFade.color;
            if (col.a > 0.0f) yield break;

            //Change the alpha to 1
            col.a = 1;
            screenFade.color = col;

            //Fade out
            while (col.a > 0.0f)
            {
                col.a -= Time.deltaTime * speed;
                screenFade.color = col;
                yield return null;
            }
        }

        public IEnumerator ScreenFadeIn(float speed, bool loadScene, string scene)
        {
            //Get the color
            Color col = screenFade.color;

            //Change the alpha to 0
            col.a = 0;
            screenFade.color = col;

            //Fade in
            while (col.a < 1.0f)
            {
                col.a += Time.deltaTime * speed;
                screenFade.color = col;
                yield return null;

                //Load the menu scene when fade completes
                if (col.a >= 1.0f)
                    SceneManager.LoadScene(scene);
            }

        }
        #endregion

        #region UI Button Functions

        public void StartCountDown(float time)
        {
            StartCoroutine(RaceManager.instance.Countdown(time));
        }

        public void PauseResume()
        {
            RaceManager.instance.PauseRace();
        }

        public void Restart()
        {
            //unpause inorder to reset timescale & audiolistener vol
            if (RaceManager.instance._raceState == RaceManager.RaceState.Paused)
            {
                PauseResume();
            }

            if (fadeOnExit && screenFade)
            {
                StartCoroutine(ScreenFadeIn(fadeSpeed * 2, true, SceneManager.GetActiveScene().name));
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void Exit()
        {
            //unpause inorder to reset timescale & audiolistener vol
            if (RaceManager.instance._raceState == RaceManager.RaceState.Paused)
            {
                PauseResume();
            }

            if (fadeOnExit && screenFade)
            {
                StartCoroutine(ScreenFadeIn(fadeSpeed * 2, true, menuScene));
            }
            else
            {
                SceneManager.LoadScene(menuScene);
            }
        }

        #endregion
    }
}