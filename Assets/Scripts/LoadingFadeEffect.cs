using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
    Script to do the fade effect on loading scenes
*/

public class LoadingFadeEffect : SingletonPersistent<LoadingFadeEffect>
{    
    public static bool s_canLoad;       // boolean to determinate when the actual loading can happen

    [SerializeField]
    Image m_loadingBackground;          // The background image to change alpha for effect

    [SerializeField]
    [Range(0f, 0.5f)]
    float m_loadingStepTime;            // A range of time to wait for every repetition on the effect

    [SerializeField]
    [Range(0f, 0.5f)]
    float m_loadingStepValue;           // The value to modify the alpha every steo time

    // Run the complete effect, use for the clients
    IEnumerator FadeAllEffect()
    {
        // Do the fade in effect 
        yield return StartCoroutine(FadeInEffect());

        // Wait a x time to call
        yield return new WaitForSeconds(1);

        // Do the fadeout effect
        yield return StartCoroutine(FadeOutEffect());
    }
        
    IEnumerator FadeInEffect()
    {
        // Get the background color
        Color backgroundColor = m_loadingBackground.color;

        // Set the background  color alpha to 0
        backgroundColor.a = 0;

        // Set the modify background color to the background image color
        m_loadingBackground.color = backgroundColor;

        // Turn on the background
        m_loadingBackground.gameObject.SetActive(true);
        
        // Repeat until the alpha is <= to 1
        while (backgroundColor.a <= 1)
        {
            // Wait 
            yield return new WaitForSeconds(m_loadingStepTime);

            // Change the background color bt the step value
            backgroundColor.a += m_loadingStepValue;

            // Set the background image to the modify var
            m_loadingBackground.color = backgroundColor;
        }

        // When the fade-in ends you can start loading the scene
        s_canLoad = true;
    }

    IEnumerator FadeOutEffect()
    {
        // Set the loading to false, it should be already load the new scene
        s_canLoad = false;

        // Get the background image color
        Color backgroundColor = m_loadingBackground.color;

        // Repeat until the alpha is >= 0
        while (backgroundColor.a >= 0)
        {
            // Wait
            yield return new WaitForSeconds(m_loadingStepTime);

            // Change the background color bt the step value
            backgroundColor.a -= m_loadingStepValue;

            // Set the background image to the modify var
            m_loadingBackground.color = backgroundColor;
        }

        // Turn of the background image
        m_loadingBackground.gameObject.SetActive(false);
    }

    // Start the fade-in effect
    public void FadeIn()
    {
        StartCoroutine(FadeInEffect());
    }

    // Start the fadeout effect
    public void FadeOut()
    {
        StartCoroutine(FadeOutEffect());
    }

    // Start a complete fade effect
    public void FadeAll()
    {
        StartCoroutine(FadeAllEffect());
    }

}
