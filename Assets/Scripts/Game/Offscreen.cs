using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Offscreen : NetworkBehaviour
{
	[SerializeField]
	float _offscreen_points;

	private LayerMask player_layer, piece_layer;
	
	bool _cd = false;

	[SerializeField] GameObject _vfx;

	private void Start()
	{
		GetComponent<BoxCollider>().enabled = true;

		player_layer = LayerMask.NameToLayer("Player");
		piece_layer = LayerMask.NameToLayer("Piece");
	}
	
	[SerializeField] Transform AIC_Spawn;
	
    private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == player_layer)
		{
			PlayerControl PC = other.GetComponent<PlayerControl>();
			if(PC != null)
			{
				if(!PC.isLocalPlayer) return;
				
				if(!_cd)
				{
					Cmd_ChangeScore(PC.team);

					_cd = true;
					Invoke("ResetCD", 1f);

					GameObject VFX = Instantiate(_vfx, other.transform.position, other.transform.rotation);
					NetworkServer.Spawn(VFX);
				}

				other.transform.position = PC.spawn_pos + Vector3.up * 10f;
			}
			else
			{
				if(!isServer) return;

				AIControl AIC = other.GetComponent<AIControl>();
				if(AIC != null)
				{
					if(!_cd)
					{
						_cd = true;
						Invoke("ResetCD", 1f);


						GameObject VFX = Instantiate(_vfx, other.transform.position, other.transform.rotation);
						NetworkServer.Spawn(VFX);
					}

					other.transform.position = AIC_Spawn.position;
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

	private void ResetCD()
	{
		_cd = false;
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
