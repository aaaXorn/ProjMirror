using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowIndicator : MonoBehaviour
{
    public static ThrowIndicator Instance;

    public Transform target;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if(target != null)
        {
            transform.position = new Vector3(target.position.x, 0.2f, target.position.z);
        }
    }
}
