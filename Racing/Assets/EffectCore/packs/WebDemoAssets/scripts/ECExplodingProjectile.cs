using UnityEngine;
using System.Collections;
using RGSK;

/* THIS CODE IS JUST FOR PREVIEW AND TESTING */
// Feel free to use any code and picking on it, I cannot guaratnee it will fit into your project
public class ECExplodingProjectile : MonoBehaviour
{
    public GameObject impactPrefab;
    public GameObject explosionPrefab;
    public float thrust;

    public Rigidbody thisRigidbody;

    public GameObject particleKillGroup;
    private Collider thisCollider;

    public bool LookRotation = true;
    public bool Missile = false;
    public Transform missileTarget;
    public float projectileSpeed;
    public float projectileSpeedMultiplier;

    public bool ignorePrevRotation = false;

    public bool explodeOnTimer = false;
    public float explosionTimer;
    float timer;

    private Vector3 previousPosition;

    // Use this for initialization
    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        if (Missile)
        {
            if (RaceManager.instance.m_Target)
                missileTarget = RaceManager.instance.m_Target.transform;
            else
                Missile = false;
        }

        thisCollider = GetComponent<Collider>();
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        /*     if(Input.GetButtonUp("Fire2"))
             {
                 Explode();
             }*/
        timer += Time.deltaTime;
        if (timer >= explosionTimer && explodeOnTimer == true)
        {
            Explode();
        }

    }

    void FixedUpdate()
    {
        if (Missile)
        {
            projectileSpeed += projectileSpeed * projectileSpeedMultiplier;
            //   transform.position = Vector3.MoveTowards(transform.position, missileTarget.transform.position, 0);

            transform.LookAt(missileTarget);

            thisRigidbody.AddForce(transform.forward * projectileSpeed);
        }

        if (LookRotation && timer >= 0.05f)
        {
            transform.rotation = Quaternion.LookRotation(thisRigidbody.velocity);
        }

        CheckCollision(previousPosition);

        previousPosition = transform.position;
    }

    void CheckCollision(Vector3 prevPos)
    {
        RaycastHit hit;
        Vector3 direction = transform.position - prevPos;
        Ray ray = new Ray(prevPos, direction);
        float dist = Vector3.Distance(transform.position, prevPos);
        if (Physics.Raycast(ray, out hit, dist) && hit.collider.transform.name != "TargetRegion" && !hit.collider.transform.name.Contains("Mine"))
        {
            transform.position = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            Vector3 pos = hit.point;
            GameObject impact = Instantiate(impactPrefab, pos, rot);
            impact.transform.SetParent(hit.collider.transform.parent);

            Explode();

            if (!explodeOnTimer && Missile == false)
            {
                if (hit.collider.transform.parent.GetComponent<Car_Controller>())
                    hit.collider.transform.parent.GetComponent<Car_Controller>().Damage(30);

                Destroy(gameObject);
            }
            else if (Missile == true)
            {
                //thisCollider.enabled = false;
                //particleKillGroup.SetActive(false);
                //thisRigidbody.velocity = Vector3.zero;
                if (hit.collider.transform.parent.GetComponent<Car_Controller>())
                    hit.collider.transform.parent.GetComponent<Car_Controller>().Damage(100);
                Destroy(gameObject, 5);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        return;

        if (collision.gameObject.tag != "FX")
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
            if (ignorePrevRotation)
            {
                rot = Quaternion.Euler(0, 0, 0);
            }
            Vector3 pos = contact.point;
            GameObject impact = Instantiate(impactPrefab, pos, rot);
            impact.transform.SetParent(collision.transform.parent);

            if (!explodeOnTimer && Missile == false)
            {
                Destroy(gameObject);
            }
            else if (Missile == true)
            {

                thisCollider.enabled = false;
                particleKillGroup.SetActive(false);
                thisRigidbody.velocity = Vector3.zero;

                Destroy(gameObject, 5);

            }
        }
    }

    void Explode()
    {
        if (explosionPrefab)
        {
            Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
            Destroy(gameObject);    
        }
    }

}