using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    /// <summary>
    /// 拓展方法工具类 
    /// </summary>
    public static class ExtendUtil     
    {

//        public static List<T> Sort<T>()
//        {
//            
//        }
        

        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            T component = transform.GetComponent<T>();
            if (component == null)
            {
                component = transform.gameObject.AddComponent<T>();
            }

            return component;
        }
        
        public static Transform GetByName(this Transform transform, string name)
        {
            var temp = transform.Find(name);
            if (temp == null)
            {
                LogUtil.LogError("can not find " + name + "under the parent:" + transform.name);
            }

            return temp;
        }
        
    }
}
