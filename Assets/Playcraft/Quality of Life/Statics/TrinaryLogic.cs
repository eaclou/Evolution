public enum Trinary 
{
    /// Condition is true if value is true 
    True,
    /// Condition is true if value is false
    False,
    /// Condition is always true (a.k.a. "don't care")
    Undefined 
}

public struct TrinaryCondition
{
    public bool value;
    public Trinary desired;
    
    public TrinaryCondition(bool value, Trinary desired)
    {
        this.value = value;
        this.desired = desired;
    }
}

public static class TrinaryLogic
{
    public static bool AllConditionsMet(TrinaryCondition[] conditions)
    {
        foreach (var condition in conditions)
            if (!ConditionMet(condition))
                return false;
                
        return true;
    }
    
    public static bool ConditionMet(TrinaryCondition condition) 
    { return ConditionMet(condition.value, condition.desired); }

    public static bool ConditionMet(bool value, Trinary desired)
    {
        return 
            desired == Trinary.Undefined ||
            value && desired == Trinary.True || 
            !value && desired == Trinary.False;
    }
}