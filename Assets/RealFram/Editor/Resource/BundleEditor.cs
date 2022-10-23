using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
public class BundleEditor
{
	private static string BuildTargetPath =
		Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();

	private static string m_VersionMD5Path = Application.dataPath + "/../Version/" +
	                                         EditorUserBuildSettings.activeBuildTarget.ToString();

	private static string m_HotPath = Application.dataPath + "/../Hot/" +
	EditorUserBuildSettings.activeBuildTarget.ToString();
	
	
	private static string ABCONFIGPATH = "Assets/RealFram/Editor/Resource/ABConfig.asset";

	private static string ABBINARYPATH = RealConfig.GetRealFram().m_ABBytePath;

	// key abname    value  abpath     allfile ABPack    
	private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
	
	//filter abpath
	private static List<string> m_AllFileAB = new List<string>();
	
	
	//all prefab dic    key abname    value  resources list
	private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
	
	
	// valid path
	private static List<string> m_oValidFilePath = new List<string>();
	
	//储存读出来的 md5 信息
	private static Dictionary<string, ABMD5Base> m_PackedMd5 = new Dictionary<string, ABMD5Base>();

	[MenuItem("Tools/ab/加密ab包")]
	public static void EncryptAB()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(BuildTargetPath);
		FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		for (int i = 0; i < fileInfos.Length; i++)
		{
			if (!fileInfos[i].Name.EndsWith(".meta") && !fileInfos[i].Name.EndsWith(".manifest"))
			{
				AES.AESFileEncrypt(fileInfos[i].FullName, "xjm");
			}
		}
		Debug.Log("加密完成!");
	}
	[MenuItem("Tools/ab/解密ab包")]
	public static void DecryptAB()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(BuildTargetPath);
		FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		for (int i = 0; i < fileInfos.Length; i++)
		{
			if (!fileInfos[i].Name.EndsWith(".meta") && !fileInfos[i].Name.EndsWith(".manifest"))
			{
				AES.AESFileDecrypt(fileInfos[i].FullName, fileInfos[i].Name);
			}
		}
		Debug.Log("解密完成!");
	}
	
	
	[MenuItem("Tools/打包")]
	public static void NormalBuild()
	{
		
		string targetPath = m_VersionMD5Path + "/ABMD5_" + PlayerSettings.bundleVersion + ".bytes";
		if (File.Exists(targetPath))
		{
			LogUtil.LogError("已经打过底包 是否需要打热更包 如果还是要打底包请删除<" + targetPath + ">文件");
			return;
		}
		
		Build();
	}
	

	public static void Build(bool hotfix = false, string abmd5Path = "", string hotCount = "1")
	{
		DataEditor.AllXmlToBinary();
		m_AllFileAB.Clear();
		m_AllFileDir.Clear();
		m_AllPrefabDir.Clear();
		m_oValidFilePath.Clear();
		

		ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
		
		foreach (ABConfig.FileDirABName dirAB in abConfig.m_AllFileDirAB)
		{
			if (m_AllFileDir.ContainsKey(dirAB.ABName))
			{
				Debug.LogError("ab file name repeat !  please check ABconfig.asset !");
			}
			else
			{
				m_AllFileDir.Add(dirAB.ABName, dirAB.Path);
				m_AllFileAB.Add(dirAB.Path);
				m_oValidFilePath.Add(dirAB.Path);
			}
		}

		string[] allStr = AssetDatabase.FindAssets("t:prefab", abConfig.m_AllPrefabPath.ToArray());
		int length = allStr.Length;
		for (int i = 0; i < length; i++)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
			m_oValidFilePath.Add(prefabPath);
			EditorUtility.DisplayProgressBar("find prefab", "repfab" + prefabPath, 1.0f * i / length);
			if (!ContansAllFileAB(prefabPath))
			{
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				string[] allDepend = AssetDatabase.GetDependencies(prefabPath);
				List<string> tempPath = new List<string>();
				int dependLength = allDepend.Length;
				for (int j = 0; j < dependLength; j++)
				{
					if (!ContansAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
					{
						m_AllFileAB.Add(allDepend[j]);
						tempPath.Add(allDepend[j]);
					}
				}

				if (m_AllPrefabDir.ContainsKey(obj.name))
				{
					Debug.LogError("ab prefab name repeat ! please check prefab name!  name -> " + obj.name);
				}
				else
				{
					m_AllPrefabDir.Add(obj.name, tempPath);
				}
			}
		}

		foreach (string name in m_AllFileDir.Keys)
		{
			SetNameAB(name, m_AllFileDir[name]);
		}
		foreach (string name in m_AllPrefabDir.Keys)
		{
			SetNameAB(name, m_AllPrefabDir[name]);
		}


		BuildAssetBundle();

		string[] ABName = AssetDatabase.GetAllAssetBundleNames();
		int abNameLength = ABName.Length;
		for (int i = 0; i < abNameLength; i++)
		{
			AssetDatabase.RemoveAssetBundleName(ABName[i], true);
			EditorUtility.DisplayCancelableProgressBar("clear ab name", "abName -> " + ABName[i],
				1.0f * i / abNameLength);
		}

		if (hotfix)
		{
			ReadMD5Com(abmd5Path, hotCount);
		}
		else
		{
			WriteABMD5();
		}
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
		
	}

	static void WriteABMD5()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(BuildTargetPath);
		FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		ABMD5 abmd5 = new ABMD5();
		abmd5.ABMD5List = new List<ABMD5Base>();
		for (int i = 0; i < files.Length; i++)
		{
			if (!files[i].Name.EndsWith(".meta") && !files[i].Name.EndsWith("manifest"))
			{
				ABMD5Base abmd5Base = new ABMD5Base();
				abmd5Base.Name = files[i].Name;
				abmd5Base.MD5 = MD5Manager.Instance.BuildFileMd5(files[i].FullName);
				abmd5Base.Size = files[i].Length / 1024.0f;
				abmd5.ABMD5List.Add(abmd5Base);
			}
		}

		string ABMD5Path = Application.dataPath + "/Resources/ABMD5.bytes";
		BinarySerializeOpt.BinarySerialize(ABMD5Path, abmd5);
		BinarySerializeOpt.Xmlserialize(Application.dataPath + "/Resources/ABMD5.xml", abmd5);
		//将打版的版本拷贝到外部进行存储
		if (!Directory.Exists(m_VersionMD5Path))
		{
			Directory.CreateDirectory(m_VersionMD5Path);
		}

		string targetPath = m_VersionMD5Path + "/ABMD5_" + PlayerSettings.bundleVersion + ".bytes";
