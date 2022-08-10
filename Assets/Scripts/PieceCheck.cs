using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PieceCheck : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;


    }
}
