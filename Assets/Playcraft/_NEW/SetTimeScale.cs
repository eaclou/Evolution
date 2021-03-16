using UnityEngine;

public class SetTimeScale : MonoBehaviour
{
    bool isPaused;

    public void TogglePause()
    {
        isPaused = !isPaused;
        SetPaused(isPaused);
    }

    public void SetPaused(bool value)
    {
        isPaused = value;   
        if (value) StopTime();
        else NormalTime();
    }
    
    public void StopTime() { Set(0f); }
    public void NormalTime() { Set(1f); }
    public void Set(float value) { Time.timeScale = value; }
}
