using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AIControl : NetworkBehaviour
{
    public Rigidbody rigid;
	//SetBool, SetInteger and SetFloat can be used with regular animator
	//SetTrigger requires NetworkAnimator as otherwise it isn't synced correctly
	private NetworkAnimator net_anim;
	private Animator anim;
	
	private enum States
    {
		Free,
		Attack,
		Hurt
    }
	private States state = States.Free;
	
	//movement speed
	[SerializeField]
	private float h_spd, rot_spd;
	
	//if the player can currently move
	public bool can_move = true;

	[SerializeField]
	private float punch_range;
	[SerializeField]
	private Transform PunchPoint;
	private LayerMask player_layer;
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
	
	private void Start()
    {
        rigid = GetComponent<Rigidbody>();
		
		//animator
		net_anim = GetComponent<NetworkAnimator>();
		anim = net_anim.animator;

		player_layer = LayerMask.GetMask("Player");
    }
	
	public override void OnStartServer()
	{
		base.OnStartServer();
		
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
				StartCoroutine("AttackState");
				break;

			case States.Hurt:
				StartCoroutine("HurtState");
				break;
        }
    }
	
	private IEnumerator FreeState()
	{
		if (can_move)
		{
			
		}

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
		yield return null;
		StateMachine(States.Free);
    }
	
	[ClientRpc]
	public void Rpc_Punch(Vector3 pos)
	{
		
	}
	
	[Command]
	public void Cmd_Drop()
    {
		
	}
}
