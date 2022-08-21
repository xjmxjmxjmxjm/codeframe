using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogUtil
{
    private static bool _isOpenLog = true;
    private static bool _isOpenLogError = true;
    
    
    public static void Log(int num)
    {
        if (!_isOpenLog) return;
        Log(num.ToString());
    }
    
    public static void Log(string str)
    {
        if (!_isOpenLog) return;
        Debug.Log(str);
    }
    
    public static void LogError(int num)
    {
        if (!_isOpenLog) return;
        LogError(num.ToString());
    }
    public static void LogError(string str)
    {
        if (!_isOpenLogError) return;
        Debug.LogError(str);
    }
}
