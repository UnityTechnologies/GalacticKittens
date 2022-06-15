using System.Collections;
using TMPro;
using UnityEngine;

public class TypescriptEffect : MonoBehaviour
{
    [SerializeField]
    private string _text;                       // Text to animate

    [SerializeField]
    private float _typescriptWaitingTime;       // The waiting time between char animations on typescript

    [SerializeField]
    private float _scaleAnimationTime;          // Time to animate the scale

    [SerializeField]
    private AnimationCurve _scaleCurve;         // Curve for the scale animation

    private TextMeshProUGUI _textUI;            // The ProGUI text on the UI

    private void OnEnable()
    {        
        // Get the components needed for animating the text
        _textUI = GetComponent<TextMeshProUGUI>();
        StartCoroutine(Effect()); 
    }

    /* 
        Typescript animation 
        The animation is two parts:
        1) Char by char animation
        2) Scale animation
        This animation is a loop
    */
    private IEnumerator Effect()
    {
        while (true)
        {
            string animText = string.Empty;
            _textUI.text = animText;

            // With a loop we create a typescript effect assigning character per character
            // in animText var and waiting X time
            for (int i = 0; i < _text.Length; i++)
            {
                animText += _text[i];
                _textUI.text = animText;
                yield return new WaitForSeconds(_typescriptWaitingTime);
            }
            
            float curveDeltaTime = 0;                       // Curve animation time
            Vector2 initialScale = new Vector2(1, 1);       // The initial scale of the object
            Vector2 scaleValues = initialScale;             // Vector 2 for the new scale

            // Scale animation block
            while (curveDeltaTime <= _scaleAnimationTime)
            {
                curveDeltaTime += Time.deltaTime;
                // Evaluate the curve an get the value
                // more info:
                // -> https://docs.unity3d.com/ScriptReference/AnimationCurve.html
                // -> https://docs.unity3d.com/ScriptReference/AnimationCurve.Evaluate.html
                float scaleCurve = _scaleCurve.Evaluate(curveDeltaTime);
                scaleValues = new Vector2(scaleCurve, scaleCurve);
                transform.localScale = scaleValues;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}