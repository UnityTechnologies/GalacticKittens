# Architecture
This document describes the high-level architecture of Galactic Kittens.
If you want to familiarize yourself with the code base, you are just in the right place!

Galactic Kittens is a 4-player co-op action game experience, where players collaborate to take down some robot enemies, and then a dangerous missle launching boss. Players can select between the 4 space explorers which can fire lasers, and collect power ups that let them activate a spaceship shield that protects against 1 hit.

## Controls
The game uses WASD keyboard controls, space bar for shooting, and the 'K' key to activata the shield.

Code is organized into three separate assemblies: `Client`, `Server` and `Shared` (which, as it's name implies, contains shared functionality that both client and the server require).