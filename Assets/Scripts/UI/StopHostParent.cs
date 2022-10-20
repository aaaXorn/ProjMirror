using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopHostParent : MonoBehaviour
{
    void Update()
    {
        if(Chat.Instance != null)
        {
            transform.parent = Chat.Instance.transform;
            Destroy(this);
        }
    }
}
