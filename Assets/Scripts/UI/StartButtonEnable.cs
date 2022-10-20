using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StartButtonEnable : NetworkBehaviour
{
    void Start()
    {
        if(!isServer)
            GetComponent<Button>().interactable = false;

        Destroy(this);
    }
}
