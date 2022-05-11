using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class NetworkRoomManagerWithSetUpPlayer : NetworkRoomManager
{
    private List<Transform> _players = new List<Transform>();

    public override GameObject OnRoomServerCreateGamePlayer(
        NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        var startPos = GetStartPosition();
        var gamePlayer = startPos != null
            ? GameObject.Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        
        _players.Add(gamePlayer.transform);
        
        foreach (var player in _players)
        {
            var lookTarget = _players[Random.Range(0, _players.Count)];
            while (lookTarget == player && _players.Count != 1)
            {
                lookTarget = _players[Random.Range(0, _players.Count)];
            }
            gamePlayer.transform.LookAt(lookTarget);
        }
        return gamePlayer;
    }
}
