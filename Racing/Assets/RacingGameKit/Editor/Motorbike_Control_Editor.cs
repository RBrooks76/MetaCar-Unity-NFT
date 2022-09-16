using UnityEngine;
using System.Collections;
using UnityEditor;
using RGSK;

[CustomEditor(typeof(Motorbike_Controller)), CanEditMultipleObjects]
public class Motorbike_Control_Editor : Editor
{


    Motorbike_Controller m_target;

    public void OnEnable()
    {
        m_target = (Motorbike_Controller)target;
    }

    public override void OnInspectorGUI()
    {
        //Wheel
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Wheel Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        m_target._speedUnit = (Motorbike_Controller.SpeedUnit)EditorGUILayout.EnumPopup("Speed Units", m_target._speedUnit);

        EditorGUILayout.Space();

        m_target.frontWheelTransform = EditorGUILayout.ObjectField("Front Wheel Transform", m_target.frontWheelTransform, typeof(Transform), true) as Transform;
        m_target.rearWheelTransform = EditorGUILayout.ObjectField("Rear Wheel Transform", m_target.rearWheelTransform, typeof(Transform), true) as Transform;


        EditorGUILayout.Space();

        m_target.frontWheelCollider = EditorGUILayout.ObjectField("Front WheelCollider", m_target.frontWheelCollider, typeof(WheelCollider), true) as WheelCollider;
        m_target.rearWheelCollider = EditorGUILayout.ObjectField("Rear WheelCollider", m_target.rearWheelCollider, typeof(WheelCollider), true) as WheelCollider;

        EditorGUILayout.Space();

        m_target.forwardSlipLimit = EditorGUILayout.FloatField("Forward Slip Limit", m_target.forwardSlipLimit);
        m_target.sidewaySlipLimit = EditorGUILayout.FloatField("Sideways Slip Limit", m_target.sidewaySlipLimit);

        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //Engine
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Engine Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        m_target.engineTorque = EditorGUILayout.FloatField("Engine Torque", m_target.engineTorque);
        m_target.brakeTorque = EditorGUILayout.FloatField("Brake Torque", m_target.brakeTorque);
        m_target.maxSteerAngle = EditorGUILayout.FloatField("Max Steer Angle", m_target.maxSteerAngle);
        m_target.numberOfGears = EditorGUILayout.IntField("Total Gears", m_target.numberOfGears);
        m_target.topSpeed = EditorGUILayout.FloatField("Top Speed", m_target.topSpeed);
        m_target.brakeForce = EditorGUILayout.FloatField("Brake Force", m_target.brakeForce);
        m_target.boost = EditorGUILayout.FloatField("Boost", m_target.boost);
        m_target.controllable = EditorGUILayout.Toggle("Controllable", m_target.controllable);
        m_target.enableSlipstream = EditorGUILayout.Toggle("Slipstream", m_target.enableSlipstream);
        m_target.enableNitro = EditorGUILayout.Toggle("Nitro", m_target.enableNitro);
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //Stability
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Stability Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target.centerOfMass = EditorGUILayout.Vector3Field("Center Of Mass", m_target.centerOfMass);
        EditorGUILayout.Space();
        m_target.chassis = EditorGUILayout.ObjectField("Chassis", m_target.chassis, typeof(GameObject), true) as GameObject;
        m_target.leanAmount = EditorGUILayout.FloatField("Chasis Lean Amount", m_target.leanAmount);
        m_target.maxLeanAngle = EditorGUILayout.FloatField("Max Lean Angle", m_target.maxLeanAngle);
        m_target.leanDamping = EditorGUILayout.FloatField("Lean Damping", m_target.leanDamping);
        m_target.downforce = EditorGUILayout.FloatField("Downforce", m_target.downforce);
        m_target.steerHelper = EditorGUILayout.Slider("Steer Helper", m_target.steerHelper, 0.0f, 1.0f);
        m_target.traction = EditorGUILayout.Slider("Traction", m_target.traction, 0.0f, 1.0f);
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //Sound
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Sound Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        //Engine sound
        m_target.engineAudioSource = EditorGUILayout.ObjectField("Engine AudioSource", m_target.engineAudioSource, typeof(AudioSource), true) as AudioSource;
        m_target.engineSound = EditorGUILayout.ObjectField("Engine Sound", m_target.engineSound, typeof(AudioClip), true) as AudioClip;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Crash Sounds");

        //Crash sounds
        for (int i = 0; i < m_target.crashSounds.Count; i++)
        {
            m_target.crashSounds[i] = EditorGUILayout.ObjectField((i + 1).ToString(), m_target.crashSounds[i], typeof(AudioClip), true) as AudioClip;
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Add Sound", GUILayout.Width(130)))
        {
            AudioClip newClip = null;
            m_target.crashSounds.Add(newClip);
        }
        if (GUILayout.Button("Remove Sound", GUILayout.Width(130)))
        {
            if (m_target.crashSounds.Count > 0)
            {
                m_target.crashSounds.Remove(m_target.crashSounds[m_target.crashSounds.Count - 1]);
            }
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        //Misc
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Misc Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        m_target.brakelightGroup = EditorGUILayout.ObjectField("Brake Lights", m_target.brakelightGroup, typeof(GameObject), true) as GameObject;
        m_target.handlebars = EditorGUILayout.ObjectField("Handle Bars", m_target.handlebars, typeof(GameObject), true) as GameObject;
        GUILayout.EndVertical();


        if (m_target.enableSlipstream)
        {
            EditorGUILayout.Space();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.Space();
            GUILayout.Box("Slipstream Settings", EditorStyles.boldLabel);
            m_target.slipstreamStrength = EditorGUILayout.Slider("Slipstream Strength", m_target.slipstreamStrength, 0.1f, 5);
            m_target.slipstreamRayHeight = EditorGUILayout.FloatField("Slipstream Ray Height", m_target.slipstreamRayHeight);
            m_target.slipstreamRayLength = EditorGUILayout.FloatField("Slipstream Ray Length", m_target.slipstreamRayLength);
            GUILayout.EndVertical();
        }

        if (m_target.enableNitro)
        {
            EditorGUILayout.Space();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.Space();
            GUILayout.Box("Nitro Settings", EditorStyles.boldLabel);
            m_target.nitroGroup = EditorGUILayout.ObjectField("Nitro Group", m_target.nitroGroup, typeof(GameObject), true) as GameObject;
            m_target.nitroAudioSource = EditorGUILayout.ObjectField("Nitro AudioSource", m_target.nitroAudioSource, typeof(AudioSource), true) as AudioSource;
            m_target.nitroSound = EditorGUILayout.ObjectField("Nitro Sound", m_target.nitroSound, typeof(AudioClip), true) as AudioClip;
            EditorGUILayout.Space();
            m_target.nitroStrength = EditorGUILayout.Slider("Nitro Strength", m_target.nitroStrength, 0.1f, 10);
            m_target.nitroRegenerationRate = EditorGUILayout.FloatField("Nitro Regeneration Rate", m_target.nitroRegenerationRate);
            m_target.nitroDepletionRate = EditorGUILayout.FloatField("Nitro Depletion Rate", m_target.nitroDepletionRate);
            GUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        //Input
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Input", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        m_target.motorInput = EditorGUILayout.FloatField("Motor Input", m_target.motorInput);
        m_target.brakeInput = EditorGUILayout.FloatField("Brake Input", m_target.brakeInput);
        m_target.steerInput = EditorGUILayout.FloatField("Steer Input", m_target.steerInput);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndVertical();

        //Set dirty
        if (GUI.changed) { EditorUtility.SetDirty(m_target); }
    }
}
