using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Firesplash.UnityAssets.SocketIO;
using System;

//The MenuManager handles all menu activity. Feel free to extend it, learn from it or even use it in your own menu.
//Please note that the menu manager was intended for demo purposes.

namespace RGSK
{
    public class MenuManager : MonoBehaviour
    {
        [System.Serializable]
        public class MenuVehicle
        {
            [Header("Details")]
            public string name;
            public string resourceName; // Make sure this string matches the coresponding vehicle name in a Reources/PlayerVehicles folder!
            public Transform vehicle;
            public int price;
            public bool unlocked;
            public Material vehicleBody;
            public Material VehicleRims;

            [Header("Specs")]
            [Range(0, 1)]
            public float speed;
            [Range(0, 1)]
            public float acceleration;
            [Range(0, 1)]
            public float handling;
            [Range(0, 1)]
            public float braking;
        }

        [System.Serializable]
        public class MenuTrack
        {
            public string name;
            public string trackLength;
            public string sceneName;
            public Sprite image;
            public RaceManager.RaceType raceType = RaceManager.RaceType.Circuit;
            public OpponentControl.AiDifficulty aiDifficulty = OpponentControl.AiDifficulty.Meduim;
            public int laps = 3;
            public int aiCount = 4;
            public int price;
            public bool unlocked;
        }

        #region Customization Classes
        [System.Serializable]
        public class CustomizeItem
        {
            public string name;
            public int ID;
            public int price;
            public Text priceText;
            [HideInInspector]public bool unlocked;
        }

        [System.Serializable]
        public class VisualUpgrade : CustomizeItem
        {
            public BodyColorAndRims[] visualUpgrade;
        }

        [System.Serializable]
        public class BodyColorAndRims 
        {
            public string vehicle_name;
            public Texture texture;
        }


        [System.Serializable]
        public class VehicleUpgrade
        {
            public string vehicle_name;
            [Space(10)]
            [Range(0, 1)]
            public float speed;
            [Range(0, 1)]
            public float acceleration;
            [Range(0, 1)]
            public float handling;
            [Range(0, 1)]
            public float braking;
        }
        #endregion
        class RankInfo
        {
            public string userId;
            public string wallet;
            public int exp;
        }

        Dictionary<int, RankInfo> m_RankInfos = new Dictionary<int, RankInfo>();

        public Text[] m_RankNumbers;
        public Text[] m_RankNames;
        public Text[] m_RankWallet;
        public Text[] m_RankScores;
        public enum State { Login, Main, VehicleSelect, TrackSelect, Customize, Settings, Ranking, Loading }
        public State state;
        public SocketIOCommunicator socket;

        [Header("Vehicle settings")]
        public MenuVehicle[] menuVehicles;

        [Header("Track Settings")]
        public MenuTrack[] menuTracks;

        [Header("Customize Settings")]
        public VisualUpgrade[] bodyColors;
        public VisualUpgrade[] rims;

        [Header("Panels")]
        public GameObject loginPanel;
        public GameObject mainPanel;
        public GameObject vehicleSelectPanel;
        public GameObject trackSelectPanel;
        public GameObject customizePanel, vehicleStats;
        public GameObject settingsPanel;
        public GameObject promptPanel;
        public GameObject rankingPanel;
        public GameObject loadingPanel;

        [Header("Top Panel UI")]
        public Text playerCurrency;
        public Text menuState;

        [Header("Vehicle Select UI")]
        public Text vehicleName;
        public Button selectVehicleButton, buyVehicleButton, customizeButton;
        public Image speed, accel, handling, braking;

        [Header("Track Select UI")]
        public Image trackImage;
        public Text trackName, raceType, lapCount, aiCount, aiDifficulty, bestTime, trackLength;
        public Button raceButton, buyTrackButton;

        [Header("Customization UI")]
        public Button apply;
        public Button buy;
        public GameObject colorsPanel;
        private int incartCr, bodyColPrice, rimPrice, upgradePrice, selectedColorID, selectedRimID, selectedUpgradeID;

        [Header("Settings UI")]
        public InputField playerName;
        public Slider masterVolume;
        public Dropdown graphicLevel;
        public Toggle mobileTouchSteer, mobileTiltSteer, mobileAutoAcceleration;
        public bool applyExpensiveGraphicChanges = false;

        [Header("Loading UI")]
        public Image loadingBar;

        [Header("Prompt Panel UI")]
        public Text promptTitle;
        public Text promptText;
        public Button accept, cancel;

        [Header("Misc UI")]
        public Text itemPrice;
        public Image locked;
        public Image cart;
        public Button nextArrow, prevArrow;

        [Header("Extra Settings")]
        public bool autoRotateVehicles = true;
        public bool rotateVehicleByDrag = true;
        public float rotateSpeed = 5.0f;
        public int maxOpponents = 5;
        [Range(1,7)]public int raceTypes = 7;
        
