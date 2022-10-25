using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Offscreen : NetworkBehaviour
{
	[SerializeField]
	float _offscreen_points;

	private LayerMask player_layer, piece_layer;
	
	private void Start()
	{
		GetComponent<BoxCollider>().enabled = true;

		player_layer = LayerMask.NameToLayer("Player");
		piece_layer = LayerMask.NameToLayer("Piece");
	}
	
    private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == player_layer)
		{
			PlayerControl PC = other.GetComponent<PlayerControl>();
			if(PC != null)
			{
				if(!PC.isLocalPlayer) return;

				if(isServer)
				{
					if(PC.team == 1)
						Manager.Instance.t2_score += 5;
					else if(PC.team == 2)
						Manager.Instance.t1_score += 5;
				}
				else Cmd_ChangeScore(PC.team);

				other.transform.position = PC.spawn_pos + Vector3.up * 10f;
			}
			else
			{
				if(!isServer) return;

				AIControl AIC = other.GetComponent<AIControl>();
				if(AIC != null)
				{
					other.transform.position = new Vector3(57.6f, 18.349f, -10);
				}
				//piece
				else
				{
					Vector3 pos = Manager.Instance.SpawnPosition();
					other.transform.position = pos;
				}
			}
		}
		else if(other.gameObject.layer == piece_layer)
		{
			Vector3 pos = Manager.Instance.SpawnPosition();
			
			other.transform.position = pos;
		}
	}

	[Command(requiresAuthority = false)]
	public void Cmd_ChangeScore(int team)
	{
		if(team == 1)
			Manager.Instance.t2_score += _offscreen_points;
		else if(team == 2)
			Manager.Instance.t1_score += _offscreen_points;
	}
}
