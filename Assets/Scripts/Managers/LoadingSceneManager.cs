using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Important: the names should be the same as the scene
public enum SceneName : byte
{
    Menu,
    CharacterSelection,
    Gameplay,
    Victory,
    Defeat,
    Controls,
    BootStrap,
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
        // LoadingFadeEffect.fadedIn?.Invoke();
        LoadingFadeEffect.Instance.FadeIn();

        // Wait until i can load
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

        // Because the scene are not heavy we can just wait a second and continue with the fade
        // in case the scene is heavy instead we should use and additive load to wait for the scene to load before continue
        yield return new WaitForSeconds(1f);

        LoadingFadeEffect.Instance.FadeOut();

    }

    // Load the scene using the SceneManager, here there is not network session active
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

    // Load the scene using the SceneManager from NetworkManager, here the network session is active
    void LoadSceneNetwork(SceneName sceneToLoad)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad.ToString(), LoadSceneMode.Single);
    }

    // The event when the scene finish loading
    // Here we set up what to do on each scene and change the music
    void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // We only care the host catch the loading 'cause every manager handles on the server the sync info
        if (!NetworkManager.Singleton.IsServer)
            return;

        Enum.TryParse<SceneName>(sceneName, out m_sceneActive);

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
                //print($"Gameplay -> {clientId}");
                GameplayManager.Instance.ServerSceneInit(clientId);
                // AudioManager.Instance.CrossPlayGameplay();
                break;
            // When a client/host connects tell the manager to create the player score ships and change the music
            case SceneName.Victory:
            case SceneName.Defeat:
                EndGameManager.Instance.ServerSceneInit(clientId);
                // AudioManager.Instance.CrossPlayIntro();
                break;
        }
    }

    // Load scene and if the loading is local or network -> isNetworkSessionActive
    public void LoadScene(SceneName sceneToLoad, bool isNetworkSessionActive = true)
    {
        // Start the load process
        StartCoroutine(Loading(sceneToLoad, isNetworkSessionActive));
    }

    // The menu scene will make me subscribe to the network events, 'cause for some reason
    // when a network session ends i cannot longer listen to the events.
    public void Init()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }
}
