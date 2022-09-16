using UnityEngine;
using System;
using System.Collections;


namespace RGSK
{

    [RequireComponent(typeof(Animator))]

    public class IKRacer : MonoBehaviour
    {

        private Animator animator;
        public bool enableIK = true;
        private Car_Controller car_controller;
        private Motorbike_Controller bike_controller;

        [Header("IK Targets")]
        public Transform rightHandTarget = null;
        public Transform leftHandTarget = null;
        public Transform leftFootTarget = null;
        public Transform rightFootTarget = null;
        public Transform driverLookTarget = null;

        [Header("Position Config")]
        public Vector3 seatPosition;

        [Header("Hand Rotation References")]
        public Transform leftHandRef;
        public Transform rightHandRef;

        private float initialDriverLookX;

        [HideInInspector]public float steer;

        void Awake()
        {
            animator = GetComponent<Animator>();

            if (transform.root.GetComponent<Car_Controller>())
            {
                car_controller = transform.root.GetComponent<Car_Controller>();
            }

            if (transform.root.GetComponent<Motorbike_Controller>())
            {
                bike_controller = transform.root.GetComponent<Motorbike_Controller>();
            }

            if (driverLookTarget)
            {
                initialDriverLookX = driverLookTarget.localPosition.x;
            }
        }

        void Update()
        {
            transform.localPosition = seatPosition;

            HeadLook();
        }

        void LateUpdate()
        {

            if (rightHandRef == null || leftHandRef == null) return;

            //Set the hand rotations according to the reference hand rotations

            Transform[] Lfingers = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponentsInChildren<Transform>();
            Transform[] Rfingers = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponentsInChildren<Transform>();
            Transform[] LfingersRef = leftHandRef.GetComponentsInChildren<Transform>();
            Transform[] RfingersRef = rightHandRef.GetComponentsInChildren<Transform>();

            for (int i = 1; i < Lfingers.Length; i++)
            {
                Lfingers[i].localRotation = LfingersRef[i].localRotation;
                Rfingers[i].localRotation = RfingersRef[i].localRotation;
            }
        }

        void HeadLook()
        {
            
            if(car_controller) steer = car_controller.steerInput;

            if (bike_controller) steer = bike_controller.steerInput;

            if (driverLookTarget)
            {
                Vector3 newLook = driverLookTarget.localPosition;
                newLook.x = steer + initialDriverLookX;
                driverLookTarget.localPosition = Vector3.Lerp(driverLookTarget.localPosition, newLook, Time.deltaTime * 3.0f);
            }
        }

        void OnAnimatorIK()
        {
            if (animator)
            {

                //if the IK is active, set the position and rotation directly to the goal. 
                if (enableIK)
                {

                    // Set the look target position, if one has been assigned
                    if (driverLookTarget != null)
                    {
                        animator.SetLookAtWeight(1);
                        animator.SetLookAtPosition(driverLookTarget.position);
                    }

                    // Set the hand target position and rotation, if assigned
                    if (rightHandTarget != null && leftHandTarget != null)
                    {

                        //Right Hand
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);

                        //Left Hand
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                    }

                    // Set the foot target positions, if assigned
                    if (rightFootTarget != null && leftFootTarget != null)
                    {

                        //Left Foot
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootTarget.rotation);

                        //Right Foot
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);
                    }
                }

                //if the IK is not active, set the position and rotation of the hand and head back to the original position
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetLookAtWeight(0);
                }
            }
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying && driverLookTarget)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(driverLookTarget.position, 0.1f);
            }
        }
        #endif
    }
}