        //Private vars
        private int vehicleIndex;
        private int prevVehicleIndex;
        private int trackIndex;
        private int raceTypeIndex = 1;
        private int aiDiffIndex = 1;
        private AsyncOperation async;
        private State previousState;
        private bool raycastTarget;
        private bool _autoRotate; // cache
        private float rotateDir = 1;
        private Texture lastColTex;
        private Texture lastRimTex;


        struct UpgradeResult
        {
            public string result;
            public int exp;
        }

        class TopPlayers
        {
            public TopPlayerInfo[] players;
        }

        [Serializable]
        class TopPlayerInfo
        {
            public string userid;
            public string wallet;
            public int exp;
        }


        void Awake()
        {
            LoadValues();
        }

        void Start()
        {
            socket = SocketManager.Instance.GetSocketIOComponent();
            state = State.Login;

            CycleVehicles();

            if (masterVolume) masterVolume.onValueChanged.AddListener(delegate { SetMasterVolFromSlider(); });
            if (playerName) playerName.onEndEdit.AddListener(delegate { SetPlayerNameFromInputField(); });
            if (graphicLevel) graphicLevel.onValueChanged.AddListener(delegate { GetGrahicLevelFromDropdown(); });
            if (graphicLevel) graphicLevel.value = QualitySettings.GetQualityLevel();
            if(mobileAutoAcceleration) mobileAutoAcceleration.onValueChanged.AddListener(delegate { ToggleAutoAccel(); });
            _autoRotate = autoRotateVehicles;
            selectedColorID = -1;
            selectedRimID = -1;
            selectedUpgradeID = -1;

            socket.Instance.On("REQ_UPGRADEEXP_RESULT", OnUpgradeExpResult);
            socket.Instance.On("REQ_TOPPLAYERLIST_RESULT", OnTopListResult);

        }

        private void OnDestroy()
        {
            socket.Instance.Off("REQ_UPGRADEEXP_RESULT", OnUpgradeExpResult);
            socket.Instance.Off("REQ_TOPPLAYERLIST_RESULT", OnTopListResult);
        }

        private void OnUpgradeExpResult(string evt)
        {
            Debug.Log("OnUpgradeExpResult : " + evt);

            UpgradeResult srv = JsonUtility.FromJson<UpgradeResult>(evt);

            string result = srv.result;


            if (result == "success")
            {
                GameManager.Singleton.m_Exp = srv.exp;
            }
            else
            {
            }
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }

        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>("{\"Items\":" + json + "}");
            return wrapper.Items;
        }

