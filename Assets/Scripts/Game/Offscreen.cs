using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offscreen : MonoBehaviour
{
	private LayerMask player_layer, piece_layer;
	
	private void Start()
	{
		player_layer = LayerMask.NameToLayer("Player");
		piece_layer = LayerMask.NameToLayer("Piece");
	}
	
    private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == player_layer)
		{
			Vector3 pos = Manager.Instance.SpawnPosition();
			
			other.transform.position = pos;
			
			PlayerControl PC = other.GetComponent<PlayerControl>();
			if(PC != null)
			{
				if(PC.team == 1)
					Manager.Instance.t2_score += 5;
				else if(PC.team == 2)
					Manager.Instance.t1_score += 5;
			}
		}
		else if(other.gameObject.layer == piece_layer)
		{
			Vector3 pos = Manager.Instance.SpawnPosition();
			
			other.transform.position = pos;
		}
	}
}
