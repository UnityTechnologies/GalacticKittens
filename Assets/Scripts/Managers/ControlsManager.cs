using Unity.Netcode;
using UnityEngine;

// This scene is show for a moment before gameplay
public class ControlsManager : NetworkBehaviour
{
    [SerializeField]
    private int m_waitingTime;

    [SerializeField]
    private SceneName m_sceneName;

    private void Start()
    {
        // Invoke the next scene, waiting some time
        Invoke(nameof(LoadNextScene), m_waitingTime);

        AudioManager.Instance.SwitchToGameplayMusic();
    }

    private void LoadNextScene()
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
    private void LoadClientRpc()
    {
        if (IsServer)
            return;

        LoadingFadeEffect.Instance.FadeAll();
    }
}
