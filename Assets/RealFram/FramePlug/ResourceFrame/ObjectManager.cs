using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{

	/// <summary>
	/// 对象池节点
	/// </summary>
	private Transform RecyclePoolTrs;

	/// <summary>
	/// 场景节点
	/// </summary>
	private Transform SceneTrs;
	
	//对象池
	protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
	
	//暂存 resObject 的 Dic
	protected Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
	
	//类对象池  ResourceObj
	protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool = null;
	
	//根据异步的 guid 储存ResourceObj，来判断是否正在异步加载
	protected Dictionary<long, ResourceObj> m_AsyncResObjs = new Dictionary<long, ResourceObj>();
		

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="recycleTrs">回收节点</param>
	/// <param name="sceneTrs">场景默认节点</param>
	public void Init(Transform recycleTrs, Transform sceneTrs)
	{
		RecyclePoolTrs = recycleTrs;
		SceneTrs = sceneTrs;
		
		m_ResourceObjClassPool = GetOrCreateClassPool<ResourceObj>(1000);
	}

	/// <summary>
	/// 根据 obj  得到离线的数据
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public OfflineData FindOffline(GameObject obj)
	{
		OfflineData data = null;
		ResourceObj resObj = null;
		m_ResourceObjDic.TryGetValue(obj.GetInstanceID(), out resObj);
		if (resObj != null)
		{
			/*if (resObj.m_OfflineData != null)
			{
				data = resObj.m_OfflineData;
			}*/
		}

		return data;
	}


	/// <summary>
	/// 清空对象池
	/// </summary>
	public void ClearCache()
	{
		List<uint> tempList = new List<uint>();
		
		uint[] tempArray = m_ObjectPoolDic.Keys.ToArray();
		for (int i = 0; i < tempArray.Length; i++)
		{
			List<ResourceObj> st = m_ObjectPoolDic[tempArray[i]];
			for (int j = st.Count - 1; j >= 0; j--)
			{
				ResourceObj resourceObj = st[j];
				if (!System.Object.ReferenceEquals(resourceObj.m_CloneObj, null) && resourceObj.m_bClear)
				{
					GameObject.Destroy(resourceObj.m_CloneObj);
					m_ResourceObjDic.Remove(resourceObj.m_CloneObj.GetInstanceID());
					resourceObj.Reset();
					m_ResourceObjClassPool.Recycle(resourceObj);
					st.Remove(resourceObj);
				}
			}

			if (st.Count <= 0)
			{
				tempList.Add(tempArray[i]);
			}
		}

		for (int i = 0; i < tempList.Count; i++)
		{
			uint temp = tempList[i];
			if (m_ObjectPoolDic.ContainsKey(temp))
			{
				m_ObjectPoolDic.Remove(temp);
			}
		}
		tempList.Clear();
	}


	/// <summary>
	/// 从对象池取对象
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	protected ResourceObj GetObjectFromPool(uint crc)
	{
		List<ResourceObj> st = null;
		if (m_ObjectPoolDic.TryGetValue(crc, out st) && st != null && st.Count > 0)
		{
			//ResourceManager 引用计数
			ResourceManager.Instance.IncreaseResourceRef(crc);
			ResourceObj resObj = st[0];
			st.RemoveAt(0);
			GameObject obj = resObj.m_CloneObj;
			if (!System.Object.ReferenceEquals(obj, null))
			{
				/*if (!System.Object.ReferenceEquals(resObj.m_OfflineData, null))
				{
					resObj.m_OfflineData.ResetProp();
				}*/
				resObj.m_Already = false;
#if UNITY_EDITOR
				if (obj.name.EndsWith("(Recycle)"))
				{
					obj.name = obj.name.Replace("(Recycle)", "");
				}
#endif
				return resObj;
			}
		}

		return null;
	}


	/// <summary>
	/// 取消异步加载
	/// </summary>
	/// <param name="guid"></param>
	public void CancleLoad(long guid)
	{
		ResourceObj resourceObj = null;
		if (m_AsyncResObjs.TryGetValue(guid, out resourceObj) && ResourceManager.Instance.CancleLoad(resourceObj))
		{
			m_AsyncResObjs.Remove(guid);
			resourceObj.Reset();
			m_ResourceObjClassPool.Recycle(resourceObj);
		}
	}

	
	/// <summary>
	/// 是否正在异步加载  
	/// </summary>
	/// <param name="guid"></param>
	/// <returns></returns>
	public bool IsingAsyncLoad(long guid)
	{
		return m_AsyncResObjs[guid] != null;
	}

	/// <summary>
	/// 该对象是否是对象池创建的
	/// </summary>
	/// <returns></returns>
	public bool IsObjectManagerCreate(GameObject obj)
	{
		if (obj == null) return false;
		ResourceObj resourceObj = m_ResourceObjDic[obj.GetInstanceID()];
		return resourceObj != null;
	}
	
	
	/// <summary>
	/// 预加载  GameObject
	/// </summary>
	/// <param name="path"></param>
	/// <param name="count"></param>
	/// <param name="clear"></param>
	public void PreLoadGameObject(string path, int count = 1, bool clear = false)
	{
		List<GameObject> tempGameObjects = new List<GameObject>();
		for (int i = 0; i < count; i++)
		{
			GameObject obj = InstantiateObject(path, false, clear);
			tempGameObjects.Add(obj);
		}

		for (int i = 0; i < count; i++)
		{
			GameObject obj = tempGameObjects[i];
			ReleaseObject(obj);
			obj = null;
		}
		
		tempGameObjects.Clear();
	}

	/// <summary>
	/// 清楚某个资源在对象池中所有的对象   //暂时不用感觉有问题
	/// </summary>
	/// <param name="crc"></param>
	public void ClearPoolObject(uint crc)
	{
		List<ResourceObj> st = null;
		if (!m_ObjectPoolDic.TryGetValue(crc, out st) || st == null)
		{
			return;
		}

		for (int i = st.Count - 1; i >= 0; i--)
		{
			ResourceObj resourceObj = st[i];
			if (resourceObj.m_bClear)
			{
				st.Remove(resourceObj);
				int tempId = resourceObj.m_CloneObj.GetInstanceID();
				GameObject.Destroy(resourceObj.m_CloneObj);
				resourceObj.Reset();
				m_ResourceObjDic.Remove(tempId);
				m_ResourceObjClassPool.Recycle(resourceObj);
			}
		}

		if (st.Count <= 0)
		{
			m_ObjectPoolDic.Remove(crc);
		}
	}

	/// <summary>
	/// 同步加载
	/// </summary>
	/// <param name="path"></param>
	/// <param name="bClear">是否跳转场景清除的变量   关联到了他的资源</param>
	/// <returns></returns>
	public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
	{
		uint crc = Crc32.GetCrc32(path);
		ResourceObj resourceObj = GetObjectFromPool(crc);
		if (resourceObj == null)
		{
			resourceObj = m_ResourceObjClassPool.Spawn(true);
			resourceObj.m_Crc = crc;
			resourceObj.m_bClear = bClear;
			//ResourceManager 提供加载方法
			resourceObj = ResourceManager.Instance.LoadResource(path, resourceObj);
			
			
			if (resourceObj.m_ResItem.m_Obj != null)
			{
				resourceObj.m_CloneObj = GameObject.Instantiate(resourceObj.m_ResItem.m_Obj) as GameObject;
				//resourceObj.m_OfflineData = resourceObj.m_CloneObj.GetComponent<OfflineData>();
			}
		}

		if (setSceneObj)
		{
			resourceObj.m_CloneObj.transform.SetParent(SceneTrs, false);
		}

		int tempId = resourceObj.m_CloneObj.GetInstanceID();
		if (!m_ResourceObjDic.ContainsKey(tempId))
		{
			m_ResourceObjDic.Add(tempId, resourceObj);
		}
		return resourceObj.m_CloneObj;
	}

	/// <summary>
	/// 异步对象加载
	/// </summary>
	/// <param name="path"></param>
	/// <param name="fealFinish"></param>
	/// <param name="priority"></param>
	/// <param name="setSceneObject"></param>
	/// <param name="bClear"></param>
	/// <param name="args"></param>
	public long InstantiateObjectAsync(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority,
		bool setSceneObject = false, bool bClear = true, object param1 = null, object param2 = null, object param3 = null)
	{
		if (string.IsNullOrEmpty(path))
		{
			return -1;
		}

		uint crc = Crc32.GetCrc32(path);
		ResourceObj resObj = GetObjectFromPool(crc);
		if (resObj != null)
		{
			if (setSceneObject)
			{
				resObj.m_CloneObj.transform.SetParent(SceneTrs, false);
			}

			if (dealFinish != null)
			{
				dealFinish(path, resObj.m_CloneObj, param1, param2, param3);
			}

			return resObj.m_Guid;
		}

		long guid = ResourceManager.Instance.CreateGuid();
		resObj = m_ResourceObjClassPool.Spawn(true);
		resObj.m_Crc = crc;
		resObj.m_SetSceneParent = setSceneObject;
		resObj.m_bClear = bClear;
		resObj.m_DealFinish = dealFinish;
		resObj.param1 = param1;
		resObj.param2= param2;
		resObj.param3 = param3;
		resObj.m_Guid = guid;
		//调用ResourceManager的异步加载接口
		ResourceManager.Instance.AsyncLoadResource(path, resObj, OnLoadResourceObjFinish, priority);
		return guid;
	}

	/// <summary>
	/// 资源加载完成回调
	/// </summary>
	/// <param name="path"></param>
	/// <param name="resObj"></param>
	/// <param name="args"></param>
	void OnLoadResourceObjFinish(string path, ResourceObj resObj, object param1 = null, object param2 = null, object param3 = null)
	{
		if (resObj == null)
		{
			return;
		}

		if (resObj.m_ResItem.m_Obj == null)
		{
#if UNITY_EDITOR
			Debug.LogError("异步资源加载的资源为空！" + path);
#endif
		}
		else
		{
			resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
			//resObj.m_OfflineData = resObj.m_CloneObj.GetComponent<OfflineData>();
		}

		//加载完成 就从正在加载异步中移除
		if (m_AsyncResObjs.ContainsKey(resObj.m_Guid))
		{
			m_AsyncResObjs.Remove(resObj.m_Guid);
		}

		if (resObj.m_CloneObj != null && resObj.m_SetSceneParent == true)
		{
			resObj.m_CloneObj.transform.SetParent(SceneTrs, false);
		}

		if (resObj.m_DealFinish != null)
		{
			int tempId = resObj.m_CloneObj.GetInstanceID();
			if (!m_ResourceObjDic.ContainsKey(tempId))
			{
				m_ResourceObjDic.Add(tempId, resObj);
			}

			resObj.m_DealFinish(path, resObj.m_CloneObj, param1, param2, param3);
		}
	}


	/// <summary>
	/// 回收对象池
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="maxCacheCount"> -1  代表对象池个数不受限制</param>
	/// <param name="destoryCache"></param>
	/// <param name="recycleParent"></param>
	public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destoryCache = false,
		bool recycleParent = true)
	{
		if (obj == null) return;

		ResourceObj resObj = null;
		int tempId = obj.GetInstanceID();
		if (!m_ResourceObjDic.TryGetValue(tempId, out resObj))
		{
			Debug.Log(obj.name + " 对象不是对象池创建的!");
			return;
		}

		if (resObj == null)
		{
			Debug.LogError("缓存的 resourceObj  为空!");
			return;
		}

		if (resObj.m_Already)
		{
			Debug.LogError(obj.name + " 对象已经放回对象池,检查引用!");
			return;
		}
		
#if UNITY_EDITOR
		obj.name += "(Recycle)";
#endif

		List<ResourceObj> st = null;
		if (maxCacheCount == 0)
		{
			m_ResourceObjDic.Remove(tempId);
			ResourceManager.Instance.ReleaseResource(resObj, destoryCache);
			resObj.Reset();
			m_ResourceObjClassPool.Recycle(resObj);
		}
		else  //回收到对象池
		{
			if (!m_ObjectPoolDic.TryGetValue(resObj.m_Crc, out st) || st == null)
			{
				st = new List<ResourceObj>();
				m_ObjectPoolDic.Add(resObj.m_Crc, st);
			}

			if (resObj.m_CloneObj)
			{
				if (recycleParent)
				{
					resObj.m_CloneObj.transform.SetParent(RecyclePoolTrs);
				}
				else
				{
					resObj.m_CloneObj.SetActive(false);
				}
			}

			if (maxCacheCount < 0 || st.Count < maxCacheCount)
			{
				st.Add(resObj);
				resObj.m_Already = true;
				//resourcemanager 做一个引用计数
				ResourceManager.Instance.DecreaseResourceRef(resObj);
			}
			else
			{
				m_ResourceObjDic.Remove(tempId);
				ResourceManager.Instance.ReleaseResource(resObj, destoryCache);
				resObj.Reset();
				m_ResourceObjClassPool.Recycle(resObj);
			}
			
		}

	}



	protected Dictionary<Type, object> m_ClassPoolDic = new Dictionary<Type, object>();
	
	/// <summary>
	/// 创建类对象池， 创建后外面可以保存   ClassObjectPool<T>  然后调用 spawn 和 recycle  对象的管理
	/// </summary>
	/// <param name="maxcount"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class, new()
	{
		Type type = typeof(T);
		object outObj = null;
		if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
		{
			ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxcount);
			m_ClassPoolDic.Add(type, newPool);
			return newPool;
		}

		return outObj as ClassObjectPool<T>;
	}

	/// <summary>
	/// 从对象池中取 T 对象
	/// </summary>
	/// <param name="maxcount"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T NewClassObjectFromPool<T>(int maxcount) where T : class, new()
	{
		ClassObjectPool<T> pool = GetOrCreateClassPool<T>(maxcount);
		if (pool == null)
		{
			return null;
		}

		return pool.Spawn(true);
	}
	
	
}




















