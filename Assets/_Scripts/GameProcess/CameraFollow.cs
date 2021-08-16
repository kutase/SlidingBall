using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class CameraFollow : MonoBehaviour {

    public Transform target;

    public float smoothSpeed = 0.125f;
    private Vector3 offset;

    private Vector3 initialOffset;

    [Inject(Id = "ResetGameEvent")]
    private UnityEvent resetGameEvent;

    private void Awake()
    {
        offset = transform.position - target.position;
        initialOffset = offset;
        
        resetGameEvent.AddListener(ResetState);
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void ResetState()
    {
        MoveToTarget(1f);
    }

    public void MoveToTarget(float speed)
    {
        var currentPosition = target.position;
        var desiredPosition = currentPosition + offset;

        desiredPosition.x = 0f;

        var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, speed);
        transform.position = smoothedPosition;
    }

    private void LateUpdate()
    {
        if (!target)
        {
            return;
        }

        MoveToTarget(smoothSpeed);
    }
}