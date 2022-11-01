using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
	[HideInInspector]
    public Rigidbody rigid;
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
		Emote1,
		Emote2,
		Hurt,
		Win,
		Lose
    }
	private States state = States.Free;

    #region movement
    //movement inputs
    private float inputX, inputZ;
	//private bool jump_input;

	//movement speed
	[SerializeField]
	private float h_spd, rot_spd;
	private bool e1_input, e2_input;

	//if the player can currently move
	[HideInInspector]
	public bool can_move = false;

	[SerializeField]
	private float punch_range;
	[SerializeField]
	private Transform PunchPoint;
	private LayerMask player_layer;
	[SerializeField]
	private float punch_force;
	#endregion

	[HideInInspector]
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
	
	[HideInInspector]
	[SyncVar]
	//grabbed object
	public GameObject GrabObj;
	#endregion

	[HideInInspector]
	[SyncVar]
	public Vector3 spawn_pos;
	[HideInInspector]
	public Quaternion spawn_rot;

	private AudioSource audioS;
	[SerializeField] AudioSource audioS_Walk;
	[SerializeField]
	private AudioClip aClip_punch_hit, aClip_punch_start, aClip_grab, aClip_throw;

	[SerializeField]
	private PlayerName PN;

	[SerializeField] GameObject VFX_Punch;

	private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
		
		//animator
		net_anim = GetComponent<NetworkAnimator>();
		anim = net_anim.animator;

		audioS = GetComponent<AudioSource>();

		player_layer = LayerMask.GetMask("Player");
    }

	//on start if object belongs to client
	public override void OnStartAuthority()//LocalPlayer()
	{
		if(Manager.Instance != null)
		{	
			Manager.Instance.local_PC = this;
			Manager.Instance.SetupUI();
			Manager.Instance.SetListenerParent(transform);

			if(Manager.Instance.ready == true)
			{
				can_move = true;
			}
		}
		else Debug.LogError("Manager Instance is null.");

		if(CameraOnline.Instance != null)
		{
			CameraOnline.Instance.ChangeTarget(transform);
		}
		else Debug.LogError("CameraOnline Instance is null.");

		if(ThrowIndicator.Instance != null)
		{
			ThrowIndicator.Instance.target = throw_target;
			ThrowIndicator.Instance.SetPos(throw_target.position);
			ThrowIndicator.Instance.gameObject.SetActive(false);
		}
		else Debug.LogError("ThrowIndicator Instance is null.");

		StateMachine(States.Free);

		spawn_pos = transform.position;
		spawn_rot = transform.rotation;

		Chat.Instance.PN = PN;
		Chat.Instance.PC = this;
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

			case States.Emote1:
				StartCoroutine("EmoteState", 1);
				break;

			case States.Emote2:
				StartCoroutine("EmoteState", 2);
				break;
			
			case States.Win:
				StartCoroutine("EmoteState", 3);
				break;
			
			case States.Lose:
				StartCoroutine("EmoteState", 4);
				break;

			case States.Hurt:
				StartCoroutine("HurtState");
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
		if (Input.GetButtonDown("Emote1")) e1_input = true;
		if (Input.GetButtonDown("Emote2")) e2_input = true;
	}

	private IEnumerator FreeState()
	{
		//while(anim == null) yield return null;
		
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
					StateMachine(States.Attack);

					throw_input = false;

					yield break;
				}

				throw_input = false;
			}
			//emotes
			else if(e1_input)
            {
				if (GrabObj == null)
				{
					StateMachine(States.Emote1);

					e1_input = false;

					yield break;
				}
            }
			else if(e2_input)
            {
				if (GrabObj == null)
				{
					StateMachine(States.Emote2);

					e2_input = false;

					yield break;
				}
            }
		}

		yield return null;

		StateMachine(States.Free);
	}

	[Command]
	private void Cmd_Footsteps(bool play)
	{
		Rpc_Footsteps(play);
	}
	[ClientRpc]
	private void Rpc_Footsteps(bool play)
	{
		if(play)
		{
			if(!audioS_Walk.isPlaying) audioS_Walk.Play();
		}
		else
		{
			if(audioS_Walk.isPlaying) audioS_Walk.Stop();
		}

	}

	//movement and rotation
	private void Move()
	{	
        //rotation
		Vector3 dir = new Vector3(inputX, 0, inputZ).normalized;
		if(dir.magnitude > 0.1f)
		{
			if(!audioS_Walk.isPlaying) Cmd_Footsteps(true);
			
			//final rotation
			Quaternion newRot = Quaternion.LookRotation(dir, Vector3.up);
			//slowly rotates towards final rotation
			transform.rotation = Quaternion.RotateTowards
								 (transform.rotation, newRot, rot_spd * Time.deltaTime);
		}
		else if(audioS_Walk.isPlaying) Cmd_Footsteps(false);
		
        //movement
		rigid.velocity = new Vector3(dir.x * h_spd, rigid.velocity.y, dir.z * h_spd);
		
		/*float max_spd
		if(rigid.velocity.magnitude < max_spd)
			rigid.AddForce(dir * h_spd);
		*/
		
		anim.SetFloat("velocity", rigid.velocity.magnitude);
	}
	[SerializeField]
	float h_force;
	private void AddForceMove(Vector3 dir, float time)
	{
		Vector3 vel = dir * h_spd;
		vel += vel.normalized * 0.2f * rigid.drag;

		float force = Mathf.Clamp(h_force, -rigid.mass / time, rigid.mass / time);

		if(rigid.velocity.magnitude == 0)
			rigid.AddForce(vel * force, ForceMode.Force);
		else
		{
			var velProjectedToTarget = (vel.normalized * Vector3.Dot(vel, rigid.velocity) / vel.magnitude);
			rigid.AddForce((vel - velProjectedToTarget) * force, ForceMode.Force);
		}
	}

	private IEnumerator AttackState()
    {
		net_anim.SetTrigger("atk");

		bool attacking = true;
		float time = 0;
		float start = 0.12f;
		float end = 0.5f;

		while (attacking)
		{
			if (can_move)
				Move();

			if(time >= start)
            {
				audioS.volume = 0.6f;
				audioS.clip = aClip_punch_start;
				audioS.Play();
				
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
								Cmd_Punch(transform.position, other_PC);
						}
						else
						{
							AIControl AIC = hit.GetComponent<AIControl>();
							if(AIC != null && team != AIC.team)
								Cmd_Punch_AI(transform.position, AIC);
						}
					}
                }
				
				//ends attack
				if (time >= end)
					attacking = false;
            }

			time += Time.deltaTime;

			yield return null;
		}

		StateMachine(States.Free);
    }

	private IEnumerator EmoteState(int emote)
    {
		if(audioS_Walk.isPlaying) Cmd_Footsteps(false);
		
		float time = 0;

		anim.SetInteger("no_emote", emote);
		net_anim.SetTrigger("emote");

		switch(emote)
        {
			//teabag
			case 1:
				rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
				time = 1.125f;
				break;

			case 2:
				rigid.velocity = new Vector3(1.5f * rigid.velocity.x, rigid.velocity.y, 1.5f * rigid.velocity.z);
				time = 1.374f;
				break;

			case 3:
				print("win");
				rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
				time = 2f;
				break;
			
			case 4:
				print("lose");
				rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
				time = 2f;
				break;

			default:
				Debug.LogError("Emote error.");
				break;
		}
		print(time);
		yield return new WaitForSeconds(time);

		anim.SetInteger("no_emote", 0);
		net_anim.SetTrigger("emote");

		StateMachine(States.Free);
    }
	[Command(requiresAuthority = false)]
	public void Cmd_EndEmote(int team)
	{
		Rpc_EndEmote(team);
	}
	public void Rpc_EndEmote(int team)
	{
		if(!hasAuthority) return;

		bool win = (team == this.team) ? true : false;

		StopAllCoroutines();

		States st = win ? States.Win : States.Lose;
		StateMachine(st);
	}

	private IEnumerator HurtState()
    {
		if(audioS_Walk.isPlaying) Cmd_Footsteps(false);

		yield return new WaitForSeconds(0.75f);
		
		StateMachine(States.Free);
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
				audioS.volume = 0.25f;
				audioS.clip = aClip_grab;
				audioS.Play();
				
				anim.SetBool("hasBox", true);
				
				GameObject obj = hits[hit_no].transform.gameObject;

				Cmd_Grab(obj);
			}
		}
    }
	
	#region punch
	[Command]
	private void Cmd_Punch(Vector3 pos, PlayerControl PC)
	{
		if(PC.GrabObj != null)
		{
			PC.GrabObj.GetComponent<PieceCheck>().Rpc_ChangeColor(0);
			PC.Cmd_Drop();
		}
		
		GameObject VFX = Instantiate(VFX_Punch, pos + Vector3.up, transform.rotation);
		NetworkServer.Spawn(VFX);

		PC.Rpc_Punch(pos, punch_force);
	}
		[Command]
		private void Cmd_Punch_AI(Vector3 pos, AIControl AIC)
		{
			if(AIC.GrabObj != null)
			{
				AIC.GrabObj.GetComponent<PieceCheck>().Rpc_ChangeColor(0);
				AIC.Cmd_Drop();
			}
			
			AIC.Cmd_Punched(pos, punch_force);
		}
	
	//when player is punched
	[ClientRpc]
	public void Rpc_Punch(Vector3 pos, float force)
	{
		if(state == States.Hurt) return;

		audioS.volume = 0.7f;
		audioS.clip = aClip_punch_hit;
		audioS.Play();

		if(!isLocalPlayer) return;
		
		StopAllCoroutines();
		StateMachine(States.Hurt);
		
		Vector3 force_pos = new Vector3(pos.x, transform.position.y, pos.z);
		//add force
		Vector3 dir = (transform.position - force_pos).normalized;
		
		rigid.AddForce((dir * force) + (Vector3.up * force * 0.5f), ForceMode.Force);
	}
	#endregion
	
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

		ThrowIndicator.Instance.gameObject.SetActive(true);

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
	public void Cmd_Drop()
    {
		anim.SetBool("hasBox", false);
		
		//sets as non-kinematic
		Rigidbody piece_rb = GrabObj.GetComponent<Rigidbody>();
		if (piece_rb != null)
		{
			piece_rb.isKinematic = false;
		}
		
		PieceCheck pCheck = GrabObj.GetComponent<PieceCheck>();
		if(pCheck != null) pCheck.Owner = null;
		Rpc_ReleaseGrab(GrabObj);

		GrabObj = null;

		ThrowIndicator.Instance.gameObject.SetActive(false);
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

		PieceCheck pCheck = GrabObj.GetComponent<PieceCheck>();
		if(pCheck != null) pCheck.Owner = null;
		Rpc_ReleaseGrab(GrabObj);

		GrabObj = null;
    }

	[ClientRpc]
	private void Rpc_ThrowSound()
	{
		audioS.volume = 1.0f;
		audioS.clip = aClip_throw;
		audioS.Play();
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

	[SerializeField]
	Transform throw_target;

	#endregion

	#endregion
	
	#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		//grab range
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(GrabPoint.position + grab_offset, grab_range);
		//punch range
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(PunchPoint.position, punch_range);
	}
	#endif
}
