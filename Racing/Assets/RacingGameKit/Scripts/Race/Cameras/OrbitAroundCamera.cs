using UnityEngine;
using System.Collections;

namespace RGSK
{
    /// <summary>
    /// The OrbitAroundCamera orbits around the "target" Transform with the given distance, height and rotateSpeed values
    /// </summary>
 
    public class OrbitAroundCamera : MonoBehaviour
    {

        public Transform target;
        public float distance = 7;
        public float height = 1f;
        public float rotateSpeed = 5.0f;
        private float x;

        void Update()
        {
            //Try and find a target
            if (!target && GameObject.FindGameObjectWithTag("Player"))
                target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void LateUpdate()
        {
            if (!target) return;

            x += Time.unscaledDeltaTime * rotateSpeed;

            Quaternion rotation = Quaternion.Euler(0, x, 0);
            Vector3 position = rotation * (new Vector3(0.0f, height, -distance)) + target.position;

            transform.rotation = rotation;
            transform.position = position;

            transform.LookAt(target);
        }
    }
}
