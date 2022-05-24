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
            Debug.Log(newUnlock);
            triggerUnlock = false;
        }
    }
}
