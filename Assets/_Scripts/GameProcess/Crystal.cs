using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class Crystal : MonoBehaviour
{
    [Inject(Id = "CrystalCollectedEvent")]
    private UnityEvent crystalCollectedEvent;

    private IEnumerator OnTriggerEnter(Collider other)
    {
        crystalCollectedEvent.Invoke();
        
        yield return null;
        
        Destroy(gameObject);
    }
}
