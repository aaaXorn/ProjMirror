using System;
using System.Collections.Generic;
using System.Linq;
using kcp2k;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Mirror;

public class OnlineManager : NetworkManager
{
	public static OnlineManager Instance {get; private set;}
	
	[SerializeField]
	private GameObject[] Skins;
	
    public int no_player;
	
	public override void Awake()
	{
		base.Awake();
		
		if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
	}
	
    /// <summary>Called on server when a client requests to add the player. Adds playerPrefab by default. Can be overwritten.</summary>
    // The default implementation for this function creates a new player object from the playerPrefab.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        //sets each player's team
        PlayerControl PC = player.GetComponent<PlayerControl>();
        if (PC != null)
        {
            no_player++;
            PC.team = (no_player % 2 == 0) ? 2 : 1;
			
			Manager.Instance.PlayerList.Add(PC);
        }

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
		no_player--;
		
        base.OnServerDisconnect(conn);
        
		PlayerListSetup.Instance.ResetPlayerList();
		
        print("disconnect");
    }
}
