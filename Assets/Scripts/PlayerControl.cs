using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    //movement inputs
    private float inputX, inputZ;
    //movement speed
    [SerializeField]
    private float h_spd;

    //camera
    /*public override void OnStartLocalPlayer()
    {

    }*/

    private void Update()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;

        inputX = inputX.GetAxis("Horizontal");
        inputZ = inputZ.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;

        //movement
    }
}
