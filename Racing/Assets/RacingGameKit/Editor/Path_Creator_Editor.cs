using UnityEngine;
using System.Collections;
using UnityEditor;
using RGSK;

[CustomEditor(typeof(PathCreator))]
public class Path_Creator_Editor : Editor
{

    PathCreator m_target;
    RaycastHit hit;

    public void OnEnable()
    {
        m_target = (PathCreator)target;
    }

    public override void OnInspectorGUI()
    {
        //LOGO
        Texture logo = (Texture)Resources.Load("EditorUI/RGSKLogo");
        GUILayout.Label(logo, GUILayout.Height(50));

        GUILayout.BeginVertical("Box");
        GUILayout.Box("Path Creation", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This component will help you visually create a path around your track.\n\nEnabling 'Node Layout Mode' will allow you to place nodes by clicking. \n\nClick the 'Finish' button when you are done.", MessageType.Info);

        m_target.layoutMode = EditorGUILayout.Toggle("Node Layout Mode", m_target.layoutMode);

        //Not yet
        //m_target.looped = EditorGUILayout.Toggle("Looped", m_target.looped);

        //Ground em' all!
        if (GUILayout.Button("Align Nodes To Ground"))
        {
            m_target.AlignToGround();
        }
		
		//Delete the last placed node
        if (GUILayout.Button("Delete Last Node"))
        {
            m_target.DeleteLastNode();
        }

        //Finish
        if (GUILayout.Button("Finish"))
        {
            CreateWaypointCircuit();
        }
        
        GUILayout.EndVertical();
    }

    void OnSceneGUI()
    {
    	
    	//Handle UI
        Handles.BeginGUI();
    	
    	Rect outRect = new Rect(Screen.width - 250, Screen.height - 100, 200, 50);
    	
    	GUILayout.BeginArea(new Rect( outRect));
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Node Layout Mode : " + m_target.layoutMode, EditorStyles.boldLabel);
        string s = (m_target.layoutMode) ? "Disable" : "Enable";
        if (GUILayout.Button(s))
        {
             m_target.layoutMode = !m_target.layoutMode;
        }
        GUILayout.EndVertical();
		GUILayout.EndArea();
  
        Handles.EndGUI();

         //Handles.Label(m_target.transform.position, "Path");

         //Layout Mode
        if (m_target.layoutMode)
        {
            if (Event.current.type == EventType.MouseDown)
            {

                Event e = Event.current;
				
				
                if (e.button == 0)
                {
                    //Make sure we cant click anythingelse
                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
				    GUIUtility.hotControl = controlID;
                    e.Use();

                    //Create a new node at clicked pos
                    Ray sceneRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    if (Physics.Raycast(sceneRay, out hit, 1000))
                    {
                        GameObject newNode = new GameObject("Node");
                        newNode.transform.position = hit.point;
                        newNode.transform.parent = m_target.transform;
                    }
                }
                else
                {
                    //Reset hot control
                    GUIUtility.hotControl = 0; 
                }
            }
        }
    }

    public void CreateWaypointCircuit()
    {
        WaypointCircuit circuit = m_target.gameObject.AddComponent<WaypointCircuit>();
        circuit.AddWaypointsFromChildren();
        //circuit.loopedPath = m_target.looped;
        DestroyImmediate(m_target.gameObject.GetComponent<PathCreator>());
    }
}
