using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LoadResPriority
{
	RES_HIGHT = 0,
	RES_MIDDLE,
	RES_SLOW,
	RES_NUM,
}

public class ResourceObj
{
	//路径对应 crc
	public uint m_Crc = 0;
	//存  ResourceItem
	public ResourceItem m_ResItem = null; 
	
	//实例化出来的 GameObject
	public GameObject m_CloneObj = null;
	//是否跳场景清除
	public bool m_bClear = true;
	//储存 guid
	public long m_Guid = 0;
	
	//是否已经放回对象池
	public bool m_Already = false;


	public OfflineData m_OfflineData = null;
	
	//--------------------------------------------
	//是否放到场景节点下面
	public bool m_SetSceneParent = false;
	//资源加载完成回调
	public OnAsyncObjFinish m_DealFinish = null;
	//异步参数
	public object param1 = null;
	public object param2 = null;
	public object param3 = null;

	public void Reset()
	{
		m_Crc = 0;
		m_ResItem = null;
		m_CloneObj = null;
		m_bClear = true;
		m_Guid = 0;
		m_Already = false;
		m_SetSceneParent = false;
		m_DealFinish = null;
		param1 = null;
		param2 = null;
		param3 = null;
		m_OfflineData = null;
	}
}


public class AsyncLoadResParam
{
	public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
	public uint m_Crc;
	public string m_Path;
	public bool m_Sprite = false;
	public LoadResPriority m_Priority = LoadResPriority.RES_SLOW;

	public void Reset()
	{
		m_CallBackList.Clear();
		m_Crc = 0;
		m_Path = string.Empty;
		m_Sprite = false;
		m_Priority = LoadResPriority.RES_SLOW;
	}
}

public class AsyncCallBack
{
	//加载完成的回调
	public OnAsyncObjFinish m_DealObjFinish = null;

	
	//针对GameObject的加载完成回调
	public OnAsyncFinish m_DealFinish = null;
	public ResourceObj m_ResObj = null;
	
	
	//回调参数
	public object param1 = null;
	public object param2 = null;
	public object param3 = null;

	public void Reset()
	{
		m_DealObjFinish = null;
		m_DealFinish = null;
		m_ResObj = null;
		object param1 = null;
		object param2 = null;
		object param3 = null;
	}
}

//资源加载完成回调
public delegate void OnAsyncObjFinish(string path, UnityEngine.Object obj, object param1 = null, object param2 = null,object param3 = null);

//实例化对象加载完成回调
public delegate void OnAsyncFinish(string path, ResourceObj resourceObj, object param1 = null, object param2 = null,object param3 = null);

public class ResourceManager : Singleton<ResourceManager>
{
	
#if UNITY_EDITOR
	public void ClearResourceManager()
	{
		m_NoRefrenceAssetMapList.Clear();
		foreach (var item in m_AssetDic.Values)
		{
			if (item.m_Obj != null)
			{
				if(item.m_Obj as GameObject)continue;
				Resources.UnloadAsset(item.m_Obj);
			}
		}
	}
#endif

	protected long m_Guid = 0;
#if UNITY_EDITOR
	public bool m_LoadFromAssetBundle = false;
#endif
	//缓存正在使用的资源列表
	//public Dictionary<uint, ResourceItem> m_AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();
	public Dictionary<uint, ResourceItem> m_AssetDic = new Dictionary<uint, ResourceItem>();

	//缓存没有使用的资源列表   缓存引用计数为0的资源列表   达到缓存最大的时候 释放缓存列表里最早没用的资源
	protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

	
	
