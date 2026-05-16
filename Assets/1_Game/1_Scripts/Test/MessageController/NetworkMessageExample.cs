using Mirror;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// 1. Обязательно создаем структуру сообщения, реализующую NetworkMessage
public struct CustomGameMessage : NetworkMessage {
    public int Id;
    public string messageText;
}
public struct RegisterGameMessageRequest : NetworkMessage {
    public string NameOfRegisterMessageType;
}
public class NetworkMessageExample : NetworkBehaviour {

   

    Dictionary<string, HashSet<NetworkConnectionToClient>> subscribers = new();
 
    public event Action<NetworkMessage> OnClientRecitveMessage;

    #region SERVER
    public override void OnStartServer() {
        base.OnStartServer();
        if (NetworkServer.active) {
            RegisterServerReciever();
            Debug.Log("Сервер активирован");
        }
    }

    public override void OnStopServer() {
        base.OnStopServer();
        NetworkServer.UnregisterHandler<RegisterGameMessageRequest>();
        NetworkServer.UnregisterHandler<CustomGameMessage>();
    }

    public void RegisterClientRequest(NetworkConnectionToClient conn, RegisterGameMessageRequest request) {
        if (!NetworkClient.active) return;

        RegisterRequest(conn, request);

    }

    public void RegisterServerReciever() {
        NetworkServer.RegisterHandler<RegisterGameMessageRequest>(RegisterRequest);
        NetworkServer.RegisterHandler<CustomGameMessage>(SendToAllSubscribers);
    }
    void RegisterRequest(NetworkConnectionToClient conn, RegisterGameMessageRequest msg) {
        RegisterClient(msg.NameOfRegisterMessageType, conn);
    }
    public void RegisterClient(string typeName, NetworkConnectionToClient conn) {
        if (!subscribers.ContainsKey(typeName))
            subscribers.Add(typeName, new HashSet<NetworkConnectionToClient>());


        subscribers[typeName].Add(conn);
        Debug.Log($"{typeName} Зарегал клиента на рассылку типа  + тперь клиенитов ");
    }

    public void SendToAllSubscribers(NetworkConnectionToClient conn, CustomGameMessage msg) {
        Debug.Log($"player conn {conn.address} is sending message to all subscribers {msg.messageText}");

        Debug.Log($"{msg.GetType().FullName}");
        var message = new CustomGameMessage() { messageText = msg.messageText, Id = conn.connectionId };
        if (subscribers.Count == 0) return;
        var subs = subscribers[msg.GetType().FullName];
        foreach (var subscriber in subs) {
            SendToSingleClient((NetworkConnectionToClient)subscriber, msg);
        }
    }

    public void SendToSingleClient(NetworkConnectionToClient targetConnection, CustomGameMessage msg) {
        Debug.Log($"[СЕРВЕР] Отправлено персональное сообщение клиенту ID: {targetConnection.connectionId}");
        if (!NetworkServer.active) return;

        // Сервер отправляет данные через объект соединения конкретного игрока
        targetConnection.Send(msg);
        Debug.Log($"[СЕРВЕР] Отправлено персональное сообщение клиенту ID: {targetConnection.connectionId}");
    }
    #endregion
    #region CLIENT
    public override void OnStartClient() {

        base.OnStartClient();
        if (NetworkClient.active) {
            RegisterClientReceiver();
            Debug.Log("Клиент активирован");
        }
    }

    public override void OnStopClient() {
        base.OnStopClient();
        NetworkClient.UnregisterHandler<CustomGameMessage>();
        NetworkClient.UnregisterHandler<RegisterGameMessageRequest>();
    }

    public void RegisterClientReceiver() {
        NetworkClient.RegisterHandler<CustomGameMessage>(OnReceiveMessageFromServer);
        NetworkClient.RegisterHandler<RegisterGameMessageRequest>(_ => { });
        Debug.Log("[КЛИЕНТ] Успешно зарегистрирован на получение CustomGameMessage!");
        NetworkClient.Send<CustomGameMessage>(new CustomGameMessage { messageText = "Я присоединился мой id : " });
    }

    private void OnReceiveMessageFromServer(CustomGameMessage msg) {
        OnClientRecitveMessage?.Invoke(msg);
        Debug.Log($"[КЛИЕНТ] Получено сообщение от сервера! Текст: '{msg.messageText}', Очки: {msg.Id}");
    }
    #endregion
}