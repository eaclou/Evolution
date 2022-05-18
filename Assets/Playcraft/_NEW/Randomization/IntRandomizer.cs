using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class IntRandomizer
{
    int min, max;
    public int count => max - min;

    public IntRandomizer(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
    
    public int SimpleRandom() { return Random.Range(min, max); }
    
    #region Random No Repeat
    
    [ReadOnly] [SerializeField] int[] all;
    [ReadOnly] [SerializeField] List<int> available;
    
    public int RandomNoRepeat()
    {
        if (all == null || all.Length != count)
            Initialize();
        
        var index = Random.Range(0, available.Count);
        var result = available[index];
        
        available.RemoveAt(index);
        
        if (available.Count == 0)
        {
            ResetAvailable();
            available.Remove(result);
        }
        
        return result;
    }
    
    void Initialize()
    {
        all = new int[count];
        SetAllSequential();
        
        available = new List<int>();
        ResetAvailable();
    }
    
    void SetAllSequential()
    {
        for (int i = 0; i < all.Length; i++)
            all[i] = min + i;
    }
    
    void ResetAvailable()
    {
        available.Clear();
        foreach (var number in all)
            available.Add(number);
    }
    
    #endregion
}
