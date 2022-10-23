using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Module.Timer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Util;

public class GameStart : MonoSingleton<GameStart>
{
	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad(gameObject);

		StartLogMessage();
		
		ResourceManager.Instance.Init(this);
		
		ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
		
		//HotPatchManager.Instance.Init(this);
		
		RectTransform uiRoot = transform.Find("UIRoot") as RectTransform;
		RectTransform wndRoot = uiRoot.transform.Find("WndRoot") as RectTransform;
		Camera uiCamera = uiRoot.transform.Find("UICamera").GetComponent<Camera>();
		EventSystem eventSystem = uiRoot.transform.Find("EventSystem").GetComponent<EventSystem>();
		UIManager.Instance.Init(uiRoot, wndRoot, uiCamera, eventSystem);
		CameraManager.Instance.Init(uiCamera);
		
		//UIManager.Instance.PopUpWnd(UiId.LoadingPanel, false, "Awake");


		//NetManager.Instance.Connect("127.0.0.1", 8888);
		//StartCoroutine(NetManager.Instance.CheckNet());

		//AssetBundleManager.Instance.LoadAssetBundleConfig();

		//ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/FPX/Attack.prefab", true);
//
//		GameObject go = new GameObject();
//		Destroy(go);
//		go = null;
//		LogUtil.Log(go.name);
	}

	private void StartLogMessage()
	{
		Application.logMessageReceived += OnLogCallBack;
	}

	private void OnLogCallBack(string condition, string stackTrace, LogType type)
	{
		LogUtil.WriteLogPath(condition, stackTrace, type);
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
		NetManager.Instance.Update();
	}


	public void UpLoadLog(string pathFileName, string str)
	{
		StartCoroutine(HttpPost("http://192.168.1.9:8888", pathFileName, Encoding.UTF8.GetBytes(str)));
	}
	
	IEnumerator HttpPost(string url, string fileName, byte[] data)
	{
		WWWForm form = new WWWForm();
        

		form.AddBinaryData(fileName, data);
		form.AddField("name", fileName, Encoding.UTF8);
		form.AddField("type", 2);
		
		UnityWebRequest request = UnityWebRequest.Post(url, form);
		var result = request.SendWebRequest();
		while (!result.isDone)
		{
			yield return null;
		}

		if (!string.IsNullOrEmpty(request.error))
		{
			LogUtil.LogError("up error:" + request.error);
		}
		else
		{
			LogUtil.Log("日志上传完毕，服务器返回信息：" + request.downloadHandler.text);
		}
        
		request.Dispose();
	}

	void SendCallBack(IAsyncResult ar)
	{
		Socket socket = (Socket)ar.AsyncState;
		if (socket == null || !socket.Connected) return;
		int count = socket.EndSend(ar);
		Debug.Log(count);
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
