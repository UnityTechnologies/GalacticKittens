using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    Animator m_menu;

    [SerializeField]
    CharacterDataSO[] m_characterDatas;

    [SerializeField]
    AudioClip m_confirmClip;

    bool m_pressAnyKeyActive = true;
    const string k_enterMenuTriggerAnim = "enter_menu";

    [SerializeField]
    SceneName nextScene = SceneName.CharacterSelection;

    IEnumerator Start()
    {
        // Clean the all the data of the characters so we can start with a clean state
        foreach (CharacterDataSO data in m_characterDatas)
        {
            data.EmptyData();
        }

        // Wait for the network Scene Manager to start
        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);

        //  Set the events on the loading manager
        // Doing this because every time the network session ends the loading manager stops detecting the events
        LoadingSceneManager.Instance.Init();
    }

    void Update()
    {
        if (m_pressAnyKeyActive)
        {
            if (Input.anyKey)
            {
                ToMenu();
                m_pressAnyKeyActive = false;
            }
        }
    }

    void ToMenu()
    {
        m_menu.SetTrigger(k_enterMenuTriggerAnim);
        AudioManager.Instance.PlaySound(m_confirmClip);
    }

    // We use a coroutine because the server is the one who makes the load
    // we need to make a fade first before calling the start client
    IEnumerator Join()
    {
        LoadingFadeEffect.Instance.FadeAll();

        yield return new WaitUntil(() => LoadingFadeEffect.s_canLoad);

        NetworkManager.Singleton.StartClient();
    }

    public void OnClickHost()
    {
        NetworkManager.Singleton.StartHost();
        AudioManager.Instance.PlaySound(m_confirmClip);
        LoadingSceneManager.Instance.LoadScene(nextScene);
    }

    public void OnClickJoin()
    {        
        AudioManager.Instance.PlaySound(m_confirmClip);
        StartCoroutine(Join());
    }

    public void OnClickQuit()
    {
        AudioManager.Instance.PlaySound(m_confirmClip);
        Application.Quit();
    }
}
