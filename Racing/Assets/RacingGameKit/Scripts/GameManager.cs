using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Singleton
    {
        get; private set;
    }

    public bool m_Mute = false;
    public float m_Volume = 1.0f;


    public bool m_isLogin = false;

    public string m_UserName = "";
    public int m_Exp = 0;

    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            DestroyObject(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
