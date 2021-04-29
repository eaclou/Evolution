using UnityEngine;

public static class Logger
{
    public static void Log(string message, bool condition = true, Object context = null)
    {
        if (!condition) return;
        Debug.Log(message, context);
    }
}
