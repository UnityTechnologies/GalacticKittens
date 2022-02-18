using Unity.Netcode;
using UnityEngine;

// This scene is show for a moment before gameplay
public class ControlsManager : NetworkBehaviour
{
    [SerializeField]
    int m_waitingTime;
    [SerializeField]
    SceneName m_sceneName;

    void Start()
    {
        // Invoke the next scene, waiting some time
        Invoke(nameof(LoadNextScene), m_waitingTime);

        AudioManager.Instance.CrossPlayGameplay();
    }

    void LoadNextScene()
    {
        // Safety check
        if (LoadingSceneManager.Instance != null)
        {
            // Tell the clients to start the loading effect
            LoadClientRpc();

            // Loading scene on server
            LoadingSceneManager.Instance.LoadScene(m_sceneName);
        }
    }

    [ClientRpc]
    void LoadClientRpc()
    {
        if (IsServer)
            return;

        LoadingFadeEffect.Instance.FadeAll();
    }
}
