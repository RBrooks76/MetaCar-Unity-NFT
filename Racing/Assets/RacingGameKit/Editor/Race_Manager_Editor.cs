using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using RGSK;

[CustomEditor(typeof(RaceManager))]
public class Race_Manager_Editor : Editor
{

    RaceManager m_target;


    public void OnEnable()
    {
        m_target = (RaceManager)target;
    }

    public override void OnInspectorGUI()
    {
        //LOGO
        Texture logo = (Texture)Resources.Load("EditorUI/RGSKLogo");
        GUILayout.Label(logo, GUILayout.Height(50));

        //RACE SETTINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Race Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target._raceType = (RaceManager.RaceType)EditorGUILayout.EnumPopup("Race Type", m_target._raceType);

        EditorGUILayout.Space();

        switch (m_target._raceType)
        {
            case RaceManager.RaceType.Circuit:
                m_target.totalLaps = EditorGUILayout.IntField("Total Laps", m_target.totalLaps);
                m_target.totalRacers = EditorGUILayout.IntField("Total Racers", m_target.totalRacers);
                break;


            /*case RaceManager.RaceType.Sprint:
                m_target.totalRacers = EditorGUILayout.IntField("Total Racers", m_target.totalRacers);
                break;
            */

            case RaceManager.RaceType.LapKnockout:
                m_target.totalRacers = EditorGUILayout.IntField("Total Racers", m_target.totalRacers);
                break;

            case RaceManager.RaceType.TimeTrial:
                m_target.timeTrialStartPoint = EditorGUILayout.ObjectField("Start Point", m_target.timeTrialStartPoint, typeof(Transform), true) as Transform;
                m_target.enableGhostVehicle = EditorGUILayout.Toggle("Use Ghost Vehicle", m_target.enableGhostVehicle);
                m_target.timeTrialAutoDrive = EditorGUILayout.Toggle("Auto Drive", m_target.timeTrialAutoDrive);

                if (m_target.enableGhostVehicle)
                {
                    GUILayout.Box("Ghost Vehicle Material Settings", EditorStyles.boldLabel);

                    m_target.useGhostMaterial = EditorGUILayout.Toggle("Set Material", m_target.useGhostMaterial);
                    if (!m_target.useGhostMaterial)
                    {
                        m_target.ghostShader = EditorGUILayout.ObjectField("Shader", m_target.ghostShader, typeof(Shader), true) as Shader;
                        m_target.ghostAlpha = EditorGUILayout.FloatField("Alpha", m_target.ghostAlpha);
                    }
                    else
                    {
                        m_target.ghostMaterial = EditorGUILayout.ObjectField("Material", m_target.ghostMaterial, typeof(Material), true) as Material;
                    }
                }

                break;

            case RaceManager.RaceType.SpeedTrap:
                m_target.totalLaps = EditorGUILayout.IntField("Total Laps", m_target.totalLaps);
                m_target.totalRacers = EditorGUILayout.IntField("Total Racers", m_target.totalRacers);
                break;

            case RaceManager.RaceType.Checkpoints:
                m_target.totalRacers = EditorGUILayout.IntField("Total Racers", m_target.totalRacers);
                m_target.totalLaps = EditorGUILayout.IntField("Total Laps", m_target.totalLaps);
                m_target.initialCheckpointTime = EditorGUILayout.FloatField("Initial Time", m_target.initialCheckpointTime);
                break;

            case RaceManager.RaceType.Elimination:
                m_target.totalLaps = EditorGUILayout.IntField("Total Laps", m_target.totalLaps);
                m_target.totalRacers = EditorGUILayout.IntField("Total Racers", m_target.totalRacers);
                m_target.eliminationTime = EditorGUILayout.FloatField("Eimination Time", m_target.eliminationTime);

                break;

            case RaceManager.RaceType.Drift:
                m_target.totalLaps = EditorGUILayout.IntField("Total Laps", m_target.totalLaps);
                m_target.timeLimit = EditorGUILayout.Toggle("Use Time Limit", m_target.timeLimit);

                if (m_target.timeLimit)
                {
                    m_target.driftTimeLimit = EditorGUILayout.FloatField("Time Limit", m_target.driftTimeLimit);
                }

                EditorGUILayout.Space();
                m_target.goldDriftPoints = EditorGUILayout.FloatField("Gold Drift Points", m_target.goldDriftPoints);
                m_target.silverDriftPoints = EditorGUILayout.FloatField("Silver Drift Points", m_target.silverDriftPoints);
                m_target.bronzeDriftPoints = EditorGUILayout.FloatField("Bronze Drift Points", m_target.bronzeDriftPoints);
                break;
        }

        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //RACE CONTAINER SETTINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Race Container Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        //Path
        if (!m_target.pathContainer)
        {

            if (!GameObject.FindObjectOfType(typeof(WaypointCircuit)))
            {
                EditorGUILayout.HelpBox("Create a Path!", MessageType.Warning);
            }
            else {
                EditorGUILayout.HelpBox("Assign the Path!", MessageType.Info);
            }

            EditorGUILayout.Space();
            if (!GameObject.FindObjectOfType(typeof(WaypointCircuit)))
            {
                if (GUILayout.Button("Create Path", GUILayout.Width(190)))
                {
                    RGSK_Editor.CreatePath();
                }
            }
            else {
                if (GUILayout.Button("Assign Path", GUILayout.Width(190)))
                {
                    WaypointCircuit path = GameObject.FindObjectOfType(typeof(WaypointCircuit)) as WaypointCircuit;
                    m_target.pathContainer = path.GetComponent<Transform>();
                }
            }
        }
        EditorGUILayout.Space();

        m_target.pathContainer = EditorGUILayout.ObjectField("Path Container", m_target.pathContainer, typeof(Transform), true) as Transform;

        //Spawnpoint
        if (!m_target.spawnpointContainer)
        {

            if (!GameObject.FindObjectOfType(typeof(SpawnpointContainer)))
            {
                EditorGUILayout.HelpBox("Create a Spawnpoint Container!", MessageType.Warning);
            }
            else {
                EditorGUILayout.HelpBox("Assign the Spawnpoint Container!", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (!GameObject.FindObjectOfType(typeof(SpawnpointContainer)))
            {
                if (GUILayout.Button("Create Spawnpoint Container", GUILayout.Width(190)))
                {
                    RGSK_Editor.CreateSpawnpoint();
                }
            }
            else {
                if (GUILayout.Button("Assign Spawnpoint Container", GUILayout.Width(190)))
                {
                    SpawnpointContainer sp = GameObject.FindObjectOfType(typeof(SpawnpointContainer)) as SpawnpointContainer;
                    m_target.spawnpointContainer = sp.GetComponent<Transform>();
                }
            }
        }

        m_target.spawnpointContainer = EditorGUILayout.ObjectField("Spawnpoint Container", m_target.spawnpointContainer, typeof(Transform), true) as Transform;

        //Checkpoint
        if (!m_target.checkpointContainer)
        {
            if (!GameObject.FindObjectOfType(typeof(CheckpointContainer)))
            {
                EditorGUILayout.HelpBox("Speed Trap & Checkpoint races require checkpoints. You can create a Checkpoint Container using the button below", MessageType.Info);
            }
            else {
                EditorGUILayout.HelpBox("Assign the Checkpoint Container!", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (!GameObject.FindObjectOfType(typeof(CheckpointContainer)))
            {
                if (GUILayout.Button("Create Checkpoint Container", GUILayout.Width(190)))
                {
                    RGSK_Editor.CreateCheckpoint();
                }
            }
            else {
                if (GUILayout.Button("Assign Checkpoint Container", GUILayout.Width(190)))
                {
                    CheckpointContainer cp = GameObject.FindObjectOfType(typeof(CheckpointContainer)) as CheckpointContainer;
                    m_target.checkpointContainer = cp.GetComponent<Transform>();
                }
            }
        }
        m_target.checkpointContainer = EditorGUILayout.ObjectField("Checkpoint Container", m_target.checkpointContainer, typeof(Transform), true) as Transform;

        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //PLAYER CAR SETINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Player Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target.playerCar = EditorGUILayout.ObjectField("Player Prefab:", m_target.playerCar, typeof(GameObject), true) as GameObject;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.Box("Spawn Settings", EditorStyles.boldLabel);
        m_target._playerSpawnPosition = (RaceManager.PlayerSpawnPosition)EditorGUILayout.EnumPopup("Player Spawn Position", m_target._playerSpawnPosition);
        if (m_target._playerSpawnPosition == RaceManager.PlayerSpawnPosition.Selected)
        {
            m_target.playerStartRank = EditorGUILayout.IntField("Player Start Rank", m_target.playerStartRank);
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //AI CAR SETTINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("AI Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.Label("Ai Prefabs :");
        EditorGUILayout.Space();
        for (int i = 0; i < m_target.opponentCars.Count; i++)
        {
            m_target.opponentCars[i] = EditorGUILayout.ObjectField((i + 1).ToString(), m_target.opponentCars[i], typeof(GameObject), true) as GameObject;
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Add", GUILayout.Width(80)))
        {
            GameObject newOpponent = null;
            m_target.opponentCars.Add(newOpponent);
        }
        if (GUILayout.Button("Remove", GUILayout.Width(80)))
        {
            if (m_target.opponentCars.Count > 0)
            {
                m_target.opponentCars.Remove(m_target.opponentCars[m_target.opponentCars.Count - 1]);
            }
        }

        EditorGUILayout.Space();

        GUILayout.Box("Spawn Settings", EditorStyles.boldLabel);
        m_target._aiSpawnType = (RaceManager.AISpawnType)EditorGUILayout.EnumPopup("AI Spawn Type", m_target._aiSpawnType);
        if (m_target._aiSpawnType == RaceManager.AISpawnType.Randomized)
        {
            m_target.allowDuplicateRacers = EditorGUILayout.Toggle("Allow Duplicate Racers", m_target.allowDuplicateRacers);
        }

        EditorGUILayout.Space();

        GUILayout.Box("Difficulty Settings", EditorStyles.boldLabel);
        m_target.aiDifficulty = (OpponentControl.AiDifficulty)EditorGUILayout.EnumPopup("AI Difficulty", m_target.aiDifficulty);

        EditorGUILayout.Space();

        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //REPLAY SETTINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Replay Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target.enableReplay = EditorGUILayout.Toggle("Enable Replay", m_target.enableReplay);
        m_target.autoStartReplay = EditorGUILayout.Toggle("Auto Start Replay After Finish", m_target.autoStartReplay);
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //MISC SETTINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Misc Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target.continueAfterFinish = EditorGUILayout.Toggle("Racers Continue After Finish", m_target.continueAfterFinish);
        m_target.showRacerNames = EditorGUILayout.Toggle("Show Racer Names", m_target.showRacerNames);
        m_target.showRacerPointers = EditorGUILayout.Toggle("Minimap pointers", m_target.showRacerPointers);
        m_target.showRaceInfoMessages = EditorGUILayout.Toggle("Race Info Messages", m_target.showRaceInfoMessages);
        m_target.showStartingGrid = EditorGUILayout.Toggle("Show Starting Grid", m_target.showStartingGrid);
        m_target.loadRacePreferences = EditorGUILayout.Toggle("Load Race Preferences", m_target.loadRacePreferences);
        m_target.forceWrongwayRespawn = EditorGUILayout.Toggle("Force Wrongway Respawn", m_target.forceWrongwayRespawn);
        m_target.countdownFrom = EditorGUILayout.IntField("Start Countdown From", m_target.countdownFrom);
        m_target.countdownDelay = EditorGUILayout.FloatField("Countdown Delay", m_target.countdownDelay);
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //MINI MAP POINTERS
        if (m_target.showRacerPointers)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Box("Minimap Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            m_target.playerPointer = EditorGUILayout.ObjectField("Player Pointer", m_target.playerPointer, typeof(GameObject), true) as GameObject;
            m_target.opponentPointer = EditorGUILayout.ObjectField("Opponent Pointer", m_target.opponentPointer, typeof(GameObject), true) as GameObject;
            GUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        //RACE NAMES SETTINGS
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Racer Names", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target.playerName = EditorGUILayout.TextField("Player Name :", m_target.playerName);
        EditorGUILayout.Space();
        if (m_target.showRacerNames)
        {
            m_target.racerName = EditorGUILayout.ObjectField("Racer Name Prefab", m_target.racerName, typeof(GameObject), true) as GameObject;
        }
        EditorGUILayout.Space();
		m_target.assignPlayerName = EditorGUILayout.Toggle("Assign Player Name", m_target.assignPlayerName);
        EditorGUILayout.Space();
        m_target.assignAiRacerNames = EditorGUILayout.Toggle("Assign AI Names", m_target.assignAiRacerNames);
        EditorGUILayout.Space();
        if (m_target.opponentNamesList.Count <= 0)
        {
            EditorGUILayout.HelpBox("Recomeneded : Generate opponent names in edit mode", MessageType.Warning);
        }
        else {
            EditorGUILayout.HelpBox(m_target.opponentNamesList.Count + " opponent names have been successfully generated from the racernames.txt file", MessageType.Info);
        }
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Opponent Names", GUILayout.Width(170)))
        {
            m_target.LoadRacerNames();
        }
        if (GUILayout.Button("Clear Opponent Names", GUILayout.Width(170)))
        {
            m_target.opponentNamesList.Clear();
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //Race Rewards
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Race Rewards", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        for (int i = 0; i < m_target.raceRewards.Count; i++)
        {
            GUILayout.Label("Position " + (i + 1), EditorStyles.boldLabel);
            m_target.raceRewards[i].currency = m_target.raceRewards[i].currency = EditorGUILayout.IntField("Currency", m_target.raceRewards[i].currency);
            m_target.raceRewards[i].vehicleUnlock = m_target.raceRewards[i].vehicleUnlock = EditorGUILayout.TextField("Vehicle Unlock", m_target.raceRewards[i].vehicleUnlock);
            m_target.raceRewards[i].trackUnlock = m_target.raceRewards[i].trackUnlock = EditorGUILayout.TextField("Track Unlock", m_target.raceRewards[i].trackUnlock);
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Reward Position", GUILayout.Width(160)))
        {
            m_target.raceRewards.Add(new RaceRewards());
        }
        if (GUILayout.Button("Remove Reward Position", GUILayout.Width(160)))
        {
            if (m_target.raceRewards.Count > 0)
            {
                m_target.raceRewards.Remove(m_target.raceRewards[m_target.raceRewards.Count - 1]);
            }
        }

        GUILayout.EndVertical();

        //Set dirty
        if (GUI.changed)
        {
            EditorUtility.SetDirty(m_target);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
