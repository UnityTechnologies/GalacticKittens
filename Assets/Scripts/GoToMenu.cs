using System.Collections;
using UnityEngine;

/*
    Script to go to the Menu scene after the loading manager is load.
*/
public class GoToMenu : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait for the loading scene manager to start
        yield return new WaitUntil(() => LoadingSceneManager.Instance != null);

        // Load the menu
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }
}