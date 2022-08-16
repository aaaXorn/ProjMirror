using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    private Rigidbody rigid;

    #region movement
    //movement inputs
    private float inputX, inputZ;
    //movement speed
    [SerializeField]
    private float h_spd, jump_spd, rot_spd;
	
	//if the player can currently move
	public bool can_move = true;
	#endregion

	//this player's team
	[SyncVar]
	public int team;

	#region grab
	private bool grab_input;

	[SerializeField]
	private float grab_range;

	[SerializeField]
	private Vector3 grab_offset;

	//where the object goes on grab
    [SerializeField]
	private Transform GrabPoint;

	[SerializeField]
	private LayerMask piece_layer;
	
	//grabbed object
	public GameObject GrabObj;
	#endregion

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
		if (Input.GetButtonDown("Fire2")) grab_input = true;
    }

    private void FixedUpdate()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;

		if (can_move)
		{
			Move();
#if UNITY_EDITOR
Debug.DrawRay(transform.position + grab_offset, transform.forward * grab_range, Color.red);
#endif
			if (grab_input)
			{
				Grab();
				grab_input = false;
			}
		}
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

#region grab
	private void Grab()
    {
		//checks if there's an object in range
		RaycastHit hit;
		//raycast that only hits the piece layer
		if(Physics.Raycast(transform.position + grab_offset, transform.forward * grab_range, out hit, piece_layer))
        {
			GameObject obj = hit.transform.gameObject;

			Cmd_Grab(obj);
		}
    }

	[Command]
	private void Cmd_Grab(GameObject obj)
    {
		//changes object position
		obj.transform.position = GrabPoint.position;

		//sets object Color
		PieceCheck piece_pc = obj.GetComponent<PieceCheck>();
		if (piece_pc != null)
		{
			piece_pc.Owner = this;
			piece_pc.Rpc_ChangeColor(team);
		}

		Rpc_Grab(obj);
	}

    [ClientRpc]
    private void Rpc_Grab(GameObject obj)
    {
		//sets the grabbed object
		GrabObj = obj;

		if (GrabObj != null)
		{
			//sets object parent
			GrabObj.transform.SetParent(GrabPoint);

			//sets object owner
			PieceCheck piece_pc = obj.GetComponent<PieceCheck>();
			if (piece_pc != null)
			{
				piece_pc.Owner = this;
			}
			else Debug.LogError("Piece PieceCheck is null.");

			//sets as kinematic
			Rigidbody piece_rb = obj.GetComponent<Rigidbody>();
			if (piece_rb != null)
			{
				piece_rb.isKinematic = true;
			}
			Physics.IgnoreCollision(obj.GetComponent<Collider>(), GetComponent<Collider>(), true);
		}
    }
    #endregion
}
