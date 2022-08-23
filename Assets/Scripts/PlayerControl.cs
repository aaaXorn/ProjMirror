using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    private Rigidbody rigid;
	//SetBool, SetInteger and SetFloat can be used with regular animator
	//SetTrigger requires NetworkAnimator as otherwise it isn't synced correctly
	private NetworkAnimator net_anim;
	private Animator anim;
	[SerializeField]
	private Collider char_col;

	private enum States
    {
		Free,
		Attack,
		Emote,
		Hurt
    }
	private States state = States.Free;

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
	private Vector3 grab_range;

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
		
		//animator
		net_anim = GetComponent<NetworkAnimator>();
		anim = net_anim.animator;
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

		StateMachine(States.Free);
	}

	private void StateMachine(States _state)
    {
		state = _state;

		switch(state)
        {
			case States.Free:
				StartCoroutine("FreeState");
				break;

			case States.Attack:

				break;

			case States.Emote:

				break;

			case States.Hurt:

				break;
        }
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

	private IEnumerator FreeState()
	{
		if (can_move)
		{
			//movement and rotation
			Move();

			if (grab_input)
			{
				//grabs an object
				if (GrabObj == null)
				{
					Grab();
				}
				//releases grabbed object
				else
				{
					anim.SetBool("hasBox", false);

					Cmd_Drop();
				}

				grab_input = false;
			}
			else if (throw_input)
			{
				//throws grabbed object
				if (GrabObj != null)
				{
					anim.SetBool("hasBox", false);
					net_anim.SetTrigger("throw");

					Cmd_Throw();
				}
				//punch
				else
				{
					net_anim.SetTrigger("atk");
				}

				throw_input = false;
			}
		}

		yield return null;

		StateMachine(States.Free);
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
								 (transform.rotation, newRot, rot_spd * Time.deltaTime);
		}
		
        //movement
		rigid.velocity = new Vector3(dir.x * h_spd, rigid.velocity.y, dir.z * h_spd);
		
		/*float max_spd
		if(rigid.velocity.magnitude < max_spd)
			rigid.AddForce(dir * h_spd);
		*/
		
		anim.SetFloat("velocity", rigid.velocity.magnitude);
	}

	private IEnumerator AttackState()
    {


		yield return null;
    }

	private IEnumerator EmoteState()
    {

		yield return null;
    }

	private IEnumerator HurtState()
    {


		yield return null;
    }

    #region grab

    #region hold
    private void Grab()
    {
		//checks if there are pieces in range
		Collider[] hits = Physics.OverlapBox(GrabPoint.position +
						  grab_offset, grab_range, Quaternion.identity,
						  piece_layer);
		
		//if there were any in the hitbox, grab the piece
		if(hits.Length > 0)
		{
			int no = 0;
			bool found = false;
			
			foreach(Collider hit in hits)
			{
				if(!hit.GetComponent<Rigidbody>().isKinematic)
					found = true;
				else
					no++;
			}
			
			if(found)
			{
				anim.SetBool("hasBox", true);
				
				GameObject obj = hits[0].transform.gameObject;

				Cmd_Grab(obj);
			}
		}
    }
	
	#if UNITY_EDITOR
	//grab range
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawCube(GrabPoint.position + grab_offset, grab_range);
	}
	#endif

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
