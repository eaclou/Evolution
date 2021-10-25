using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Pond Water/Agent/Name List")]
public class NameList : ScriptableObject
{
    [SerializeField] Name[] names;
    
    public string GetRandomName()
    {
        var index = Random.Range(0, names.Length);
        return names[index].firstName;
    }

    // Extend to specify male/female or any other data associated with specific names
    [Serializable] public class Name { public string firstName; }
}
