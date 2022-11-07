using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VFX_Behaviour : NetworkBehaviour
{
    [SerializeField] float _destroyTime = 2f;

    public override void OnStartServer()
    {
        base.OnStartServer();

        Invoke("VFX_Finish", _destroyTime);
    }

    void VFX_Finish()
    {
        NetworkServer.Destroy(gameObject);
    }
}
