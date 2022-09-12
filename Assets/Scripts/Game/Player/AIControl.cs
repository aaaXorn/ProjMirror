using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class AIControl : NetworkBehaviour
{
	[SerializeField]
	private Collider char_col;

	[HideInInspector]
    public Rigidbody rigid;
	//SetBool, SetInteger and SetFloat can be used with regular animator
	//SetTrigger requires NetworkAnimator as otherwise it isn't synced correctly
	private NetworkAnimator net_anim;
	private Animator anim;
	private NavMeshAgent nav;

	private enum States
    {
		Falling,
		Free,
		Follow,
		Attack,
		Grab,
		Hurt
    }
	private States state = States.Falling;
	
	//movement speed
	[SerializeField]
	private float h_spd, rot_spd;
	
	//if the player can currently move
	public bool can_move = true;

	[SerializeField]
	private float punch_range;
	[SerializeField]
	private Transform PunchPoint;
	private LayerMask player_layer, wall_layer;
	[SerializeField]
	private float punch_force;
	
	[HideInInspector]
	//this player's team
	[SyncVar]
	public int team;
	
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
	
	#region follow
	private GameObject FollowTarget;

	[SerializeField]
	private float followDist_piece, followDist_player, throw_centerDist;

	private bool searchPlayer_isRunning, searchPiece_isRunning;
	private bool targetIsPlayer;
	#endregion

	private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
		
		//animator
		net_anim = GetComponent<NetworkAnimator>();
		anim = net_anim.animator;
		nav = GetComponent<NavMeshAgent>();
		nav.speed = h_spd;
		nav.angularSpeed = rot_spd;

		player_layer = LayerMask.GetMask("Player");
		wall_layer = LayerMask.GetMask("Wall");
    }
	
	public override void OnStartServer()
	{
		base.OnStartServer();
		
		StateMachine(state);
	}
	
	private void StateMachine(States _state)
    {
		state = _state;

		switch(state)
        {
			case States.Falling:
				StartCoroutine("FallingState");
				break;
			
			case States.Free:
				StartCoroutine("FreeState");
				break;

			case States.Follow:
				StartCoroutine("FollowState");
				break;

			case States.Attack:
				StartCoroutine("AttackState");
				break;

			case States.Grab:
				StartCoroutine("GrabState");
				break;

			case States.Hurt:
				StartCoroutine("HurtState");
				break;
        }
    }
	
	private IEnumerator FallingState()
	{
		if(nav.enabled) nav.enabled = false;
		if(rigid.isKinematic) rigid.isKinematic = false;
		
		rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
		
		//ends state when AI hits the floor
		while(!Physics.Raycast(transform.position + Vector3.up * 0.1f, -Vector3.up, 0.2f, wall_layer))
		{
			yield return null;
		}
		
		StateMachine(States.Free);
	}
	
	#region search
	private IEnumerator SearchPlayer()
	{
		if(searchPlayer_isRunning)
		{
			StopCoroutine("SearchPlayer");
			searchPlayer_isRunning = false;
		}
		if(searchPiece_isRunning)
		{
			StopCoroutine("SearchPiece");
			searchPiece_isRunning = false;
		}
		searchPlayer_isRunning = true;

		float dist = followDist_player;
		
		foreach(KeyValuePair<NetworkConnectionToClient, PlayerControl> key in Manager.Instance.PlayerDict)
		{
			if(GrabObj != null) break;
			
			float target_dist = Vector3.Distance(transform.position, key.Value.transform.position);
			if(target_dist < dist)
			{
				FollowTarget = key.Value.gameObject;
				dist = target_dist;

				if(!targetIsPlayer) targetIsPlayer = true;
			}
		}
		yield return null;

		searchPlayer_isRunning = false;
	}
	private IEnumerator SearchPiece()
	{
		if(searchPlayer_isRunning)
		{
			StopCoroutine("SearchPlayer");
			searchPlayer_isRunning = false;
		}
		if(searchPiece_isRunning)
		{
			StopCoroutine("SearchPiece");
			searchPiece_isRunning = false;
		}
		searchPiece_isRunning = true;

		float dist = followDist_piece;

		foreach(GameObject obj in Manager.Instance.ActPieceList)
		{
			if(GrabObj != null) break;

			float target_dist = Vector3.Distance(transform.position, obj.transform.position);
			if(target_dist < dist)
			{
				FollowTarget = obj;
				dist = target_dist;

				if(targetIsPlayer) targetIsPlayer = false;
			}
		}
		yield return null;

		searchPiece_isRunning = false;
	}
	#endregion

	private IEnumerator FreeState()
	{
		if(!nav.enabled) nav.enabled = true;
		if(!rigid.isKinematic) rigid.isKinematic = true;
		if(FollowTarget) FollowTarget = null;
		
		float time = 0f;

		float dist = 5f;
		
		//get position
		Vector3 pos = Vector3.zero;

		while(time < 2f)
		{
			if (Vector3.Distance(transform.position, pos) < 0.2f)
				time = 2f;
			else
			{
				if (can_move)
				{
					nav.SetDestination(pos);
					anim.SetFloat("velocity", nav.velocity.magnitude);
				}
				
				
				time += Time.deltaTime;
			}

			yield return null;
		}
		yield return null;
		
		//check for follow target
		StartCoroutine("SearchPiece");

		if(FollowTarget == null)
			StateMachine(States.Free);
		else
			StateMachine(States.Follow);
	}
	
	private void Move(Vector3 pos)
	{
		Vector3 dir = pos.normalized;
		
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
		
		anim.SetFloat("velocity", rigid.velocity.magnitude);
	}

	private IEnumerator FollowState()
	{
		bool follow = true;

		while(follow)
		{
			nav.SetDestination(FollowTarget.transform.position);
			anim.SetFloat("velocity", nav.velocity.magnitude);
			//Move(FollowTarget.transform.position);

			if(Vector3.Distance(transform.position, FollowTarget.transform.position) < 1f)
			{
				if(targetIsPlayer)
				{
					StateMachine(States.Attack);
					yield break;
				}
				else
				{
					GameObject obj = Grab();
					print(obj);
					if(obj != null)
					{
						Use_Grab(obj);
						StateMachine(States.Grab);
						yield break;
					}
				}
			}

			if(FollowTarget == null) follow = false;

			yield return null;
		}

		StateMachine(States.Free);
	}

	private IEnumerator AttackState()
    {
		net_anim.SetTrigger("atk");

		bool attacking = true;
		float time = 0;
		float start = 0.12f;
		float end = 0.45f;

		while (attacking)
		{
			if (can_move)
				nav.SetDestination(FollowTarget.transform.position);
				//Move(FollowTarget.transform.position);

			if(time >= start)
            {
				//hitbox
				Collider[] hitCol;

				hitCol = Physics.OverlapSphere(PunchPoint.position, punch_range, player_layer);

				foreach(var hit in hitCol)
                {
					if (hit.transform.root != transform)
					{
						PlayerControl other_PC = hit.GetComponent<PlayerControl>();
						if(other_PC != null)
						{
							if(team != other_PC.team)
								Punch(transform.position, other_PC);
						}
						/*else//not needed as there's only one AI
						{
							AIControl AIC = hit.GetComponent<AIControl>();
							if(AIC != null && team != AIC.team)
								Cmd_Punch_AI(transform.position, AIC);
						}*/
					}
                }
				
				//ends attack
				if (time >= end)
					attacking = false;
            }

			time += Time.deltaTime;

			yield return null;
		}

		FollowTarget = null;

		StateMachine(States.Free);
    }
	
	private IEnumerator GrabState()
	{
		Vector3 target = Vector3.zero;

		while(Vector3.Distance(transform.position, target) > throw_centerDist)
		{
			nav.SetDestination(target);

			if(GrabObj == null)
			{
				StateMachine(States.Free);
				yield break;
			}

			yield return null;
		}

		anim.SetBool("hasBox", false);
		net_anim.SetTrigger("throw");
		Throw();

		StateMachine(States.Free);
	}

	private IEnumerator HurtState()
    {
		
		
		yield return new WaitForSeconds(1.25f);
		
		nav.enabled = true;
		rigid.isKinematic = true;

		if(FollowTarget == null)
			StateMachine(States.Free);
		else
			StateMachine(States.Follow);
    }
	
	#region punch
	//[Command]
	private void Punch(Vector3 pos, PlayerControl PC)
	{
		if(PC.GrabObj != null)
		{
			PC.GrabObj.GetComponent<PieceCheck>().Rpc_ChangeColor(0);
			PC.Cmd_Drop();
		}
		
		PC.Rpc_Punch(pos);
	}

	//when ai is punched
	[Command(requiresAuthority = false)]
	public void Cmd_Punched(Vector3 pos)
	{
		if(!isServer || state == States.Hurt) return;
		
		StopAllCoroutines();
		StateMachine(States.Hurt);
		
		StartCoroutine("SearchPlayer");

		if(nav.enabled) nav.enabled = false;
		if(rigid.isKinematic) rigid.isKinematic = false;
		
		//add force
		Vector3 dir = (transform.position - pos).normalized;
		rigid.AddForce(dir * punch_force, ForceMode.Force);
	}
	#endregion
	
	#region grab
	private GameObject Grab()
    {
		//checks if there are pieces in range
		Collider[] hits = Physics.OverlapBox(GrabPoint.position +
						  grab_offset, grab_range, Quaternion.identity,
						  piece_layer);
		
		//if there were any in the hitbox, grab the piece
		if(hits.Length > 0)
		{
			int no = 0;
			int hit_no = 0;
			bool found = false;
			
			foreach(Collider hit in hits)
			{
				if(!hit.GetComponent<Rigidbody>().isKinematic)
				{
					found = true;
					hit_no = no;
				}
				else
					no++;
			}
			
			if(found)
			{
				anim.SetBool("hasBox", true);
				
				GameObject obj = hits[hit_no].transform.gameObject;

				return obj;
			}
		}
		
		return null;
    }
	private void Use_Grab(GameObject obj)
    {
		//changes object position
		obj.transform.position = GrabPoint.position;

		//sets object owner, color and team
		PieceCheck piece_pc = obj.GetComponent<PieceCheck>();
		if (piece_pc != null)
		{
			GrabObj = obj;

			piece_pc.Owner = gameObject;
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

	private void Throw()
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

	[Command]
	public void Cmd_Drop()
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
}
