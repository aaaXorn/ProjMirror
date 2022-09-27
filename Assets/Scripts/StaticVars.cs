using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StaticVars
{
    public static int avatar => NetworkClient.avatar;
	public static string username;
	public static float volume_main, volume_sfx, volume_music;
}
