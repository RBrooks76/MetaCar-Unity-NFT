using UnityEngine;
using System.Collections;

namespace RGSK
{
    /// <summary>
    /// Brakezones are attached to a trigger and tell the AI to brake to the "targetSpeed" while in the trigger
    /// They are useful for really tight corners at high speeds that the AI wont be able to make with their current logic
    /// </summary>
	public class Brakezone : MonoBehaviour {

        [Tooltip("The speed the AI will try to get to when in this trigger")]
		public float targetSpeed = 50;
	
	}
}
