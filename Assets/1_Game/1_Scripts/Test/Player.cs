using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Player : NetworkBehaviour {
    [SerializeField] Button register;
    [SerializeField] Button send;
    [SerializeField] TextMeshProUGUI textMesh;

    [SerializeField]
    NetworkMessageExample messageController;

   
    public override void OnStartClient() {
        base.OnStartClient();
        register.onClick.AddListener(() => RegisterClientToTakeMessages());
        send.onClick.AddListener(() => SendHelloMessage());
        if (!isLocalPlayer) {
            register.gameObject.SetActive(false);
            send.gameObject.SetActive(false);
            textMesh.gameObject.SetActive(false);
            return;
        }
       

        //Events
        messageController.OnClientRecitveMessage += ChangeText;
    }
    public override void OnStopClient() {
        base.OnStopClient();
        register.onClick.RemoveAllListeners();
        send.onClick.RemoveAllListeners();

        //Events
        messageController.OnClientRecitveMessage -= ChangeText;
    }

    public void Initialize(NetworkMessageExample messageContorller) {
        this.messageController = messageContorller;
    }

    public void RegisterClientToTakeMessages() {                  // Send request to server to take CustomMessageTypes
        if (!isLocalPlayer) return;

        if (!NetworkClient.active) return;

        Debug.Log("Player запрос на регистрацию");
        NetworkClient.Send<RegisterGameMessageRequest>(new RegisterGameMessageRequest { NameOfRegisterMessageType = "CustomGameMessage" });
        //messageController.RegisterClientRequest(connectionToClient, new RegisterGameMessageRequest { NameOfRegisterMessageType = "CustomGameMessage" });
    }
    public void SendHelloMessage() {
        if(!isLocalPlayer) return;

        NetworkClient.Send<CustomGameMessage>(new CustomGameMessage { messageText = "Я отправил сообщение мой ID : "});

    }

    void ChangeText(NetworkMessage msg) {
        var type = msg.GetType();     

        if(type == typeof(CustomGameMessage)) {
            CustomGameMessage customGameMessage = (CustomGameMessage)msg;
            textMesh.text = customGameMessage.messageText + customGameMessage.Id;
        }
    }
}
