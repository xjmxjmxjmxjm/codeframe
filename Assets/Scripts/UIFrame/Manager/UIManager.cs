using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;

public enum UIMessageId
{
	NONE = 0,
}

public class UIManager : Singleton<UIManager>
{
	//UI节点
	private RectTransform m_UiRoot;
	//窗口节点
	public RectTransform m_WndRoot;
	//ui摄像机
	private Camera m_UiCamera;
	//eventSystem
	private EventSystem m_EventSystem;
	//屏幕的宽高比
	private float m_CanvasRate = 0;
	

	private string m_UIPrefabPath = "Assets/GameData/Prefabs/UGUI/Panel/";

	//注册的字典
	//private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, Type>();
	
	//所有打开的窗口
	private Dictionary<string, UIBase> m_UIBaseDic = new Dictionary<string, UIBase>();

	private UILayerManager m_UiLayerManager;

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="uiRoot"></param>
	/// <param name="wndRoot"></param>
	/// <param name="uiCamera"></param>
	public void Init(RectTransform uiRoot, RectTransform wndRoot, Camera uiCamera, EventSystem eventSystem)
	{
		m_UiRoot = uiRoot;
		m_WndRoot = wndRoot;
		m_UiCamera = uiCamera;
		m_EventSystem = eventSystem;
		m_CanvasRate = Screen.height / (m_UiCamera.orthographicSize * 2);
		
		m_UiLayerManager = new UILayerManager();
		m_UiLayerManager.Init(wndRoot.transform);
	}

	/// <summary>
	/// 设置所有界面UI路径
	/// </summary>
	/// <param name="path"></param>
	public void SetUIPrefabPath(string path)
	{
		m_UIPrefabPath = path;
	}

	/// <summary>
	/// 显示或者隐藏所有UI
	/// </summary>
	public void ShowOrHideUI(bool show)
	{
		if (m_UiRoot != null)
		{
			m_UiRoot.gameObject.SetActive(show);
		}
	}

	/// <summary>
	/// 设置默认选择对象
	/// </summary>
	/// <param name="obj"></param>
	public void SetNormalSelectObj(GameObject obj)
	{
		if (m_EventSystem == null)
		{
			m_EventSystem = EventSystem.current;
		}

		m_EventSystem.firstSelectedGameObject = obj;
	}

	public void OnUpdate()
	{
		List<UIBase> st = m_UIBaseDic.Values.ToList();
		for (int i = 0; i < st.Count; i++)
		{
			UIBase wnd = st[i];
			if (wnd != null)
			{
				if (wnd.IsHotFix)
				{
					
				}
				else
				{
					wnd.OnUpdate();
				}
			}
		}
	}

//	/// <summary>
//	/// 窗口注册
//	/// </summary>
//	/// <param name="name"></param>
//	/// <typeparam name="T">窗口泛型类</typeparam>
//	public void Register<T>(string name) where T : UIBase
//	{
//		m_RegisterDic[name] = typeof(T);
//	}


	/// <summary>
	/// 发送消息给窗口
	/// </summary>
	/// <param name="name"></param>
	/// <param name="id"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public bool SendMessageToWnd(string name, UIMessageId id = 0, object param1 = null, object param2 = null, object param3 = null)
	{
		UIBase wnd = FindWndByName<UIBase>(name);
		if (wnd != null)
		{
			return wnd.OnMessage(id, param1, param2, param3);
		}

		return false;
	}

	/// <summary>
	/// 根据窗口名字找到窗口
	/// </summary>
	/// <param name="name"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T FindWndByName<T>(string name) where T : UIBase
	{
		UIBase wnd = null;
		m_UIBaseDic.TryGetValue(name, out wnd);
		if (wnd == null) return null;
		return (T) wnd;
	}

	public UIBase PopUpWnd(UiId wndName, UILayer uilayer, bool Resource = false, object param1 = null, object param2 = null, object param3 = null)
	{
		return PopUpWnd(wndName.ToString(), uilayer, Resource, param1, param2, param3);
	}

