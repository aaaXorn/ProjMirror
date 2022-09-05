using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class AIControl : NetworkBehaviour
{
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
	
	private GameObject FollowTarget;

	private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
		
		//animator
		net_anim = GetComponent<NetworkAnimator>();
		anim = net_anim.animator;
		nav = GetComponent<NavMeshAgent>();
		nav.speed = h_spd;

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
	
	private IEnumerator FreeState()
	{
		if(!nav.enabled) nav.enabled = true;
		if(!rigid.isKinematic) rigid.isKinematic = true;
		
		float time = 0f;

		bool searching = true;
		
		//get position
		Vector3 pos = Vector3.zero;

		while(searching)
		{
			searching = false;

			yield return null;
		}

		while(time < 5f)
		{
			if (Vector3.Distance(transform.position, pos) < 0.2f)
				time = 5f;
			else
			{
				if (can_move)
				{
					print(pos);
					Move(pos);
				}
				
				
				time += Time.deltaTime;
			}

			yield return null;
		}
		
		//check for follow target

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
		Move(FollowTarget.transform.position);

		yield return null;
		StateMachine(States.Free);
	}

	private IEnumerator AttackState()
    {
		yield return null;
		StateMachine(States.Free);
    }
	
	private IEnumerator HurtState()
    {
		
		
		yield return new WaitForSeconds(1.25f);
		
		StateMachine(States.Free);
    }
	
	//when ai is punched
	[ClientRpc]
	public void Rpc_Punch(Vector3 pos)
	{
		if(!isServer || state == States.Hurt) return;
		
		StopAllCoroutines();
		StateMachine(States.Hurt);
		
		if(nav.enabled) nav.enabled = false;
		if(rigid.isKinematic) rigid.isKinematic = false;
		
		//add force
		Vector3 dir = (transform.position - pos).normalized;
		rigid.AddForce(dir * punch_force, ForceMode.Force);
	}
	
	[Command]
	public void Cmd_Drop()
    {
		
	}
}
