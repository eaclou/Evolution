using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Narration/Cause of Death")]
public class CauseOfDeathSO : ScriptableObject
{
    public string causeOfDeath;
    public string eventMessage;
}

public enum CauseOfDeathId
{
    SwallowedWhole,
    Starved,
    Suffocated,
    Injuries,
    OldAge,
    DivineJudgment,
}
