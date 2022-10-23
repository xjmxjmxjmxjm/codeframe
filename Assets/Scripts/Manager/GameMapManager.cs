using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMapManager : Singleton<GameMapManager> {

	//加载场景完成回调
	public Action LoadSceneOverCallBack;
	//加载场景开始回调
	public Action LoadSceneEnterCallBack;
	
	
	//当前场景名字
	private string CurrentMapName { get; set; }
	
	//场景加载是否完成
	public bool AlreadyLoadScene { get; set; }
	
	//切换场景进度条
	public static int LoadingProgress = 0;


	private WaitForEndOfFrame _waitEndOfFrame;


	private MonoBehaviour m_Mono;
	public void Init(MonoBehaviour mono)
	{
		m_Mono = mono;
		_waitEndOfFrame = new WaitForEndOfFrame();
	}
	
	/// <summary>
	/// 场景切换接口
	/// </summary>
	/// <param name="name"></param>
	public void LoadScene(string name)
	{
		LoadingProgress = 0;

		m_Mono.StartCoroutine(LoadSceneAsync(name));

		UIManager.Instance.PopUpWnd(UiId.LoadingPanel, false, name);
	}

	/// <summary>
	/// 设置场景环境
	/// </summary>
	/// <param name="name"></param>
	void SetSceneSetting(string name)
	{
		//设置各种场景环境  可以根据配表来
	}

	private IEnumerator LoadSceneAsync(string name)
	{
		if (LoadSceneEnterCallBack != null)
		{
			LoadSceneEnterCallBack();
			LoadSceneEnterCallBack = null;
		}
		ClearCache();
		AlreadyLoadScene = false;
		AsyncOperation unLoadScene = SceneManager.LoadSceneAsync(SceneStr.EMPTYSCENE, LoadSceneMode.Single);
		while (unLoadScene != null && !unLoadScene.isDone) 
		{
			yield return _waitEndOfFrame;
		}

		LoadingProgress = 0;
		int targetProgress = 0;
		AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
		if (asyncScene != null && !asyncScene.isDone)
		{
			asyncScene.allowSceneActivation = false;
			while (asyncScene.progress < 0.9f)
			{
				targetProgress = (int) (asyncScene.progress * 100);
				yield return _waitEndOfFrame;
				//平滑过渡
				while (LoadingProgress < targetProgress)
				{
					++LoadingProgress;
					yield return _waitEndOfFrame;
				}
			}

			CurrentMapName = name;
			SetSceneSetting(name);
			//自行加载剩余的 10% 
			targetProgress = 100;
			while (LoadingProgress < targetProgress - 1)
			{
				++LoadingProgress;
				yield return _waitEndOfFrame;
			}

			LoadingProgress = 100;
			asyncScene.allowSceneActivation = true;
			AlreadyLoadScene = true;
			if (LoadSceneOverCallBack != null)
			{
				LoadSceneOverCallBack();
				LoadSceneOverCallBack = null;
			}
		}
		
		yield return null;
	}

	/// <summary>
	/// 跳场景需要清除的东西
	/// </summary>
	private void ClearCache()
	{
		ObjectManager.Instance.ClearCache();
		ResourceManager.Instance.ClearCache();
	}
}
















