using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
public class AssetBundleManager : Singleton<AssetBundleManager>
{
	
		
#if UNITY_EDITOR
	public void ClearAssetBundleManager()
	{
		foreach (var item in m_ResourceItemDic.Values)
		{
			if (item.m_Obj != null)
			{
				if(item.m_Obj as GameObject)continue;
				Resources.UnloadAsset(item.m_Obj);
			}
		}
	}
#endif

	protected string m_ABConfigABName = "assetbundleconfig";
	
	//资源关系依赖配表   可以根据 crc 来找到对应的资源块
	public Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();

	//储存已经加载的  ab pack
	protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
	
	
	//AssetBundleItem 类对象池
	protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool =
		ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

	protected string ABLoadPath
	{
#if UNITY_ANDROID
		get { return Application.persistentDataPath + "/Origin/"; } 
#else
		get { return Application.streamingAssetsPath + "/AB/"; } 
#endif
	}
	
	
	/// <summary>
	/// 加载ab
	/// </summary>
	/// <returns></returns>
	public bool LoadAssetBundleConfig()
	{
#if UNITY_EDITOR
		if (ResourceManager.Instance.m_LoadFromAssetBundle == false) return false;
#endif
		
		m_ResourceItemDic.Clear();
		
		
		//string configPath = ABLoadPath + m_ABConfigABName;
		string hotABPath = HotPatchManager.Instance.ComputeABPath(m_ABConfigABName);
		string configPath = string.IsNullOrEmpty(hotABPath) ? ABLoadPath + m_ABConfigABName : hotABPath;
		byte[] bytes = AES.AESFileByteDecrypt(configPath, "xjm");
		AssetBundle configAB = AssetBundle.LoadFromMemory(bytes);

		TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");

		if (textAsset == null)
		{
			Debug.LogError("assetbundleconfig is no exist!");
			return false;
		}
		MemoryStream ms = new MemoryStream(textAsset.bytes);
		BinaryFormatter bf = new BinaryFormatter();
		AssetBundleConfig serilize = (AssetBundleConfig)bf.Deserialize(ms);
		ms.Close();

		List<ABBase> abBases = serilize.m_oAllABBase;
		int length = abBases.Count;
		for (int i = 0; i < length; i++)
		{
			ABBase abBase = abBases[i];
			ResourceItem item = new ResourceItem();
			item.m_Crc = abBase.Crc;
			item.m_AssetName = abBase.AssetName;
			item.m_ABName = abBase.ABName;
			item.m_DependceAssetBundle = abBase.ABDependce;
			if (m_ResourceItemDic.ContainsKey(item.m_Crc))
			{
				Debug.LogError("resource ab pack   crc repeat error ! assetName->" + item.m_AssetName +
				               "   abname -> " + item.m_ABName);
			}
			else
			{
				m_ResourceItemDic.Add(item.m_Crc, item);
			}
		}
		
		return true;
	}

	/// <summary>
	/// 根据路径  crc  加载中间类  ResourceItem
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public ResourceItem LoadResourceAssetBundle(uint crc)
	{
		ResourceItem item = null;
		if (!m_ResourceItemDic.TryGetValue(crc, out item) || item == null)
		{
			Debug.LogError(string.Format("LoadResourceAssetBundle error : can not find crc {0} in AssetBundleConfig",
				crc.ToString()));
			return item;
		}

		/*if (item.m_AssetBundle != null)
		{
			return item;
		}*/


		if (item.m_DependceAssetBundle != null)
		{
			for (int i = 0; i < item.m_DependceAssetBundle.Count; i++)
			{
				LoadAssetBundle(item.m_DependceAssetBundle[i]);
			}
		}

		item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
		
		return item;
	}

	
	/// <summary>
	/// 加载单个  assetbundle  根据名字
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private AssetBundle LoadAssetBundle(string name)
	{
		AssetBundleItem item = null;
		uint crc = Crc32.GetCrc32(name);

		if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
		{
			AssetBundle assetBundle = null;
			string hotABPath = HotPatchManager.Instance.ComputeABPath(name);
			string fullPath = string.IsNullOrEmpty(hotABPath) ? ABLoadPath + name : hotABPath;
			//if (File.Exists(fullPath))
			//{
			byte[] bytes = AES.AESFileByteDecrypt(fullPath, "xjm");
			assetBundle = AssetBundle.LoadFromMemory(bytes);
			//}

			if (assetBundle == null)
			{
				Debug.LogError("load AssetBundle error : " + fullPath);
			}

			item = m_AssetBundleItemPool.Spawn(true);
			item.assetBundle = assetBundle;
			item.RefCount++;
			m_AssetBundleItemDic.Add(crc, item);
		}
		else
		{
			item.RefCount++;
		}

		return item.assetBundle;
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	/// <param name="item"></param>
	public void ReleaseAsset(ResourceItem item)
	{
		if (item == null) return;

		if (item.m_DependceAssetBundle != null && item.m_DependceAssetBundle.Count > 0)
		{
			for (int i = 0; i < item.m_DependceAssetBundle.Count; i++)
			{
				UnLoadAssetBundle(item.m_DependceAssetBundle[i]);
			}
		}

		UnLoadAssetBundle(item.m_ABName);
	}

	private void UnLoadAssetBundle(string name)
	{
		AssetBundleItem item = null;
		uint crc = Crc32.GetCrc32(name);
		if (m_AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
		{
			item.RefCount--;
			if (item.RefCount <= 0 && item.assetBundle != null)
			{
				item.assetBundle.Unload(true);
				item.Reset();
				m_AssetBundleItemPool.Recycle(item);
				m_AssetBundleItemDic.Remove(crc);
			}
		}
	}

	/// <summary>
	/// 根据 crc 查找 resourceitem
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	public ResourceItem FindResourceItem(uint crc)
	{
		ResourceItem item = null;
		m_ResourceItemDic.TryGetValue(crc, out item);
		return item;
	}
	
}


public class AssetBundleItem
{
	public AssetBundle assetBundle = null;
	public int RefCount = 0;

	public void Reset()
	{
		assetBundle = null;
		RefCount = 0;
	}
}


public class ResourceItem
{
	//资源路径的 crc
	public uint m_Crc = 0;
	//资源的名字
	public string m_AssetName = string.Empty;
	//改资源所在的  asssetbundle
	public string m_ABName = string.Empty;
	//该资源所依赖的 assetbundle
	public List<string> m_DependceAssetBundle = null;
	//该资源加载完的 ab 包
	public AssetBundle m_AssetBundle = null;

	//------------------------------------------------------
	
	//资源对象
	public UnityEngine.Object m_Obj = null;
	//资源唯一标识
	public int m_Guid = 0;
	//资源最后所使用的时间
	public float m_LastUseTime = 0.0f;
	//引用计数
	protected int m_RefCount = 0;
	
	//是否跳场景清除
	public bool m_Clear = true;

	public int RefCount
	{
		get { return m_RefCount; }
		set
		{
			m_RefCount = value;
			if (m_RefCount < 0)
			{
				Debug.LogError("refCount < 0 " + m_RefCount + "," + (m_Obj != null ? m_Obj.name : "name is null"));
			}
			
		}
	}
}




























