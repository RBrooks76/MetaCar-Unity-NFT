using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firesplash.UnityAssets.SocketIO;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance { get; private set; }
    void Awake()
    {        
        if (Instance == null)
        {

            GetComponent<SocketIOCommunicator>().enabled = true;
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            //Destroy(gameObject);
            return;
        }
    }

    public SocketIOCommunicator GetSocketIOComponent()
    {
        return GetComponent<SocketIOCommunicator>();
    }
}
