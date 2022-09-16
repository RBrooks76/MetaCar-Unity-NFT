using UnityEngine;
using UnityEditor;
using System.IO;
using RGSK;

public class RGSK_Editor : Editor
{

    //Race Components
    [MenuItem("Window/RacingGameStarterKit/Create/Create Race Components")]
    public static void CreateRaceManager()
    {

        if (GameObject.FindObjectOfType(typeof(RaceManager)))
        {
            Debug.LogWarning("A RaceManager already exists. Please make sure that you only have 1 Race Manager in your scene.");
        }

        GameObject _raceComponentParent = new GameObject("RGSK Race Components");


        //Add neccesray components to the race manager
        GameObject _raceManager = new GameObject("Race Manager");
        _raceManager.AddComponent<RaceManager>();
        _raceManager.AddComponent<ReplayManager>();
        _raceManager.AddComponent<RankManager>();

        GameObject _raceUI = new GameObject("Race UI");
        _raceUI.AddComponent<RaceUI>();

        GameObject _camManager = new GameObject("Camera Manager");
        _camManager.AddComponent<CameraManager>();

        GameObject _soundManager = new GameObject("Sound Manager");
        _soundManager.AddComponent<SoundManager>();

        //GameObject _dataLoader = new GameObject("Data Loader");
        //_dataLoader.AddComponent<DataLoader>();

        _raceManager.transform.parent = _raceComponentParent.transform;
        _raceUI.transform.parent = _raceComponentParent.transform;
        _camManager.transform.parent = _raceComponentParent.transform;
        _soundManager.transform.parent = _raceComponentParent.transform;
        //_dataLoader.transform.parent = _raceComponentParent.transform;
    }



    [MenuItem("Window/RacingGameStarterKit/Create/Create Race Path")]
    public static void CreatePath()
    {

        if (!GameObject.FindObjectOfType(typeof(PathCreator)))
        {

            GameObject path = new GameObject("Path");

            //Add neccesray components to the path
            Camera camera = SceneView.lastActiveSceneView.camera;
            path.AddComponent<PathCreator>();
            path.transform.position = camera.transform.position;

            //Select the newly created path
            Selection.objects = new Object[] { path };
            SceneView.lastActiveSceneView.FrameSelected();
        }

        else
        {
            Debug.Log("A Path already exists!");
        }
    }



    [MenuItem("Window/RacingGameStarterKit/Create/Create Race Spawnpoints")]
    public static void CreateSpawnpoint()
    {

        if (!GameObject.FindObjectOfType(typeof(SpawnpointContainer)))
        {

            GameObject spawnpointC = new GameObject("Spawnpoint Container");
            GameObject child = new GameObject("01");

            //Add neccesray components to the spawnpoint
            Camera camera = SceneView.lastActiveSceneView.camera;
            spawnpointC.AddComponent<SpawnpointContainer>();
            child.transform.parent = spawnpointC.transform;
            child.transform.position = camera.transform.position;

            //Select the newly created spawnpoint
            Selection.objects = new Object[] { child };
            SceneView.lastActiveSceneView.FrameSelected();
        }
        else
        {
            Debug.Log("A Spawnpoint Container already exists!");
        }
    }



    [MenuItem("Window/RacingGameStarterKit/Create/Create Race Checkpoints")]
    public static void CreateCheckpoint()
    {

        if (!GameObject.FindObjectOfType(typeof(CheckpointContainer)))
        {

            GameObject checkpointC = new GameObject("Checkpoint Container");
            GameObject child = new GameObject("Checkpoint");
            child.layer = LayerMask.NameToLayer("Ignore Raycast");
            child.AddComponent<Checkpoint>();
            child.AddComponent<BoxCollider>();
            child.GetComponent<BoxCollider>().isTrigger = true;
            child.GetComponent<BoxCollider>().size = new Vector3(30, 10, 5);

            //Add neccesray components to the checkpointC
            Camera camera = SceneView.lastActiveSceneView.camera;
            checkpointC.AddComponent<CheckpointContainer>();
            child.transform.parent = checkpointC.transform;
            child.transform.position = camera.transform.position;

            //Select the newly created checkpointC
            Selection.objects = new Object[] { child };
            SceneView.lastActiveSceneView.FrameSelected();
        }
        else
        {
            Debug.Log("A Checkpoint Container already exists!");
        }
    }

