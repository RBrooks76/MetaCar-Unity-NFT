using UnityEngine;
using System.Collections.Generic;

namespace RGSK
{
    //IKBoneViewer helps you visualize and tweak your IK racer's hand bone rotation reference.

    [ExecuteInEditMode]
    public class IKBoneViewer : MonoBehaviour
    {

        public Transform rootNode; //This would be your hand reference Transform
        public Color gizmoColor = Color.red;
        [HideInInspector]public Transform[] childBones;

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (rootNode != null)
            {
                if (childBones.Length <= 0)
                {
                    //get all joints to draw
                    AddBones();
                }


                foreach (Transform child in childBones)
                {
                    if (child != rootNode)
                    {
                        Gizmos.color = gizmoColor;

                        Gizmos.DrawLine(child.position, child.parent.position);

                        Gizmos.DrawSphere(child.position, 0.005f);
                    }
                }
            }
        }

        public void AddBones()
        {
            childBones = rootNode.GetComponentsInChildren<Transform>();
        }
#endif
    }
}
