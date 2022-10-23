using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Util;

public class LogUtil
{
    private static bool _isOpenLog = true;
    private static bool _isOpenWarning = true;
    private static bool _isOpenLogError = true;

    /// <summary>
    /// 是否上传报错日志
    /// </summary>
    private static bool _isUpLoadExceptionLog = true;

    /// <summary>
    /// 是否保存日志消息
    /// </summary>
    private static bool _isSaveLog = false;

    
    public static void WriteLogPath(string condition, string stackTrace, LogType type)
    {
        string timer = System.DateTime.Now.ToString("yyyyMMddhhmmss");
        string path = string.Format("{0}/output_{1}.txt", Application.persistentDataPath, System.DateTime.Now.ToString("yyyyMMddhhmmss"));
        
        string str = StringUtil.GetMergeStr(condition, "\n", stackTrace, "\n", type.ToString());

        if (str.Length <= 0) return;
        if (LogUtil._isSaveLog)
        {
            if (!File.Exists(path))
            {
                var fs = File.Create(path);
                fs.Close();
            }

            using (var sw = File.AppendText(path))
            {
                sw.WriteLine(str);
            }
        }

        if (_isUpLoadExceptionLog && type == LogType.Exception)
        {
            GameStart.Instance.UpLoadLog(timer, str);
        }
    }

    
    
    
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
    
    public static void LogWarning(int num)
    {
        if (!_isOpenLog) return;
        LogWarning(num.ToString());
    }
    public static void LogWarning(string str)
    {
        if (!_isOpenLogError) return;
        Debug.LogWarning(str);
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
