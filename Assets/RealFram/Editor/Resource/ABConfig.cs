using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework;


[CreateAssetMenu(fileName = "ABConfig" , menuName = "CreateABConfig" , order = 0)]
public class ABConfig : ScriptableObject
{

	public List<string> m_AllPrefabPath = new List<string>();
	public List<FileDirABName> m_AllFileDirAB = new List<FileDirABName>();



	[Serializable]
	public struct FileDirABName
	{
		public string ABName;
		public string Path;
	}
}
