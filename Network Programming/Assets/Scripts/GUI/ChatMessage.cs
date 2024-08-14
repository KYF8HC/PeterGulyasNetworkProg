using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    public void SetMessage(string message)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = message;
    }
}