        private void OnTopListResult(string evt)
        {
            Debug.Log("OnTopListResult : " + evt);

            TopPlayerInfo[] srv = FromJson<TopPlayerInfo>(evt);

            m_RankInfos.Clear();

            for (int i = 0; i < srv.Length; i++)
            {
                if (srv[i].exp > 0)
                {
                    RankInfo info = new RankInfo();
                    info.userId = srv[i].userid;
                    info.wallet = srv[i].wallet;
                    info.exp = srv[i].exp;
                    m_RankInfos.Add(i, info);
                }
            }

            ShowRanking();
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape)) Back();

            RotateVehicle();

            LerpStats();
        }


        void CycleVehicles()
        {
            // Cycle between vehicles based on the "vehicleIndex" value
            for (int i = 0; i < menuVehicles.Length; i++)
            {
                if (vehicleIndex == i)
                {
                    menuVehicles[i].vehicle.rotation = menuVehicles[prevVehicleIndex].vehicle.rotation;
                    menuVehicles[i].vehicle.gameObject.SetActive(true);
                    UpdateUI();
                }
                else
                {
                    menuVehicles[i].vehicle.gameObject.SetActive(false);
                }
            }
        }

        void CycleTracks()
        {
            // Cycle between tracks based on the "trackIndex" value
            UpdateUI();
        }

        void UpdateUI()
        {

            if (playerCurrency) playerCurrency.text = PlayerData.currency.ToString("N0") + " Cr";

            if (cart) cart.enabled = state == State.Customize;

            if (nextArrow) nextArrow.gameObject.SetActive(state == State.VehicleSelect || state == State.TrackSelect);

            if (prevArrow) prevArrow.gameObject.SetActive(state == State.VehicleSelect || state == State.TrackSelect);

            if (vehicleStats) vehicleStats.SetActive(state == State.VehicleSelect || state == State.Customize);

            switch (state)
            {
                case State.Login:
                    loginPanel.SetActive(true);
                    mainPanel.SetActive(false);
                    vehicleSelectPanel.SetActive(false);
                    trackSelectPanel.SetActive(false);
                    customizePanel.SetActive(false);
                    settingsPanel.SetActive(false);
                    loadingPanel.SetActive(false);
                    rankingPanel.SetActive(false);

                    break;
                case State.Main:
                    mainPanel.SetActive(true);
                    vehicleSelectPanel.SetActive(false);
                    trackSelectPanel.SetActive(false);
                    customizePanel.SetActive(false);
                    settingsPanel.SetActive(false);
                    loadingPanel.SetActive(false);
                    rankingPanel.SetActive(false);

                    if (itemPrice) itemPrice.text = string.Empty;

                    if (locked) locked.enabled = false;

                    break;

                case State.VehicleSelect:
                    mainPanel.SetActive(false);
                    vehicleSelectPanel.SetActive(true);
                    trackSelectPanel.SetActive(false);
                    customizePanel.SetActive(false);
                    settingsPanel.SetActive(false);
                    loadingPanel.SetActive(false);
                    rankingPanel.SetActive(false);

                    if (vehicleName) vehicleName.text = menuVehicles[vehicleIndex].name;

                    if (itemPrice) itemPrice.text = menuVehicles[vehicleIndex].unlocked ? string.Empty : menuVehicles[vehicleIndex].price.ToString("N0") + " Cr";

                    if (locked) locked.enabled = !menuVehicles[vehicleIndex].unlocked;

                    if (selectVehicleButton) selectVehicleButton.gameObject.SetActive(menuVehicles[vehicleIndex].unlocked);

                    if (buyVehicleButton) buyVehicleButton.gameObject.SetActive(!menuVehicles[vehicleIndex].unlocked);

                    //if (customizeButton) customizeButton.gameObject.SetActive(menuVehicles[vehicleIndex].unlocked);

                    if (menuState) menuState.text = "VEHICLE SELECT";

                    break;

                case State.TrackSelect:
                    mainPanel.SetActive(false);
                    vehicleSelectPanel.SetActive(false);
                    trackSelectPanel.SetActive(true);
                    customizePanel.SetActive(false);
                    settingsPanel.SetActive(false);
                    loadingPanel.SetActive(false);
                    rankingPanel.SetActive(false);

                    if (trackName) trackName.text = menuTracks[trackIndex].name;

                    if (trackLength) trackLength.text = menuTracks[trackIndex].trackLength;

                    if (trackImage && menuTracks[trackIndex].image) trackImage.sprite = menuTracks[trackIndex].image;

                    if (raceType) raceType.text = menuTracks[trackIndex].raceType.ToString();

                    if (lapCount) lapCount.text = menuTracks[trackIndex].laps.ToString();

                    if (aiCount) aiCount.text = menuTracks[trackIndex].aiCount.ToString();

                    if (aiDifficulty) aiDifficulty.text = menuTracks[trackIndex].aiDifficulty.ToString();

                    if (itemPrice) itemPrice.text = menuTracks[trackIndex].unlocked ? string.Empty : menuTracks[trackIndex].price.ToString("N0") + " Cr";

                    if (locked) locked.enabled = !menuTracks[trackIndex].unlocked;

                    if (raceButton) raceButton.gameObject.SetActive(menuTracks[trackIndex].unlocked);

                    if (buyTrackButton) buyTrackButton.gameObject.SetActive(!menuTracks[trackIndex].unlocked);

                    if (bestTime) bestTime.text = (PlayerPrefs.HasKey("BestTime" + menuTracks[trackIndex].sceneName)) ? PlayerPrefs.GetString("BestTime" + menuTracks[trackIndex].sceneName) : "--:--:--";

                    if (menuState) menuState.text = "TRACK SELECT";

                    break;

                case State.Customize:
                    mainPanel.SetActive(false);
                    vehicleSelectPanel.SetActive(false);
                    trackSelectPanel.SetActive(false);
                    customizePanel.SetActive(true);
                    settingsPanel.SetActive(false);
                    loadingPanel.SetActive(false);
                    rankingPanel.SetActive(false);

                    //Calculate the in cart currency
                    incartCr = bodyColPrice + rimPrice + upgradePrice;

                    //Fill in the price texts (BODY COLORS)
                    for (int c = 0; c < bodyColors.Length; c++)
                    {
                        if (bodyColors[c].priceText) bodyColors[c].priceText.text = !bodyColors[c].unlocked ? bodyColors[c].price.ToString("N0") : "Owned";
                    }

                    //Fill in the price texts (RIMS)
                    for (int r = 0; r < rims.Length; r++)
                    {
                        if (rims[r].priceText) rims[r].priceText.text = !rims[r].unlocked ? rims[r].price.ToString("N0") : "Owned";
                    }

                    if (colorsPanel) colorsPanel.SetActive(true);

                    if (apply) apply.gameObject.SetActive(incartCr <= 0 && selectedColorID >= 0 || incartCr <= 0 &&  selectedRimID >= 0 || incartCr <= 0 && selectedUpgradeID >= 0);

                    if (buy) buy.gameObject.SetActive(incartCr > 0);

                    if (itemPrice) itemPrice.text = incartCr.ToString("N0") + " Cr";

                    if (menuState) menuState.text = "CUSTOMIZE";

                    break;

                case State.Settings:
                    mainPanel.SetActive(false);
                    vehicleSelectPanel.SetActive(false);
                    trackSelectPanel.SetActive(false);
                    customizePanel.SetActive(false);
                    settingsPanel.SetActive(true);
                    rankingPanel.SetActive(false); 
                    loadingPanel.SetActive(false);

                    if (menuState) menuState.text = "SETTINGS";

                    break;

                case State.Ranking:

                    socket.Instance.Emit("REQ_TOPPLAYERLIST");
                    //SocketManager.Instance.GetSocketIOComponent().Emit("REQ_TOPPLAYERLIST");

                    mainPanel.SetActive(false);
                    rankingPanel.SetActive(true);

                    if (menuState) menuState.text = "RANKING";

                    break;

                case State.Loading:
                    mainPanel.SetActive(false);
                    vehicleSelectPanel.SetActive(false);
                    trackSelectPanel.SetActive(false);
                    customizePanel.SetActive(false);
                    settingsPanel.SetActive(false);
                    loadingPanel.SetActive(true);
                    rankingPanel.SetActive(false);

                    break;
            }
        }

        void ShowRanking()
        {
            for (int i = 0; i < 10; i++)
            {
                m_RankNumbers[i].text = "";
                m_RankNames[i].text = "";
                m_RankWallet[i].text = "";
                m_RankScores[i].text = "";
            }

            for (int i = 0; i < m_RankInfos.Count; i++)
            {
                m_RankNumbers[i].text = (i + 1).ToString();
                m_RankNames[i].text = m_RankInfos[i].userId;
                m_RankWallet[i].text = m_RankInfos[i].wallet;
                m_RankScores[i].text = m_RankInfos[i].exp.ToString();
            }
        }

        /// <summary>
        /// Lerps the stat values to suit the selected vehicle
        /// </summary>
        private void LerpStats()
        {
            //Normal Stats
            if (speed) speed.fillAmount = Mathf.Lerp(speed.fillAmount, menuVehicles[vehicleIndex].speed, Time.deltaTime * 3.0f);

            if (accel) accel.fillAmount = Mathf.Lerp(accel.fillAmount, menuVehicles[vehicleIndex].acceleration, Time.deltaTime * 3.0f);

            if (handling) handling.fillAmount = Mathf.Lerp(handling.fillAmount, menuVehicles[vehicleIndex].handling, Time.deltaTime * 3.0f);

            if (braking) braking.fillAmount = Mathf.Lerp(braking.fillAmount, menuVehicles[vehicleIndex].braking, Time.deltaTime * 3.0f);
        }


        private void RotateVehicle()
        {
            if (autoRotateVehicles) menuVehicles[vehicleIndex].vehicle.Rotate(0, (rotateSpeed * Time.deltaTime) * rotateDir, 0);


            //Rotate by drag raycast check
            if (rotateVehicleByDrag)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        Collider[] childTransforms = menuVehicles[vehicleIndex].vehicle.GetComponentsInChildren<Collider>();

                        foreach (Collider t in childTransforms)
                        {
                            if (hit.collider == t)
                            {
                                autoRotateVehicles = false;
                                raycastTarget = true;
                            }
                            else
                            {
                                raycastTarget = false;
                                if (_autoRotate) autoRotateVehicles = true;
                            }
                        }
                    }
                }

                if (Input.GetButtonUp("Fire1"))
                {
                    Vector3 mPos = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

                    if (raycastTarget) rotateDir = (mPos.x < 0.5f) ? 1 : -1;

                    if (_autoRotate) autoRotateVehicles = true;

                    raycastTarget = false;
                }

                if (!raycastTarget) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

                menuVehicles[vehicleIndex].vehicle.Rotate(0, -Input.GetAxis("Mouse X"), 0);

