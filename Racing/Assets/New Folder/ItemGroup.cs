using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGSK;

public class ItemGroup : MonoBehaviour
{
    public GameObject m_lap1;
    public GameObject m_lap2;
    public GameObject m_lap3;
    public GameObject m_lap4;

    private Statistics player;
    private int m_curlap = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Statistics>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player.lap == 1)
        {
            m_lap1.SetActive(true);
            m_lap2.SetActive(false);
            m_lap3.SetActive(false);
            m_lap4.SetActive(false);
        }
        if (player.lap == 2)
        {
            m_lap1.SetActive(false);
            m_lap2.SetActive(true);
            m_lap3.SetActive(false);
            m_lap4.SetActive(false);
        }
        if (player.lap == 3)
        {
            m_lap1.SetActive(false);
            m_lap2.SetActive(false);
            m_lap3.SetActive(true);
            m_lap4.SetActive(false);
        }
        if (player.lap == 4)
        {
            m_lap1.SetActive(false);
            m_lap2.SetActive(false);
            m_lap3.SetActive(false);
            m_lap4.SetActive(true);
        }
    }
}
