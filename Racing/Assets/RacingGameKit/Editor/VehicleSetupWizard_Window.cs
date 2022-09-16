using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RGSK;
using System.IO;

public class VehicleSetupWizard_Window : EditorWindow
{

    public enum VehicleType { Car, Motorbike }
    public enum VehicleControlType { Player, AI }
    public VehicleType vehicleType;
    public VehicleControlType vehicleControlType;
    public Transform vehicleToConfigure;
    public bool addCollider = true;
    public bool saveAsPrefab;

    public Transform FL;
    public Transform FR;
    public Transform RL;
    public Transform RR;

    void OnGUI()
    {
        //Main Vehicle Settings
        GUILayout.Label("Vehicle Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Select the type of vehicle");
            vehicleType = (VehicleType)EditorGUILayout.EnumPopup("", vehicleType);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Select the type");
            vehicleControlType = (VehicleControlType)EditorGUILayout.EnumPopup("", vehicleControlType);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Drag your vehicle model here");
            vehicleToConfigure = EditorGUILayout.ObjectField(vehicleToConfigure, typeof(Transform), true) as Transform;
        }
        EditorGUILayout.EndHorizontal();

        //Wheel Settings
        GUILayout.Label("Vehicle Wheel Settings", EditorStyles.boldLabel);

        switch (vehicleType)
        {
            case VehicleType.Car:
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Front Left Wheel");
                    FL = EditorGUILayout.ObjectField(FL, typeof(Transform), true) as Transform;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Front Right Wheel");
                    FR = EditorGUILayout.ObjectField(FR, typeof(Transform), true) as Transform;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Rear Left Wheel");
                    RL = EditorGUILayout.ObjectField(RL, typeof(Transform), true) as Transform;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Rear Right Wheel");
                    RR = EditorGUILayout.ObjectField(RR, typeof(Transform), true) as Transform;
                }
                EditorGUILayout.EndHorizontal();
                break;

