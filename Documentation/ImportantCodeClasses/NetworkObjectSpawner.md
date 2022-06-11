# NetworkObjectSpawner
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
