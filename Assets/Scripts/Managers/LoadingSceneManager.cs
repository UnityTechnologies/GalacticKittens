using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Important: the names in the enum value should be the same as the scene you're trying to load
public enum SceneName : byte
{
    Bootstrap,
    Menu,
    CharacterSelection,
    Controls,
    Gameplay,
    Victory,
    Defeat,
    //.
    //.
    // Add more scenes states if needed
};

public class LoadingSceneManager : SingletonPersistent<LoadingSceneManager>
{    
    private SceneName m_sceneActive;

    public SceneName SceneActive => m_sceneActive;

    // Coroutine for the loading effect. It use an alpha in out effect 
    IEnumerator Loading(SceneName sceneToLoad, bool isNetworkSessionActive)
    {
        LoadingFadeEffect.Instance.FadeIn();

        // Wait until I can load
        yield return new WaitUntil(() => LoadingFadeEffect.s_canLoad);

        // The actual load of the scene, here the player still see a black screen  
        // Check if the networking session is active
        if (isNetworkSessionActive)
        {
            if (NetworkManager.Singleton.IsServer)
                LoadSceneNetwork(sceneToLoad);
        }
        else
        {
            LoadSceneLocal(sceneToLoad);
        }

        // Because the scenes are not heavy we can just wait a second and continue with the fade.
        // In case the scene is heavy instead we should use additive loading to wait for the
        // scene to load before we continue
        yield return new WaitForSeconds(1f);

        LoadingFadeEffect.Instance.FadeOut();
    }

    // Load the scene using the regular SceneManager, use this while there's no active networked session
    void LoadSceneLocal(SceneName sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad.ToString());
        switch (sceneToLoad)
        {
            case SceneName.Menu:
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMusic(AudioManager.MusicName.intro);
                break;
        }
    }

    // Load the scene using the SceneManager from NetworkManager. Use this when there is an active network session
    void LoadSceneNetwork(SceneName sceneToLoad)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneToLoad.ToString(),
            LoadSceneMode.Single);
    }

    // The callback function triggered when the scene is finished loading
    // Here we set up what to do on each scene and change the music
    void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // We only care the host/server is loading because every manager handles
        // their information and behavior on the server runtime
        if (!NetworkManager.Singleton.IsServer)
            return;

        Enum.TryParse(sceneName, out m_sceneActive);

        if (!ClientConnection.Instance.CanClientConnect(clientId))
            return;

        // What to initially do on every scene.
        switch (m_sceneActive)
        {            
            // When a client/host connects tell the manager
            case SceneName.CharacterSelection:
                CharacterSelectionManager.Instance.ServerSceneInit(clientId);
                break;

            // When a client/host connects tell the manager to create the ship and change de music
            case SceneName.Gameplay:
                GameplayManager.Instance.ServerSceneInit(clientId);
                break;

            // When a client/host connects tell the manager to create the player score ships and change the music
            case SceneName.Victory:
            case SceneName.Defeat:
                EndGameManager.Instance.ServerSceneInit(clientId);
                break;
        }
    }

    // Load scene and if the loading is local or network -> isNetworkSessionActive
    public void LoadScene(SceneName sceneToLoad, bool isNetworkSessionActive = true)
    {
        // Start the load process
        StartCoroutine(Loading(sceneToLoad, isNetworkSessionActive));
    }

    // The menu scene will make me subscribe to the network events, because for some reason
    // when a network session ends it cannot longer listen to the events.
    public void Init()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }
}
