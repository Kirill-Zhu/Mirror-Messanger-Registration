using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Zenject;
public class MyNetworkManager : NetworkManager
{
    [Inject, SerializeField] NetworkMessageExample messageController;
    public override void OnStartClient() {
        base.OnStartClient();
        playerPrefab.TryGetComponent<Player>(out var player);
        player.Initialize(messageController);
    }
}
