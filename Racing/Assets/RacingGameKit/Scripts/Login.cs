using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firesplash.UnityAssets.SocketIO;
using System;


public class Login : MonoBehaviour
{
    public GameObject m_WaitingLoginPanel;
    public GameObject m_LoginPanel;
    public GameObject m_RegistPanel;
    public GameObject m_MessageBox;
    public GameObject m_MenuPanel;
    public TMP_InputField m_UserName;
    public TMP_InputField m_Password;
    public Toggle m_KeepUser;
    public Button m_Login;
    public Button m_Create;

    public TMP_InputField m_UserNameReg;
    public TMP_InputField m_PasswordReg;
    public TMP_InputField m_ConfirmPassword;
    public TMP_InputField m_DiscordId;
    public TMP_InputField m_Wallet;
    public Button m_CreateAccount;

    public SocketIOCommunicator socket;

    [Serializable]
    struct LoginData
    {
        public string userid;
        public string userpassword;
    }

    struct LogoutData
    {
        public string userid;
    }

    struct LoginResultData
    {
        public string result;
        //public string level;
        //public string exp;
    }


    struct RegisterData
    {
        public string userid;
        public string password;
        public string discordid;
        public string wallet;
    }


    // Start is called before the first frame update
    void Start()
    {
        socket = SocketManager.Instance.GetSocketIOComponent();

        //GameManager.Singleton.m_isLogin = true;

        if (GameManager.Singleton.m_isLogin)
        {
            m_MenuPanel.SetActive(true);

            m_LoginPanel.SetActive(false);
        }
        else
        {
            socket.Instance.Connect();

            m_LoginPanel.SetActive(true);
        }

        if (PlayerPrefs.HasKey("Remember"))
        {
            if (PlayerPrefs.GetInt("Remember") == 1)
            {
                m_UserName.text = PlayerPrefs.GetString("username");
                m_Password.text = PlayerPrefs.GetString("password");
                m_KeepUser.isOn = true;
            }
            else
            {
                m_UserName.text = "";
                m_Password.text = "";
                m_KeepUser.isOn = false;
            }
        }

        socket.Instance.On("REQ_REGISTER_RESULT", OnGetRegisterResult);
        socket.Instance.On("REQ_LOGIN_RESULT", OnGetLoginResult);
    }

    private void OnDestroy()
    {
        socket.Instance.Off("REQ_REGISTER_RESULT", OnGetRegisterResult);
        socket.Instance.Off("REQ_LOGIN_RESULT", OnGetLoginResult);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_UserName.text == "" || m_Password.text == "")
            m_Login.interactable = false;
        else
            m_Login.interactable = true;

