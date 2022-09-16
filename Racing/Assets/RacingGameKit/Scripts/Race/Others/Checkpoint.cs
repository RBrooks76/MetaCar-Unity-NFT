using UnityEngine;
using System.Collections;

namespace RGSK
{
    public class Checkpoint : MonoBehaviour
    {

        public enum CheckpointType { Speedtrap, TimeCheckpoint };
        public CheckpointType checkpointType;
        public float timeToAdd = 10.0f; //time to add (Checkpoints Race Only)

    }
}
