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
	private string m_ItemPrefabPath = "Assets/GameData/Prefabs/UGUI/Item/";

	//注册的字典
	//private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, Type>();
	
	//所有打开的窗口
	private Dictionary<string, PanelBase> m_PanelBaseDic = new Dictionary<string, PanelBase>();
	
	/// <summary>
	/// 所有的 item节点
	/// </summary>
	private Dictionary<int, ItemBase> m_ItemBaseDic = new Dictionary<int, ItemBase>();
	

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
		List<PanelBase> st = m_PanelBaseDic.Values.ToList();
		for (int i = 0; i < st.Count; i++)
		{
			PanelBase wnd = st[i];
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
		
		List<ItemBase> ib = m_ItemBaseDic.Values.ToList();
		for (int i = 0; i < ib.Count; i++)
		{
			ItemBase item = ib[i];
			if (item != null)
			{
				item.OnUpdate();
			}
		}
	}

//	/// <summary>
//	/// 窗口注册
//	/// </summary>
//	/// <param name="name"></param>
//	/// <typeparam name="T">窗口泛型类</typeparam>
//	public void Register<T>(string name) where T : PanelBase
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
		PanelBase wnd = FindWndByName<PanelBase>(name);
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
	public T FindWndByName<T>(string name) where T : PanelBase
	{
		PanelBase wnd = null;
		m_PanelBaseDic.TryGetValue(name, out wnd);
		if (wnd == null) return null;
		return (T) wnd;
	}

	/// <summary>
	/// 界面 panel
	/// </summary>
	/// <param name="wndName"></param>
	/// <param name="uilayer"></param>
	/// <param name="Resource"></param>
	/// <param name="param1"></param>
	/// <param name="param2"></param>
	/// <param name="param3"></param>
	/// <returns></returns>
	public PanelBase PopUpWnd(UiId wndName, bool Resource = false, object param1 = null, object param2 = null, object param3 = null)
	{
		return PopUpWnd(wndName.ToString(), Resource, param1, param2, param3);
	}
	public PanelBase PopUpWnd(string wndName, bool Resource = false, object param1 = null, object param2 = null, object param3 = null)
	{
		PanelBase wnd = FindWndByName<PanelBase>(wndName);
		if (wnd == null)
		{
			GameObject wndObj = null;
			if (Resource)
			{
				wndObj = GameObject.Instantiate(Resources.Load<GameObject>(wndName/*.Replace(".prefab", "")*/));
			}
			else
			{
				string path = StringUtil.GetMergeStr(m_UIPrefabPath, wndName, ".prefab");
				wndObj = ObjectManager.Instance.InstantiateObject(path, false, false);
			}
			if (wndObj == null)
			{
				LogUtil.Log("create PanelBase prefab error! -> " + wndName);
				return null;
			}

			// type  得到 需要绑定到这个预制体上的  mono脚本
			Type type = ReflectUtil.GetTypeInGame(wndName);
			if (type == null)
			{
				return null;
			}
			wndObj.AddComponent(type);
			
			//wnd 得到他的逻辑类  wndName + "UI"
			string wndUi = StringUtil.GetMergeStr(StringUtil.GetReultRemoveStr(wndName,"Panel"), "UI");
			
			type = ReflectUtil.GetTypeInGame(wndUi);      					// 通过类名获取同名类
			if (type == null)
			{
				return null;
			}
			wnd = System.Activator.CreateInstance(type) as PanelBase;       // 创建实例
			
			
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

			Transform parent = m_UiLayerManager.GetLayerObject(wnd.GetUiLayer());
			wndObj.transform.SetParent(parent, false);

			
			if (wnd.IsHotFix)
			{
				
			}
			else
			{
				wnd.OnShow(param1, param2, param3);
			}
			
			if (!m_PanelBaseDic.ContainsKey(wndName))
			{
				m_PanelBaseDic.Add(wndName, wnd);
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
		PanelBase wnd = FindWndByName<PanelBase>(name);
		CloseWnd(wnd, destory);
	}

	
	/// <summary>
	/// 删除某个层级下的所有 ui
	/// </summary>
	/// <param name="layer"></param>
	public void CloseLayerAllPanelBase(UILayer layer)
	{
		
	}

	/// <summary>
	/// 根据窗口对象关闭窗口
	/// </summary>
	/// <param name="wnd"></param>
	/// <param name="destory"></param>
	public void CloseWnd(PanelBase wnd, bool destory = true)
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

			if (m_PanelBaseDic.ContainsKey(wnd.Name))
			{
				m_PanelBaseDic.Remove(wnd.Name);
			}

			if (!wnd.Resource)
			{
//				if (destory)
//				{
//					ObjectManager.Instance.ReleaseObject(wnd.GameObject, 0, true);
//				}
//				else
//				{
//					ObjectManager.Instance.ReleaseObject(wnd.GameObject, recycleParent: false);
//				}
				ObjectManager.Instance.ReleaseObject(wnd.GameObject, 0, true);
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
		PanelBase[] lst = m_PanelBaseDic.Values.ToArray();
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
		PanelBase wnd = FindWndByName<PanelBase>(name);
		HideWnd(wnd);
	}

	public void HideWnd(PanelBase wnd)
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
		PanelBase wnd = FindWndByName<PanelBase>(name);
		ShowWnd(wnd, param1, param2, param3);
	}

	/// <summary>
	/// 根据窗口对象显示窗口
	/// </summary>
	/// <param name="wnd"></param>
	/// <param name="args"></param>
	public void ShowWnd(PanelBase wnd, object param1 = null, object param2 = null, object param3 = null)
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



	public ItemBase GetItem(ItemId itemName, Transform parent, object param1 = null, object param2 = null, object param3 = null)
	{
		return GetItem(itemName.ToString(), parent, param1, param2, param3);
	}
	public ItemBase GetItem(string itemName, Transform parent, object param1 = null, object param2 = null, object param3 = null)
	{
		GameObject itemObj = null;
		string path = StringUtil.GetMergeStr(m_ItemPrefabPath, itemName, ".prefab");
		itemObj = ObjectManager.Instance.InstantiateObject(path, false, false);
		if (itemObj == null)
		{
			LogUtil.Log("create ItemBase prefab error! -> " + itemName);
			return null;
		}

		// type  得到 需要绑定到这个预制体上的  mono脚本
		Type type = ReflectUtil.GetTypeInGame(itemName);
		if (type == null)
		{
			return null;
		}
		itemObj.AddComponent(type);
			
		//wnd 得到他的逻辑类  wndName + "UI"
		string itemUi = StringUtil.GetMergeStr(itemName, "UI");
			
		type = ReflectUtil.GetTypeInGame(itemUi);      					// 通过类名获取同名类
		if (type == null)
		{
			return null;
		}
		ItemBase item = System.Activator.CreateInstance(type) as ItemBase;       // 创建实例
			
#if UNITY_EDITOR
		itemObj.name = itemName.Replace(".prefab", "");
#endif

		item.GameObject = itemObj;
		item.Transform = itemObj.transform;
		item.Name = itemName;
		
		item.Awake(param1, param2, param3);

		itemObj.transform.SetParent(parent, false);

			
		item.OnShow(param1, param2, param3);

		if (!m_ItemBaseDic.ContainsKey(itemObj.GetInstanceID()))
		{
			m_ItemBaseDic.Add(itemObj.GetInstanceID(), item);
		}
		return item;
	}
	
	/// <summary>
	/// 一般item 
	/// </summary>
	/// <param name="wnd"></param>
	/// <param name="destory"></param>
	public void DestroyItem(ItemBase itemObj, bool destory = true)
	{
		itemObj.onDisable();
		itemObj.OnClose();

		if (m_ItemBaseDic.ContainsKey(itemObj.GameObject.GetInstanceID()))
		{
			m_ItemBaseDic.Remove(itemObj.GameObject.GetInstanceID());
		}
		ObjectManager.Instance.ReleaseObject(itemObj.GameObject, 0, true);
		
//		if (destory)
//		{
//			ObjectManager.Instance.ReleaseObject(itemObj.GameObject, 0, true);
//		}
//		else
//		{
//			ObjectManager.Instance.ReleaseObject(itemObj.GameObject, recycleParent: false);
//		}
		
		
			

		itemObj.GameObject = null;
		itemObj.Transform = null;
		itemObj.Name = null;
		itemObj = null;
	}
}

















