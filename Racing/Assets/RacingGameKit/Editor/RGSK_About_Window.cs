using UnityEngine;
using UnityEditor;

public class RGSK_About_Window : EditorWindow
{

    GUIStyle style;
    public string version = "1.1.0a";

    void SetupGUIStyle()
    {
        style = new GUIStyle(EditorStyles.boldLabel);
        style.alignment = TextAnchor.MiddleCenter;
    }

    void OnGUI()
    {
        if (style == null)
            SetupGUIStyle();

        GUILayout.BeginVertical("Box");

        //LOGO
        Texture logo = (Texture)Resources.Load("EditorUI/RGSKLogo");
        GUILayout.Label(logo, style, GUILayout.Height(100));

        GUILayout.Label("Version " + version,style);

        if (GUILayout.Button("Forums"))
        {
            Application.OpenURL("http://forum.unity3d.com/threads/racing-game-starter-kit-easily-create-racing-games.337366/");
        }

        GUILayout.EndVertical();
    }
}
