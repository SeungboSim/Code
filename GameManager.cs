using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager Instance = null;
    public static GameManager instance
    {
        get
        {
            if (null == Instance)
            {
                return null;
            }
            return Instance;
        }
    }

    /// <summary>
    /// Class Declaration
    /// </summary>
    #region Managed Class Declaration

    #endregion

    /// <summary>
    /// Global Variable Declaration
    /// </summary>
    #region Global Variable Declaration

    #endregion

    private void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }

        Init();
    }
    
    public PlayerInfoManager PlayerInfoManager;
    public AchiveManager AchiveManager;
    public TimeManager TimeManager;
    public FirebaseDBManager FirebaseDBManager;
    public AudioManager AudioManager;
    public SceneEffectManager SceneEffectManager;
    public NetworkManager NetworkManager;
    public AdmobManager AdmobManager;

    private bool Init()
    {
        PlayerInfoManager = GameObject.Find("PlayerInfoManager").GetComponent<PlayerInfoManager>();
        AchiveManager = GameObject.Find("AchiveManager").GetComponent<AchiveManager>();
        TimeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
        FirebaseDBManager = GameObject.Find("FirebaseDBManager").GetComponent<FirebaseDBManager>();
        AudioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        SceneEffectManager = GameObject.Find("SceneEffectManager").GetComponent<SceneEffectManager>();
        NetworkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        AdmobManager = GameObject.Find("AdmobManager").GetComponent<AdmobManager>();

        return true;
    }    
    #region Member Method Declaratives
#if UNITY_STANDALONE


#elif UNITY_ANDROID
    /// <summary>
    /// This function makes it possible to call when building a fixed screen.
    /// You can choose from a drop-down format in the editor, and you can call it from anywhere.
    /// </summary>
    /// <param name="orName"></param>
    public void OrientationChanged(string orName)
    {
        if (orName == "Portrait")
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else if (orName == "LandscapeLeft")
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        else if (orName == "LandscapeRight")
        {
            Screen.orientation = ScreenOrientation.LandscapeRight;
        }
        else
        {
            Debug.Log("Chacking your Orientations Name");
        }
    }

    public void DestroyGameManager()
    {
        Destroy(this.gameObject);
    }

#endif
    #endregion
}
