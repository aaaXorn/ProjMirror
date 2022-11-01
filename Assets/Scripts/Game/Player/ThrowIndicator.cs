using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowIndicator : MonoBehaviour
{
    public static ThrowIndicator Instance;

    public Transform target;

    float pos_y;

    void Awake()
    {
        Instance = this;

        pos_y = transform.position.y;
    }

    void Update()
    {
        if(target != null)
        {
            SetPos(target.position);
        }
    }

    public void SetPos(Vector3 pos)
    {
        Vector3 go_to = new Vector3(pos.x, pos_y, pos.z);
        transform.position = go_to;
    }
}
