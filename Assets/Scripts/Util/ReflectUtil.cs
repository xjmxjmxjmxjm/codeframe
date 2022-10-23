using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ReflectUtil
{

    private static Assembly _Assembly;
    
    /// <summary>
    /// 通过类名获得类  脚本生成
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Type GetType(string name)
    {
        if (Application.isPlaying)
        {
            Type type = Type.GetType(name);
            if (type == null)
            {
                //LogUtil.LogError("don't find  wnd   wndName -> " + name);
            }
            return type;
        }
        else
        {
            if (_Assembly == null)
            {
                _Assembly = Assembly.Load("Assembly-CSharp");
            }
            Type type = _Assembly.GetType(name);
            if (type == null)
            {
                //LogUtil.LogError("don't find  wnd   wndName -> " + name);
            }
            return type;
        }
       
    }
    
    
    public static Type GetTypeInGame(string name)
    {
        if (Application.isPlaying && false)
        {
            Type type = Type.GetType(name);
            if (type == null)
            {
                LogUtil.LogError("don't find  wnd   wndName -> " + name);
            }
            return type;
        }
        else
        {
            if (_Assembly == null)
            {
                _Assembly = Assembly.Load("Assembly-CSharp");
            }
            
            Type type = _Assembly.GetType(name);
            if (type == null)
            {
                LogUtil.LogError("don't find  wnd   wndName -> " + name);
            }
            return type;
        }
       
    }
    
    
    
}
