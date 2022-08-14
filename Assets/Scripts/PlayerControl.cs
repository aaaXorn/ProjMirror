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
    private float h_spd, jump_spd, rot_spd;
	
	//if the player can currently move
	public bool can_move = true;
	
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
	
	//on start if object belongs to client
	public override void OnStartLocalPlayer()
	{
		if(Manager.Instance != null)
		{	
			Manager.Instance.local_PC = this;
			Manager.Instance.SetupUI();
		}
		else Debug.LogError("Manager Instance is null.");
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
	
	//movement and rotation
	private void Move()
	{	
        //rotation
		Vector3 dir = new Vector3(inputX, 0, inputZ).normalized;
		if(dir.magnitude > 0.1f)
		{
			//final rotation
			Quaternion newRot = Quaternion.LookRotation(dir, Vector3.up);
			//slowly rotates towards final rotation
			transform.rotation = Quaternion.RotateTowards
								 (transform.rotation, newRot, rot_spd);
		}
		
        //movement
		rigid.velocity = dir * h_spd;
		
		/*float max_spd
		if(rigid.velocity.magnitude < max_spd)
			rigid.AddForce(dir * h_spd);
		*/
	}
}
