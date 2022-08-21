using System;
using System.Collections.Generic;
using UnityEngine;

public class UILayerManager     
{
    private readonly Dictionary<UILayer, Transform> _layerDictionary = new Dictionary<UILayer, Transform>();
        
        
    public void Init(Transform transform)
    {
        Transform temp;
        foreach (UILayer item in Enum.GetValues(typeof(UILayer)))
        {
            temp = transform.Find(item.ToString());
            if (temp == null)
            {
                LogUtil.LogError("can not find layer:" + item);
                continue;
            }
            else
            {
                _layerDictionary.Add(item, temp);
            }
        }
    }

    public Transform GetLayerObject(UILayer layer)
    {
        Transform temp;
        _layerDictionary.TryGetValue(layer, out temp);
        if (temp == null)
        {
            LogUtil.LogError("_layerDictionary did not contains layer:" + layer);
        }
        return temp;
    }
    
}

