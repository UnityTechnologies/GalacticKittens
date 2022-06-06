# Setup Instructions

**From the Editor:**

1. Open the project. You can start with the latest **Unity Editor ver. 2020.3**

2. Open the scene called `Bootstrap`. This scene sets up the [NetworkManager](https://docs-multiplayer.unity3d.com/netcode/current/components/networkmanager) Singleton.
<img src="Documentation/Images/listOfMainScenes.png" style="width:70%;height:70%">

3. Play the scene.

4. Press any key on the title screen.

5. When you reach the main menu, click on the **HOST** button:
<img src="Documentation/Images/MainMenu.png" style="width:70%;height:70%">


6. Select a character, using the *A* or *W* keys and then press the spacebar or click on the **Ready!** button below.
<img src="Documentation/Images/SelectCharacter_1player.png" style="width:70%;height:70%">

7. The game will automatically take you to the `Controls` scene, and then shortly after will transition to the main gameplay.
<img src="Documentation/Images/controls.png" style="width:70%;height:70%">

**From a Build:**
1. Open the project. You can start with the latest **Unity Editor ver. 2020.3**

2. Open the build menu, by clicking on `File\BuildSettings...` on the main menu toolbar. Build the game as it is, by clicking on the `Build` button below:
<img src="Documentation/Images/BuildSettings.png" style="width:60%;height:60%">

3. Open the output executable, and play the game. Follow the same steps as running from the editor to play the game, starting at step **#4**.

<img src="Documentation/Images/openingBuild.gif" style="width:50%;height:50%">

**Testing Local Multiplayer:**

1. Build the game following the instructions above.

2. Open one instance of the game, and run it as the host, by clicking on the **HOST** button:
<img src="Documentation/Images/clickingOnHost.png" style="width:50%;height:50%">

3. Open another instance of the game, and run it as the client, by clicking on the **JOIN** button:
<img src="Documentation/Images/clickingOnJoin.png" style="width:50%;height:50%">

4. The client instance will now join the host's networked session! Now select a character on both running instances, and  click on the **READY!** button:
<img src="Documentation/Images/choosingCharacters.png" style="width:50%;height:50%">

5. Both instances will now automatically proceed to the gameplay stage, after displaying the controls:
<img src="Documentation/Images/playingSideBySide.png" style="width:50%;height:50%">

6. You can try this with up to `4` concurrent instances!
