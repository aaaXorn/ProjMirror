using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectJonas : MonoBehaviour
{
	public static ConnectJonas Instance {get; private set;}
	
	[HideInInspector]
    public AudioSource audioS;
	
	private void Awake()
	{
        if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
	}
	private void Start()
	{
		audioS = GetComponent<AudioSource>();
	}
}
