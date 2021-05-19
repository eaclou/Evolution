using UnityEngine;
using UnityEngine.Events;

public class RepeatWhileEnabled : MonoBehaviour
{
    [Tooltip("Overrides start and repeat delay, leave blank to set those fields manually")]
    [SerializeField] RepeatingEventDelay data;
    [Tooltip("Time before first output trigger on enable")]
    [SerializeField] float startDelay = 0f;
    [Tooltip("Time between subsequent output triggers while enabled")]
    [SerializeField] float repeatDelay = 0.2f;
    [SerializeField] UnityEvent Output;

    void OnValidate() { RefreshData(); }
    void Start() { RefreshData(); }
    
    void RefreshData()
    {
        if (!data) return;
        startDelay = data.startDelay;
        repeatDelay = data.repeatDelay;        
    }
    
    void OnEnable() { InvokeRepeating(nameof(Refresh), startDelay, repeatDelay); }
    void OnDisable() { CancelInvoke(nameof(Refresh)); }
    void Refresh() { Output.Invoke(); }
}
