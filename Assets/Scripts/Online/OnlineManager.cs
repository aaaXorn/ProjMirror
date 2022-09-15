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
	[SerializeField] private GameObject ChatCanvas;

    public int no_player;

	public override void Awake()
	{
        //debugging
        //NetworkClient.avatar = UnityEngine.Random.Range(0, 3);

		base.Awake();
		
		if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
	}
	
    bool plzwork = false;
    /// <summary>Called on server when a client requests to add the player. Adds playerPrefab by default. Can be overwritten.</summary>
    // The default implementation for this function creates a new player object from the playerPrefab.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn, int skin)
    {
        if(!plzwork)
        {
            GameObject obj = Instantiate(ChatCanvas);
            NetworkServer.Spawn(obj);
            
            plzwork = true;
        }

        GameObject prefab = Skins[skin];

        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(prefab, startPos.position, startPos.rotation)
            : Instantiate(prefab);

        //different skins as prefab variants
        //Instantiate(Skins[2], player.transform);

        //sets each player's team
        PlayerControl PC = player.GetComponent<PlayerControl>();
        if (PC != null)
        {
            no_player++;
            PC.team = (no_player % 2 == 0) ? 2 : 1;
			
			//Manager.Instance.PlayerList.Add(PC);
			Manager.Instance.PlayerDict.Add(conn, PC);
            print("Connect: " + Manager.Instance.PlayerDict[conn]);
        }

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public void CreatePlayer(int skin, NetworkConnectionToClient conn)
    {
        if(skin < 0) skin = 0; else if(skin > 3) skin = 3;
        GameObject prefab = Skins[skin];

        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(prefab, startPos.position, startPos.rotation)
            : Instantiate(prefab);

        //different skins as prefab variants
        //Instantiate(Skins[2], player.transform);

        //sets each player's team
        PlayerControl PC = player.GetComponent<PlayerControl>();
        if (PC != null)
        {
            no_player++;
            PC.team = (no_player % 2 == 0) ? 2 : 1;
			
			//Manager.Instance.PlayerList.Add(PC);
			Manager.Instance.PlayerDict.Add(conn, PC);
            print("Connect: " + Manager.Instance.PlayerDict[conn]);
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
        
        print("Disconnect: " + Manager.Instance.PlayerDict[conn]);
		Manager.Instance.PlayerDict.Remove(conn);
		//PlayerListSetup.Instance.ResetPlayerList();
    }
}
