# Project Structure of Galactic Kittens
Galactic Kittens is a 4-player co-op action game experience, where players collaborate to take down some robot enemies, and then a dangerous missle launching boss. Players can select between the 4 space explorers which can fire lasers, and collect power ups that let them activate a spaceship shield that protects against 1 hit.

This document describes the high-level architecture of Galactic Kittens.
If you want to familiarize yourself with the code base, you are just in the right place!

<br>

Main topics discussed:
* [Game State / Scene Flow](GameFlow.md)

* [Connection Flow](ConnectionFlow.md)

* Important code classes:
  * [LoadingSceneManager](ImportantCodeClasses/LoadingSceneManager.md)
  * [NetworkObjectSpawner](ImportantCodeClasses/NetworkObjectSpawner.md)
  * [AudioManager](ImportantCodeClasses/AudioManager.md)

* Characters:
  * [Player Spaceship](Characters/PlayerSpaceship.md)
  * [Basic Enemies](Characters/BasicEnemies.md)
  * [Boss Battle](Characters/BossBattle.md)