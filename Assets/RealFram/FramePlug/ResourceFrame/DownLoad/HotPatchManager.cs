using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class HotPatchManager : Singleton<HotPatchManager>
{

	StringBuilder stringBuilder = new StringBuilder();
	private static readonly object lock_sb = new object();
	
	private string m_CurVersion;
	public string CurVersion
	{
		get { return m_CurVersion; }
	}
	private string m_CurPackageName;
	//当前热更 patches
	private Pathces m_CurPatches;

	public Pathces CurPatches
	{
		get { return m_CurPatches; }
	}

	private MonoBehaviour m_Mono;
	private string m_UnPackPath = Application.persistentDataPath + "/Origin";
	private string m_DownLoadPath = Application.persistentDataPath + "/DownLoad";

	private string m_ServerXmlPath = Application.persistentDataPath + "/ServerInfo.xml";
	private string m_LocalXmlPath = Application.persistentDataPath + "/LocalInfo.xml";

	private ServerInfo m_ServerInfo;
	private ServerInfo m_LocalInfo;
	private VersionInfo m_GameVersion;
	
	//所有热歌的东西
	private Dictionary<string, Patch> m_HotFixDic = new Dictionary<string, Patch>();
	//所有需要下载的东西
	private List<Patch> m_DownLoadList = new List<Patch>();
	//所有需要下载的东西 dic
	private Dictionary<string, Patch> m_DownLoadDic = new Dictionary<string, Patch>();
	
	//服务器上的资源名对应的 md5  用于下载后md5校验
	private Dictionary<string, string> m_DownLoadMD5Dic = new Dictionary<string, string>();
	
	//计算需要解压的文件
	private List<string> m_UnPackedList = new List<string>();
	//原包记录的md5
	private Dictionary<string, ABMD5Base> m_PackedMD5 = new Dictionary<string, ABMD5Base>();
	
	//服务器列表获取错误回调
	public Action ServerInfoError;
	//文件下载出错回调
	public Action<string> ItemError;
	//下载完成回调
	public Action LoadOver;
	
	//文件解压出错回调
	public Action<string> UnPackError;
	//解压完成回调
	public Action UnPackOver;
	
	//储存已经下载的文件
	public List<Patch> m_AlreadyDownList = new List<Patch>();
	//是否已经开始下载
	public bool m_StartDownLoad = false;
	//尝试重新下载次数
	private int m_TryDownCount = 0;
	private const int DOWNLOADCOUNT = 4;
	//尝试重新解压次数
	private int m_TryUnPackCount = 0;
	private const int UNPACKCOUNT = 4;
	//当前正在下载资源
	private DownLoadAssetBundle m_CurDownLoad = null;

	//需要下载的资源总个数
	public int LoadFileCount { get; set; }
	//需要下载资源的总大小  KB
	public float LoadSumSize { get; set; }


	//是否开始解压
	public bool StartUnPack = false;
	//解压文件总大小
	public float UnPackSumSize { get; set; }
	//已解压大小
	public float AlreadyUnPackSize { get; set; }
	
	public void Init(MonoBehaviour mono)
	{
		m_Mono = mono;
		ReadMD5();
	}

	/// <summary>
	/// 读取 本地 资源 md5 码
	/// </summary>
	void ReadMD5()
	{
		m_PackedMD5.Clear();
		TextAsset md5 = Resources.Load<TextAsset>("ABMD5");
		if (md5 == null)
		{
			Debug.LogError("未读取到本地 MD5");
			return;
		}

		using (MemoryStream stream = new MemoryStream(md5.bytes))
		{
			BinaryFormatter bf = new BinaryFormatter();
			ABMD5 abmd5 = bf.Deserialize(stream) as ABMD5;
			foreach (ABMD5Base abmd5Base in abmd5.ABMD5List)
			{
				m_PackedMD5.Add(abmd5Base.Name, abmd5Base);
			}
		}
	}

	/// <summary>
	/// 获取解压进度
	/// </summary>
	/// <returns></returns>
	public float GetUnPackProgress()
	{
		return AlreadyUnPackSize / UnPackSumSize;
	}

	/// <summary>
	/// 开始解压文件
	/// </summary>
	/// <param name="callback"></param>
	public void StartUnPackFile(Action callback, List<string> allUnPack = null)
	{
		StartUnPack = true;
		m_Mono.StartCoroutine(UnPackToPersistentDataPath(callback, allUnPack));
	}

	/// <summary>
	/// 将包里的原始资源解压到本地
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	IEnumerator UnPackToPersistentDataPath(Action callback, List<string> allUnPack = null)
	{
		if (allUnPack == null)
		{
			allUnPack = m_UnPackedList;
		}

		List<string> tempUnPack = new List<string>();
		
		for (int i = 0; i < allUnPack.Count; i++)
		{
			string fileName = allUnPack[i];
			
			UnityWebRequest unityWebRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileName);
			unityWebRequest.timeout = 30;
			yield return unityWebRequest.SendWebRequest();
			if (unityWebRequest.isNetworkError)
			{
				Debug.LogError("UnPack Error!" + unityWebRequest.error);
			}
			else
			{
				byte[] bytes = unityWebRequest.downloadHandler.data;
				FileTool.CreateFile(m_UnPackPath + "/" + fileName, bytes);
				
				
				if (m_PackedMD5.ContainsKey(fileName))
				{
					AlreadyUnPackSize += m_PackedMD5[fileName].Size;
				}
			}

			tempUnPack.Add(fileName);
			unityWebRequest.Dispose();
		}

		VerifyMD5(tempUnPack, callback);
	}

	public void VerifyMD5(List<string> unPack, Action callback)
	{
		List<string> continueTryUnPack = new List<string>();
		for (int i = 0; i < unPack.Count; i++)
		{
			string fileName = unPack[i];
			string filePath = m_UnPackPath + "/" + fileName;
			if (File.Exists(filePath))
			{
				string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
				if (m_PackedMD5[fileName].MD5 != md5)
				{
					continueTryUnPack.Add(fileName);
				}
			}
			else
			{
				continueTryUnPack.Add(fileName);
			}
		}

		if (continueTryUnPack.Count <= 0)
		{
			if (callback != null)
			{
				callback();
			}

			StartUnPack = false;

			if (UnPackOver != null)
			{
				UnPackOver();
			}
		}
		else
		{
			if (m_TryUnPackCount >= UNPACKCOUNT)
			{
				string allName = "";
				StartUnPack = false;
			
				for (int i = 0; i < continueTryUnPack.Count; i++)
				{
					allName += continueTryUnPack[i] + ";";
				}

				if (UnPackError != null)
				{
					UnPackError(allName);
				}
				Debug.LogError("资源重复解压4次 MD5 校验都失败，请检查资源!" + allName);
			}
			else
			{
				m_TryUnPackCount++;
				//自动重新解压
				StartUnPackFile(callback, continueTryUnPack);
			}
		}
	}
	
	/// <summary>
	/// 计算需要解压的文件
	/// </summary>
	/// <returns></returns>
	public bool ComputeUnPackFile()
	{
#if UNITY_ANDROID
		if (!Directory.Exists(m_UnPackPath))
		{
			Directory.CreateDirectory(m_UnPackPath);
		}
		m_UnPackedList.Clear();

		foreach (string fileName in m_PackedMD5.Keys)
		{
			string filePath = m_UnPackPath + "/" + fileName;
			if (File.Exists(filePath))
			{
				string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
				if (m_PackedMD5[fileName].MD5 != md5)
				{
					m_UnPackedList.Add(fileName);
				}
			}
			else
			{
				m_UnPackedList.Add(fileName);
			}
		}

		for (int i = 0; i < m_UnPackedList.Count; i++)
		{
			if (m_PackedMD5.ContainsKey(m_UnPackedList[i]))
			{
				UnPackSumSize += m_PackedMD5[m_UnPackedList[i]].Size;
			}
		}
		return m_UnPackedList.Count > 0;
#else
		return false;
#endif
	}

	/// <summary>
	/// 计算ab包路径
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public string ComputeABPath(string name)
	{
		string str = "";
		Patch patch = null;
		m_HotFixDic.TryGetValue(name, out patch);
		
		if (patch != null)
		{
			lock (lock_sb)
			{
				stringBuilder.Clear();
				stringBuilder.Append(m_DownLoadPath);
				stringBuilder.Append("/");
				stringBuilder.Append(name);
				str = stringBuilder.ToString();
			}
		}
		return str;
	}
	
	public void CheckVersion(Action<bool> hotCallBack = null)
	{
		m_TryDownCount = 0;
		m_HotFixDic.Clear();
		ReadVersion();
		m_Mono.StartCoroutine(ReadXml(() =>
		{
			if (m_ServerInfo == null)
			{
				if (ServerInfoError != null)
				{
					ServerInfoError();
				}
				return;
			}

			foreach (VersionInfo versionInfo in m_ServerInfo.GameVersion)
			{
				if (versionInfo.Version == m_CurVersion)
				{
					m_GameVersion = versionInfo;
					break;
				}
			}

			GetHotAB();
			if (CheckLocalAndServerPatch())
			{
				ComputeDownLoad();
				if (File.Exists(m_ServerXmlPath))
				{
					if (File.Exists(m_LocalXmlPath))
					{
						File.Delete(m_LocalXmlPath);
					}

					File.Move(m_ServerXmlPath, m_LocalXmlPath);
				}
			}
			else
			{
				ComputeDownLoad();
			}
			
			LoadFileCount = m_DownLoadList.Count;
			LoadSumSize = m_DownLoadList.Sum(x => x.Size);
			
			
			if (hotCallBack != null)
			{
				hotCallBack(m_DownLoadList.Count > 0);
			}

		}));
	}


	/// <summary>
	/// 检查本地热更信息与服务器热更信息比较
	/// </summary>
	/// <returns></returns>
	bool CheckLocalAndServerPatch()
	{
		if (!File.Exists(m_LocalXmlPath))
			return true;

		m_LocalInfo = BinarySerializeOpt.XmlDeserialize(m_LocalXmlPath, typeof(ServerInfo)) as ServerInfo;

		VersionInfo localGameVersion = null;
		if (m_LocalInfo != null)
		{
			foreach (VersionInfo versionInfo in m_LocalInfo.GameVersion)
			{
				if (versionInfo.Version == m_CurVersion)
				{
					localGameVersion = versionInfo;
					break;
				}
			}
		}

		if (localGameVersion != null && m_GameVersion.Pathces != null && localGameVersion.Pathces != null &&
		    m_GameVersion.Pathces.Length > 0 &&
		    (m_GameVersion.Pathces[m_GameVersion.Pathces.Length - 1].Version !=
		     localGameVersion.Pathces[localGameVersion.Pathces.Length - 1].Version))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// 读取打包时的版本
	/// </summary>
	void ReadVersion()
	{
		TextAsset versionText = Resources.Load<TextAsset>("Version");

		if (versionText == null)
		{
			Debug.LogError("未读到本地版本!");
			return;
		}

		string[] all = versionText.text.Split('\n');
		if (all.Length > 0)
		{
			string[] infoList = all[0].Split(';');
			if (infoList.Length >= 2)
			{
				m_CurVersion = infoList[0].Split('|')[1];
				m_CurPackageName = infoList[1].Split('|')[1];
			}
		}
	}

	IEnumerator ReadXml(Action callBack)
	{
		string xmlUrl = "localhost:8085/ServerInfo.xml";
		UnityWebRequest webRequest = UnityWebRequest.Get(xmlUrl);
		webRequest.timeout = 30;

		yield return webRequest.SendWebRequest();

		if (webRequest.isNetworkError)
		{
			Debug.Log("Download Error!" + webRequest.error);
		}

		if (webRequest.error != null)
		{
			Debug.Log("Download Error! Cant Find ServerInfo.xml  in  Server! -> " + webRequest.error);
		}
		else
		{
			FileTool.CreateFile(m_ServerXmlPath, webRequest.downloadHandler.data);
			if (File.Exists(m_ServerXmlPath))
			{
				m_ServerInfo = BinarySerializeOpt.XmlDeserialize(m_ServerXmlPath, typeof(ServerInfo)) as ServerInfo;
			}
			else
			{
				Debug.LogError("热更配置读取错误！");
			}
		}

		if (callBack != null)
		{
			callBack();
		}
	}

	/// <summary>
	/// 获取所有热更包信息
	/// </summary>
	void GetHotAB()
	{
		if (m_GameVersion != null && m_GameVersion.Pathces != null && m_GameVersion.Pathces.Length > 0)
		{
			Pathces lastPatches = m_GameVersion.Pathces[m_GameVersion.Pathces.Length - 1];
			if (lastPatches != null && lastPatches.Files != null)
			{
				foreach (Patch patch in lastPatches.Files)
				{
					m_HotFixDic.Add(patch.Name, patch);
				}
			}
		}
	}

	/// <summary>
	/// 计算下载的资源
	/// </summary>
	void ComputeDownLoad()
	{
		m_DownLoadMD5Dic.Clear();
		m_DownLoadDic.Clear();
		m_DownLoadList.Clear();

		if (m_GameVersion != null && m_GameVersion.Pathces != null && m_GameVersion.Pathces.Length > 0)
		{
			m_CurPatches = m_GameVersion.Pathces[m_GameVersion.Pathces.Length - 1];
			if (m_CurPatches.Files != null && m_CurPatches.Files.Count > 0)
			{
				foreach (Patch patch in m_CurPatches.Files)
				{
					if ((Application.platform == RuntimePlatform.WindowsPlayer ||
					     Application.platform == RuntimePlatform.WindowsEditor) && patch.Platform.Contains("StandaloneWindows64"))
					{
						AddDownLoadList(patch);
					}
					
					else if ((Application.platform == RuntimePlatform.Android||
					          Application.platform == RuntimePlatform.WindowsEditor) && patch.Platform.Contains("Android"))
					{
						AddDownLoadList(patch);
					}
					else if ((Application.platform == RuntimePlatform.IPhonePlayer||
					          Application.platform == RuntimePlatform.WindowsEditor) && patch.Platform.Contains("IOS"))
					{
						AddDownLoadList(patch);
					}
				}
			}
		}
	}

	void AddDownLoadList(Patch patch)
	{
		string filePath = m_DownLoadPath + "/" + patch.Name;
		//存在这个文件时 进行对比看 是否与服务器 md5 码一样
		//不一样放下载队列 如果不存在直接放入下载队列
		if (File.Exists(filePath))
		{
			string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
			if (patch.Md5 != md5)
			{
				m_DownLoadList.Add(patch);
				m_DownLoadDic.Add(patch.Name, patch);
				m_DownLoadMD5Dic.Add(patch.Name,patch.Md5);
			}
		}
		else
		{
			m_DownLoadList.Add(patch);
			m_DownLoadDic.Add(patch.Name, patch);
			m_DownLoadMD5Dic.Add(patch.Name,patch.Md5);
		}
	}

	/// <summary>
	/// 获取下载总进度
	/// </summary>
	/// <returns></returns>
	public float GetProgress()
	{
		return GetLoadSize() / LoadSumSize;
	}

	/// <summary>
	/// 获取已经下载总大小
	/// </summary>
	/// <returns></returns>
	public float GetLoadSize()
	{
		float alreadySize = m_AlreadyDownList.Sum(x => x.Size);
		float curAlreadySize = 0;
		if (m_CurDownLoad != null)
		{
			Patch patch = FindPatchByGamePath(m_CurDownLoad.FileName);
			if (patch != null && !m_AlreadyDownList.Contains(patch))
			{
				curAlreadySize = m_CurDownLoad.GetProcess() * patch.Size;
			}
		}

		return alreadySize + curAlreadySize;
	}
	
	//开启携程开始 下载ab包
	public void StartIEDownLoadAB(Action callback, List<Patch> downloadList = null)
	{
		m_Mono.StartCoroutine(StartDownLoadAB(callback, downloadList));
	}

	/// <summary>
	/// 开始下载AB包
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	public IEnumerator StartDownLoadAB(Action callback, List<Patch> allPatch = null)
	{
		m_AlreadyDownList.Clear();
		m_StartDownLoad = true;

		if (allPatch == null)
		{
			allPatch = m_DownLoadList;
		}
		
		if (!Directory.Exists(m_DownLoadPath))
		{
			Directory.CreateDirectory(m_DownLoadPath);
		}

		List<DownLoadAssetBundle> downLoadAssetBundles = new List<DownLoadAssetBundle>();

		int allPatchlength = allPatch.Count;
		for (int i = 0; i < allPatchlength; i++)
		{
			downLoadAssetBundles.Add(new DownLoadAssetBundle(allPatch[i].Url, m_DownLoadPath));
		}
		int downLoadAssetBundleslength = downLoadAssetBundles.Count;
		for (int i = 0; i < downLoadAssetBundleslength; i++)
		{
			m_CurDownLoad = downLoadAssetBundles[i];
			yield return m_Mono.StartCoroutine(downLoadAssetBundles[i].DownLoad());
			Patch patch = FindPatchByGamePath(downLoadAssetBundles[i].FileName);
			if (patch != null)
			{
				m_AlreadyDownList.Add(patch);
			}
			downLoadAssetBundles[i].Destory();
		}
		
		//md5 码校验 如果校验没通过 自动重新下载没通过的文件  重复下载计数 达到一定次数后反馈
		//xx 文件下载失败

		VerifyMD5(downLoadAssetBundles, callback);
	}

	
	void VerifyMD5(List<DownLoadAssetBundle> downLoadAssetBundles, Action callback)
	{
		List<Patch> downloadList = new List<Patch>();
		foreach (DownLoadAssetBundle download in downLoadAssetBundles)
		{
			string md5 = "";
			if (m_DownLoadMD5Dic.TryGetValue(download.FileName, out md5))
			{
				if (MD5Manager.Instance.BuildFileMd5(download.SaveFilePath) != md5)
				{
					Debug.Log(string.Format("此文件{0}MD5校验失败！,即将重新下载！", download.FileName));
					Patch patch = FindPatchByGamePath(download.FileName);
					if (patch != null)
					{
						downloadList.Add(patch);
					}
				}
			}
		}

		if (downloadList.Count <= 0)
		{
			m_DownLoadMD5Dic.Clear();
			if (callback != null)
			{
				m_StartDownLoad = false;
				callback();
				if (LoadOver != null)
				{
					LoadOver();
				}
			}
		}
		else
		{
			if (m_TryDownCount >= DOWNLOADCOUNT)
			{
				string allName = "";
				m_StartDownLoad = false;
				foreach (Patch patch in downloadList)
				{
					allName += patch.Name + ";";
				}
				
				Debug.LogError("资源重复下载4次 MD5 校验都失败，请检查资源!" + allName);
				if (ItemError != null)
				{
					ItemError(allName);
				}
			}
			else
			{
				m_TryDownCount++;
				m_DownLoadMD5Dic.Clear();
				foreach (Patch patch in downloadList)
				{
					m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
				}
				//自动重新下载
				StartIEDownLoadAB(callback, downloadList);
			}
		}
	}

	/// <summary>
	/// 根据名字查找对象的热更 patch
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	Patch FindPatchByGamePath(string name)
	{
		Patch patch = null;
		m_DownLoadDic.TryGetValue(name, out patch);
		return patch;
	}
}

public class FileTool
{
	/// <summary>
	/// 创建文件
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="bytes"></param>
	public static void CreateFile(string filePath, byte[] bytes)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
		FileInfo file = new FileInfo(filePath);
		Stream stream = file.Create();
		stream.Write(bytes, 0, bytes.Length);
		stream.Close();
		stream.Dispose();
	}
}