#else
         if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
         {
             Vector2 fingerPos = Input.GetTouch(0).deltaPosition;
        
             menuVehicles[vehicleIndex].vehicle.Rotate(0, -fingerPos.x, 0);
         }
#endif
            }
        }

        private void ApplyColorCustomization(int bodyCol, int tovehicleIndex)
        {

            bodyColors[bodyCol].unlocked = true;

            //Unclock the color
            if (!PlayerPrefs.HasKey("BodyColor" + bodyColors[bodyCol].ID + menuVehicles[tovehicleIndex].name))
                PlayerPrefs.SetInt("BodyColor" + bodyColors[bodyCol].ID + menuVehicles[tovehicleIndex].name, 1);

            //Save as the vehicle's current color
            PlayerPrefs.SetInt("CurrentBodyColor" + menuVehicles[tovehicleIndex].name, bodyCol);

            try { menuVehicles[tovehicleIndex].vehicleBody.mainTexture = bodyColors[bodyCol].visualUpgrade[tovehicleIndex].texture; }
            catch { Debug.LogError("You haven't properly configured color customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + bodyCol + "] of this customization exists or isn't null."); }

            lastColTex = null;
        }

        private void ApplyRimCustomization(int rimIndex, int tovehicleIndex)
        {

            rims[rimIndex].unlocked = true;

            if (!PlayerPrefs.HasKey("VehicleRim" + rims[rimIndex].ID + menuVehicles[vehicleIndex].name))
                PlayerPrefs.SetInt("VehicleRim" + rims[rimIndex].ID + menuVehicles[vehicleIndex].name, 1);

            //Save as the vehicle's current rim
            PlayerPrefs.SetInt("CurrentRim" + menuVehicles[tovehicleIndex].name, rimIndex);

            try { menuVehicles[tovehicleIndex].VehicleRims.mainTexture = rims[rimIndex].visualUpgrade[tovehicleIndex].texture; }
            catch { Debug.LogError("You haven't properly configured rim customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + rimIndex + "] of this customization exists or isn't null."); }


            lastRimTex = null;
        }

        /// <summary>
        /// Loads important values such as currency & preferences
        /// </summary>
        private void LoadValues()
        {
            PlayerData.LoadCurrency();

            //Last selected vehicle
            if (PlayerPrefs.HasKey("SelectedVehicle")) vehicleIndex = PlayerPrefs.GetInt("SelectedVehicle");

            //Master Vol
            if (masterVolume) masterVolume.value = (PlayerPrefs.HasKey("MasterVolume")) ? PlayerPrefs.GetFloat("MasterVolume") : 1;

            //Graphic Level
            if (PlayerPrefs.HasKey("GraphicLevel")) SetGraphicsQuality(PlayerPrefs.GetInt("GraphicLevel"));

            //Player Name
            if (PlayerPrefs.HasKey("PlayerName")) { if (playerName) playerName.text = PlayerPrefs.GetString("PlayerName"); }

            //Toggles
            if (mobileAutoAcceleration) mobileAutoAcceleration.isOn = PlayerPrefs.GetString("AutoAcceleration") == "True";
            if (mobileTouchSteer) mobileTouchSteer.isOn = PlayerPrefs.GetString("MobileControlType") == "Touch";
            if (mobileTiltSteer) mobileTiltSteer.isOn = PlayerPrefs.GetString("MobileControlType") == "Tilt";

            //Other important stuff
            CheckForUnlockedVehiclesAndTracks();
            LoadCustomizations();

        }

        private void LoadCustomizations()
        {
            for (int i = 0; i < menuVehicles.Length; i++)
            {
                if (PlayerPrefs.HasKey("CurrentBodyColor" + menuVehicles[i].name))
                {
                    ApplyColorCustomization(PlayerPrefs.GetInt("CurrentBodyColor" + menuVehicles[i].name), i);
                }

                if (PlayerPrefs.HasKey("CurrentRim" + menuVehicles[i].name))
                {
                    ApplyRimCustomization(PlayerPrefs.GetInt("CurrentRim" + menuVehicles[i].name), i);
                }
            }
        }

        private void CheckForUnlockedVehiclesAndTracks()
        {

            //Check for unlokced vehicles
            for (int i = 0; i < menuVehicles.Length; i++)
            {
                //First check if the vehicle is pre-unlocked
                if (menuVehicles[i].unlocked)
                {
                    PlayerPrefs.SetInt(menuVehicles[i].name, 1);
                }

                if (PlayerPrefs.GetInt(menuVehicles[i].name) == 1)
                {
                    menuVehicles[i].unlocked = true;
                }
                else
                {
                    menuVehicles[i].unlocked = false;
                }
            }

            //Check for unlokced tracks
            for (int i = 0; i < menuTracks.Length; i++)
            {
                //First check if the track is pre-unlocked
                if (menuTracks[i].unlocked)
                {
                    PlayerPrefs.SetInt(menuTracks[i].name, 1);
                }

                if (PlayerPrefs.GetInt(menuTracks[i].name) == 1)
                {
                    menuTracks[i].unlocked = true;
                }
                else
                {
                    menuTracks[i].unlocked = false;
                }
            }
        }

        private void CheckForUnlockedCustomizations()
        {
            for (int i = 0; i < bodyColors.Length; i++)
            {
                if (PlayerPrefs.GetInt("BodyColor" + bodyColors[i].ID + menuVehicles[vehicleIndex].name) == 1)
                {
                    bodyColors[i].unlocked = true;
                }
                else
                {
                    bodyColors[i].unlocked = false;
                }
            }

            for (int i = 0; i < rims.Length; i++)
            {
                if (PlayerPrefs.GetInt("VehicleRim" + rims[i].ID + menuVehicles[vehicleIndex].name) == 1)
                {
                    rims[i].unlocked = true;
                }
                else
                {
                    rims[i].unlocked = false;
                }
            }
        }

        private void RevertCustomizationChanges()
        {
            if (lastColTex && menuVehicles[vehicleIndex].vehicleBody) menuVehicles[vehicleIndex].vehicleBody.mainTexture = lastColTex;
            if (lastRimTex && menuVehicles[vehicleIndex].VehicleRims) menuVehicles[vehicleIndex].VehicleRims.mainTexture = lastRimTex;

            incartCr = 0;
            bodyColPrice = 0;
            rimPrice = 0;
            upgradePrice = 0;
            selectedColorID = -1;
            selectedRimID = -1;
            selectedUpgradeID = -1;
            lastColTex = null;
            lastRimTex = null;

            //for (int i = 0; i < bodyColors.Length; i++)
            //{
            //    bodyColors[i].unlocked = false;
            //}
        }

        private void CreatePromptPanel(string title, string prompt)
        {

            if (promptTitle) promptTitle.text = title;

            if (promptText) promptText.text = prompt;

            if (promptPanel) promptPanel.SetActive(true);
        }

        #region Button Functions
        public void NextArrow()
        {
            ButtonSFX();

            if (state == State.VehicleSelect)
            {
                if (vehicleIndex < menuVehicles.Length - 1)
                {
                    prevVehicleIndex = vehicleIndex;
                    vehicleIndex++;
                }
                else
                {
                    prevVehicleIndex = vehicleIndex;
                    vehicleIndex = 0;
                }

                CycleVehicles();
            }

            if (state == State.TrackSelect)
            {
                if (trackIndex < menuTracks.Length - 1)
                {
                    trackIndex++;
                }
                else
                {
                    trackIndex = 0;
                }

                CycleTracks();
            }
        }

        public void PreviousArrow()
        {
            ButtonSFX();

            if (state == State.VehicleSelect)
            {
                if (vehicleIndex > 0)
                {
                    prevVehicleIndex = vehicleIndex;
                    vehicleIndex--;
                }
                else
                {
                    prevVehicleIndex = vehicleIndex;
                    vehicleIndex = menuVehicles.Length - 1;
                }

                CycleVehicles();
            }

            if (state == State.TrackSelect)
            {
                if (trackIndex > 0)
                {
                    trackIndex--;
                }
                else
                {
                    trackIndex = menuTracks.Length - 1;
                }

                CycleTracks();
            }
        }

        public void Play()
        {
            state = State.Loading;

            UpdateUI();

            //Save all preferences
            PlayerPrefs.SetString("PlayerVehicle", menuVehicles[vehicleIndex].resourceName);
            PlayerPrefs.SetString("RaceType", menuTracks[trackIndex].raceType.ToString());
            PlayerPrefs.SetString("AiDifficulty", menuTracks[trackIndex].aiDifficulty.ToString());
            PlayerPrefs.SetInt("Opponents", menuTracks[trackIndex].aiCount);
            PlayerPrefs.SetInt("Laps", menuTracks[trackIndex].laps);

            PlayerPrefs.SetString("RaceName", menuTracks[trackIndex].name);

            StartCoroutine(LoadScene());
        }


        public void Buy()
        {
            ButtonSFX();

            //BUY VEHILCE
            if (state == State.VehicleSelect)
            {
                if (PlayerData.currency >= menuVehicles[vehicleIndex].price)
                {
                    if (accept)
                    {
                        accept.onClick.RemoveAllListeners();
                        accept.onClick.AddListener(() => AcceptPrompt());
                    }

                    if (cancel)
                    {
                        cancel.gameObject.SetActive(true);
                        cancel.onClick.RemoveAllListeners();
                        cancel.onClick.AddListener(() => ClosePromptPanel());
                    }

                    CreatePromptPanel("CONFIRM ACTION", "Do you really want to purchase this vehicle?");
                }
                else
                {
                    if (accept)
                    {
                        accept.onClick.RemoveAllListeners();
                        accept.onClick.AddListener(() => ClosePromptPanel());
                    }

                    if (cancel) cancel.gameObject.SetActive(false);

                    CreatePromptPanel("NOT ENOUGH CURRENCY", "You do not have enough currency to buy this vehicle");
                }
            }

            //BUY TRACK
            if (state == State.TrackSelect)
            {
                if (PlayerData.currency >= menuTracks[trackIndex].price)
                {
                    if (accept)
                    {
                        accept.onClick.RemoveAllListeners();
                        accept.onClick.AddListener(() => AcceptPrompt());
                    }

                    if (cancel)
                    {
                        cancel.gameObject.SetActive(true);
                        cancel.onClick.RemoveAllListeners();
                        cancel.onClick.AddListener(() => ClosePromptPanel());
                    }

                    CreatePromptPanel("CONFIRM ACTION", "Do you really want to purchase this track?");
                }
                else
                {
                    if (accept)
                    {
                        accept.onClick.RemoveAllListeners();
                        accept.onClick.AddListener(() => ClosePromptPanel());
                    }

                    if (cancel) cancel.gameObject.SetActive(false);

                    CreatePromptPanel("NOT ENOUGH CURRENCY", "You do not have enough currency to buy this track");
                }
            }


            //BUY CUSTOMIZATION
            if (state == State.Customize)
            {
                if (PlayerData.currency >= incartCr)
                {
                    if (accept)
                    {
                        accept.onClick.RemoveAllListeners();
                        accept.onClick.AddListener(() => AcceptPrompt());
                    }

                    if (cancel)
                    {
                        cancel.gameObject.SetActive(true);
                        cancel.onClick.RemoveAllListeners();
                        cancel.onClick.AddListener(() => ClosePromptPanel());
                    }

                    CreatePromptPanel("CONFIRM ACTION", "Do you really want to make this purchase?");
                }
                else
                {
                    if (accept)
                    {
                        accept.onClick.RemoveAllListeners();
                        accept.onClick.AddListener(() => ClosePromptPanel());
                    }

                    if (cancel) cancel.gameObject.SetActive(false);

                    CreatePromptPanel("NOT ENOUGH CURRENCY", "You do not have enough currency to make this purchase");
                }
            }
        }

        public void VehicleSelect()
        {
            ButtonSFX();

            state = State.VehicleSelect;

            UpdateUI();
        }

        public void TrackSelect()
        {
            ButtonSFX();

            state = State.TrackSelect;

            UpdateUI();
        }

        public void Customize()
        {
            ButtonSFX();

            state = State.Customize;

            CheckForUnlockedCustomizations();

            UpdateUI();
        }

        public void Settings()
        {
            ButtonSFX();

            if (state != State.Settings) previousState = state;

            state = state != State.Settings ? State.Settings : previousState;

            UpdateUI();
        }

        public void Ranking()
        {
            ButtonSFX(); 

            if (state != State.Ranking) previousState = state;

            state = state != State.Ranking ? State.Ranking : previousState;

            UpdateUI();
        }


        public void SetGraphicsQuality(int level)
        {
            QualitySettings.SetQualityLevel(level, applyExpensiveGraphicChanges);

            PlayerPrefs.SetInt("GraphicLevel", level);
        }

        private void GetGrahicLevelFromDropdown()
        {
            SetGraphicsQuality(graphicLevel.value);
        }

        private void SetMasterVolFromSlider()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);

            if (SoundManager.instance)
                SoundManager.instance.SetVolume();
        }

        private void SetPlayerNameFromInputField()
        {
            PlayerPrefs.SetString("PlayerName", playerName.text);
        }

        public void ToggleTouchControl(bool b)
        {
            mobileTouchSteer.isOn = true;

            mobileTiltSteer.isOn = false;

            PlayerPrefs.SetString("MobileControlType", "Touch");
        }

        public void ToggleTiltControl(bool b)
        {
            mobileTouchSteer.isOn = false;

            mobileTiltSteer.isOn = true;

            PlayerPrefs.SetString("MobileControlType", "Tilt");
        }

        public void ToggleAutoAccel()
        {
            string isOn = mobileAutoAcceleration.isOn ? "True" : "False";
            PlayerPrefs.SetString("AutoAcceleration", isOn);
        }

        public void ChooseVehicle()
        {
            PlayerPrefs.SetInt("SelectedVehicle", vehicleIndex);
            Back();
        }

        public void AdjustRaceType(int val)
        {
            raceTypeIndex += val;
            raceTypeIndex = Mathf.Clamp(raceTypeIndex, 1, raceTypes);

            switch (raceTypeIndex)
            {

                case 1:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.Circuit;
                    break;

                case 2:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.LapKnockout;
                    break;

                case 3:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.TimeTrial;
                    break;

                case 4:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.SpeedTrap;
                    break;

                case 5:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.Checkpoints;
                    break;

                case 6:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.Elimination;
                    break;

                case 7:
                    menuTracks[trackIndex].raceType = RaceManager.RaceType.Drift;
                    break;
            }

            UpdateUI();
        }

        public void AdjustLaps(int val)
        {
            menuTracks[trackIndex].laps += val;
            menuTracks[trackIndex].laps = Mathf.Clamp(menuTracks[trackIndex].laps, 1, 1000);

            UpdateUI();
        }

        public void AdjustAiCount(int val)
        {
            menuTracks[trackIndex].aiCount += val;
            menuTracks[trackIndex].aiCount = Mathf.Clamp(menuTracks[trackIndex].aiCount, 0, maxOpponents);

            UpdateUI();
        }

        public void AdjustAiDifficulty(int val)
        {

            aiDiffIndex += val;
            aiDiffIndex = Mathf.Clamp(aiDiffIndex, 1, 4);

            switch (aiDiffIndex)
            {

                case 1:
                    menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Custom;
                    break;

                case 2:
                    menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Easy;
                    break;

                case 3:
                    menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Meduim;
                    break;

                case 4:
                    menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Hard;
                    break;
            }

            UpdateUI();
        }

        public void SelectColor(int c)
        {
            ButtonSFX();

            if (!menuVehicles[vehicleIndex].vehicleBody) return;

            if (!lastColTex) lastColTex = menuVehicles[vehicleIndex].vehicleBody.mainTexture;

            for (int i = 0; i < bodyColors.Length; i++)
            {
                if (c == bodyColors[i].ID)
                {
                    selectedColorID = i;
                    bodyColPrice = !bodyColors[selectedColorID].unlocked ? bodyColors[selectedColorID].price : 0;

                    try { menuVehicles[vehicleIndex].vehicleBody.mainTexture = bodyColors[i].visualUpgrade[vehicleIndex].texture; }
                    catch { Debug.Log("You haven't properly configured color customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + i + "] of this customization exists or isn't null."); }
                }
            }

            UpdateUI();
        }

        public void SelectRim(int r)
        {
            ButtonSFX();

            if (!menuVehicles[vehicleIndex].VehicleRims) return;

            if (!lastRimTex) lastRimTex = menuVehicles[vehicleIndex].VehicleRims.mainTexture;

            for (int i = 0; i < rims.Length; i++)
            {
                if (r == rims[i].ID)
                {
                    selectedRimID = i;
                    rimPrice = !rims[selectedRimID].unlocked ? rims[selectedRimID].price : 0;

                    try { menuVehicles[vehicleIndex].VehicleRims.mainTexture = rims[i].visualUpgrade[vehicleIndex].texture; }
                    catch { Debug.Log("You haven't properly configured rim customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + i + "] of this customization exists or isn't null."); }
                }
            }

            UpdateUI();
        }

        public void ApplyCustomizationChanges()
        {
            if (selectedColorID >= 0) ApplyColorCustomization(selectedColorID, vehicleIndex);

            if (selectedRimID >= 0) ApplyRimCustomization(selectedRimID, vehicleIndex);

            Back();
        }

        public void AcceptPrompt()
        {
            switch (state)
            {
                case State.VehicleSelect:
                    PlayerData.DeductCurrency(menuVehicles[vehicleIndex].price);

                    menuVehicles[vehicleIndex].unlocked = true;
                    PlayerPrefs.SetInt(menuVehicles[vehicleIndex].name, 1);
                    break;

                case State.TrackSelect:
                    PlayerData.DeductCurrency(menuTracks[trackIndex].price);

                    menuTracks[trackIndex].unlocked = true;
                    PlayerPrefs.SetInt(menuTracks[trackIndex].name, 1);
                    break;

                case State.Customize:
                    PlayerData.DeductCurrency(incartCr);

                    if (selectedColorID >= 0) ApplyColorCustomization(selectedColorID, vehicleIndex);

                    if (selectedRimID >= 0) ApplyRimCustomization(selectedRimID, vehicleIndex);

                    Back();
                    break;
            }

            UpdateUI();
            ClosePromptPanel();
        }

        public void ClosePromptPanel()
        {
            if (promptPanel) promptPanel.SetActive(false);

            RevertCustomizationChanges();

            UpdateUI();
        }

        public void Back()
        {
            ButtonSFX();

            switch (state)
            {
                case State.Main:
                    Application.Quit();
                    break;

                case State.VehicleSelect:
                    state = State.Main;
                    vehicleIndex = PlayerPrefs.GetInt("SelectedVehicle");
                    CycleVehicles();
                    break;

                case State.TrackSelect:
                    state = State.Main;
                    break;

                case State.Customize:
                    RevertCustomizationChanges();
                    state = State.VehicleSelect;
                    break;

                case State.Settings:
                    state = previousState;
                    break;

                case State.Ranking:
                    state = State.Main;
                    break;
            }

            UpdateUI();
        }
        #endregion

        IEnumerator LoadScene()
        {
            async = SceneManager.LoadSceneAsync(menuTracks[trackIndex].sceneName);

            while (!async.isDone)
            {
                if (loadingBar) loadingBar.fillAmount = async.progress;

                yield return null;
            }
        }

        void ButtonSFX()
        {
            if (SoundManager.instance) SoundManager.instance.PlaySound("Button", true);
        }
        
    }
}
