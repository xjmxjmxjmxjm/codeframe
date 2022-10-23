using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
public class BuildApp
{
	private static string m_AppName = PlayerSettings.productName;//RealConfig.GetRealFram().m_AppName;
	private static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
	private static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS/";
	private static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";
	
	//[MenuItem(("Build/Package/标准包"))]
	public static void Build()
	{
		//打ab包
		BundleEditor.NormalBuild();
		//生成执行程序
		
		string abPath = Application.dataPath + "/../AssetBundle/" +
		                EditorUserBuildSettings.activeBuildTarget.ToString() + "/";

		Copy(abPath, Application.streamingAssetsPath);
		
		string savePath = "";
		
		if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
		{
			PlayerSettings.Android.keyaliasName = "user";
			PlayerSettings.Android.keystoreName =
				Application.dataPath.Replace("Assets", "") + "Key/user.keystore";
			PlayerSettings.Android.keystorePass = "123456";
			PlayerSettings.Android.keyaliasPass = "123456";
			savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
			           string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
		}
		else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
		{
			savePath = m_IOSPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
			           string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
		}
		else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
		         EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
		{
			savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
			           string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, m_AppName);
		}

		
		BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath,
			EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);


		DeleteDir(Application.streamingAssetsPath);
	}

	[MenuItem("Tools/Version写入")]
	public static void Version()
	{
		SaveVersion(PlayerSettings.bundleVersion, PlayerSettings.applicationIdentifier);
		AssetDatabase.Refresh();
	}
	
	
	static void SaveVersion(string version, string package)
	{
		string content = "Version|" + version + ";PackageName|" + package + ";";
		string savePath = Application.dataPath + "/Resources/Version.txt";
		string oneLine = "";
		string all = "";
		using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
			FileShare.ReadWrite))
		{
			using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
			{
				all = sr.ReadToEnd();
				oneLine = all.Split('\r')[0];
			}
		}

		using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate))
		{
			using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
			{
				if (string.IsNullOrEmpty(all))
				{
					all = content;
				}
				else
				{
					all = all.Replace(oneLine, content);
				}
				sw.Write(all);
			}
		}
	}

	private static string[] FindEnableEditorScenes()
	{
		List<string> editorScenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled) continue;
			editorScenes.Add(scene.path);
		}

		return editorScenes.ToArray();
	}

	private static void Copy(string srcPath, string targetPath)
	{
		try
		{
			if (!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
			}

			string targetdir = Path.Combine(targetPath, Path.GetFileName(srcPath));
			if (Directory.Exists(srcPath))
			{
				targetdir += Path.DirectorySeparatorChar;
			}

			if (Directory.Exists(targetdir))
			{
				Directory.CreateDirectory(targetdir);
			}

			string[] files = Directory.GetFileSystemEntries(srcPath);

			foreach (string file in files)
			{
				if (Directory.Exists(file))
				{
					Copy(file, targetdir);
				}
				else
				{
					File.Copy(file, targetdir + Path.GetFileName(file), true);
				}
			}

		}
		catch (Exception e)
		{
			Debug.LogError("无法复制 ！  srcPath:" + srcPath + "到" + targetPath);
			throw;
		}
	}

	public static void DeleteDir(string srcPath)
	{
		try
		{
			DirectoryInfo dir = new DirectoryInfo(srcPath);
			FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();
			foreach (FileSystemInfo info in fileInfo)
			{
				if (info is DirectoryInfo)
				{
					DirectoryInfo subdir = new DirectoryInfo(info.FullName);
					subdir.Delete(true);
				}
				else
				{
					File.Delete(info.FullName);
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e);
			throw;
		}
	}

	#region 打包机调用android jenkins
	public static void BuildAndroid()
	{
		//打ab包
		BundleEditor.NormalBuild();
		
		PlayerSettings.Android.keyaliasName = "user";
		PlayerSettings.Android.keystoreName =
			Application.dataPath.Replace("Assets", "") + "AndroidStudioGrade/Keystore/user.keystore";
		PlayerSettings.Android.keystorePass = "123456";
		PlayerSettings.Android.keyaliasPass = "123456";

		BuildSetting buildSetting = GetAndroidBuildSetting();
		string suffix = SetAndroidBuildSetting(buildSetting);
		//生成执行程序
		string abPath = Application.dataPath + "/../AssetBundle/" +
		                EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
		
		//清空文件夹
		DeleteDir(m_AndroidPath);
		
		Copy(abPath, Application.streamingAssetsPath);
		
		string savePath = m_AndroidPath + m_AppName + "_Android" + suffix + string.Format("_{0:yyyy_MM_dd_HH_mm}.apk", DateTime.Now);

		
		BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath,
			EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);


		DeleteDir(Application.streamingAssetsPath);
	}
	

	#endregion

	static BuildSetting GetAndroidBuildSetting()
	{
		string[] parameters = Environment.GetCommandLineArgs();
		BuildSetting buildSetting = new BuildSetting();
		foreach (string str in parameters)
		{
			if (str.StartsWith("Place"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Place = (Place) Enum.Parse(typeof(Place), tempParam[1], true);
				}
			}
			else if (str.StartsWith("Version"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Version = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("Build"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Build = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("Name"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Name = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("Debug"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					bool.TryParse(tempParam[1], out buildSetting.Debug);
				}
			}
			else if (str.StartsWith("MulRendering"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					bool.TryParse(tempParam[1], out buildSetting.MulRendering);
				}
			}
			else if (str.StartsWith("IL2CPP"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					bool.TryParse(tempParam[1], out buildSetting.IL2CPP);
				}
			}
		}
		return buildSetting;
	}

	static string SetAndroidBuildSetting(BuildSetting buildSetting)
	{
		string suffix = "_";
		if (buildSetting.Place != Place.None)
		{
			//代表了渠道包
			string symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,
				symbol + ";" + buildSetting.Place.ToString());
			suffix += buildSetting.Place.ToString();
		}
		if (!string.IsNullOrEmpty(buildSetting.Version))
		{
			PlayerSettings.bundleVersion = buildSetting.Version;
			suffix += "_" + buildSetting.Version;
		}

		if (!string.IsNullOrEmpty(buildSetting.Build))
		{
			PlayerSettings.Android.bundleVersionCode = int.Parse(buildSetting.Build);
			suffix += "_" + buildSetting.Build;
		}

		if (!string.IsNullOrEmpty(buildSetting.Name))
		{
			PlayerSettings.productName = buildSetting.Name;
			//PlayerSettings.applicationIdentifier = "com.TTT." + buildSetting.Name;
		}

		if (buildSetting.MulRendering)
		{
			PlayerSettings.MTRendering = true;
			suffix += "_MTR";
		}
		else
		{
			PlayerSettings.MTRendering = false;
		}

		if (buildSetting.IL2CPP)
		{
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
			suffix += "_IL2CPP";
		}
		else
		{
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
		}
		
		EditorUserBuildSettings.development = buildSetting.Debug;
		EditorUserBuildSettings.connectProfiler = buildSetting.Debug;
		if (buildSetting.Debug)
		{
			suffix += "_" + "Debug";
		}
		else
		{
			suffix += "_" + "Release";
		}
		
		return suffix;
	}

	
	#region 打包机调用打包pc版本 jenkins
	public static void BuildPC()
	{
		//打ab包

		BuildSetting buildSetting = GetPCBuildSetting();

		if (buildSetting.IsHotFix)
		{
			BundleEditor.Build(buildSetting.IsHotFix, buildSetting.HotPath, buildSetting.HotCount.ToString());
			return;
		}
		
		
		BundleEditor.NormalBuild();
		
		
		string suffix = SetPCBuildSetting(buildSetting);
		
		
		
		//生成执行程序
		string abPath = Application.dataPath + "/../AssetBundle/" +
		                EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
		
		//清空文件夹
		DeleteDir(m_WindowsPath);
		
		Copy(abPath, Application.streamingAssetsPath);

		string dir = m_AppName + "_PC" + suffix + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
		string name = string.Format("/{0}.exe", m_AppName);

		string savePath = m_WindowsPath + dir + name;

		
		BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath,
			EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);


		DeleteDir(Application.streamingAssetsPath);
		WriteBuildName(dir);
	}
	

	#endregion

	public static void WriteBuildName(string name)
	{
		FileInfo fileInfo = new FileInfo(Application.dataPath+"/../buildname.txt");
		StreamWriter sw = fileInfo.CreateText();
		sw.WriteLine(name);
		sw.Close();
		sw.Dispose();
	}

	/// <summary>
	/// 根据jenkins的参数读取到buildsetting里
	/// </summary>
	/// <returns></returns>
	private static BuildSetting GetPCBuildSetting()
	{
		string[] parameters = Environment.GetCommandLineArgs();
		BuildSetting buildSetting = new BuildSetting();
		foreach (string str in parameters)
		{
			if (str.StartsWith("Version"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Version = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("Build"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Build = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("Name"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.Name = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("Debug"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					bool.TryParse(tempParam[1], out buildSetting.Debug);
				}
			}
			else if (str.StartsWith("IsHotFix"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					bool.TryParse(tempParam[1], out buildSetting.IsHotFix);
				}
			}
			else if (str.StartsWith("HotVerPath"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					buildSetting.HotPath = tempParam[1].Trim();
				}
			}
			else if (str.StartsWith("HotCount"))
			{
				var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
				if (tempParam.Length == 2)
				{
					int.TryParse(tempParam[1], out buildSetting.HotCount);
				}
			}
		}
		return buildSetting;
	}
	/// <summary>
	/// 根据读取的数据设置 buildSetting
	/// </summary>
	/// <returns></returns>
	private static string SetPCBuildSetting(BuildSetting buildSetting)
	{
		string suffix = "_";
		if (!string.IsNullOrEmpty(buildSetting.Version))
		{
			PlayerSettings.bundleVersion = buildSetting.Version;
			suffix += buildSetting.Version;
		}

		if (!string.IsNullOrEmpty(buildSetting.Build))
		{
			PlayerSettings.macOS.buildNumber = buildSetting.Build;
			suffix += "_" + buildSetting.Build;
		}

		if (!string.IsNullOrEmpty(buildSetting.Name))
		{
			PlayerSettings.productName = buildSetting.Name;
		}

		EditorUserBuildSettings.development = buildSetting.Debug;
		EditorUserBuildSettings.connectProfiler = buildSetting.Debug;
		if (buildSetting.Debug)
		{
			suffix += "_" + "Debug";
		}
		else
		{
			suffix += "_" + "Release";
		}

		return suffix;
	}










//	[MenuItem("ILRuntime/dll to/将文件转成 bytes 文件")]
//	public static void SetILRunTimeDllToTxt()
//	{
//		
//		string DLLPATH = "Assets/GameData/HotFix/HotFix.dll";
//		string PDBPATH = "Assets/GameData/HotFix/HotFix.pdb";
//
//		if (File.Exists(DLLPATH))
//		{
//			string targetPath = DLLPATH + ".bytes";
//			if (File.Exists(targetPath))
//			{
//				File.Delete(targetPath);
//			}
//			File.Move(DLLPATH, targetPath);
//		}
//		if (File.Exists(PDBPATH))
//		{
//			string targetPath = PDBPATH + ".bytes";
//			if (File.Exists(targetPath))
//			{
//				File.Delete(targetPath);
//			}
//			File.Move(PDBPATH, targetPath);
//		}
//
//		AssetDatabase.Refresh();
//	}
}

public class BuildSetting
{
	//版本号
	public string Version = "";
	//build次数
	public string Build = "";
	//程序名称
	public string Name = "";
	//是否debug
	public bool Debug = true;
	
	
	//android 附加
	public Place Place = Place.None;
	//多线程渲染
	public bool MulRendering = true;
	//是否 il2cpp
	public bool IL2CPP = false;
	
	//是否热更
	public bool IsHotFix = false;
	//对应原版本数据路径
	public string HotPath = "";
	//热更次数
	public int HotCount = 0;
}

public enum Place
{
	None = 0,
	XiaoMi,
	Bilibili,
	Huawei,
	Meizu,
	Weixin,
}












