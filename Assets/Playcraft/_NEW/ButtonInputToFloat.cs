using UnityEngine;

public class ButtonInputToFloat : MonoBehaviour
{
    [SerializeField] float multiplier = 1f;
    [SerializeField] FloatEvent Output;

    float value = 0f;
      
    void Update()
    {
        Output.Invoke(value * multiplier);
        value = 0f;
    }

    public void Positive() { value += 1f; }
    public void Negative() { value -= 1f; }
}
