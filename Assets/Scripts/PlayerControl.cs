using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    private Rigidbody rigid;

    //movement inputs
    private float inputX, inputZ;
    //movement speed
    [SerializeField]
    private float h_spd, jump_spd;
	
	//if the player can currently move
	private bool can_move;
	
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;
        
        //movement inputs
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;

        if(can_move) Move();
    }
	
	private void Move()
	{
		//movement and rotation
        //transform.Rotate
        //rigid.velocity = new Vector3(inputX, rigid.velocity.y, inputZ) * h_spd;
        transform.Translate(inputX * h_spd, 0, inputZ * h_spd);
	}
}
