using UnityEngine;
using System.Collections;

//ChildCameraPosition.cs simply handles telling the player camera what CameraMode it should set itself to on this child camera position

namespace RGSK
{
    public class ChildCameraPosition : MonoBehaviour
    {
        public PlayerCamera.CameraMode mode = PlayerCamera.CameraMode.Fixed;
    }
}
