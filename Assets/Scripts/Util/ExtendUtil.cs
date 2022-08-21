using System;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    /// <summary>
    /// 拓展方法工具类 
    /// </summary>
    public static class ExtendUtil     
    {
        public static void AddBtnListener(this RectTransform rect, Action action)
        {
            var button = rect.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
           

            button.onClick.AddListener(()=> action());
        }
        
        public static void AddBtnListener(this Transform rect, Action action)
        {
            var button = rect.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
           

            button.onClick.AddListener(() =>
            {
                action();
            });
        }

        public static RectTransform RectTransform(this Transform transform)
        {
            var rect = transform.GetComponent<RectTransform>();
            if (rect != null)
            {
                return rect;
            }
            else
            {
                LogUtil.LogError("can not find RectTransform");
                return null;
            }
        }
        
        public static Image Image(this Transform transform)
        {
            var image = transform.GetComponent<Image>();
            if (image != null)
            {
                return image;
            }
            else
            {
                LogUtil.LogError("can not find Image");
                return null;
            }
        }
        
        public static Text Text(this Transform transform)
        {
            var text = transform.GetComponent<Text>();
            if (text != null)
            {
                return text;
            }
            else
            {
                LogUtil.LogError("can not find Text");
                return null;
            }
        }
        
        public static Button Button(this Transform transform)
        {
            var btn = transform.GetComponent<Button>();
            if (btn != null)
            {
                return btn;
            }
            else
            {
                LogUtil.LogError("can not find Button");
                return null;
            }
        }

        

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
