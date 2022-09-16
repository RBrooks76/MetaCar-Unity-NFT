//RankManager.cs handles setting each racer's position/rank
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{
    public class RankManager : MonoBehaviour
    {

        [System.Serializable]
        public class Ranker : IComparer<Ranker>
        {
            public GameObject racer;
            public float raceCompletion;
            public float speedRecord; //speed trap

            public int Compare(Ranker x, Ranker y)
            {
                if (RaceManager.instance._raceType != RaceManager.RaceType.SpeedTrap)
                {
                    return x.raceCompletion.CompareTo(y.raceCompletion);

                }
                else {
                    return x.speedRecord.CompareTo(y.speedRecord);
                }
            }
        }

        public static RankManager instance;
        [HideInInspector]
        public List<Ranker> racerRanks = new List<Ranker>(new Ranker[100]);//allow upto 100 racers
        [HideInInspector]
        public List<ProgressTracker> racerStats = new List<ProgressTracker>();
        [HideInInspector]
        public int totalRacers; //number of racers when the race begins
        [HideInInspector]
        public int currentRacers; //number of racers that are currently not knocked out or eliminated

        void Awake()
        {
            //create an instance
            instance = this;
        }


        void Start()
        {
            InvokeRepeating("SetCarRank", 0.1f, 0.5f);
        }


        //Finds the number of racers in the race.
        public void RefreshRacerCount()
        {
            Statistics[] m_racers = GameObject.FindObjectsOfType(typeof(Statistics)) as Statistics[];

            totalRacers = m_racers.Length;

            for (int i = 0; i < m_racers.Length; i++)
            {
                if (m_racers[i].knockedOut == false)
                {
                    if (!racerStats.Contains(m_racers[i].GetComponent<ProgressTracker>()))
                    {
                        racerStats.Add(m_racers[i].GetComponent<ProgressTracker>());
                    }
                }
                else {
                    racerStats.Remove(m_racers[i].GetComponent<ProgressTracker>());
                }
            }

            //Resize the list
            racerRanks.RemoveRange(totalRacers, racerRanks.Count - totalRacers);

            currentRacers = racerStats.Count;
        }


        void Update()
        {
            //Fill & sort the list in order
            for (int i = 0; i < currentRacers; i++)
            {
                if (racerRanks[i] != null && racerStats[i] != null)
                {
                    racerRanks[i].racer = racerStats[i].gameObject;
                    racerRanks[i].raceCompletion = racerStats[i].raceCompletion - ((float)racerStats[i].GetComponent<Statistics>().rank / 10000);
                    racerRanks[i].speedRecord = racerRanks[i].racer.GetComponent<Statistics>().speedRecord;
                }
            }

            Ranker m_ranker = new Ranker();
            racerRanks.Sort(m_ranker);
            racerRanks.Reverse();
        }


        //Sets the car ranks according to the sorted list
        void SetCarRank()
        {
            for (int r = 0; r < currentRacers; r++)
            {
                if (RaceManager.instance._raceType != RaceManager.RaceType.Drift)
                {
                    if (racerRanks[r].racer && racerRanks[r].racer.GetComponent<Statistics>())
                    {
                        racerRanks[r].racer.GetComponent<Statistics>().rank = r + 1;
                    }
                }
                else
                {
                    float totalPoints = (racerRanks[r].racer.GetComponent<DriftPointController>()) ? racerRanks[r].racer.GetComponent<DriftPointController>().totalDriftPoints : 0;

                    if (racerRanks[r].racer && racerRanks[r].racer.GetComponent<Statistics>())
                    {
                        if (totalPoints >= RaceManager.instance.goldDriftPoints)
                            racerRanks[r].racer.GetComponent<Statistics>().rank = 1;

                        if (totalPoints < RaceManager.instance.goldDriftPoints && totalPoints >= RaceManager.instance.silverDriftPoints)
                            racerRanks[r].racer.GetComponent<Statistics>().rank = 2;

                        if (totalPoints < RaceManager.instance.silverDriftPoints && totalPoints >= RaceManager.instance.bronzeDriftPoints)
                            racerRanks[r].racer.GetComponent<Statistics>().rank = 3;

                        if (totalPoints < RaceManager.instance.bronzeDriftPoints)
                            racerRanks[r].racer.GetComponent<Statistics>().rank = 4;
                    }
                }
            }
        }
    }
}
