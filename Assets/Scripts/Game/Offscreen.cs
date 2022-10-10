using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Offscreen : NetworkBehaviour
{
	private LayerMask player_layer, piece_layer;
	
	private void Start()
	{
		GetComponent<BoxCollider>().enabled = true;

		player_layer = LayerMask.NameToLayer("Player");
		piece_layer = LayerMask.NameToLayer("Piece");
	}
	
    private void OnTriggerEnter(Collider other)
	{
		if(!isServer) return;

		if(other.gameObject.layer == player_layer)
		{
			PlayerControl PC = other.GetComponent<PlayerControl>();
			if(PC != null)
			{
				if(PC.team == 1)
					Manager.Instance.t2_score += 5;
				else if(PC.team == 2)
					Manager.Instance.t1_score += 5;

				other.transform.position = PC.spawn_pos + Vector3.up * 10f;
			}
			else
			{
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
}
