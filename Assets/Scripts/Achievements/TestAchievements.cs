using UnityEngine;

public class TestAchievements : MonoBehaviour
{
    AchievementListInfo achievementsList => Lookup.instance.achievements;

    public bool triggerUnlock;
    public bool triggerLock;
    public AchievementId achievement;

    void OnValidate()
    {
        if (triggerUnlock)
        {
            var newUnlock = achievementsList.SetUnlocked(achievement, true);
            triggerUnlock = false;
        }
        if (triggerLock)
        {
            achievementsList.SetUnlocked(achievement, false);
            triggerLock = false;
        }
    }
    
    void Start()
    {
        achievementsList.onUnlock += OnUnlockNew;
    }
    
    void OnDestroy()
    {
        achievementsList.onUnlock -= OnUnlockNew;
    }
    
    void OnUnlockNew(AchievementData achievement)
    {
        // This will get called when a new achievement gets unlocked
        // (optional) Filter by info if you only care about some achievements
        // Logic goes here for the response
    }
}
