using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class BallController : MonoBehaviour
{
    public float Velocity = 10f;

    private bool isRight = false;

    public LayerMask BlockLayer;

    private bool isMoving = false;
    private bool isFallen = false;

    [Inject(Id = "GameOverEvent")]
    private UnityEvent gameOverEvent;

    [Inject(Id = "ResetGameEvent")]
    private UnityEvent resetGameEvent;

    [Inject(Id = "StartGameEvent")]
    private UnityEvent startGameEvent;

    private void Awake()
    {
        resetGameEvent.AddListener(ResetState);
        startGameEvent.AddListener(OnStartGame);
    }

    private void Update()
    {
        if (!isMoving)
        {
            return;
        }

        if (isFallen)
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRight = !isRight;
        }

        var direction = isRight ? new Vector3(-1f, 0f, 1f) : new Vector3(1f, 0f, 1f);

        transform.position += direction.normalized * (Velocity * Time.deltaTime);

        if (!Physics.SphereCast(new Ray(transform.position, Vector3.down * 2f), 0.2f, BlockLayer))
        {
            isFallen = true;
            gameOverEvent.Invoke();
        }
    }

    private void ResetState()
    {
        isFallen = false;
        isMoving = false;

        isRight = false;

        var position = transform.position;
        position.x = 0f;
        position.z = 0f;

        transform.position = position;
    }

    private void OnStartGame()
    {
        isMoving = true;
    }
}
