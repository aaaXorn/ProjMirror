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
	
	private void HandleNewMessage(string msg, string name)
	{
		string s = name != null ? name : "meucu";
		txt.text += "\n" + s + ": "+ msg;
	}
	
	[Client]
	public void Send(string msg)
	{
		if(!Input.GetKeyDown(KeyCode.Return)) return;
		if(string.IsNullOrWhiteSpace(msg)) msg = iField.text;
		
		if(!isServer) Cmd_SendMessage(iField.text, PN.nickname); else SrvSendMessage(iField.text);
		
		iField.text = string.Empty;
	}
	
	[Command(requiresAuthority = false)]
	private void Cmd_SendMessage(string msg, string name)
	{
		Rpc_HandleMessage(msg, name);
	}

	private void SrvSendMessage(string msg)
	{
		Rpc_HandleMessage(msg, PN.nickname);
	}
	
	[ClientRpc]
	private void Rpc_HandleMessage(string msg, string name)
	{
		HandleNewMessage(msg, name);
	}
}
