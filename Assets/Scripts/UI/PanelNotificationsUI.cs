using UnityEngine;
using UnityEngine.UI;

// Rename?
public class PanelNotificationsUI : MonoBehaviour
{
    UIManager manager => UIManager.instance;
    
    [SerializeField] Text text;
    [SerializeField] Image image;
    
    public void Narrate(NarrationSO value) { Narrate(value.message, value.color); }
    public void Narrate(string message, Color color)
    {
        text.text = message;
        text.color = color;
        image.raycastTarget = false;
        manager.BeginAnnouncement();        
    }
}
