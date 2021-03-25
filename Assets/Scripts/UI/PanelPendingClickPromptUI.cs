using UnityEngine;
using UnityEngine.UI;

public class PanelPendingClickPromptUI : MonoBehaviour
{
    [SerializeField] UIManager manager;
    [SerializeField] Text text;
    [SerializeField] Image image;

    public void Narrate(string message, Color color)
    {
        text.text = message;
        text.color = color;
        image.raycastTarget = false;
        manager.BeginAnnouncement();
    }
}
