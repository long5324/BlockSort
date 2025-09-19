using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayHideObj : MonoBehaviour
{
    [SerializeField] float timeDelay = 1;
    void OnEnable()
    {
        Invoke("Hide", timeDelay);
    }

    void Hide()
    {
        LeanPool.Despawn(gameObject);
    }
}
