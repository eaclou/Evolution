using UnityEngine;

public class TestAchievements : MonoBehaviour
{
    AchievementListInfo achievementsList => Lookup.instance.achievements;

    public bool triggerUnlock;
    public AchievementInfo achievement;

    void OnValidate()
    {
        if (triggerUnlock && achievement)
        {
            var newUnlock = achievementsList.Unlock(achievement);
            Debug.Log(newUnlock);  // OK - true -> false
            triggerUnlock = false;
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
    
    void OnUnlockNew(AchievementInfo achievement)
    {
        // This will get called when a new achievement gets unlocked
        // (optional) Filter by info if you only care about some achievements
        // Logic goes here for the response
    }
}