    [MenuItem("Window/RacingGameStarterKit/Create/Create Race Cameras")]
    public static void CreateRaceCameras()
    {
        GameObject raceCamsParent = new GameObject("RGSK Race Cameras");

        GameObject playerCam = new GameObject("Player Camera");
        playerCam.tag = "MainCamera";
        playerCam.AddComponent<Camera>();
        playerCam.GetComponent<Camera>().depth = -1;
        playerCam.GetComponent<Camera>().nearClipPlane = 0.01f;
        playerCam.AddComponent<FlareLayer>();
        playerCam.AddComponent<AudioListener>();
        playerCam.AddComponent<PlayerCamera>();

        GameObject sgCam = new GameObject("Starting Grid Camera");
        sgCam.AddComponent<Camera>();
        sgCam.GetComponent<Camera>().depth = -1;
        sgCam.AddComponent<FlareLayer>();
        sgCam.AddComponent<OrbitAroundCamera>();
        sgCam.GetComponent<Camera>().enabled = false;

        GameObject cinematicCam = new GameObject("Cinematic Camera");
        cinematicCam.AddComponent<Camera>();
        cinematicCam.GetComponent<Camera>().depth = -1;
        cinematicCam.AddComponent<FlareLayer>();
        cinematicCam.AddComponent<CinematicCamera>();
        cinematicCam.GetComponent<Camera>().enabled = false;

        GameObject minimapCam = new GameObject("Minimap Camera");
        minimapCam.AddComponent<Camera>();
        minimapCam.GetComponent<Camera>().clearFlags = CameraClearFlags.Depth;
        minimapCam.GetComponent<Camera>().orthographic = true;
        minimapCam.GetComponent<Camera>().orthographicSize = 1500;
        minimapCam.GetComponent<Camera>().rect = new Rect(-0.8f, -0.15f, 1, 1);
        minimapCam.transform.eulerAngles = new Vector3(90, 0, 0);
        minimapCam.GetComponent<Camera>().depth = 0;
        minimapCam.AddComponent<MinimapFollowTarget>();

        //Set all camera parent
        playerCam.transform.parent = raceCamsParent.transform;
        sgCam.transform.parent = raceCamsParent.transform;
        cinematicCam.transform.parent = raceCamsParent.transform;
        minimapCam.transform.parent = raceCamsParent.transform;
    }

    [MenuItem("Window/RacingGameStarterKit/Create/Create Input Manager")]
    public static void CreateInputManager()
    {
        if (!GameObject.FindObjectOfType(typeof(InputManager)))
        {
            GameObject _inputManager = new GameObject("RGSK Input Manager");
            _inputManager.AddComponent<InputManager>();
        }
        else
        {
            Debug.Log("An Input Manager already exists");
        }
    }

    [MenuItem("Window/RacingGameStarterKit/Create/Create Surface Manager")]
    public static void CreateSurfaceManager()
    {
        if (!GameObject.FindObjectOfType(typeof(SurfaceManager)))
        {
            GameObject _surfaceManager = new GameObject("RGSK Surface Manager");
            GameObject _skidmarkRoad = new GameObject("Skidmarks");

            _surfaceManager.AddComponent<SurfaceManager>();
            _skidmarkRoad.AddComponent<Skidmark>();

            _skidmarkRoad.transform.parent = _surfaceManager.transform;
        }
        else
        {
            Debug.Log("Surface Manager already exists");
        }
    }

    //Vehicle Config
    [MenuItem("Window/RacingGameStarterKit/Vehicle Configuration/Vehicle Setup Wizard")]
    static void SetupVehicle()
    {
        //Show existing window instance. If one doesn't exist, make one.
        VehicleSetupWizard_Window window = (VehicleSetupWizard_Window)EditorWindow.GetWindow(typeof(VehicleSetupWizard_Window));
        window.minSize = new Vector2(500, 300);
    }

    [MenuItem("Window/RacingGameStarterKit/Vehicle Configuration/External Vehicle Physics/Add Required Player Components")]
    public static void AddPlayerComponentsToSelectedVehicle()
    {
        GameObject veh = Selection.activeGameObject;

        if (veh == null) return;

        veh.tag = "Player";
        if (!veh.GetComponent<Statistics>()) veh.AddComponent<Statistics>();
        if (!veh.GetComponent<ProgressTracker>()) veh.AddComponent<ProgressTracker>();
        if (!veh.GetComponent<WaypointArrow>()) veh.AddComponent<WaypointArrow>();
    }


    [MenuItem("Window/RacingGameStarterKit/Vehicle Configuration/External Vehicle Physics/Add Required AI Components")]
    public static void AddAiComponentsToSelectedVehicle()
    {
        GameObject veh = Selection.activeGameObject;

        if (veh == null) return;

        veh.tag = "Opponent";
        if (!veh.GetComponent<OpponentControl>()) veh.AddComponent<OpponentControl>();
        if (!veh.GetComponent<Statistics>()) veh.AddComponent<Statistics>();
        if (!veh.GetComponent<ProgressTracker>()) veh.AddComponent<ProgressTracker>();
        if (veh.GetComponent<PlayerControl>()) DestroyImmediate(veh.GetComponent<PlayerControl>());
        if (veh.GetComponent<WaypointArrow>()) DestroyImmediate(veh.GetComponent<WaypointArrow>());

    }

    //Utility
    [MenuItem("Window/RacingGameStarterKit/Utility/Clear Player Data")]
    public static void ClearPlayerData()
    {
        PlayerData.ClearData();
    }


    //About
    [MenuItem("Window/RacingGameStarterKit/About")]
    static void ShowAboutWindow()
    {
        GUIContent windowContent = new GUIContent();
        windowContent.text = "About";
        RGSK_About_Window window = (RGSK_About_Window)EditorWindow.GetWindow(typeof(RGSK_About_Window));
        window.titleContent = windowContent;
        window.minSize = new Vector2(250, 160);
        window.maxSize = new Vector2(250, 160);
        window.Show();
    }
}