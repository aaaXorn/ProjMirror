using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Save
{
    public static void SetAvatar()
	{
		PlayerPrefs.SetInt("Avatar", NetworkClient.avatar);
	}
	public static int GetAvatar()
	{
		return PlayerPrefs.GetInt("Avatar", 0);
	}
	
	public static void SetUsername()
	{
		PlayerPrefs.SetString("Username", StaticVars.username);
	}
	public static string GetUsername()
	{
		return PlayerPrefs.GetString("Username", "Jonas");
	}
	
	public static void SetVMaster()
	{
		PlayerPrefs.SetFloat("VMaster", StaticVars.volume_master);
	}
	public static float GetVMain()
	{
		return PlayerPrefs.GetFloat("VMaster", 1f);
	}
	public static void SetVSfx()
	{
		PlayerPrefs.SetFloat("VSfx", StaticVars.volume_sfx);
	}
	public static float GetVSfx()
	{
		return PlayerPrefs.GetFloat("VSfx", 1f);
	}
	public static void SetVMusic()
	{
		PlayerPrefs.SetFloat("VMusic", StaticVars.volume_music);
	}
	public static float GetVMusic()
	{
		return PlayerPrefs.GetFloat("VMusic", 1f);
	}
	public static void SetGraphics()
	{
		PlayerPrefs.SetFloat("Graphics", StaticVars.graphics);
	}
	public static float GetGraphics()
	{
		return PlayerPrefs.GetFloat("Graphics", 6);
	}
}
