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

    [SerializeField]
    SceneName nextScene = SceneName.CharacterSelection;

    [SerializeField]
    private TMPro.TMP_InputField m_joinCodeInput;

    bool m_pressAnyKeyActive = true;
    const string k_enterMenuTriggerAnim = "enter_menu";

    IEnumerator Start()
    {
        // -- To test with latency on development builds --
        // To set the latency, jitter and packet-loss percentage values for develop builds we need
        // the following code to execute before NetworkManager attempts to connect (changing the
        // values of the parameters as desired).
        //
        // If you'd like to test without the simulated latency, just set all parameters below to zero(0).
        //
        // More information here:
        // https://docs-multiplayer.unity3d.com/docs/tutorials/testing/testing_with_artificial_conditions#debug-builds
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().
            SetDebugSimulatorParameters(
                packetDelay: 50,
                packetJitter: 5,
                dropRate: 3);
#endif

        // Clean the all the data of the characters so we can start with a clean slate
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

    public async void OnClickHost()
    {
        if (RelayManager.Instance.IsRelayEnabled)
            await RelayManager.Instance.SetupRelay();

        NetworkManager.Singleton.StartHost();
        AudioManager.Instance.PlaySound(m_confirmClip);

        LoadingSceneManager.Instance.LoadScene(nextScene);
    }

    public async void OnClickJoin()
    {
        System.Threading.Tasks.Task<RelayJoinData> joinRelayTask = null;
        if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(m_joinCodeInput.text))
        {
            joinRelayTask = RelayManager.Instance.JoinRelay(m_joinCodeInput.text);
        }

        if (joinRelayTask == null)
            return;

        await joinRelayTask;

        AudioManager.Instance.PlaySound(m_confirmClip);

        if(joinRelayTask.Result.AllocationID != System.Guid.Empty)
            StartCoroutine(Join());
    }

    public void OnClickQuit()
    {
        AudioManager.Instance.PlaySound(m_confirmClip);
        Application.Quit();
    }
}