	//中间类  回调类对象池
	protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
		//ObjectManager.Instance.GetOrCreateClassPool<AsyncLoadResParam>(50);
	//中间类  回调参数类对象池
	protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);
		//ObjectManager.Instance.GetOrCreateClassPool<AsyncCallBack>(100);
	//mono脚本
	protected MonoBehaviour m_StartMono;
	//正在异步加载的资源列表
	protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int) LoadResPriority.RES_NUM];
	//正在异步加载的 dic
	protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();
	
	
	
	//最长连续卡着加载资源的时间  微妙
	private const long MAXLOADRESTIME = 200000;

	//最大缓存个数
	private const int MAXCACHECOUNT = 500;
	
	public void Init(MonoBehaviour mono)
	{
		for (int i = 0; i < (int) LoadResPriority.RES_NUM; i++)
		{
			m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
		}
		m_StartMono = mono;
		m_StartMono.StartCoroutine(AsyncLoadCor());
	}

	/// <summary>
	/// 创建唯一的 GUID
	/// </summary>
	public long CreateGuid()
	{
		return m_Guid++;
	}
	
	/// <summary>
	/// 跳场景 清空缓存
	/// </summary>
	public void ClearCache()
	{
		List<ResourceItem> tempList = new List<ResourceItem>();
		ResourceItem[] tempArray = m_AssetDic.Values.ToArray();
		for (int i = 0; i < tempArray.Length; i++)
		{
			if (tempArray[i].m_Clear)
			{
				tempList.Add(tempArray[i]);
			}
		}
		

		for (int i = 0; i < tempList.Count; i++)
		{
			DestoryResourceItem(tempList[i], true);
		}
		tempList.Clear();
	}

	/// <summary>
	/// 取消异步加载资源
	/// </summary>
	/// <returns></returns>
	public bool CancleLoad(ResourceObj resourceObj)
	{
		AsyncLoadResParam para = null;
		if (m_LoadingAssetDic.TryGetValue(resourceObj.m_Crc, out para) &&
		    m_LoadingAssetList[(int) para.m_Priority].Contains(para)) 
		{
			for (int i = para.m_CallBackList.Count - 1; i >= 0; i--)
			{
				AsyncCallBack tempCallBack = para.m_CallBackList[i];
				if (tempCallBack != null && resourceObj == tempCallBack.m_ResObj)
				{
					tempCallBack.Reset();
					m_AsyncCallBackPool.Recycle(tempCallBack);
					para.m_CallBackList.Remove(tempCallBack);
					break;
				}
			}

			if (para.m_CallBackList.Count <= 0)
			{
				para.Reset();
				m_LoadingAssetList[(int) para.m_Priority].Remove(para);
				m_AsyncLoadResParamPool.Recycle(para);
				m_LoadingAssetDic.Remove(resourceObj.m_Crc);
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 根据 obj 增加引用计数
	/// </summary>
	/// <returns></returns>
	public int IncreaseResourceRef(ResourceObj resObj, int count = 1)
	{
		return resObj != null ? IncreaseResourceRef(resObj.m_Crc, count) : 0;
	}

	/// <summary>
	/// 根据 crc 增加引用计数
	/// </summary>
	/// <returns></returns>
	public int IncreaseResourceRef(uint crc = 0, int count = 1)
	{
		ResourceItem item = null;
		if (!m_AssetDic.TryGetValue(crc, out item) || item == null)
		{
			return 0;
		}

		item.RefCount += count;
		item.m_LastUseTime = Time.realtimeSinceStartup;

		return item.RefCount;
	}
	/// <summary>
	/// 根据 obj 减少引用计数
	/// </summary>
	/// <returns></returns>
	public int DecreaseResourceRef(ResourceObj resObj, int count = 1)
	{
		return resObj != null ? DecreaseResourceRef(resObj.m_Crc, count) : 0;
	}
	/// <summary>
	/// 根据 crc 减少引用计数
	/// </summary>
	/// <returns></returns>
	public int DecreaseResourceRef(uint crc = 0, int count = 1)
	{
		ResourceItem item = null;
		if (!m_AssetDic.TryGetValue(crc, out item) || item == null)
		{
			return 0;
		}

		item.RefCount -= count;
		//item.m_LastUseTime = Time.realtimeSinceStartup;

		return item.RefCount;
	}
	
	/// <summary>
	/// 预加载
	/// </summary>
	/// <param name="path"></param>
	public void PreloadRes(string path)
	{
		if (string.IsNullOrEmpty(path)) return;

		uint crc = Crc32.GetCrc32(path);
		ResourceItem item = GetCacheResourceItem(crc, 0);
		if (item != null)
		{
			return;
		}

		UnityEngine.Object obj = null;
#if UNITY_EDITOR
		if (!m_LoadFromAssetBundle)
		{
			item = AssetBundleManager.Instance.FindResourceItem(crc);
			if (item != null && item.m_Obj != null)
			{
				obj = item.m_Obj as UnityEngine.Object;
			}
			else
			{
				if (item == null)
				{
					item = new ResourceItem();
					item.m_Crc = crc;
				}
				obj = LoadAssetByEditor<UnityEngine.Object>(path);
			}
		}
#endif
		if (obj == null)
		{
			item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
			if (item != null && item.m_AssetBundle != null)
			{
				if (item.m_Obj != null)
				{
					obj = item.m_Obj as UnityEngine.Object;
				}
				else
				{
					obj = item.m_AssetBundle.LoadAsset<UnityEngine.Object>(item.m_AssetName);
				}
			}
		}

		CacheResource(path, ref item, crc, obj);
		
		//跳转场景不清空缓存
		item.m_Clear = false;

		ReleaseResource(obj);
	}

	/// <summary>
	/// 同步加载资源 针对给 objectManager 的接口
	/// </summary>
	/// <param name="path"></param>
	/// <param name="resObj"></param>
	/// <returns></returns>
	public ResourceObj LoadResource(string path, ResourceObj resObj)
	{
		if (resObj == null)
		{
			return null;
		}

		uint crc = resObj.m_Crc == 0 ? Crc32.GetCrc32(path) : resObj.m_Crc;

		ResourceItem item = GetCacheResourceItem(crc);
		if (item != null)
		{
			resObj.m_ResItem = item;
			return resObj;
		}

		UnityEngine.Object obj = null;
#if UNITY_EDITOR
		if (!m_LoadFromAssetBundle)
		{
			item = AssetBundleManager.Instance.FindResourceItem(crc);
			if (item != null && item.m_Obj != null)
			{
				obj = item.m_Obj as UnityEngine.Object;
			}
			else
			{
				if (item == null)
				{
					item = new ResourceItem();
					item.m_Crc = crc;
				}
				obj = LoadAssetByEditor<UnityEngine.Object>(path);
			}
		}
#endif

		if (obj == null)
		{
			item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
			if (item != null && item.m_AssetBundle != null)
			{
				if (item.m_Obj != null)
				{
					obj = item.m_Obj as UnityEngine.Object;
				}
				else
				{
					obj = item.m_AssetBundle.LoadAsset<UnityEngine.Object>(item.m_AssetName);
				}
			}
		}

		CacheResource(path, ref item, crc, obj);
		resObj.m_ResItem = item;
		item.m_Clear = resObj.m_bClear;

		return resObj;
	}

	/// <summary>
	/// 同步资源加载  外部直接调用 仅加载不需要实例化的资源
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T LoadResource<T>(string path) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}

		uint crc = Crc32.GetCrc32(path);
		ResourceItem item = GetCacheResourceItem(crc);
		if (item != null)
		{
			return item.m_Obj as T;
		}

		T obj = null;
#if UNITY_EDITOR
		if (!m_LoadFromAssetBundle)
		{
			item = AssetBundleManager.Instance.FindResourceItem(crc);
			if (item != null && item.m_Obj != null)
			{
				obj = item.m_Obj as T;
			}
			else
			{
				if (item == null)
				{
					item = new ResourceItem();
					item.m_Crc = crc;
				}
				obj = LoadAssetByEditor<T>(path);
			}
		}
#endif
		if (obj == null)
		{
			item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
			if (item != null && item.m_AssetBundle != null)
			{
				if (item.m_Obj != null)
				{
					obj = item.m_Obj as T;
				}
				else
				{
					obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
				}
			}
		}

		CacheResource(path, ref item, crc, obj);

		return obj;
	}

	
	/// <summary>
	/// 需要实例化的资源的释放
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="destroyObj"></param>
	/// <returns></returns>
	public bool ReleaseResource(ResourceObj resObj, bool destroyObj = false)
	{
		if (resObj == null) return false;

		ResourceItem item = null;
		if (!m_AssetDic.TryGetValue(resObj.m_Crc, out item) || item == null)
		{
			Debug.LogError("m_AssetDic 里 不存在改资源：" + resObj.m_CloneObj.name + "可能释放多次。");
		}

		GameObject.Destroy(resObj.m_CloneObj);
		item.RefCount--;
		DestoryResourceItem(item, destroyObj);
		return true;
	}
	
	/// <summary>
	/// 不需要实例化的资源的释放
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="destroyObj"></param>
	/// <returns></returns>
	public bool ReleaseResource(UnityEngine.Object obj, bool destroyObj = false)
	{
		if (obj == null) return false;

		ResourceItem item = null;
		ResourceItem[] tempArray = m_AssetDic.Values.ToArray();
		for (int i = 0; i < tempArray.Length; i++)
		{
			if (tempArray[i].m_Guid == obj.GetInstanceID())
			{
				item = tempArray[i];
				break;
			}
		}
		
		if (item == null)
		{
			Debug.LogError("m_AssetDic 里 不存在改资源：" + obj.name + "可能释放多次。");
			return false;
		}

		item.RefCount--;

		DestoryResourceItem(item, destroyObj);
		return true;
	}

	/// <summary>
	/// 不需要实例化的资源卸载  根据路径
	/// </summary>
	/// <param name="path"></param>
	/// <param name="destroyObj"></param>
	/// <returns></returns>
	public bool ReleaseResource(string path, bool destroyObj = false)
	{
		if (string.IsNullOrEmpty(path)) return false;

		uint crc = Crc32.GetCrc32(path);
		
		ResourceItem item = null;
		if (!m_AssetDic.TryGetValue(crc, out item) || item == null)
		{
			Debug.LogError("m_AssetDic 里 不存在改资源：" + path + "可能释放多次。");
		}

		item.RefCount--;

		DestoryResourceItem(item, destroyObj);
		return true;
	}
	
	/// <summary>
	/// 缓存加载的资源
	/// </summary>
	/// <param name="path"></param>
	/// <param name="item"></param>
	/// <param name="crc"></param>
	/// <param name="obj"></param>
	/// <param name="addrefCount"></param>
	void CacheResource(string path, ref ResourceItem item, uint crc, UnityEngine.Object obj, int addrefCount = 1)
	{
		//缓存太多 清除最早没有使用的资源
		WashOut();
		
		
		if (item == null)
		{
			Debug.LogError("ResourceItem is null ! path:" + path);
		}

		if (obj == null)
		{
			Debug.LogError("Resource load error !" + path);
		}

		item.m_Obj = obj;
		item.m_Guid = obj.GetInstanceID();
		item.m_LastUseTime = Time.realtimeSinceStartup;
		item.RefCount += addrefCount;
		ResourceItem oldItem = null;
		if (m_AssetDic.TryGetValue(crc, out oldItem))
		{
			m_AssetDic[item.m_Crc] = item;
		}
		else
		{
			m_AssetDic.Add(item.m_Crc, item);
		}
	}

	/// <summary>
	/// 缓存太多清除最早没有使用的资源
	/// </summary>
	protected void WashOut()
	{
		//  con 1
		//当大于 缓存个数时 进行释放 MAXCACHECOUNT
		while (m_NoRefrenceAssetMapList.Size() >= MAXCACHECOUNT)
		{
			for (int i = 0; i < MAXCACHECOUNT * 0.5; i++)
			{
				ResourceItem item = m_NoRefrenceAssetMapList.Back();
				DestoryResourceItem(item, true);
			}
		}
		
		
	}

	/// <summary>
	/// 回收一个资源
	/// </summary>
	/// <param name="item"></param>
	/// <param name="destroy"></param>
	protected void DestoryResourceItem(ResourceItem item, bool destroyCache = false)
	{
		if (item == null || item.RefCount > 0)
		{
			return;
		}
		
		if (!destroyCache)
		{
			m_NoRefrenceAssetMapList.InsertToHead(item);
			return;
		}
		
		
		if (!m_AssetDic.Remove(item.m_Crc))
		{
			return;
		}

		m_NoRefrenceAssetMapList.Remove(item);
		AssetBundleManager.Instance.ReleaseAsset(item);

		ObjectManager.Instance.ClearPoolObject(item.m_Crc);

		if (item.m_Obj != null)
		{
#if UNITY_EDITOR
			//Resources.UnloadAsset(item.m_Obj);
#endif
			item.m_Obj = null;
			
#if UNITY_EDITOR
			Resources.UnloadUnusedAssets();
#endif
		}
	}
	
#if UNITY_EDITOR
	protected T LoadAssetByEditor<T>(string path)where T : UnityEngine.Object
	{
		return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
	}
#endif

	ResourceItem GetCacheResourceItem(uint crc,int addrefCount = 1)
	{
		ResourceItem item = null;
		if (m_AssetDic.TryGetValue(crc, out item))
		{
			if (item != null)
			{
				item.RefCount += addrefCount;
				item.m_LastUseTime = Time.realtimeSinceStartup;

				/*if (item.RefCount <= 0)
				{
					m_NoRefrenceAssetMapList.Remove(item);
				}*/
			}
		}

		return item;
	}



	/// <summary>
	/// 异步加载资源 (仅不需要实例化的资源)
	/// </summary>
	public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority,
		bool IsSprite = false, uint crc = 0, object param1 = null, object param2 = null, object param3 = null)
	{
		if (crc == 0)
		{
			crc = Crc32.GetCrc32(path);
		}

		ResourceItem item = GetCacheResourceItem(crc);
		if (item != null)
		{
			if (dealFinish != null)
			{
				dealFinish(path, item.m_Obj, param1, param2, param3);
			}

			return;
		}
		//判断是否在加载中
		AsyncLoadResParam para = null;
		if (!m_LoadingAssetDic.TryGetValue(crc, out para) || para == null)
		{
			para = m_AsyncLoadResParamPool.Spawn(true);
			para.m_Crc = crc;
			para.m_Path = path;
			para.m_Priority = priority;
			para.m_Sprite = IsSprite;
			m_LoadingAssetDic.Add(crc, para);
			m_LoadingAssetList[(int) priority].Add(para);
		}
		
		//往回调列表里面加回调
		AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
		callBack.m_DealObjFinish = dealFinish;
		callBack.param1 = param1;
		callBack.param2 = param2;
		callBack.param3 = param3;
		para.m_CallBackList.Add(callBack);
	}


	/// <summary>
	/// 针对 objectManager
	/// </summary>
	/// <param name="path"></param>
	/// <param name="resourceObj"></param>
	/// <param name="dealFinish"></param>
	/// <param name="priority"></param>
	public void AsyncLoadResource(string path, ResourceObj resourceObj, OnAsyncFinish dealFinish,
		LoadResPriority priority)
	{
		ResourceItem item = GetCacheResourceItem(resourceObj.m_Crc);
		if (item != null)
		{
			resourceObj.m_ResItem = item;
			if (dealFinish != null)
			{
				dealFinish(path, resourceObj, resourceObj.param1, resourceObj.param2, resourceObj.param3);
			}

			return;
		}
		//判断是否在加载中
		AsyncLoadResParam para = null;
		if (!m_LoadingAssetDic.TryGetValue(resourceObj.m_Crc, out para) || para == null)
		{
			para = m_AsyncLoadResParamPool.Spawn(true);
			para.m_Crc = resourceObj.m_Crc;
			para.m_Path = path;
			para.m_Priority = priority;
			m_LoadingAssetDic.Add(resourceObj.m_Crc, para);
			m_LoadingAssetList[(int) priority].Add(para);
		}
		//往回调列表里面加回调
		AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
		callBack.m_DealFinish = dealFinish;
		callBack.m_ResObj = resourceObj;
		para.m_CallBackList.Add(callBack);
	}
	
	
	/// <summary>
	/// 异步加载
	/// </summary>
	/// <returns></returns>
	IEnumerator AsyncLoadCor()
	{
		List<AsyncCallBack> callBackList = null;
		//上一次 yield 的时间
		long lastYiledTime = System.DateTime.Now.Ticks;
		while (true)
		{
			bool haveYield = false;
			for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
			{
				if (m_LoadingAssetList[(int) LoadResPriority.RES_HIGHT].Count > 0)
				{
					i = (int) LoadResPriority.RES_HIGHT;
				}
				else if(m_LoadingAssetList[(int) LoadResPriority.RES_MIDDLE].Count > 0)
				{
					i = (int) LoadResPriority.RES_MIDDLE;
				}
				List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
				if (loadingList.Count <= 0) continue;

				AsyncLoadResParam loadingItem = loadingList[0];
				loadingList.RemoveAt(0);
				callBackList = loadingItem.m_CallBackList;

				UnityEngine.Object obj = null;
				ResourceItem item = null;
#if UNITY_EDITOR
				if (!m_LoadFromAssetBundle)
				{
					if (loadingItem.m_Sprite)
					{
						obj = LoadAssetByEditor<Sprite>(loadingItem.m_Path);
					}
					else
					{
						obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
					}
					//模拟异步加载
					//模拟异步加载
					yield return new WaitForSeconds(0.2f);

					item = AssetBundleManager.Instance.FindResourceItem(loadingItem.m_Crc);
					if (item == null)
					{
						item = new ResourceItem();
						item.m_Crc = loadingItem.m_Crc;
					}
				}		
#endif

				if (obj == null)
				{
					item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
					if (item != null && item.m_AssetBundle != null)
					{
						AssetBundleRequest abRequest = null;
						if (loadingItem.m_Sprite)
						{
							abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
						}
						else
						{
							abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
						}
						yield return abRequest;
						if (abRequest.isDone)
						{
							obj = abRequest.asset;
						}

						lastYiledTime = System.DateTime.Now.Ticks;
					}
				}

				CacheResource(loadingItem.m_Path, ref item, loadingItem.m_Crc, obj, callBackList.Count);

				for (int j = 0; j < callBackList.Count; j++)
				{
					AsyncCallBack callBack = callBackList[j];

					if (callBack != null && callBack.m_DealFinish != null && callBack.m_ResObj != null)
					{
						ResourceObj resObj = callBack.m_ResObj;
						resObj.m_ResItem = item;
						callBack.m_DealFinish(loadingItem.m_Path, resObj, resObj.param1, resObj.param2, resObj.param3);
						callBack.m_DealFinish = null;
						resObj = null;
					}
					
					if (callBack != null && callBack.m_DealObjFinish != null)
					{
						callBack.m_DealObjFinish(loadingItem.m_Path, obj, callBack.param1, callBack.param2, callBack.param3);
						callBack.m_DealObjFinish = null;
					}
					
					callBack.Reset();
					m_AsyncCallBackPool.Recycle(callBack);
				}

				obj = null;
				callBackList.Clear();
				m_LoadingAssetDic.Remove(loadingItem.m_Crc);
				loadingItem.Reset();
				m_AsyncLoadResParamPool.Recycle(loadingItem);
				
				if (System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
				{
					yield return null;
					lastYiledTime = System.DateTime.Now.Ticks;
					haveYield = true;
				}
				
			}

			if (!haveYield || System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
			{
				lastYiledTime = System.DateTime.Now.Ticks;
				yield return null;
			}
		}
	} 
}

//双向链表结构节点
public class DoubleLinkedListNode<T> where T : class, new()
{
	//前一个节点
	public DoubleLinkedListNode<T> prev = null;
	//后一个节点
	public DoubleLinkedListNode<T> next = null;
	
	//当前节点
	public T t = null;
	
}

//双向链表结构
public class DoubleLinkedList<T> where T : class, new()
{
	//表头
	public DoubleLinkedListNode<T> Head = null;
	//表尾
	public DoubleLinkedListNode<T> Tail = null;
	
	//双向链表结构类对象池
	protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool =
		ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
	
	//个数 
	protected int m_Count = 0;
	public int Count
	{
		get { return m_Count; }
	}

	/// <summary>
	/// 添加一个节点到头部
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public DoubleLinkedListNode<T> AddToHeader(T t)
	{
		DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
		pList.next = null;
		pList.prev = null;
		pList.t = t;
		return AddToHeader(pList);
	}
	public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
	{
		if (pNode == null) return null;

		pNode.next = null;
		if (Head == null)
		{
			Head = Tail = pNode;
		}
		else
		{
			pNode.next = Head;
			Head.prev = pNode;
			Head = pNode;
		}

		m_Count++;
		return Head;
	}

	/// <summary>
	/// 添加一个节点到尾部
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public DoubleLinkedListNode<T> AddToTail(T t)
	{
		DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
		pList.next = null;
		pList.prev = null;
		pList.t = t;
		return AddToTail(t);
	}
	public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
	{
		if (pNode == null) return null;
		
		pNode.prev = null;
		if (Tail == null)
		{
			Head = Tail = pNode;
		}
		else
		{
			pNode.prev = Tail;
			Tail.next = pNode;
			Tail = pNode;
		}

		m_Count++;
		return Tail;
	}

	/// <summary>
	/// 移除某个节点
	/// </summary>
	/// <param name="pNode"></param>
	public void RemoveNode(DoubleLinkedListNode<T> pNode)
	{
		if (pNode == null) return;

		if (pNode == Head)
		{
			Head = pNode.next;
		}

		if (pNode == Tail)
		{
			Tail = pNode.prev;
		}

		if (pNode.prev != null)
		{
			pNode.prev.next = pNode.next;
		}

		if (pNode.next != null)
		{
			pNode.next.prev = pNode.prev;
		}

		pNode.next = pNode.prev = null;
		pNode.t = null;
		m_DoubleLinkNodePool.Recycle(pNode);
		m_Count--;
	}

	/// <summary>
	/// 把某个节点移动到头部
	/// </summary>
	/// <param name="pNode"></param>
	public void MoveToHead(DoubleLinkedListNode<T> pNode)
	{
		if (pNode == null || pNode == Head) return;

		if (pNode.prev == null && pNode.next == null)
		{
			return;
		}

		if (pNode == Tail)
		{
			Tail = pNode.prev;
		}

		if (pNode.prev != null)
		{
			pNode.prev.next = pNode.next;
		}
		if (pNode.next != null)
		{
			pNode.next.prev = pNode.prev;
		}

		pNode.next = Head;
		pNode.prev = null;
		Head.prev = pNode;
		Head = pNode;

		if (Tail == null)
		{
			Tail = Head;
		}
	}
	
}

public class CMapList<T> where T : class, new()
{
	DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();

	Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

	~CMapList()
	{
		Clear();
	}

	/// <summary>
	/// 清空双向链表
	/// </summary>
	public void Clear()
	{
		while (m_DLink.Tail != null)
		{
			Remove(m_DLink.Tail.t);
		}
	}
	
	
	/// <summary>
	/// 插入一个节点到表头
	/// </summary>
	/// <param name="t"></param>
	public void InsertToHead(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (m_FindMap.TryGetValue(t, out node) && node != null)
		{
			//m_DLink.AddToHeader(node);
			m_DLink.MoveToHead(node);
			return;
		}

		m_DLink.AddToHeader(t);
		m_FindMap.Add(t, m_DLink.Head);
	}


	/// <summary>
	/// 从表尾弹出一个节点
	/// </summary>
	public void Pop()
	{
		if (m_DLink.Tail != null)
		{
			Remove(m_DLink.Tail.t);	
		}
	}

	/// <summary>
	/// 删除某个节点
	/// </summary>
	/// <param name="t"></param>
	public void Remove(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (!m_FindMap.TryGetValue(t, out node) || node == null)
		{
			return;
		}

		m_DLink.RemoveNode(node);
		m_FindMap.Remove(t);
	}

	/// <summary>
	/// 获取到尾部节点
	/// </summary>
	/// <returns></returns>
	public T Back()
	{
		return m_DLink.Tail.t == null ? null : m_DLink.Tail.t;
	}


	/// <summary>
	/// 返回节点个数
	/// </summary>
	/// <returns></returns>
	public int Size()
	{
		return m_DLink.Count;
	}

	/// <summary>
	/// 查找是否存在该节点
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public bool Find(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (!m_FindMap.TryGetValue(t, out node) || node == null)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// 刷新一个节点 移动到头结点
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public bool Refresh(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (!m_FindMap.TryGetValue(t, out node) || node == null)
		{
			return false;
		}

		m_DLink.MoveToHead(node);
		return true;
	}
}






