        if (m_UserNameReg.text == "" || m_PasswordReg.text == "" || m_ConfirmPassword.text == "" || m_DiscordId.text == "" || m_Wallet.text == "")
            m_Create.interactable = false;
        else
            m_Create.interactable = true;
    }

    public void onClickLogout()
    {
        m_MenuPanel.SetActive(false);

        //         Dictionary<string, string> data = new Dictionary<string, string>();
        //         data.Add("userid", m_UserName.text);
        //         JSONObject jdata = new JSONObject(data);
        //         socket.Emit("REQ_LOGOUT", jdata);
        LogoutData data = new LogoutData()
        {
            userid = GameManager.Singleton.m_UserName,
        };

        socket.Instance.Emit("REQ_LOGOUT", JsonUtility.ToJson(data), false);

        m_LoginPanel.SetActive(true);

        GameManager.Singleton.m_isLogin = false;
    }
    public void onClickLoginBtn()
    {
//         Dictionary<string, string> data = new Dictionary<string, string>();
//         data.Add("userid", m_UserName.text);
//         data.Add("userpassword", m_Password.text);
//         JSONObject jdata = new JSONObject(data);
//         socket.Emit("REQ_LOGIN", jdata);

        LoginData data = new LoginData()
        {
            userid = m_UserName.text,
            userpassword = m_Password.text
        };

        socket.Instance.Emit("REQ_LOGIN", JsonUtility.ToJson(data), false); //Please note the third parameter which is semi-required if JSON.Net is not installed

        LockWnd();

        Invoke("displayErr", 10.0f);

    }

    void displayErr()
    {
        if (GameManager.Singleton.m_isLogin == false)
        {
            UnLockWnd();
            m_MessageBox.SetActive(true);
            m_MessageBox.transform.Find("Content").GetComponent<TMP_Text>().text = "You can't connect server. Please try again.";
        }
    }

    public void onClickCreateBtn()
    {
        m_LoginPanel.SetActive(false);
        m_RegistPanel.SetActive(true);
    }

    public void onClickRegistBtn()
    {
        if (m_PasswordReg.text != m_ConfirmPassword.text)
        {
            m_MessageBox.SetActive(true);
            m_MessageBox.transform.Find("Content").GetComponent<TMP_Text>().text = "Please enter password correctly.";
            return;
        }

//         Dictionary<string, string> data = new Dictionary<string, string>();
//         data.Add("userid", m_UserNameReg.text);
//         data.Add("password", m_PasswordReg.text);
//         data.Add("discordid", m_DiscordId.text);
//         data.Add("wallet", m_Wallet.text);
//         JSONObject jdata = new JSONObject(data);
//         socket.Emit("REQ_REGISTER", jdata);

        RegisterData data = new RegisterData()
        {
            userid = m_UserNameReg.text,
            password = m_PasswordReg.text,
            discordid = m_DiscordId.text,
            wallet = m_Wallet.text
        };

        socket.Instance.Emit("REQ_REGISTER", JsonUtility.ToJson(data), false); 

        LockWnd();

    }

    public void onClickCancelRegist()
    {
        m_LoginPanel.SetActive(true);
        m_RegistPanel.SetActive(false);
    }

    private void OnGetRegisterResult(string evt)
    {
        Debug.Log("OnGetRegisterResult : " + evt);
        LoginResultData srv = JsonUtility.FromJson<LoginResultData>(evt);

        string result = srv.result;

        UnLockWnd();

        if (result == "success")
        {
            m_UserName.text = m_UserNameReg.text;
            m_Password.text = m_PasswordReg.text;

            m_MenuPanel.SetActive(true);
            m_LoginPanel.SetActive(false);
            m_RegistPanel.SetActive(false);

            GameManager.Singleton.m_isLogin = true;
            GameManager.Singleton.m_UserName = m_UserName.text;

            if (m_KeepUser.isOn)
            {
                PlayerPrefs.SetInt("Remember", 1);
                PlayerPrefs.SetString("username", m_UserName.text);
                PlayerPrefs.SetString("password", m_Password.text);
            }
            else
            {
                PlayerPrefs.SetInt("Remember", 0);
                PlayerPrefs.SetString("username", "");
                PlayerPrefs.SetString("password", "");
            }
        }
        else
        {
            m_MessageBox.SetActive(true);
            m_MessageBox.transform.Find("Content").GetComponent<TMP_Text>().text = "Same username is already exits!";
        }
    }

    private void OnGetLoginResult(string evt)
    {
        Debug.Log("OnGetLoginResult : " + evt);
        LoginResultData srv = JsonUtility.FromJson<LoginResultData>(evt);

        string result = srv.result;

        UnLockWnd();

        CancelInvoke("displayErr");

        if (result == "success")
        {
            m_MenuPanel.SetActive(true);
            m_LoginPanel.SetActive(false);

            GameManager.Singleton.m_isLogin = true;
            GameManager.Singleton.m_UserName = m_UserName.text;

            if (m_KeepUser.isOn)
            {
                PlayerPrefs.SetInt("Remember", 1);
                PlayerPrefs.SetString("username", m_UserName.text);
                PlayerPrefs.SetString("password", m_Password.text);
            }
            else
            {
                PlayerPrefs.SetInt("Remember", 0);
                PlayerPrefs.SetString("username", "");
                PlayerPrefs.SetString("password", "");
            }
        }
        else if (result == "ERR_LOGINED")
        {
            m_MessageBox.SetActive(true);
            m_MessageBox.transform.Find("Content").GetComponent<TMP_Text>().text = "This account is already logined.";
        }
        else
        {
            m_MessageBox.SetActive(true);
            m_MessageBox.transform.Find("Content").GetComponent<TMP_Text>().text = "Your username or password is incorrect.";
        }
    }

    public void onClickOKBtnMsg()
    {
        m_MessageBox.SetActive(false);
    }

    public void LockWnd()
    {
        m_WaitingLoginPanel.SetActive(true);    // waiting login
    }

    public void UnLockWnd(bool cancelwaiting = false)
    {
        m_WaitingLoginPanel.SetActive(false);
    }
}
