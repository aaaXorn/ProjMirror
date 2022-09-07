using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpawn : NetworkBehaviour
{
    public override void OnStartClient()
    {
        //OnlineManager.Instance.CreatePlayer(StaticVars.avatar, );
    }
}
