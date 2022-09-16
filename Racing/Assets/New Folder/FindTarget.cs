using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGSK;

public class FindTarget : MonoBehaviour
{
    public GameObject m_Target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent && other.transform.parent.CompareTag("Opponent"))
        {
            RaceManager.instance.m_Target = other.transform.parent.gameObject;

            Debug.Log("Target=" + RaceManager.instance.m_Target.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent && other.transform.parent.CompareTag("Opponent"))
        {
            if (RaceManager.instance.m_Target == other.transform.parent.gameObject)
            {
                RaceManager.instance.m_Target = null;
                //Debug.Log("Target=Null");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.parent && other.transform.parent.CompareTag("Opponent"))
        {
            if (RaceManager.instance.m_Target == null)
            {
                RaceManager.instance.m_Target = other.transform.parent.gameObject;

                //Debug.Log("Target=" + RaceManager.instance.m_Target.name);
            }
        }
    }
}