	public UIBase PopUpWnd(string wndName, UILayer uilayer, bool Resource = false, object param1 = null, object param2 = null, object param3 = null)
	{
		UIBase wnd = FindWndByName<UIBase>(wndName);
		if (wnd == null)
		{
			GameObject wndObj = null;
			if (Resource)
			{
				wndObj = GameObject.Instantiate(Resources.Load<GameObject>(wndName/*.Replace(".prefab", "")*/));
			}
			else
			{
				string path = ToolUtil.GetMergeStr(m_UIPrefabPath, wndName, ".prefab");
				wndObj = ObjectManager.Instance.InstantiateObject(path, false, false);
			}
			if (wndObj == null)
			{
				LogUtil.Log("create UIBase prefab error! -> " + wndName);
				return null;
			}

			// type  得到 需要绑定到这个预制体上的  mono脚本
			Type type = ToolUtil.GetType(wndName);
			if (type == null)
			{
				return null;
			}
			wndObj.AddComponent(type);
			
			//wnd 得到他的逻辑类  wndName + "UI"
			string wndUi = ToolUtil.GetMergeStr(ToolUtil.GetReultRemoveStr(wndName,"Panel"), "UI");
			
			type = Type.GetType(wndUi);      					// 通过类名获取同名类
			if (type == null)
			{
				return null;
			}
			wnd = System.Activator.CreateInstance(type) as UIBase;       // 创建实例
			
			if (!m_UIBaseDic.ContainsKey(wndName))
			{
				m_UIBaseDic.Add(wndName, wnd);
			}
#if UNITY_EDITOR
			wndObj.name = wndName.Replace(".prefab", "");
#endif

			wnd.GameObject = wndObj;
			wnd.Transform = wndObj.transform;
			wnd.Name = wndName;
			wnd.Resource = Resource;
			if (wnd.IsHotFix)
			{
				
			}
			else
			{
				wnd.Awake(param1, param2, param3);
			}

			Transform parent = m_UiLayerManager.GetLayerObject(uilayer);
			wndObj.transform.SetParent(parent, false);

			
			if (wnd.IsHotFix)
			{
				
			}
			else
			{
				wnd.OnShow(param1, param2, param3);
			}
		}
		else
		{
			ShowWnd(wndName, param1, param2, param3);
		}

		return wnd;
	}


	
	

	/// <summary>
	/// 关闭窗口
	/// </summary>
	/// <param name="name"></param>
	/// <param name="destory"></param>
	public void CloseWnd(string name, bool destory = false)
	{
		UIBase wnd = FindWndByName<UIBase>(name);
		CloseWnd(wnd, destory);
	}

	
	/// <summary>
	/// 删除某个层级下的所有 ui
	/// </summary>
	/// <param name="layer"></param>
	public void CloseLayerAllUIBase(UILayer layer)
	{
		
	}

	/// <summary>
	/// 根据窗口对象关闭窗口
	/// </summary>
	/// <param name="wnd"></param>
	/// <param name="destory"></param>
	public void CloseWnd(UIBase wnd, bool destory = false)
	{
		if (wnd != null)
		{
			if (wnd.IsHotFix)
			{
				
			}
			else
			{
				wnd.onDisable();
				wnd.OnClose();
			}

			if (m_UIBaseDic.ContainsKey(wnd.Name))
			{
				m_UIBaseDic.Remove(wnd.Name);
			}

			if (!wnd.Resource)
			{
				if (destory)
				{
					ObjectManager.Instance.ReleaseObject(wnd.GameObject, 0, true);
				}
				else
				{
					ObjectManager.Instance.ReleaseObject(wnd.GameObject, recycleParent: false);
				}
			}
			else
			{
				GameObject.Destroy(wnd.GameObject);
			}
			

			wnd.GameObject = null;
			wnd.Transform = null;
			wnd.Name = null;
			wnd = null;
		}
	}

	/// <summary>
	/// 关闭所有窗口
	/// </summary>
	public void CloseAllWnd()
	{
		UIBase[] lst = m_UIBaseDic.Values.ToArray();
		int length = lst.Length;
		for (int i = length - 1; i >= 0; i--)
		{
			CloseWnd(lst[i]);
		}
	}

	/// <summary>
	/// 隐藏窗口
	/// </summary>
	/// <param name="name"></param>
	public void HideWnd(string name)
	{
		UIBase wnd = FindWndByName<UIBase>(name);
		HideWnd(wnd);
	}

	public void HideWnd(UIBase wnd)
	{
		if (wnd != null)
		{
			wnd.GameObject.SetActive(false);
			if (wnd.IsHotFix)
			{
				
			}
			else
			{
				wnd.onDisable();
			}
		}
	}

	/// <summary>
	/// 根据窗口名字显示窗口
	/// </summary>
	/// <param name="name"></param>
	/// <param name="args"></param>
	public void ShowWnd(string name, object param1 = null, object param2 = null, object param3 = null)
	{
		UIBase wnd = FindWndByName<UIBase>(name);
		ShowWnd(wnd, param1, param2, param3);
	}

	/// <summary>
	/// 根据窗口对象显示窗口
	/// </summary>
	/// <param name="wnd"></param>
	/// <param name="args"></param>
	public void ShowWnd(UIBase wnd, object param1 = null, object param2 = null, object param3 = null)
	{
		if (wnd != null)
		{
			if (wnd.GameObject != null && !wnd.GameObject.activeSelf)
			{
				wnd.GameObject.SetActive(true);
				
				if (wnd.IsHotFix)
				{
					
				}
				else
				{
					wnd.OnShow(param1, param2, param3);
				}
			}
		}
	}
	
	
	
	

}

















