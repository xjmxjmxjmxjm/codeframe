using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BaseItem : MonoBehaviour
{

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
