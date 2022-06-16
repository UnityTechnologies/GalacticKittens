# LoadingSceneManager
The job of this class is to load the scenes using these two options, local or networked.

> __IMPORTANT__:
> - When changing scenes one must use one of the `SceneName` enum values:
> ```C#
>   public enum SceneName : byte
>   {
>       BootStrap,
>       Menu,
>       CharacterSelection,
>       Controls,
>       Gameplay,
>       Victory,
>       Defeat,
>       //.
>       //.
>       // Add more scenes states if needed
>   };
>```
> **NOTE**: 🚨 The enum value, **must** match the name of the scene you'd like to change to. This is case sensitive as well. 🚨

The main public method to change scenes is called `LoadScene`, which internally figures out which internal Unity function to call:
```C#
public class LoadingSceneManager : SingletonPersistent<LoadingSceneManager>
{
    public SceneName SceneActive => m_sceneActive;

    private SceneName m_sceneActive;

    // After running the menu scene, which initiates this manager, we subscribe to these events
    // due to the fact that when a network session ends it cannot longer listen to them.
    public void Init()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public void LoadScene(SceneName sceneToLoad, bool isNetworkSessionActive = true)
    {
        StartCoroutine(Loading(sceneToLoad, isNetworkSessionActive));
    }

    ...
}
```
Note that when this manager gets initialized, the game listens to the [`OnLoadComplete`](https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkSceneManager#onloadcomplete) event from the Network Manager's own SceneManager.
```C#
    // This callback function gets triggered when a scene is finished loading
    // Here we set up what to do for each scene, like changing the music
    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
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

            // When a client/host connects tell the manager to create the ship and change the music
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
```

<br>

The game code will use the `NetworkManager.SceneManagment` for loading the new scene, this is only done by the server. The local loading uses Unity's local `SceneManagment` for loading a new scene.

More information about scene management in NGO [here](https://docs-multiplayer.unity3d.com/netcode/current/basics/scene-management).