using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RGSK
{

    /// <summary>
    /// SoundManager handles playing game sounds and music
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {

        [System.Serializable]
        public class DefaultSounds
        {
            public AudioClip countdownSound;
            public AudioClip startRaceSound;
            public AudioClip checkpointSound;
            public AudioClip speedTrapSound;
        }

        [System.Serializable]
        public class AdditionalGameSounds
        {
            public string soundName;
            public AudioClip sound;
        }

        public static SoundManager instance;

        private AudioSource audioSource;

        [Header("Default Sounds")]
        public DefaultSounds defaultSounds;

        [Header("Additional Sounds")]
        public List<AdditionalGameSounds> additionalGameSounds = new List<AdditionalGameSounds>();

        [Header("Background Music")]
        public MusicStart musicStart;
        public PlayMode playMode;
        [Range(0, 1)]
        public float musicVolume = 0.25f;
        public List<AudioClip> backgroundMusic = new List<AudioClip>();
        public enum PlayMode { Order, Random }
        public enum MusicStart { Immediate, BeforeCountdown, AfterCountdown }
        private AudioSource bgmAudio;
        private int trackIndex;
        private int lastIndex;

        void Awake()
        {
            instance = this;

            audioSource = GetComponent<AudioSource>();

            SetVolume();
        }

        void Start()
        {
            if (musicStart == MusicStart.Immediate)
                StartMusic();
        }

        #region Sounds

        //Plays a default sound
        public void PlayDefaultSound(AudioClip c)
        {
            audioSource.spatialBlend = 0;

            audioSource.PlayOneShot(c);
        }

        //Plays a sound in the list with 2 parameters - it's name and whether it's 2D/3D
        public void PlaySound(string name, bool sound2D)
        {
            if (sound2D)
            {
                audioSource.spatialBlend = 0;
            }
            else {
                audioSource.spatialBlend = 1;
            }

            for (int i = 0; i < additionalGameSounds.Count; i++)
            {
                if (name == additionalGameSounds[i].soundName)
                {
                    audioSource.PlayOneShot(additionalGameSounds[i].sound);
                }
            }
        }

        //Optional if you want to play sound in the list at a certain location
        public void PlaySoundAtLocation(string name, Vector3 location)
        {
            audioSource.spatialBlend = 1;

            for (int i = 0; i < additionalGameSounds.Count; i++)
            {
                if (name == additionalGameSounds[i].soundName)
                {
                    AudioSource.PlayClipAtPoint(additionalGameSounds[i].sound, location);
                }
            }
        }

        //Optional if you want to play a clip located in a different class at a certain location
        public void PlayClip(AudioClip clip, Vector3 position, float volume, float minDistance)
        {
            GameObject go = new GameObject("One shot audio");
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>() as AudioSource;
            source.spatialBlend = 1.0f;
            source.clip = clip;
            source.volume = volume;
            source.minDistance = minDistance;
            source.Play();
            Destroy(go, clip.length);
        }
        #endregion

        #region Music
        public void StartMusic()
        {
            if (backgroundMusic.Count > 0)
            {
                //Set up bgm audio source
                GameObject bgm = new GameObject("Background Music");
                bgm.AddComponent<AudioSource>();
                bgmAudio = bgm.GetComponent<AudioSource>();
                bgmAudio.GetComponent<AudioSource>().loop = (backgroundMusic.Count == 1); //loop if only 1 track is assigned
                bgmAudio.GetComponent<AudioSource>().spatialBlend = 0;
                int trackIndex = (playMode != PlayMode.Random) ? 0 : Random.Range(0, backgroundMusic.Count);
                PlayMusicTrack(trackIndex);
            }
        }

        //Music to the ears :)
        void PlayMusicTrack(int index)
        {
            bgmAudio.clip = backgroundMusic[index];
            bgmAudio.Play();
            lastIndex = index;
        }


        void Update()
        {

            if (bgmAudio)
            {
                //Switch track when finishes
                if (!bgmAudio.isPlaying)
                {
                    if (playMode == PlayMode.Random)
                    {
                        //Play a new random track
                        NewRandomTrack();
                    }
                    else
                    {
                        trackIndex++;
                        if (trackIndex >= backgroundMusic.Count) { trackIndex = 0; }
                        PlayMusicTrack(trackIndex);
                    }
                }

                //Handle music volume
                bgmAudio.volume = musicVolume;
            }
        }


        void NewRandomTrack()
        {
            int val = 0;

            Init:

            while (true)
            {
                val = Random.Range(0, backgroundMusic.Count);
                for (int i = 0; i < backgroundMusic.Count; i++)
                {
                    if (val == lastIndex) goto Init;
                }
                goto Done;
            }

            Done:

            PlayMusicTrack(val);
        }
        #endregion

        //Sets saved volume
        public void SetVolume()
        {
            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
            }
            else
            {
                //else set a deafult val of 1.0
                AudioListener.volume = 1.0f;
            }
        }
    }
}
