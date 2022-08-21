using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{

    /// <summary>
    /// 如果 想要让模型 在摄像机前面显示  z 轴调整到这个距离
    /// </summary>
    public readonly float MAX_FAR = -1f;

    private Camera m_UiCamera;

    public void Init(Camera uicamera)
    {
        m_UiCamera = uicamera;
    }


    /// <summary>
    /// 这个设置主要是为了 让模型可以显示在ui前面的需求
    /// </summary>
    public void SetUICameraDotClear()
    {
        m_UiCamera.clearFlags = CameraClearFlags.Nothing;
    }

    public void ResetUICameraClearFlags()
    {
        m_UiCamera.clearFlags = CameraClearFlags.Depth;
    }
}
