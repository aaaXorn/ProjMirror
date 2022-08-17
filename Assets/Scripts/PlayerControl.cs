using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    private Rigidbody rigid;
	[SerializeField]
	private Collider char_col;

    #region movement
    //movement inputs
    private float inputX, inputZ;
	//private bool jump_input;

	//movement speed
	[SerializeField]
	private float h_spd, rot_spd;//, jump_spd;
	
	//if the player can currently move
	public bool can_move = true;
	#endregion

	//this player's team
	[SyncVar]
	public int team;

	#region grab
	private bool grab_input, throw_input;

	[SerializeField]
	private float grab_range;

	[Tooltip("Throw force.")]
	[SerializeField]
	private float throw_spd_Z, throw_spd_Y;

	[SerializeField]
	private Vector3 grab_offset;

	//where the object goes on grab
    [SerializeField]
	private Transform GrabPoint;

	[SerializeField]
	private LayerMask piece_layer;
	
	[SyncVar]
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
		//if (Input.GetButtonDown("Jump")) jump_input = true;
		if (Input.GetButtonDown("Fire2")) grab_input = true;
		if (Input.GetButtonDown("Fire1")) throw_input = true;
    }

    private void FixedUpdate()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;

		if (can_move)
		{
			//movement and rotation
			Move();

			/*if(jump_input)
            {
				if(JumpCheck())
					Jump();
				
				jump_input = false;
            }*/
			
			#if UNITY_EDITOR
			//debug so raycast is visible in editor
			Debug.DrawRay(transform.position + grab_offset, transform.forward * grab_range, Color.red);
			Debug.DrawRay(transform.position + transform.up * 0.1f, -transform.up * 0.2f, Color.blue);
			#endif

			if (grab_input)
			{
				//grabs an object
				if(GrabObj == null)
				{
					Grab();
				}
				//releases grabbed object
				else
				{
					Cmd_Drop();
				}

				grab_input = false;
			}
			else if(throw_input)
            {
				if(GrabObj != null)
                {
					Cmd_Throw();
                }

				throw_input = false;
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
		rigid.velocity = new Vector3(dir.x * h_spd, rigid.velocity.y, dir.z * h_spd);
		
		/*float max_spd
		if(rigid.velocity.magnitude < max_spd)
			rigid.AddForce(dir * h_spd);
		*/
	}

	/*private bool JumpCheck()
    {
		//checks if there's an object in range
		RaycastHit hit;
		if (Physics.Raycast(transform.position + Vector3.up * 0.1f, -Vector3.up * 0.2f, out hit))
		{
			return true;
		}
		return false;
    }

	private void Jump()
    {
		//force
		rigid.AddForce(Vector3.up * jump_spd, ForceMode.Impulse);
    }*/
    #region grab

    #region hold
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

		//sets object owner, color and team
		PieceCheck piece_pc = obj.GetComponent<PieceCheck>();
		if (piece_pc != null)
		{
			GrabObj = obj;

			piece_pc.Owner = this;
			piece_pc.Rpc_ChangeColor(team);
		}
		//sets as kinematic
		Rigidbody piece_rb = obj.GetComponent<Rigidbody>();
		if (piece_rb != null)
		{
			piece_rb.isKinematic = true;
		}
		else Debug.LogError("Piece PieceCheck is null.");

		Rpc_Grab(obj);
	}

    [ClientRpc]
    private void Rpc_Grab(GameObject obj)
    {
		if (obj != null)
		{
			//sets object parent
			obj.transform.SetParent(GrabPoint);
			
			//disables collision between the player and the grabbed object
			Physics.IgnoreCollision(obj.GetComponent<Collider>(), char_col, true);
		}
    }
    #endregion

    #region release
    [Command]
	private void Cmd_Drop()
    {
		//sets as non-kinematic
		Rigidbody piece_rb = GrabObj.GetComponent<Rigidbody>();
		if (piece_rb != null)
		{
			piece_rb.isKinematic = false;
		}

		Rpc_ReleaseGrab(GrabObj);

		GrabObj = null;
    }

	[Command]
	private void Cmd_Throw()
    {
		//sets as non-kinematic
		Rigidbody piece_rb = GrabObj.GetComponent<Rigidbody>();
		if (piece_rb != null)
		{
			piece_rb.isKinematic = false;

			//force
			piece_rb.AddForce(transform.forward * throw_spd_Z + transform.up * throw_spd_Y, ForceMode.Impulse);
		}

		Rpc_ReleaseGrab(GrabObj);

		GrabObj = null;
    }

	[ClientRpc]
	private void Rpc_ReleaseGrab(GameObject obj)
    {
		if (obj != null)
		{
			//de-parents object
			obj.transform.parent = null;

			//enables collision between the player and the grabbed object
			Physics.IgnoreCollision(obj.GetComponent<Collider>(), char_col, false);
		}
	}
	#endregion

	#endregion
}
