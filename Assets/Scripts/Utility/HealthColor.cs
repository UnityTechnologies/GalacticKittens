using System;
using UnityEngine;

[Serializable]
public class HealthColor
{
    public Color normalColor;
    public Color warningColor;
    public Color dangerColor;

    [Range(0f,1f)]
    public float warningHealthValue;

    [Range(0f,1f)]
    public float dangerHealthValue;

    public Color GetHealthColor(float currentHealth)
    {
        Color healthColor = normalColor;
        
        if (currentHealth <= warningHealthValue && currentHealth > dangerHealthValue)
        {
            healthColor = warningColor;
        }
        else if (currentHealth <= dangerHealthValue)
        {
            healthColor = dangerColor;
        }

        return healthColor;
    }
}