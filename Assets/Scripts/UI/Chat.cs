using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Chat : NetworkBehaviour
{
	public static Chat Instance {get; private set;}
	
	[HideInInspector]
	public PlayerName PN;
	
	[SerializeField] private GameObject ChatBox;
    [SerializeField] private Text txt;
	[SerializeField] private InputField iField;
	
	private void Awake()
	{
        if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
	}
	
	public override void OnStartAuthority()
	{
		ChatBox.SetActive(true);
	}
	
	private void HandleNewMessage(string msg)
	{
		string s = PN != null ? PN.nickname : "meucu";
		txt.text += "\n" + s + ": "+ msg;
		print("msg");
	}
	
	[Client]
	public void Send(string msg)
	{
		if(!Input.GetKeyDown(KeyCode.Return)) return;
		if(string.IsNullOrWhiteSpace(msg)) msg = iField.text;
		
		if(!isServer) Cmd_SendMessage(iField.text); else SrvSendMessage(iField.text);
		
		iField.text = string.Empty;
		print("fdp");
	}
	
	[Command]
	private void Cmd_SendMessage(string msg)
	{
		Rpc_HandleMessage(msg);
		print("vsf");
	}

	private void SrvSendMessage(string msg)
	{
		Rpc_HandleMessage(msg);
		print("vsf");
	}
	
	[ClientRpc]
	private void Rpc_HandleMessage(string msg)
	{
		HandleNewMessage(msg);
		print("rpc");
	}
}
