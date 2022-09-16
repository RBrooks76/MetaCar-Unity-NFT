using UnityEngine;
using System.Collections;

namespace RGSK
{
    /// <summary>
    /// RacerName.cs handles displaying a racer's name above their vehicle
    /// </summary>

    public class RacerName : MonoBehaviour
    {

        public enum DisplayMode { OnlyRankAhead, AlwaysDisplay }
        public DisplayMode displayMode;
        [HideInInspector]
        public Transform target; //This is automatically assigned by the RaceManager
        private Statistics target_stats;

        [Header("3D Texts")]
        public TextMesh racerPosition;
        public TextMesh racerName;
        public TextMesh racerDistance;

        [Header("Misc Settings")]
        public Vector3 positionOffset = new Vector3(0, 1.5f, 0.5f);
        public float visibleDistance = 30.0f; //How far(in Meters) you have to be from a racer to see their names.
        private GameObject player;

        public void Initialize()
        {
            //Set it's position & rotation
            Vector3 rot;
            transform.position = target.localPosition + positionOffset;
            rot = target.transform.eulerAngles;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, rot.y, transform.eulerAngles.z);
            transform.parent = target;

            //Get the target's statistics component
            target_stats = target.GetComponent<Statistics>();

            //Find the player
            if (GameObject.FindGameObjectWithTag("Player"))
                player = GameObject.FindGameObjectWithTag("Player");
        }

        void Update()
        {

            if (!player || !target) return;

            switch (displayMode)
            {

                //Always Display
                case DisplayMode.AlwaysDisplay:

                    if (GetDistanceFromPlayer() <= visibleDistance && !IsPlayerAhead())
                    {

                        Display();
                    }

                    //Handle visibilty
                    foreach (Transform t in transform)
                    {
                        if (RaceManager.instance && RaceManager.instance._raceState == RaceManager.RaceState.Racing)
                        {
                            t.gameObject.SetActive(GetDistanceFromPlayer() <= visibleDistance && !IsPlayerAhead());
                        }
                        else
                        {
                            t.gameObject.SetActive(false);
                        }
                    }
                    break;


                case DisplayMode.OnlyRankAhead:

                    if (GetDistanceFromPlayer() <= visibleDistance && !IsPlayerAhead() && target.GetComponent<Statistics>().rank == player.GetComponent<Statistics>().rank - 1)
                    {

                        Display();
                    }

                    //Update gameObject visiblity
                    foreach (Transform t in transform)
                    {
                        if (RaceManager.instance && RaceManager.instance._raceState == RaceManager.RaceState.Racing)
                        {
                            t.gameObject.SetActive(GetDistanceFromPlayer() <= visibleDistance && !IsPlayerAhead() && target.GetComponent<Statistics>().rank == player.GetComponent<Statistics>().rank - 1);
                        }
                        else
                        {
                            t.gameObject.SetActive(false);
                        }
                    }
                    break;
            }
        }

        void Display()
        {
            //Show Position if assigned
            if (racerPosition)
            {
                racerPosition.text = target_stats.rank.ToString();
            }

            //Show Name if assigned
            if (racerName)
            {
                racerName.text = target_stats.racerDetails.racerName;
            }

            //Show Distance if assigned
            if (racerDistance)
            {
                racerDistance.text = (int)GetDistanceFromPlayer() + "M";
            }
        }

        bool IsPlayerAhead()
        {
            Vector3 targetPos = player.transform.InverseTransformPoint(target.position);

            if (targetPos.z < 0)
            {
                return true;
            }
            else {
                return false;
            }
        }

        float GetDistanceFromPlayer()
        {
            return Vector3.Distance(transform.position, player.transform.position);
        }
    }
}
