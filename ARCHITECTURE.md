# Architecture of Galactic Kittens
This document describes the high-level architecture of Galactic Kittens.
If you want to familiarize yourself with the code base, you are just in the right place!

Galactic Kittens is a 4-player co-op action game experience, where players collaborate to take down some robot enemies, and then a dangerous missle launching boss. Players can select between the 4 space explorers which can fire lasers, and collect power ups that let them activate a spaceship shield that protects against 1 hit.

<br>

## Game State / Scene Flow
The following scenes that define the main flow of the game:

* **Bootstrap:** Set up the NetworkManager  and the LoadingSceneManager instances. Note: The NetworkManager is a singleton class that comes with the Netcode API. LoadingSceneManager was made for this project, and is also a singleton. This scene should only be used once per execution of the application. Afterwards, there’s no reason to use it again. 

* **Menu:** The initial menu of the game, this is the only scene in the game that needs to load on local because there is no network session active.

* **CharacterSelection:** This scene is the first one on a network session active, here the players can select the character to use.

* **Controls:** Small scene to show the keys to use in game.

* **Gameplay:** The main scene where the players control their ship and try to win the game. Enemies, asteroids, and the boss appear in this scene.

* **Victory:** Scene to show the score of the players, select which one was the best player based on the score and exit to the menu scene.

* **Defeat:** Scene to show score of the player and exit to the menu scene.

This is how the scenes are laid out, every scene has a manage classr that sets up the initial information needed to play the scene, on a network session normally you want to spawn a player that every client will control:<br>
![](Documentation/Images/listOfMainScenes.png)


For that, the `LoadingSceneManager` class has a method that receives a callback every time a client connects, this is only on server/host side. In this callback we call the manager for each scene to set up the initial data.

<br>

## Connection Flow
The connection of clients starts with the `MenuManager` script where a player can start a new session(host) or join an existing session (client). This script makes the calls to the NetworkManager commands `StartHost()` or `StartClient()`, which come built in with the Netcode library.

<br>

## Data Model
[TODO: ADD INFO]

## Important classes

**`LoadingSceneManager`**<br>
The job of this class is to load the scenes using these two options, local or networked.

> __IMPORTANT__:
> - When changing scenes one must use one of the `SceneName` enum values:
> ```
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

The network loading will use the NetworkManager.SceneManagment for loading the new scene, this is only done by the server. The local loading uses Unity.SceneManagment for loading a new scene,  this is only used when loading the menu scene because this is the only scene in the project where there is not a network session active.

More information about scene management [here](https://docs-multiplayer.unity3d.com/docs/basics/scene-management).

<br>

**`NetworkObjectSpawner`**<br>
This class lets you spawn a new `GameObject` on the active networked session. There are 2 main methods which are `SpawnNewNetworkObject` and `SpawnNewNetworkObjectChangeOwnershipToClient`. The 2nd one is used when spawning the player spaceships for each connected player/cient.

**List of Methods and Overloads:**

SpawnNewNetworkObject:
* `Prefab:` the prefab to spawn.
* `DestroyWithScene:` default to yes, set if you one the prefab to destroy with the scene.

SpawnNewNetworkObject:
* `Prefab:` the prefab to spawn.
* `Position:` set the position to spawn the prefab.
* `DestroyWithScene:` default to yes, set if you one the prefab to destroy with the scene.

SpawnNewNetworkObject:
* `Prefab:` the prefab to spawn.
* `Position:` set the position to spawn the prefab.
* `Rotation:` set the rotation of the spawned prefab.
* `DestroyWithScene:` default to yes, set if you one the prefab to destroy with the scene.

SpawnNewNetworkObjectChangeOwnershipToClient:

* `Prefab:` the prefab to spawn.
* `Position:` set the position to spawn the prefab.
* `newClientOwnerId:` the id of the client who will own the object
* `DestroyWithScene:` default to yes, set if you one the prefab to destroy with the scene.

Using these methods, you instantiate a prefab on the hosting server and replicate it on all clients. More information on object spawning [here](https://docs-multiplayer.unity3d.com/docs/basics/object-spawning).

**`AudioManager`**<br>