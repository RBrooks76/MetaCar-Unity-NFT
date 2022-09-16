using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGSK;

public class Mine : MonoBehaviour
{
    public GameObject m_Explosion;
    bool m_isActive = false;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("active", 1.0f);
    }

    void active()
    {
        m_isActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_isActive == false)
            return;

        if (other.transform.name == "TargetRegion")
            return;

        if (other.transform.parent.gameObject.tag == "Player" || other.transform.parent.gameObject.tag == "Opponent")
        {
            Instantiate(m_Explosion, transform.position, transform.rotation);

            if (other.transform.parent.gameObject.tag == "Player")
            {
                RGSK.PlayerCamera playerCam = GameObject.FindObjectOfType(typeof(RGSK.PlayerCamera)) as RGSK.PlayerCamera;
                playerCam.Shake(0.3f, 3.0f, 200f);
            }

            other.transform.parent.gameObject.GetComponent<Car_Controller>().Damage(60);
            Destroy(this.gameObject);
        }
        Debug.Log("MineTriggerName=" + other.gameObject.name);

    }   
}
