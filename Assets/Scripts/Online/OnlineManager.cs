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
    private int no_player;

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
        }

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
