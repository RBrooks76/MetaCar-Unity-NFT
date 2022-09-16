//Simple class that fades in/out a UI Text.
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RGSK
{
    [RequireComponent(typeof(Text))]
    public class TextAlpha : MonoBehaviour
    {

        public float fadeSpeed = 2.0f;
        private Text text;

        void Start()
        {
            text = this.GetComponent<Text>();
        }

        void Update()
        {
            if (!text.enabled || text.text == "")
                return;

            Color alpha = text.color;

            alpha.a = (Mathf.PingPong(Time.unscaledTime * fadeSpeed, 1.0f));

            text.color = alpha;
        }
    }
}
