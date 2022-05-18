using System;

[Serializable]
public class TwoIntRandomizer
{
    public IntRandomizer subprocess;
    int width;
    
    /// Assumes minimums of zero
    public TwoIntRandomizer(int width, int height)
    {
        this.width = width;
        subprocess = new IntRandomizer(0, width * height);
    }
    
    public (int, int) SimpleRandom() { return BreakNumber(subprocess.SimpleRandom()); }
    public (int, int) RandomNoRepeat() { return BreakNumber(subprocess.RandomNoRepeat()); }
    
    (int, int) BreakNumber(int value)
    {
        var x = value % width;
        var y = value / width + 1;
        return (x, y);
    }
}
