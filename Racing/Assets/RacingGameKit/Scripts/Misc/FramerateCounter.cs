using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RGSK
{
    [RequireComponent(typeof(Text))]

    public class FramerateCounter : MonoBehaviour
    {

        public float updateInterval = 0.5f;
        private Text fpsText;
        private float accum = 0;
        private int frames = 0;
        private float timeleft;
        private Color color;

        void Start()
        {
            fpsText = GetComponent<Text>();
            timeleft = updateInterval;
        }

        void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            if (timeleft <= 0.0)
            {
                float fps = accum / frames;
                int fpsInt = (int)fps;

                fpsText.text = fpsInt + " FPS";

                if (fps > 30)
                {
                    color = Color.green;
                }

                if (fps < 30 && fps > 20)
                {
                    color = Color.yellow;
                }

                if (fps < 20)
                {
                    color = Color.red;
                }

                fpsText.color = color;
                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
    }
}
