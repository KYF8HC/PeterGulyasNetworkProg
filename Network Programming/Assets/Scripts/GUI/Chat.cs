using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Chat : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private ChatMessage chatMessagePrefab;
    [SerializeField] private CanvasGroup content;

    private void Start()
    {
        if (inputReader != null)
        {
            inputReader.SendEvent += OnSend;
        }
    }
    private void OnSend()
    {
        FixedString128Bytes message = chatInputField.text;
        SubmitMessageRPC(message);
        chatInputField.text = "";
    }
    

    [Rpc(SendTo.Server)]
    public void SubmitMessageRPC(FixedString128Bytes message)
    {
        UpdateMessageRPC(message);
        Debug.Log("Message Sent");
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateMessageRPC(FixedString128Bytes message)
    {
        var cm = Instantiate(chatMessagePrefab, content.transform);
        cm.SetMessage(message.ToString());
    }
}