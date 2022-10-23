using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public abstract class PanelBase
{

	//引用 GameObject
	public GameObject GameObject { get; set; }
	
	//引用 Transform
	public Transform Transform { get; set; }
	
	//名字
	public string Name { get; set; }

	//是否从resource加载
	public bool Resource { get; set; } = false;

	public bool IsHotFix { get; set; } = false;
	
	//public string HotFixClassName { get; set; } = string.Empty;
	
	//所有的 button
	protected List<Button> m_AllButton = new List<Button>();
	
	//所有的 toggle
	protected List<Toggle> m_AllToggle = new List<Toggle>();


	public virtual bool OnMessage(UIMessageId id, params object[] args)
	{
		return true;
	}


	public virtual void Awake(object param1 = null, object param2 = null, object param3 = null){}
	
	public virtual void OnShow(object param1 = null, object param2 = null, object param3 = null){}
	
	public virtual void onDisable(){}
	
	public virtual void OnUpdate(){}

	public virtual void OnClose()
	{
		RemoveAllButtonListener();
		RemoveAllToggleListener();
		m_AllButton.Clear();
		m_AllToggle.Clear();
	}

	/// <summary>
	/// 同步替换图片
	/// </summary>
	/// <param name="path"></param>
	/// <param name="image"></param>
	/// <param name="setNativeSize"></param>
	/// <returns></returns>
	public bool SetImageSprite(string path, Image image, bool setNativeSize = false)
	{
		if (image == null) return false;

		Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
		if (sp != null)
		{
			image.sprite = sp;
			if (setNativeSize)
			{
				image.SetNativeSize();
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// 异步加载回调
	/// </summary>
	/// <param name="path"></param>
	/// <param name="image"></param>
	/// <param name="setNativeSize"></param>
	/// <param name="action">传回的 object 是 这个 image 组件</param>
	public void SetImageSpriteAsync(string path, Image image, bool setNativeSize = false)
	{
		if (image == null) return;

		ResourceManager.Instance.AsyncLoadResource(path, OnLoadSpriteFinish, LoadResPriority.RES_SLOW, true, 0,
			image, setNativeSize);
	}

	private void OnLoadSpriteFinish(string path, UnityEngine.Object obj, object param1, object param2, object param3)
	{
		if (obj != null)
		{
			Sprite sp = obj as Sprite;
			Image image = param1 as Image;
			bool setNativeSize = (bool)param2;
			
			image.sprite = sp;
			if (setNativeSize)
			{
				image.SetNativeSize();
			}
		}
	}
	
	
	public void SetActive(bool isActive)
	{
		if (GameObject.activeSelf == isActive)
		{
            
		}
		else
		{
			GameObject.SetActive(isActive);
		}
	}
	public void SetText(Text text ,string context = "")
	{
		if (text.text == context) return;
		text.text = context;
	}
	
	
	/// <summary>
	/// 得到 预制体 的唯一
	/// </summary>
	/// <returns></returns>
	public abstract UiId GetUiId();

	public abstract UILayer GetUiLayer();
	

	/// <summary>
	/// 移除所有btn事件
	/// </summary>
	private void RemoveAllButtonListener()
	{
		int length = m_AllButton.Count;
		for (int i = 0; i < length; i++)
		{
			Button btn = m_AllButton[i];
			if (btn == null) continue;
			btn.onClick.RemoveAllListeners();
		}
	}
	/// <summary>
	/// 移除所有toggle事件
	/// </summary>
	private void RemoveAllToggleListener()
	{
		int length = m_AllToggle.Count;
		for (int i = 0; i < length; i++)
		{
			Toggle toggle = m_AllToggle[i];
			if (toggle == null) continue;
			toggle.onValueChanged.RemoveAllListeners();
		}
	}

	/// <summary>
	/// 添加 button 事件监听
	/// </summary>
	/// <param name="btn"></param>
	/// <param name="action"></param>
	/// <param name="type"></param>
	public void AddButtonClickListener(Button btn, UnityAction action, PressBtnPlaySound type = PressBtnPlaySound.NONE)
	{
		if (btn != null)
		{
			if (!m_AllButton.Contains(btn))
			{
				m_AllButton.Add(btn);
			}
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(action);
			btn.onClick.AddListener(delegate { BtnPlaySound(type); });
		}
	}

	/// <summary>
	/// toggle 事件监听
	/// </summary>
	/// <param name="toggle"></param>
	/// <param name="action"></param>
	/// <param name="type"></param>
	public void AddToggleClickListener(Toggle toggle, UnityAction<bool> action, PressTogglePlaySound type = PressTogglePlaySound.NONE)
	{
		if (toggle != null)
		{
			if (!m_AllToggle.Contains(toggle))
			{
				m_AllToggle.Add(toggle);
			}
			toggle.onValueChanged.RemoveAllListeners();
			toggle.onValueChanged.AddListener(action);
			toggle.onValueChanged.AddListener(delegate { TogglePlaySound(type); });
		}
	}

	/// <summary>
	/// 播放  btn  声音
	/// </summary>
	/// <param name="type"></param>
	private void BtnPlaySound(PressBtnPlaySound type)
	{
		switch (type)
		{
			case PressBtnPlaySound.NONE:
				
				break;
		}
	}
	/// <summary>
	/// 播放  toggle  声音
	/// </summary>
	/// <param name="type"></param>
	private void TogglePlaySound(PressTogglePlaySound type)
	{
		switch (type)
		{
			case PressTogglePlaySound.NONE:
				
				break;
		}
	}
	
}
