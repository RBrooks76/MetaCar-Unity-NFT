//Path_Creator.cs helps visually create a path around your race track

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{


    [ExecuteInEditMode]
    public class PathCreator : MonoBehaviour
    {

        [HideInInspector]
        public Transform[] nodes;
        private Color pathColor = new Color(1, 1, 1, 0.2f);
        private Color nodeColor = Color.yellow;
        public bool layoutMode;
        //public bool looped = true;

        void OnDrawGizmos()
        {
            Gizmos.color = nodeColor;

            //Draw green cube on main transform to show path parent
            Gizmos.DrawWireCube(transform.position, new Vector3(2, 2, 2));

            //Draw spheres on each node
            if (nodes.Length > 0)
            {
                for (int i = 1; i < nodes.Length; i++)
                {
                    Gizmos.DrawWireSphere(new Vector3(nodes[i].position.x, nodes[i].position.y + 1.0f, nodes[i].position.z), .75f);
                }
            }
        }


        void Update()
        {
            Transform[] transforms = GetComponentsInChildren<Transform>();

            nodes = new Transform[transforms.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = transforms[i];
            }

            for (int n = 0; n < nodes.Length; n++)
            {
                //if (looped)
                //{
                    Debug.DrawLine(nodes[n].position - Vector3.down, nodes[(n + 1) % nodes.Length].position - Vector3.down, pathColor);
                //}

                //else
                //{
                //   if(n < nodes.Length - 1)
                //        Debug.DrawLine(nodes[n].position - Vector3.down, nodes[(n + 1)].position - Vector3.down, pathColor);
                //}
            }

            int c = 0;
            foreach (Transform child in transforms)
            {
                if (child != transform)
                {
                    child.name = (c++).ToString("000");
                }
            }
        }


        public void AlignToGround()
        {
            for (int i = 1; i < nodes.Length; i++)
            {
                Ray ray = new Ray(nodes[i].position, -transform.up);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 500))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                    {
                        nodes[i].position = new Vector3(nodes[i].position.x, hit.collider.transform.position.y, nodes[i].position.z);
                    }
                }
            }
        }

        public void DeleteLastNode()
        {
            Transform[] nodes = GetComponentsInChildren<Transform>();

            if (nodes.Length > 2)
                DestroyImmediate(nodes[nodes.Length - 1].gameObject);
        }
    }
}
