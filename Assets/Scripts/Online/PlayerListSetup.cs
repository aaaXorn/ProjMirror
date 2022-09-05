using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerListSetup : NetworkBehaviour
{
	//instance global reference
    public static PlayerListSetup Instance { get; private set; }
	
	private void Awake()
	{
		if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
		
		ResetPlayerList();
	}
	
	#region spaghetti
	public void ResetPlayerList()
	{
		print("test");
		Cmd_ResetPlayerList();
	}
	
	[Command]
	public void Cmd_ResetPlayerList()
	{
		print("test1");
		Manager.Instance.PlayerList.Clear();
		
		Rpc_ResetPlayerList();
	}
	
	[ClientRpc]
	public void Rpc_ResetPlayerList()
	{
		print("test2");
		Cmd_AddPlayerList(Manager.Instance.local_PC);
	}
	
	[Command]
	public void Cmd_AddPlayerList(PlayerControl player)
	{
		print("test3");
		Manager.Instance.PlayerList.Add(player);
	}
	#endregion
}
