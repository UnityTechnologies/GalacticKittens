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

Always use networked when loading a scene when the network session is active and use local for when there are not network sessions. 

<br>

The network loading will use the NetworkManager.SceneManagment for loading the new scene, this is only done by the server. The local loading uses Unity.SceneManagment for loading a new scene,  this is only used when loading the menu scene because this is the only scene in the project where there is not a network session active.

More information about scene management [here](https://docs-multiplayer.unity3d.com/docs/basics/scene-management).