//		if (File.Exists(targetPath))
//		{
//			File.Delete(targetPath);
//		}

		File.Copy(ABMD5Path, targetPath);
	}

	static void ReadMD5Com(string abmd5Path, string hotCount)
	{
		m_PackedMd5.Clear();
		using (FileStream fs = new FileStream(abmd5Path, FileMode.Open, FileAccess.Read))
		{
			BinaryFormatter bf = new BinaryFormatter();
			ABMD5 abmd5 = bf.Deserialize(fs) as ABMD5;
			foreach (ABMD5Base abmd5Base in abmd5.ABMD5List)
			{
				m_PackedMd5.Add(abmd5Base.Name, abmd5Base);
			}
		}
		
		List<string> changeList = new List<string>();
		DirectoryInfo directoryInfo = new DirectoryInfo(BuildTargetPath);
		FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			if (!files[i].Name.EndsWith(".meta") && !files[i].Name.EndsWith(".manifest"))
			{
				string name = files[i].Name;
				string md5 = MD5Manager.Instance.BuildFileMd5(files[i].FullName);
				ABMD5Base abmd5Base = null;
				if (!m_PackedMd5.ContainsKey(name))
				{
					changeList.Add(name);
				}
				else
				{
					if (m_PackedMd5.TryGetValue(name, out abmd5Base))
					{
						if (md5 != abmd5Base.MD5)
						{
							changeList.Add(name);
						}
					}
				}
			}
		}

		CopyABAndGeneratXml(changeList, hotCount);
	}

	/// <summary>
	/// 拷贝删选的 ab 包 及自动生成服务器列表
	/// </summary>
	/// <param name="changeList"></param>
	/// <param name="hotCount"></param>
	static void CopyABAndGeneratXml(List<string> changeList, string hotCount)
	{
		if (!Directory.Exists(m_HotPath))
		{
			Directory.CreateDirectory(m_HotPath);
		}

		DeleteAllFile(m_HotPath);

		foreach (string str in changeList)
		{
			if (!str.EndsWith(".manifest"))
			{
				File.Copy(BuildTargetPath + "/" + str, m_HotPath + "/" + str);
			}
		}
		
		//生成服务器 Patch
		DirectoryInfo directoryInfo = new DirectoryInfo(m_HotPath);
		FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		Pathces pathces = new Pathces();
		pathces.Version = int.Parse(hotCount);
		pathces.Files = new List<Patch>();
		for (int i = 0; i < files.Length; i++)
		{
			Patch patch = new Patch();
			patch.Md5 = MD5Manager.Instance.BuildFileMd5(files[i].FullName);
			//Debug.Log(files[i].FullName);
			patch.Name = files[i].Name;
			patch.Size = files[i].Length / 1024.0f;
			patch.Platform = EditorUserBuildSettings.activeBuildTarget.ToString();
			patch.Url = "localhost:8085/AssetBundle/" + PlayerSettings.bundleVersion + "/" + hotCount + "/" + files[i].Name;
			pathces.Files.Add(patch);
		}

		BinarySerializeOpt.Xmlserialize(m_HotPath + "/Patch.xml", pathces);
		
		
	}

	/// <summary>
	/// 设置资源  ab  pack   名字
	/// </summary>
	/// <param name="name"></param>
	/// <param name="path"></param>
	static void SetNameAB(string name, string path)
	{
		AssetImporter importer = AssetImporter.GetAtPath(path);
		if (importer == null)
		{
			Debug.LogError("res path error ! please check !  path -> " + path);
		}
		else
		{
			importer.assetBundleName = name;
		}
	}
	static void SetNameAB(string name, List<string> pathLst)
	{
		int length = pathLst.Count;
		for (int i = 0; i < length; i++)
		{
			SetNameAB(name, pathLst[i]);
		}
	}

	/// <summary>
	/// ab pack   打包
	/// </summary>
	static void BuildAssetBundle()
	{
		//key resPath   value abname
		Dictionary<string,string> tempDic = new Dictionary<string, string>();
		
		string[] oBundleNames = AssetDatabase.GetAllAssetBundleNames();
		int bundleNameLen = oBundleNames.Length;
		for (int i = 0; i < bundleNameLen; i++)
		{
			string bundleName = oBundleNames[i];
			string[] assetsBybundleName = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
			int assetsLen = assetsBybundleName.Length;
			for (int j = 0; j < assetsLen; j++)
			{
				if(assetsBybundleName[j].EndsWith(".cs"))continue;
				tempDic.Add(assetsBybundleName[j], bundleName);
			}
		}

		if (!Directory.Exists(BuildTargetPath))
		{
			Directory.CreateDirectory(BuildTargetPath);
		}

		
		DeleteAB();
		WriteData(tempDic);

		
		AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(BuildTargetPath, BuildAssetBundleOptions.ChunkBasedCompression,
			EditorUserBuildSettings.activeBuildTarget);
		if (manifest == null)
		{
			Debug.LogError("AssetBundle  打包失败！");
		}
		else
		{
			Debug.Log("AssetBundle  打包完毕!");
		}

		DeleteManifest();
		//EncryptAB();
	}

	static void DeleteManifest()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(BuildTargetPath);
		FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			if (files[i].Name.EndsWith(".manifest"))
			{
				File.Delete(files[i].FullName);
			}
		}
	}

	/// <summary>
	/// 得到所有的资源进行    生成依赖文件
	/// </summary>
	/// <param name="tempDic"></param>
	static void WriteData(Dictionary<string, string> tempDic)
	{
		AssetBundleConfig config = new AssetBundleConfig();
		config.m_oAllABBase = new List<ABBase>();
		foreach (string path in tempDic.Keys)
		{
			if(!IsValidPath(path))continue;
			
			ABBase abBase = new ABBase();
			abBase.Path = path;
			abBase.Crc = Crc32.GetCrc32(path);
			abBase.ABName = tempDic[path];
			abBase.AssetName = path.Remove(0, path.LastIndexOf('/') + 1);
			abBase.ABDependce = new List<string>();

			string[] assetDependces = AssetDatabase.GetDependencies(path);
			int length = assetDependces.Length;
			for (int i = 0; i < length; i++)
			{
				string dependce = assetDependces[i];
				if(dependce == path || dependce.EndsWith(".cs"))continue;

				string dependceABName = "";
				if (tempDic.TryGetValue(dependce, out dependceABName))
				{
					if(dependceABName == abBase.ABName)continue;
					if (!abBase.ABDependce.Contains(dependceABName))
					{
						abBase.ABDependce.Add(dependceABName);
					}
				}
			}

			config.m_oAllABBase.Add(abBase);
		}
		
		
		//write xml
		FileStream xmlfs = null;
		StreamWriter xmlsw = null;
		try
		{
			string xmlPath = Application.dataPath + "/../AssetBundle/AssetBundleConfig.xml";
			if (File.Exists(xmlPath)) File.Delete(xmlPath);
			xmlfs = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			xmlsw = new StreamWriter(xmlfs, Encoding.UTF8);
			XmlSerializer xmlSerializer = new XmlSerializer(config.GetType());
			xmlSerializer.Serialize(xmlsw, config);
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
		finally
		{
			xmlsw.Close();
			xmlfs.Close();
		}
		
		//write binary
		foreach (var abBase in config.m_oAllABBase)
		{
			abBase.Path = "";
		}

		FileStream binaryfs = null;
		try
		{
			binaryfs = new FileStream(ABBINARYPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			binaryfs.Seek(0, SeekOrigin.Begin);
			binaryfs.SetLength(0);
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(binaryfs, config);
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
		finally
		{
			binaryfs.Close();
		}
		

		SetNameAB("assetbundleconfig", RealConfig.GetRealFram().m_ABByteRoot);
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// 删除现有的  无用的  ab pack
	/// </summary>
	static void DeleteAB()
	{
		string[] allBundleName = AssetDatabase.GetAllAssetBundleNames();

		DirectoryInfo directoryInfo = new DirectoryInfo(BuildTargetPath);
		FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		int fileLen = files.Length;
		for (int i = 0; i < fileLen; i++)
		{
			if (ContansABName(files[i].Name, allBundleName) || files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith(".manifest") || files[i].Name.EndsWith("assetbundleconfig"))
			{
				continue;
			}
			else
			{
				if (File.Exists(files[i].FullName))
				{
					File.Delete(files[i].FullName);
					Debug.Log("delete unuse ab pack!   name -> " + files[i].Name);
				}

				if (File.Exists(files[i].FullName + ".manifest"))
				{
					File.Delete(files[i].FullName + ".manifest");
				}
			}
		}
	}

	/// <summary>
	/// 判断之前打的  ab  pack           是否存在于   即将 打的  ab	pack       
	/// </summary>
	/// <param name="name"></param>
	/// <param name="str"></param>
	/// <returns></returns>
	static bool ContansABName(string name, string[] str)
	{
		int length = str.Length;
		for (int i = 0; i < length; i++)
		{
			if (name == str[i])
			{
				return true;
			}
		}

		return false;
	}
	

	/// <summary>
	/// 文件的过滤
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	static bool ContansAllFileAB(string path)
	{
		int length = m_AllFileAB.Count;
		for (int i = 0; i < length; i++)
		{
			if (path == m_AllFileAB[i] ||
			    (path.Contains(m_AllFileAB[i]) && path.Replace(m_AllFileAB[i], "")[0] == '/'))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 判断有效路径  过滤 xml bytes  文件
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	static bool IsValidPath(string path)
	{
		int length = m_oValidFilePath.Count;
		for (int i = 0; i < length; i++)
		{
			string validPath = m_oValidFilePath[i];
			if (path.Contains(validPath)) return true;
		}

		return false;
	}

	/// <summary>
	/// 删除指定文件目录下的所有文件
	/// </summary>
	/// <param name="fullPath"></param>
	/// <returns></returns>
	public static void DeleteAllFile(string fullPath)
	{
		if (Directory.Exists(fullPath))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
			FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Name.EndsWith(".meta"))
				{
					continue;
				}

				File.Delete(files[i].FullName);
			}
		}
	}

}