            case VehicleType.Motorbike:
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Front Wheel");
                    FL = EditorGUILayout.ObjectField(FL, typeof(Transform), true) as Transform;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Rear Wheel");
                    RL = EditorGUILayout.ObjectField(RL, typeof(Transform), true) as Transform;
                }

                EditorGUILayout.EndHorizontal();
                break;
        }

        //Misc Settings
        GUILayout.Label("Misc Settings", EditorStyles.boldLabel);
        addCollider = EditorGUILayout.Toggle("Add Collider To Vehicle", addCollider);
        saveAsPrefab = EditorGUILayout.Toggle("Save Vehicle As Prefab", saveAsPrefab);

        EditorGUILayout.Space();  EditorGUILayout.Space();
        EditorGUILayout.Space();  EditorGUILayout.Space();

        if (GUILayout.Button("Configure Vehicle"))
        {
            if (!vehicleToConfigure) return;

            vehicleToConfigure.tag = (vehicleControlType == VehicleControlType.Player) ? "Player" : "Opponent";

            if(vehicleType == VehicleType.Car && FL && FR && RL && RR)
                CreateCar();

            if (vehicleType == VehicleType.Motorbike && FL && RL)
                CreateMotorbike();
        }
    }

    void CreateCar()
    {
        //Create Parent Objects
        GameObject wheelTransforms = new GameObject("WheelTransorms");
        GameObject wheelColliders = new GameObject("WheelColliders");
        GameObject vehicleAudio = new GameObject ("Audio");
        
        //Create WheelColliders
        GameObject FL_wheelCollider = new GameObject("FL Wheel Collider");
        FL_wheelCollider.AddComponent<AudioSource>();
        FL_wheelCollider.transform.position = FL.position;
        FL_wheelCollider.AddComponent<WheelCollider>();
        FL_wheelCollider.AddComponent<Wheels>();
        AdjustWheelColliderRadius(FL_wheelCollider.GetComponent<WheelCollider>(), FL);
        SetWheelColliderFrictionCurve(FL_wheelCollider.GetComponent<WheelCollider>());

        GameObject FR_wheelCollider = new GameObject("FR Wheel Collider");
        FR_wheelCollider.AddComponent<AudioSource>();
        FR_wheelCollider.transform.position = FR.position;
        FR_wheelCollider.AddComponent<WheelCollider>();
        FR_wheelCollider.AddComponent<Wheels>();
        AdjustWheelColliderRadius(FR_wheelCollider.GetComponent<WheelCollider>(), FR);
        SetWheelColliderFrictionCurve(FR_wheelCollider.GetComponent<WheelCollider>());

        GameObject RL_wheelCollider = new GameObject("RL Wheel Collider");
        RL_wheelCollider.AddComponent<AudioSource>();
        RL_wheelCollider.transform.position = RL.position;
        RL_wheelCollider.AddComponent<WheelCollider>();
        RL_wheelCollider.AddComponent<Wheels>();
        AdjustWheelColliderRadius(RL_wheelCollider.GetComponent<WheelCollider>(), RL);
        SetWheelColliderFrictionCurve(RL_wheelCollider.GetComponent<WheelCollider>());

        GameObject RR_wheelCollider = new GameObject("RR Wheel Collider");
        RR_wheelCollider.AddComponent<AudioSource>();
        RR_wheelCollider.transform.position = RR.position;
        RR_wheelCollider.AddComponent<WheelCollider>();
        RR_wheelCollider.AddComponent<Wheels>();
        AdjustWheelColliderRadius(RR_wheelCollider.GetComponent<WheelCollider>(), RR);
        SetWheelColliderFrictionCurve(RR_wheelCollider.GetComponent<WheelCollider>());

        //Create Audio
        GameObject engineAudio = new GameObject("Engine Audio");
        engineAudio.AddComponent<AudioSource>();
        engineAudio.transform.position = vehicleToConfigure.position;
        engineAudio.transform.parent = vehicleAudio.transform;
        GameObject nitroAudio = new GameObject("Nitro Audio");
        nitroAudio.AddComponent<AudioSource>();
        nitroAudio.transform.position = vehicleToConfigure.position;
        nitroAudio.transform.parent = vehicleAudio.transform;

        //Create Collider
        if (addCollider)
        {
            GameObject collider = new GameObject("Collider");
            collider.transform.position = vehicleToConfigure.position;
            collider.AddComponent<BoxCollider>();
            collider.GetComponent<BoxCollider>().size = new Vector3(2, 1.25f, 5);
            collider.GetComponent<BoxCollider>().center = new Vector3(0, 0.85f, 0);
            collider.transform.parent = vehicleToConfigure;
        }

        //Set Parents
        wheelTransforms.transform.parent = vehicleToConfigure;
        wheelColliders.transform.parent = vehicleToConfigure;
        vehicleAudio.transform.parent = vehicleToConfigure;
        FL.transform.parent = wheelTransforms.transform;
        FR.transform.parent = wheelTransforms.transform;
        RL.transform.parent = wheelTransforms.transform;
        RR.transform.parent = wheelTransforms.transform;
        FL_wheelCollider.transform.parent = wheelColliders.transform;
        FR_wheelCollider.transform.parent = wheelColliders.transform;
        RL_wheelCollider.transform.parent = wheelColliders.transform;
        RR_wheelCollider.transform.parent = wheelColliders.transform;

        //Create Rigidbody
        vehicleToConfigure.gameObject.AddComponent<Rigidbody>();
        vehicleToConfigure.GetComponent<Rigidbody>().mass = 1000;
        vehicleToConfigure.GetComponent<Rigidbody>().angularDrag = 0.05f;
        vehicleToConfigure.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

        //Add Components     
        if (!vehicleToConfigure.GetComponent<Car_Controller>())
        {
            vehicleToConfigure.gameObject.AddComponent<Car_Controller>();           
            vehicleToConfigure.gameObject.AddComponent<Statistics>();
            vehicleToConfigure.gameObject.AddComponent<ProgressTracker>();

            if (vehicleControlType == VehicleControlType.Player)
            {
                vehicleToConfigure.gameObject.AddComponent<PlayerControl>();
                vehicleToConfigure.gameObject.AddComponent<WaypointArrow>();
            }
            else
            {
                vehicleToConfigure.gameObject.AddComponent<OpponentControl>();
                vehicleToConfigure.GetComponent<Car_Controller>().steerHelper = 0.8f;
            }

            vehicleToConfigure.GetComponent<Car_Controller>().FL_Wheel = FL;
            vehicleToConfigure.GetComponent<Car_Controller>().FR_Wheel = FR;
            vehicleToConfigure.GetComponent<Car_Controller>().RL_Wheel = RL;
            vehicleToConfigure.GetComponent<Car_Controller>().RR_Wheel = RR;

            vehicleToConfigure.GetComponent<Car_Controller>().FL_WheelCollider = FL_wheelCollider.GetComponent<WheelCollider>();
            vehicleToConfigure.GetComponent<Car_Controller>().FR_WheelCollider = FR_wheelCollider.GetComponent<WheelCollider>();
            vehicleToConfigure.GetComponent<Car_Controller>().RL_WheelCollider = RL_wheelCollider.GetComponent<WheelCollider>();
            vehicleToConfigure.GetComponent<Car_Controller>().RR_WheelCollider = RR_wheelCollider.GetComponent<WheelCollider>();

            vehicleToConfigure.GetComponent<Car_Controller>().engineAudioSource = engineAudio.GetComponent<AudioSource>();
            vehicleToConfigure.GetComponent<Car_Controller>().nitroAudioSource = nitroAudio.GetComponent<AudioSource>();
        }

        FinishVehicleConfiguration();      
    }


    void CreateMotorbike()
    {
        //Create Parent Objects
        GameObject motorbikeChassis = new GameObject("Chassis");
        GameObject wheelTransforms = new GameObject("WheelTransorms");
        GameObject wheelColliders = new GameObject("WheelColliders");
        GameObject vehicleAudio = new GameObject("Audio");

        //Create WheelColliders
        GameObject FL_wheelCollider = new GameObject("FL Wheel Collider");
        FL_wheelCollider.AddComponent<AudioSource>();
        FL_wheelCollider.transform.position = FL.position;
        FL_wheelCollider.AddComponent<WheelCollider>();
        FL_wheelCollider.AddComponent<Wheels>();
        AdjustWheelColliderRadius(FL_wheelCollider.GetComponent<WheelCollider>(), FL);
        SetWheelColliderFrictionCurve(FL_wheelCollider.GetComponent<WheelCollider>());

        GameObject RL_wheelCollider = new GameObject("RL Wheel Collider");
        RL_wheelCollider.AddComponent<AudioSource>();
        RL_wheelCollider.transform.position = RL.position;
        RL_wheelCollider.AddComponent<WheelCollider>();
        RL_wheelCollider.AddComponent<Wheels>();
        AdjustWheelColliderRadius(RL_wheelCollider.GetComponent<WheelCollider>(), RL);
        SetWheelColliderFrictionCurve(RL_wheelCollider.GetComponent<WheelCollider>());

        //Create Audio
        GameObject engineAudio = new GameObject("Engine Audio");
        engineAudio.AddComponent<AudioSource>();
        engineAudio.transform.position = vehicleToConfigure.position;
        engineAudio.transform.parent = vehicleAudio.transform;
        GameObject nitroAudio = new GameObject("Nitro Audio");
        nitroAudio.AddComponent<AudioSource>();
        nitroAudio.transform.position = vehicleToConfigure.position;
        nitroAudio.transform.parent = vehicleAudio.transform;

        //Create Collider
        if (addCollider)
        {
            GameObject collider = new GameObject("Collider");
            collider.transform.position = vehicleToConfigure.position;
            collider.AddComponent<BoxCollider>();
            collider.GetComponent<BoxCollider>().size = new Vector3(0.9f, 1.25f, 3.25f);
            collider.GetComponent<BoxCollider>().center = new Vector3(0, 0.15f, 0);
            collider.transform.parent = vehicleToConfigure;
        }

        //Set Parents
        motorbikeChassis.transform.position = vehicleToConfigure.position;
        motorbikeChassis.transform.parent = vehicleToConfigure;
        wheelTransforms.transform.parent = vehicleToConfigure;
        wheelColliders.transform.parent = vehicleToConfigure;
        vehicleAudio.transform.parent = vehicleToConfigure;
        FL.transform.parent = wheelTransforms.transform;
        RL.transform.parent = wheelTransforms.transform;
        FL_wheelCollider.transform.parent = wheelColliders.transform;
        RL_wheelCollider.transform.parent = wheelColliders.transform;

        //Add additional children to chassis
        List<Transform> chassisChildren = new List<Transform>();
        foreach (Transform t in vehicleToConfigure)
        {
            if (t != motorbikeChassis.transform)
                chassisChildren.Add(t);
        }
        foreach (Transform c in chassisChildren)
        {
            c.parent = motorbikeChassis.transform;
        }

        //Create Rigidbody
        vehicleToConfigure.gameObject.AddComponent<Rigidbody>();
        vehicleToConfigure.GetComponent<Rigidbody>().mass = 800;
        vehicleToConfigure.GetComponent<Rigidbody>().angularDrag = 0.05f;
        vehicleToConfigure.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

        //Add Components     
        if (!vehicleToConfigure.GetComponent<Motorbike_Controller>())
        {
            vehicleToConfigure.gameObject.AddComponent<Motorbike_Controller>();
            vehicleToConfigure.gameObject.AddComponent<Statistics>();
            vehicleToConfigure.gameObject.AddComponent<ProgressTracker>();

            if (vehicleControlType == VehicleControlType.Player)
            {
                vehicleToConfigure.gameObject.AddComponent<PlayerControl>();
                vehicleToConfigure.gameObject.AddComponent<WaypointArrow>();
            }
            else
            {
                vehicleToConfigure.gameObject.AddComponent<OpponentControl>();
                vehicleToConfigure.GetComponent<Motorbike_Controller>().steerHelper = 0.8f;
            }

            vehicleToConfigure.GetComponent<Motorbike_Controller>().chassis = motorbikeChassis;

            vehicleToConfigure.GetComponent<Motorbike_Controller>().frontWheelTransform = FL;
            vehicleToConfigure.GetComponent<Motorbike_Controller>().rearWheelTransform = RL;

            vehicleToConfigure.GetComponent<Motorbike_Controller>().frontWheelCollider = FL_wheelCollider.GetComponent<WheelCollider>();
            vehicleToConfigure.GetComponent<Motorbike_Controller>().rearWheelCollider = RL_wheelCollider.GetComponent<WheelCollider>();

            vehicleToConfigure.GetComponent<Motorbike_Controller>().engineAudioSource = engineAudio.GetComponent<AudioSource>();
            vehicleToConfigure.GetComponent<Motorbike_Controller>().nitroAudioSource = nitroAudio.GetComponent<AudioSource>();
        }

        FinishVehicleConfiguration();
    }

    void FinishVehicleConfiguration()
    {
        //Save
        if (saveAsPrefab) { SavePrefab(vehicleToConfigure); }

        //Focus on the vehicle
        Selection.objects = new Object[] { vehicleToConfigure.gameObject };
        SceneView.lastActiveSceneView.FrameSelected();

        Debug.Log("Vehicle was successfully configured!");

        this.Close();  
    }

    void SavePrefab(Transform prefab)
    {
        switch (vehicleControlType)
        {

            case VehicleControlType.Player:

                //create the folder
                if (!Directory.Exists("Assets/Resources/PlayerVehicles"))
                {
                    Directory.CreateDirectory("Assets/Resources/PlayerVehicles");
                    AssetDatabase.Refresh();
                }

                //Create the prefab
                string prefabPath = "Assets/Resources/PlayerVehicles/" + prefab.name + ".prefab";
                Object newPrefab = PrefabUtility.CreatePrefab(prefabPath, prefab.gameObject);

                //Replace it
                PrefabUtility.ReplacePrefab(prefab.gameObject, newPrefab, ReplacePrefabOptions.ConnectToPrefab);

                break;

            case VehicleControlType.AI:

                //Create the folder
                if (!Directory.Exists("Assets/Prefabs/AIVehicles"))
                {
                    Directory.CreateDirectory("Assets/Prefabs/AIVehicles");
                    AssetDatabase.Refresh();
                }

                //Create the prefab
                string ai_prefabPath = "Assets/Prefabs/AIVehicles/" + prefab.name + ".prefab";
                Object ai_newPrefab = PrefabUtility.CreatePrefab(ai_prefabPath, prefab.gameObject);

                //Replace it
                PrefabUtility.ReplacePrefab(prefab.gameObject, ai_newPrefab, ReplacePrefabOptions.ConnectToPrefab);

                break;
        }
    }

    void AdjustWheelColliderRadius(WheelCollider wc, Transform wt)
    {
        wc.center = new Vector3(0, 0.15f, 0);

        if (wt.GetComponent<Renderer>())
        {
            float wheelRadius;
            wheelRadius = wt.GetComponent<Renderer>().bounds.size.y / 2;
            wc.radius = wheelRadius;
        }
    }

    void SetWheelColliderFrictionCurve(WheelCollider wc)
    {
        WheelFrictionCurve FWDCurve = wc.forwardFriction;
        FWDCurve.stiffness = 2.0f;
        wc.forwardFriction = FWDCurve;

        //Set a 0.5 sideways friction for bike's front wheel
        if (vehicleType == VehicleType.Motorbike && wc.name.Contains("FL"))
        {
            WheelFrictionCurve SWDCurve = wc.forwardFriction;
            SWDCurve.stiffness = 0.5f;
            wc.sidewaysFriction = SWDCurve;
        }
    }
}
