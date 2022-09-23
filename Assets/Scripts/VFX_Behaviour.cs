using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VFX_Behaviour : NetworkBehaviour
{
    void Start()
    {
        Invoke("VFX_Finish", 2f);
    }

    void VFX_Finish()
    {
        NetworkServer.Destroy(gameObject);
    }
}
