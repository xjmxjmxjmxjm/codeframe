using System.Collections.Generic;
using UnityEngine;
public class ConfigerManager : Singleton<ConfigerManager>
{
	//储存所有已经加载的配置表
	protected Dictionary<string, ExcelBase> m_AllExcelData = new Dictionary<string, ExcelBase>();
	
	/// <summary>
	/// 加载数据表
	/// </summary>
	/// <param name="path">二进制路径</param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T LoadData<T>(string path) where T : ExcelBase
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}

		
		if (m_AllExcelData.ContainsKey(path))
		{
			Debug.LogError("重复加载相同配置表文件！" + path);
			return m_AllExcelData[path] as T;
		}

		T data = BinarySerializeOpt.BinaryDeserialize<T>(path);
		
#if UNITY_EDITOR
		if (data == null)
		{
			Debug.Log("从 xml 加载了数据，未转成二进制!" + path);
			string xmlPath = path.Replace("Binary", "Xml").Replace(".bytes", ".xml");
			data = BinarySerializeOpt.XmlDeserializeRun<T>(xmlPath);
		}		
#endif

		if (data != null)
		{
			data.Init();
		}

		m_AllExcelData.Add(path, data);
		
		return data;
	}

	public T FindData<T>(string path)where T : ExcelBase
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}

		ExcelBase excelBase = null;
		if (m_AllExcelData.TryGetValue(path, out excelBase))
		{
			return excelBase as T;
		}
		else
		{
			return LoadData<T>(path);
		}

		return null;
	}
}

public class CFG
{
	//配置表路径
	public const string TABLE_MONSTER = "Assets/GameData/Data/Binary/MonsterData.bytes";
	public const string TABLE_BUFF = "Assets/GameData/Data/Binary/BuffData.bytes";
}
