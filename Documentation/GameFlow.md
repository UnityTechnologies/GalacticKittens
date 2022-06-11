# Game State / Scene Flow
The following scenes that define the main flow of the game:

* **Bootstrap:** Set up the NetworkManager  and the LoadingSceneManager instances. Note: The NetworkManager is a singleton class that comes with the Netcode API. LoadingSceneManager was made for this project, and is also a singleton. This scene should only be used once per execution of the application. Afterwards, there’s no reason to use it again. 

* **Menu:** The initial menu of the game, this is the only scene in the game that needs to load on local because there is no network session active.

* **CharacterSelection:** This scene is the first one on a network session active, here the players can select the character to use.

* **Controls:** Small scene to show the keys to use in game.

* **Gameplay:** The main scene where the players control their ship and try to win the game. Enemies, asteroids, and the boss appear in this scene.

* **Victory:** Scene to show the score of the players, select which one was the best player based on the score and exit to the menu scene.

* **Defeat:** Scene to show score of the player and exit to the menu scene.

This is how the scenes are laid out, every scene has a manage classr that sets up the initial information needed to play the scene, on a network session normally you want to spawn a player that every client will control:<br>
![](Images/listOfMainScenes.png)


For that, the `LoadingSceneManager` class has a method that receives a callback every time a client connects, this is only on server/host side. In this callback we call the manager for each scene to set up the initial data.