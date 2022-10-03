using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StaticVars
{
    public static int avatar => NetworkClient.avatar;
	public static string username;
	public static float volume_master, volume_sfx, volume_music;
	public static int graphics;
}
