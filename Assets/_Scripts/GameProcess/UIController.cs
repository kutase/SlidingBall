using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class UIController : MonoBehaviour
{
    public TMP_Text CrystalsCountText;
    public TMP_Text GameOverText;
    public TMP_Text TapToStartText;

    private bool isWaitingToStart = true;
    private bool isGameOver = false;

    [Inject]
    private GameController gameController;

    [Inject(Id = "UICrystalsCountUpdateEvent")]
    private UnityEvent uiCrystalsCountUpdatedEvent;

    [Inject(Id = "UIGameOverEvent")]
    private UnityEvent uiGameOverEvent;

    [Inject(Id = "UIResetEvent")]
    private UnityEvent uiResetEvent;

    [Inject(Id = "UIStartGameEvent")]
    private UnityEvent uiStartGameEvent;

    [Inject(Id = "StartGameEvent")]
    private UnityEvent startGameEvent;
    
    [Inject(Id = "ResetGameEvent")]
    private UnityEvent resetGameEvent;

    private void Awake()
    {
        uiCrystalsCountUpdatedEvent.AddListener(OnCrystalsCountChanged);
        uiResetEvent.AddListener(ResetState);
        uiStartGameEvent.AddListener(OnStartGame);
        uiGameOverEvent.AddListener(OnGameOver);

        ResetState();
    }

    private void OnCrystalsCountChanged()
    {
        CrystalsCountText.SetText(gameController.CrystalsCount.ToString());
    }

    private void Update()
    {
        if (isGameOver && Input.GetMouseButtonUp(0))
        {
            isGameOver = false;
            resetGameEvent.Invoke();
        }

        if (!isGameOver && isWaitingToStart && Input.GetMouseButtonUp(0))
        {
            isWaitingToStart = false;
            startGameEvent.Invoke();
        }
    }

    private void ResetState()
    {
        Debug.Log($"UI reset state");
        CrystalsCountText.SetText("0");
        GameOverText.gameObject.SetActive(false);
        TapToStartText.gameObject.SetActive(true);
        isWaitingToStart = true;
        isGameOver = false;
    }

    private void OnStartGame()
    {
        CrystalsCountText.SetText("0");
        GameOverText.gameObject.SetActive(false);
        TapToStartText.gameObject.SetActive(false);
    }

    private void OnGameOver()
    {
        isGameOver = true;
        GameOverText.gameObject.SetActive(true);
        TapToStartText.gameObject.SetActive(false);
    }
}
