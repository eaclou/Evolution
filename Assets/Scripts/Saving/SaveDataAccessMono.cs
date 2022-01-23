using UnityEngine;

/// Display save data in the inspector for debugging
public class SaveDataAccessMono : MonoBehaviour
{
    [SerializeField] bool triggerSave;
    [SerializeField] bool triggerLoad;
    
    void OnValidate()
    {
        if (triggerSave)
        {
            process.Save();
            triggerSave = false;
        }
        if (triggerLoad)
        {
            process.Load();
            triggerLoad = false;
        }
    }

    [SerializeField] SaveDataAccess process;
}
