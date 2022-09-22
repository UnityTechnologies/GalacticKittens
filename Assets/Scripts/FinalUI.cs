using System;
using UnityEngine;

public class FinalUI : MonoBehaviour
{
    public static Action onGameOverEvent;

    [SerializeField]
    private GameObject gameOverPanel;

    private void OnEnable()
    {
        onGameOverEvent += ActivateGameOverUI;
    }

    private void OnDisable()
    {
        onGameOverEvent -= ActivateGameOverUI;
    }

    private void ActivateGameOverUI()
    {
        gameOverPanel.SetActive(true);
    }
}