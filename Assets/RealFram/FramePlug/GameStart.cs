using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad(gameObject);
		
		ResourceManager.Instance.Init(this);
		
		ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
		
		//HotPatchManager.Instance.Init(this);
		
		RectTransform uiRoot = transform.Find("UIRoot") as RectTransform;
		RectTransform wndRoot = uiRoot.transform.Find("WndRoot") as RectTransform;
		Camera uiCamera = uiRoot.transform.Find("UICamera").GetComponent<Camera>();
		EventSystem eventSystem = uiRoot.transform.Find("EventSystem").GetComponent<EventSystem>();
		UIManager.Instance.Init(uiRoot, wndRoot, uiCamera, eventSystem);
		CameraManager.Instance.Init(uiCamera);
		
		UIManager.Instance.PopUpWnd(UiId.LoadingPanel, false, "Awake");
		
	}

	private void Start()
	{
		//UIManager.Instance.PopUpWnd(UiId.HOTFIXPANEL, Resource: true);
	}

	public IEnumerator StartGame(Image image, Text text)
	{
		image.fillAmount = 0;
		yield return null;
		text.text = "加载本地数据... ...";
		
		AssetBundleManager.Instance.LoadAssetBundleConfig();
		
		image.fillAmount = 0.1f;
		yield return null;
		text.text = "加载本地数据... ...";
		
		//ILRuntimeManager.Instance.Init();
		
		image.fillAmount = 0.2f;
		yield return null;
		text.text = "加载数据表... ...";

		LoadConfiger();
		
		image.fillAmount = 0.6f;
		yield return null;
		text.text = "加载配置... ...";
		
		
		image.fillAmount = 0.9f;
		yield return null;
		text.text = "初始化地图... ...";
		
		GameMapManager.Instance.Init(this);
		image.fillAmount = 1.0f;
		yield return null;
	}
	

	private void LoadConfiger()
	{
//		ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
	}


	private void Update()
	{
		UIManager.Instance.OnUpdate();
	}

//	public static void OpenCommonConfirm(string title, string des, UnityAction sureAction, UnityAction canleAction)
//	{
//		GameObject commonObj = GameObject.Instantiate(Resources.Load<GameObject>("CommonConfirm"));
//		commonObj.transform.SetParent(UIManager.Instance.m_WndRoot, false);
//		CommonConfirm commonConfirm = commonObj.GetComponent<CommonConfirm>();
//		commonConfirm.Show(title, des, sureAction, canleAction);
//	}

	private void OnApplicationQuit()
	{
#if UNITY_EDITOR
		ResourceManager.Instance.ClearResourceManager();
		AssetBundleManager.Instance.ClearAssetBundleManager();
		Resources.UnloadUnusedAssets();
		LogUtil.Log("清空编辑器缓存");
#endif
	}
}
