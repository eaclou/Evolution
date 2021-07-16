using UnityEngine;

// Sends out a BoolEvent when cursor moves in/out of the bounds of a RectTransform
public class EnterExitRectBounds : MonoBehaviour
{
    [SerializeField] CursorInRectBounds process;
    [SerializeField] BoolEvent onChangeState;
    
    bool inBounds;
    bool wasInBounds;

    void Start()
    {
        inBounds = process.inBounds;
        onChangeState.Invoke(inBounds);
        wasInBounds = inBounds;
    }

    void Update()
    {
        inBounds = process.inBounds;
        
        if (inBounds != wasInBounds)
        {
            onChangeState.Invoke(inBounds);
            wasInBounds = inBounds;
        }
    }
}
