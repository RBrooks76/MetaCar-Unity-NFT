using UnityEngine;
using System.Collections;

namespace RGSK {

    /// <summary>
    /// MinimapFollowTarget is attached to your MinimapCamera to follow the player
    /// </summary>
    
    public class MinimapFollowTarget : MonoBehaviour
    {

        public Transform target;
        public float height = 100;
        public bool followPosition; //should the camera rotate with the target
        public bool followRotation; //should the camera rotate with the target

        void Update()
        {
            //find the target
            if (!target && GameObject.FindGameObjectWithTag("Player"))
                target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void LateUpdate()
        {
            if (!target) return;

            if (followRotation)         
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);

            if(followPosition)
                transform.position = new Vector3(target.position.x, height, target.position.z);            
        }
    }
}
