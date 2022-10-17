using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    [SerializeField] float _destroyTime;

    void Start()
    {
        Invoke("DestroyThis", _destroyTime);
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }
}
