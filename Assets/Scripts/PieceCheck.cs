using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PieceCheck : NetworkBehaviour
{
    
    private Material material;
    [SyncVar]
    public int team = 0;

    private bool set;

    public override void OnStartClient()
    {
        material = GetComponent<Renderer>().material;
        material.color = Manager.Instance.mat[0];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if(other.CompareTag("Hole") && !set)
        {   
            Rpc_ChangeColor();

            StartCoroutine("MoveTo");
        }
    }

    [ClientRpc]
    public void Rpc_ChangeColor()
    {
        material.color = Manager.Instance.mat[team];
    }

    private IEnumerator MoveTo()
    {
        yield break;
    }
}
