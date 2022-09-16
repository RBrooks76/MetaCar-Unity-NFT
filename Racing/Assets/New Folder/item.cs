using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGSK;

public class item : MonoBehaviour
{
    public int m_Type = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.name == "Collider")
        {
            if (other.transform.parent.GetComponent<Car_Controller>().m_isPlayer)
            {
                other.transform.parent.GetComponent<Car_Controller>().GetItems(m_Type);
                Destroy(this.gameObject);
            }
        }
    }
}
