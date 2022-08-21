using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEditor;
using UnityEditor.Purchasing;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataEditor
{
	public static string XmlPath = RealConfig.GetRealFram().m_XmlPath;
	public static string BinaryPath = RealConfig.GetRealFram().m_BinaryPath;
	public static string ScriptsPath = RealConfig.GetRealFram().m_ScriptsPath;
	public static string ProtobufPath = RealConfig.GetRealFram().m_ProtobufPath;
	public static string ExcelPath = Application.dataPath + "/../Data/Excel/";
	public static string RegPath = Application.dataPath + "/../Data/Reg/";
	
	[MenuItem("Assets/类转xml")]
	public static void AssetsClassToXml()
	{
		Object[] objs = Selection.objects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayProgressBar("文件夹的 类 转 xml ", "正在扫描" + objs[i].name, 1.0f * i / objs.Length);
			ClassToXml(objs[i].name);
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Assets/Xml转Binary")]
	public static void AssetsXmlToBinary()
	{
		Object[] objs = Selection.objects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayProgressBar("文件夹的 xml 转 binary ", "正在扫描" + objs[i].name, 1.0f * i / objs.Length);
			XmlToBinary(objs[i].name);
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Assets/Xml转Excel")]
	public static void AssetsXmlToExcel()
	{
		Object[] objs = Selection.objects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayProgressBar("文件夹的 xml 转 excel ", "正在扫描" + objs[i].name, 1.0f * i / objs.Length);
			XmlToExcel(objs[i].name);
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Tools/(All)/XML/Xml转成Binary")]
	public static void AllXmlToBinary()
	{
		string path = Application.dataPath.Replace("Assets", "") + XmlPath;
		string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		for (int i = 0; i < filesPath.Length; i++)
		{
			EditorUtility.DisplayProgressBar("文件夹的 all xml 转 binary ", "正在扫描" + filesPath[i], 1.0f * i / filesPath.Length);
			if (filesPath[i].EndsWith(".xml"))
			{
				string name = Path.GetFileName(filesPath[i]).Replace(".xml", "");
				XmlToBinary(name);
			}
		}
		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Tools/(All)/EXCEL/Excel转xml")]
	public static void AllExcelToXml()
	{
		string[] filePaths = Directory.GetFiles(RegPath, "*", SearchOption.AllDirectories);
		for (int i = 0; i < filePaths.Length; i++)
		{
			if (!filePaths[i].EndsWith(".xml"))
			{
				continue;
			}

			string temp = filePaths[i].Substring(filePaths[i].LastIndexOf("/") + 1);
			ExcelToXml(temp.Replace(".xml", ""));
			EditorUtility.DisplayProgressBar("文件夹的 all exel 转 xml ", "正在扫描" + filePaths[i], 1.0f * i / filePaths.Length);
		}
		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}
	
	
	[MenuItem("Assets/Xml转Protobuf")]
	public static void AssetsXmlToProtobuf()
	{
		Object[] objs = Selection.objects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayProgressBar("文件夹的 xml 转 protobuf ", "正在扫描" + objs[i].name, 1.0f * i / objs.Length);
			XmlToProtobuf(objs[i].name);
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}
	
	[MenuItem("Tools/(All)/Protobuf/Xml转成Protobuf")]
	public static void AllXmlToProtobuf()
	{
		string path = Application.dataPath.Replace("Assets", "") + XmlPath;
		string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		for (int i = 0; i < filesPath.Length; i++)
		{
			EditorUtility.DisplayProgressBar("文件夹的 all xml 转 Protobuf ", "正在扫描" + filesPath[i], 1.0f * i / filesPath.Length);
			if (filesPath[i].EndsWith(".xml"))
			{
				string name = Path.GetFileName(filesPath[i]).Replace(".xml", "");
				XmlToProtobuf(name);
			}
		}
		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}
	

#region Test

	[MenuItem("Tools/TEST/测试protobuf")]
	public static void TestProtoBuf()
	{
		string path = "Assets/GameData/Data/ProtoBufData/MonsterData.bytes";
		MonsterData data = BinarySerializeOpt.ProtoDeSerialize<MonsterData>(path);

		for (int i = 0; i < data.AllMonster.Count; i++)
		{
			Debug.Log(data.AllMonster[i].Id);
			Debug.Log(data.AllMonster[i].Name);
			Debug.Log(data.AllMonster[i].OutLook);
			Debug.Log(data.AllMonster[i].Level);
		}
		
		for (int i = 0; i < data.AllSuperMonster.Count; i++)
		{
			Debug.Log(data.AllSuperMonster[i].Id);
			Debug.Log(data.AllSuperMonster[i].Hp);
			Debug.Log(data.AllSuperMonster[i].OutLook);
			Debug.Log(data.AllSuperMonster[i].Level);
		}
	}
	
	
	[MenuItem("Tools/TEST/测试读取 xml")]
	public static void TestReadXml()
	{
		string xmlpath = Application.dataPath + "/../Data/Reg/MonsterData.xml";

		XmlReader reader = null;
		try
		{
			XmlDocument xml = new XmlDocument();
			reader = XmlReader.Create(xmlpath);
			xml.Load(reader);

			XmlNode xn = xml.SelectSingleNode("data");
			XmlElement xe = (XmlElement)xn;
			string className = xe.GetAttribute("name");
			string xmlName = xe.GetAttribute("to");
			string excelName = xe.GetAttribute("from");
			reader.Close();
			
//			Debug.Log(className);
//			Debug.Log(xmlName);
//			Debug.Log(excelName);

			foreach (XmlNode node in xe.ChildNodes)
			{
				XmlElement tempXe = node as XmlElement;
				string name = tempXe.GetAttribute("name");
				string type = tempXe.GetAttribute("type");
				Debug.Log(name);
				Debug.Log(type);
				XmlNode listNode = tempXe.FirstChild;
				XmlElement listEle = listNode as XmlElement;
				string listName = listEle.GetAttribute("name");
				string sheetName = listEle.GetAttribute("sheetname");
				string mainKey = listEle.GetAttribute("mainKey");
				Debug.Log(listName);
				Debug.Log(sheetName);
				Debug.Log(mainKey);

				foreach (XmlNode nd in listEle.ChildNodes)
				{
					XmlElement ndXe = nd as XmlElement;
					string ndname = ndXe.GetAttribute("name");
					string ndcol = ndXe.GetAttribute("col");
					string ndtype = ndXe.GetAttribute("type");
					Debug.Log(ndname);
					Debug.Log(ndcol);
					Debug.Log(ndtype);
				}
			}
			
		}
		catch (Exception e)
		{
			if (reader != null)
			{
				reader.Close();
			}
			Debug.Log(e);
		}
		
	}
	
	[MenuItem("Tools/TEST/测试写入 xlsx")]
	public static void TestWriteXml()
	{
		string xlsxpath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";

		FileInfo xlsxFile = new FileInfo(xlsxpath);
		if (xlsxFile.Exists)
		{
			xlsxFile.Delete();
			xlsxFile = new FileInfo(xlsxpath);
		}

		using (ExcelPackage package = new ExcelPackage(xlsxFile))
		{
			ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("怪物配置");

			worksheet.View.FreezePanes(2, 2);
			worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
			worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
			worksheet.Cells.AutoFitColumns();
			
			ExcelRange range = worksheet.Cells[1, 1];
			range.Style.WrapText = true;
			range.Value = "xxxxxx";
			package.Save();
		}


	}

	[MenuItem("Tools/TEST/测试已有类反射")]
	public static void TestReflection()
	{
		TestInfo testInfo = new TestInfo
		{
			Id = 2,
			Name = "测试反射",
			IsA = false,
			AllStrList = new List<string>(),
			AllTestInfoList = new List<TestInfoTwo>(),
		};
		
		testInfo.AllStrList.Add("x");
		testInfo.AllStrList.Add("j");
		testInfo.AllStrList.Add("m");

		for (int i = 0; i < 3; i++)
		{
			TestInfoTwo t = new TestInfoTwo
			{
				Id = i,
				Name = i + "xxx",
			};
			testInfo.AllTestInfoList.Add(t);
		}

//		object nameValue = null;
//		object IdValue = null;
//		object strLst = null;
//		nameValue = GetMemberValue(testInfo, "Name");
//		IdValue = GetMemberValue(testInfo, "Id");
//		strLst = GetMemberValue(testInfo, "AllStrList");
//		Debug.LogError(nameValue);
//		Debug.LogError(IdValue);
//		int count = Convert.ToInt32(strLst.GetType().InvokeMember("get_Count",
//			BindingFlags.Default | BindingFlags.InvokeMethod, null, strLst, new object[] { }));
//
//		for (int i = 0; i < count; i++)
//		{
//			object item = strLst.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
//				null, strLst, new object[] {i});
//			Debug.LogError(item);
//		}
		
		object testInfoLst = null;
		testInfoLst = GetMemberValue(testInfo, "AllTestInfoList");
		int count = Convert.ToInt32(testInfoLst.GetType().InvokeMember("get_Count",
			BindingFlags.Default | BindingFlags.InvokeMethod, null, testInfoLst, new object[] { }));

		for (int i = 0; i < count; i++)
		{
			object item = testInfoLst.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
				null, testInfoLst, new object[] {i});
			object id = GetMemberValue(item, "Id");
			object name = GetMemberValue(item, "Name");
			Debug.LogError(id);
			Debug.LogError(name);
		}
		
	}

	[MenuItem("Tools/TEST/测试已有数据反射")]
	public static void TestReflection2()
	{
		object obj = CreateClass("TestInfo");
		PropertyInfo info = obj.GetType().GetProperty("Id");
		//info.SetValue(obj, 10);
		SetValue(info, obj, "10", "int");
		info = obj.GetType().GetProperty("Name");
		//info.SetValue(obj, "xjm");
		SetValue(info, obj, "xjm", "string");
		info = obj.GetType().GetProperty("IsA");
		//info.SetValue(obj, false);
		SetValue(info, obj, "true", "bool");
		info = obj.GetType().GetProperty("Height");
		//info.SetValue(obj, 3.2f);
		SetValue(info, obj, "3.2", "float");
		info = obj.GetType().GetProperty("testEnum");
		//object infoValue = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString("VAR2"); 
		//info.SetValue(obj, infoValue);
		SetValue(info, obj, "VAR2", "enum");

		Type type = typeof(string);
		object list = CreateList(type);
		for (int i = 0; i < 3; i++)
		{
			object addItem = "测试填数据" + i;
			list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
				new[] {addItem});
		}

		obj.GetType().GetProperty("AllStrList").SetValue(obj, list);

		Type twoType = typeof(TestInfoTwo);
		object twolist = CreateList(twoType);
		for (int i = 0; i < 3; i++)
		{
			object addItem = CreateClass("TestInfoTwo");
			PropertyInfo itemInfoId = addItem.GetType().GetProperty("Id");
			SetValue(itemInfoId, addItem, "152", "int");
			PropertyInfo itemInfoName = addItem.GetType().GetProperty("Name");
			SetValue(itemInfoName, addItem, "测试类-" + i, "string");
			twolist.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, twolist,
				new[] {addItem});
		}

		obj.GetType().GetProperty("AllTestInfoList").SetValue(obj, twolist);
		
//		Debug.LogError(GetMemberValue(obj,"Id"));
//		Debug.LogError(GetMemberValue(obj,"Name"));
//		Debug.LogError(GetMemberValue(obj,"IsA"));
//		Debug.LogError(GetMemberValue(obj,"Height"));
//		Debug.LogError((TestEnum)GetMemberValue(obj,"testEnum"));
		TestInfo testInfo = (obj as TestInfo);
		foreach (string str in testInfo.AllStrList)
		{
			Debug.LogError(str);
		}
		foreach (TestInfoTwo str in testInfo.AllTestInfoList)
		{
			Debug.LogError(str.Id);
			Debug.LogError(str.Name);
		}
	}

	
#endregion

	
	private static void ExcelToXml(string name)
	{
		string className = string.Empty;
		string xmlName = string.Empty;
		string excelName = string.Empty;
		//第一步 读取reg文件 确定类的结构
		Dictionary<string, SheetClass> allSheetClassDic = ReadReg(name, ref excelName, ref xmlName, ref className);
		//第二步 读取excel里面的数据
		string excelPath = ExcelPath + excelName;
		Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
		try
		{
			using (FileStream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) 
			{
				using (ExcelPackage package = new ExcelPackage(stream))
				{
					ExcelWorksheets worksheetArray = package.Workbook.Worksheets;
					int sheetCount = worksheetArray.Count;
					for (int i = 0; i < sheetCount; i++)
					{
						SheetData sheetData = new SheetData();
						ExcelWorksheet worksheet = worksheetArray[i + 1];
						SheetClass sheetClass = allSheetClassDic[worksheet.Name];
						int colCount = worksheet.Dimension.End.Column;
						int rowCount = worksheet.Dimension.End.Row;
						for (int j = 0; j < sheetClass.VarList.Count; j++)
						{
							sheetData.AllName.Add(sheetClass.VarList[j].Name);
							sheetData.AllType.Add(sheetClass.VarList[j].Type);
						}

						for (int j = 1; j < rowCount; j++)
						{
							RowData rowData = new RowData();
							int k = 0;
							if (string.IsNullOrEmpty(sheetClass.SplitStr) && sheetClass.ParentVar != null &&
							    !string.IsNullOrEmpty(sheetClass.ParentVar.Foregin))
							{
								rowData.ParentValue = worksheet.Cells[j + 1, 1].Value.ToString();
								k = 1;
							}

							for (; k < colCount; k++)
							{
								ExcelRange range = worksheet.Cells[j + 1, k + 1];
								string value = "";
								if (range.Value != null)
								{
									value = range.Value.ToString().Trim();
								}
								string colValue = worksheet.Cells[1, k + 1].Value.ToString().Trim();
								rowData.RowDataDic.Add(
									GetNameFromCol(sheetClass.VarList, colValue),
									value
								);

							}
							sheetData.AllData.Add(rowData);
						}

						sheetDataDic.Add(worksheet.Name, sheetData);
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e);
			return;
		}
		//第三步 根据类的结构 创建类 并且给每个变量赋值 从excel里读出来的值
		object objClass = CreateClass(className);

		List<string> outKeyList = new List<string>();
		foreach (string str in allSheetClassDic.Keys)
		{
			SheetClass sheetClass = allSheetClassDic[str];
			if (sheetClass.Depth == 1)
			{
				outKeyList.Add(str);
			}
		}

		for (int i = 0; i < outKeyList.Count; i++)
		{
			ReadDataToClass(objClass, allSheetClassDic[outKeyList[i]], sheetDataDic[outKeyList[i]]
				, allSheetClassDic, sheetDataDic, null);
		}

		BinarySerializeOpt.Xmlserialize(XmlPath + xmlName, objClass);
		Debug.Log(excelName + "表 导入 unity 完成");
		//AssetDatabase.Refresh();
	}
	
	
	
	
	private static void ReadDataToClass(object objClass, SheetClass sheetClass, SheetData sheetData,
		Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic, object keyValue)
	{
		object item = CreateClass(sheetClass.Name); //只是为了得到变量类型
		object list = CreateList(item.GetType());

		for (int i = 0; i < sheetData.AllData.Count; i++)
		{
			if (keyValue != null && !string.IsNullOrEmpty(sheetData.AllData[i].ParentValue))
			{
				if (sheetData.AllData[i].ParentValue != keyValue.ToString())
				{
					continue;
				}
			}	
			object addItem = CreateClass(sheetClass.Name);
			for (int j = 0; j < sheetClass.VarList.Count; j++)
			{
				VarClass varClass = sheetClass.VarList[j];
				if (varClass.Type == "list" && string.IsNullOrEmpty(varClass.SplitStr))
				{
					ReadDataToClass(addItem, allSheetClassDic[varClass.ListSheetName],
						sheetDataDic[varClass.ListSheetName], allSheetClassDic, sheetDataDic,
						GetMemberValue(addItem, sheetClass.MainKey));
				}
				else if (varClass.Type == "list")
				{
					string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
					SetSplitClass(addItem, allSheetClassDic[varClass.ListSheetName], value);
				}
				else if (varClass.Type == "listStr" || varClass.Type == "listFloat" || varClass.Type == "listInt" ||
				         varClass.Type == "listBool")
				{
					string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
					SetSplitBaseClass(addItem, varClass, value);
				}
				else
				{
					string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
					if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DefaultValue))
					{
						value = varClass.DefaultValue;
					}

					if (string.IsNullOrEmpty(value))
					{
						Debug.LogError(sheetClass.SheetName + "表中空数据 --- reg文件未配置默认值！" + sheetData.AllName[j]);
						continue;
					}
					SetValue(addItem.GetType().GetProperty(sheetData.AllName[j]), addItem, value, sheetData.AllType[j]);
				}
			}

			list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
				new object[] {addItem});
		}

		objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
	}


	/// <summary>
	/// 自定义list赋值
	/// </summary>
	/// <param name="objClass"></param>
	/// <param name="sheetClass"></param>
	/// <param name="value"></param>
	private static void SetSplitClass(object objClass, SheetClass sheetClass, string value)
	{
		object item = CreateClass(sheetClass.Name);
		object list = CreateList(item.GetType());
		if (string.IsNullOrEmpty(value))
		{
			Debug.Log("excel里面自定义list的列里有空值!" + sheetClass.SheetName);
			return;
		}
		else
		{
			string splitStr = sheetClass.ParentVar.SplitStr.Replace("\\n", "\n");
			string[] rowArray = value.Split(new string[] {splitStr}, StringSplitOptions.None);

			for (int i = 0; i < rowArray.Length; i++)
			{
				object addItem = CreateClass(sheetClass.Name);
				string[] valueArray = rowArray[i].Trim().Split(new string[] {sheetClass.SplitStr}, StringSplitOptions.None);
				for (int j = 0; j < valueArray.Length; j++)
				{
					SetValue(addItem.GetType().GetProperty(sheetClass.VarList[j].Name),addItem,valueArray[j].Trim(),sheetClass.VarList[j].Type);
				}
			
				list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
					new object[] {addItem});
			}
		}
		
		objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
	}
	
	/// <summary>
	/// 基础 list 赋值
	/// </summary>
	/// <param name="objClass"></param>
	/// <param name="varClass"></param>
	/// <param name="value"></param>
	private static void SetSplitBaseClass(object objClass,VarClass varClass, string value)
	{
		Type type = null;
		if (varClass.Type == "listStr")
		{
			type = typeof(string);
		}
		else if (varClass.Type == "listFloat")
		{
			type = typeof(float);
		}
		else if (varClass.Type == "listInt")
		{
			type = typeof(int);
		}
		else if (varClass.Type == "listBool")
		{
			type = typeof(bool);
		}

		object list = CreateList(type);
		string[] rowArray = value.Split(new string[] {varClass.SplitStr}, StringSplitOptions.None);
		for (int i = 0; i < rowArray.Length; i++)
		{
			object addItem = rowArray[i].Trim();
			try
			{
				list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
					new object[] {addItem});
			}
			catch (Exception e)
			{
				Debug.Log(varClass.ListSheetName + "里" + varClass.Name + "列表添加失败！");
			}
		}

		objClass.GetType().GetProperty(varClass.Name).SetValue(objClass, list);
	}
	
	/// <summary>
	/// 根据列名获取变量名
	/// </summary>
	/// <param name="varClasses"></param>
	/// <param name="col"></param>
	/// <returns></returns>
	private static string GetNameFromCol(List<VarClass> varClasses, string col)
	{
		foreach (VarClass varClass in varClasses)
		{
			if (varClass.Col == col)
			{
				return varClass.Name;
			}
		}

		return null;
	}
	


	//[MenuItem("Tools/XML/Xml转Excel")]
	private static void XmlToExcel(string name)
	{
		//string name = "BuffData";
		
		string className = string.Empty;
		string xmlName = string.Empty;
		string excelName = string.Empty;
		Dictionary<string, SheetClass> allSheetClassDic = ReadReg(name, ref excelName, ref xmlName, ref className);
		Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
		
		object data = GetObjFromXml(className);
		
		if(data == null)
		{
			Debug.LogError("XmlToExcel  程序集中未找到对应的类" + className);
		}
		
		List<SheetClass> outSheetList = new List<SheetClass>();
		foreach (SheetClass sheetClass in allSheetClassDic.Values)
		{
			if (sheetClass.Depth == 1)
			{
				outSheetList.Add(sheetClass);
			}
		}

		for (int i = 0; i < outSheetList.Count; i++)
		{
			ReadData(data, outSheetList[i], allSheetClassDic, sheetDataDic, null);
		}

		string xlsxPath = ExcelPath + excelName;
		if (FileIsUsed(xlsxPath))
		{
			Debug.LogError(xlsxPath + "   xlsx 已经被打开  无法修改!");
			return;
		}

		try
		{
			FileInfo xlsxFile = new FileInfo(xlsxPath);
			if (xlsxFile.Exists)
			{
				xlsxFile.Delete();
				xlsxFile = new FileInfo(xlsxPath);
			}

			using (ExcelPackage package = new ExcelPackage(xlsxFile))
			{
				
				foreach (string str in sheetDataDic.Keys)
				{
					ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(str);
					worksheet.View.FreezePanes(2, 2);
					worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
					worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
					
					SheetData sheetData = sheetDataDic[str];

					for (int i = 0; i < sheetData.AllName.Count; i++)
					{
						ExcelRange range = worksheet.Cells[1, i + 1];
						range.Value = sheetData.AllName[i];
						range.AutoFitColumns();
					}

					for (int i = 0; i < sheetData.AllData.Count; i++)
					{
						RowData rowData = sheetData.AllData[i];
						for (int j = 0; j < sheetData.AllData[i].RowDataDic.Count; j++)
						{
							ExcelRange range = worksheet.Cells[i + 2, j + 1];
							string value = rowData.RowDataDic[sheetData.AllName[j]];
							range.Value = rowData.RowDataDic[sheetData.AllName[j]];
							range.AutoFitColumns();
							if (value.Contains("\n")) 
							{
								range.Style.WrapText = true;
							}
						}
					}
					worksheet.Cells.AutoFitColumns();

				}
				
				package.Save();
			}
		}
		catch (Exception e)
		{
			Debug.Log("生成 " + xlsxPath + " 失败");
			Debug.LogError(e);
		}
		Debug.Log("生成 " + xlsxPath + " 成功");
	}

	private static Dictionary<string, SheetClass> ReadReg(string name, ref string excelName,ref string xmlName,ref string className)
	{
		//储存所有变量的表
		Dictionary<string, SheetClass> allSheetClassDic = new Dictionary<string, SheetClass>();
		string regPath = RegPath + name + ".xml";
		if (!File.Exists(regPath))
		{
			Debug.LogError("此数据不存在配置转换 xml :" + name);
			return allSheetClassDic;
		}
		
		XmlDocument xml = new XmlDocument();
		XmlReader reader = null;
		try
		{
			reader = XmlReader.Create(regPath);
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true; //忽略xml里面的注释
			xml.Load(reader);
			XmlNode xn = xml.SelectSingleNode("data");
			XmlElement xe = xn as XmlElement;
			className = xe.GetAttribute("name");
			xmlName = xe.GetAttribute("to");
			excelName = xe.GetAttribute("from");
			ReadXmlNode(xe, allSheetClassDic, 0);
		}
		catch (Exception e)
		{

		}
		finally
		{
			if (reader != null)
			{
				reader.Close();
			}
		}
		
		return allSheetClassDic;
	}

	/// <summary>
	/// 返序列化 xml 到 类
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private static object GetObjFromXml(string name)
	{
		object data = null;
		Type type = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type tempType = assembly.GetType(name);
			if (tempType != null)
			{
				type = tempType;
				break;
			}
		}

		if (type != null)
		{
			string xmlpath = XmlPath + name + ".xml";
			data = BinarySerializeOpt.XmlDeserialize(xmlpath, type);
		}

		return data;
	}

	/// <summary>
	/// 递归读取类里面的数据
	/// </summary>
	/// <param name="data"></param>
	/// <param name="sheetClass"></param>
	/// <param name="allSheetClassDic"></param>
	/// <param name="sheetDataDic"></param>
	private static void ReadData(object data, SheetClass sheetClass, Dictionary<string, SheetClass> allSheetClassDic,
		Dictionary<string, SheetData> sheetDataDic, string mainKey)
	{
		List<VarClass> varList = sheetClass.VarList;
		VarClass varClass = sheetClass.ParentVar;

		object dataList = GetMemberValue(data, varClass.Name);

		int listCount = Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",
			BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

		SheetData sheetData = new SheetData();

		if (!string.IsNullOrEmpty(varClass.Foregin))
		{
			sheetData.AllName.Add(varClass.Foregin);
			sheetData.AllType.Add(varClass.Type);
		}

		for (int i = 0; i < varList.Count; i++)
		{
			if (!string.IsNullOrEmpty(varList[i].Col))
			{
				sheetData.AllName.Add(varList[i].Col);
				sheetData.AllType.Add(varList[i].Type);
			}
			else
			{
				//Debug.LogError(sheetClass.Name + ".xml reg file -> col or type  error！");
			}
		}

		string tempKey = mainKey;

		for (int i = 0; i < listCount; i++)
		{
			object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
				null, dataList, new object[] { i });
			
			RowData rowData = new RowData();

			if (!string.IsNullOrEmpty(varClass.Foregin) && !string.IsNullOrEmpty(tempKey))
			{
				rowData.RowDataDic.Add(varClass.Foregin, tempKey);
			}

			if (!string.IsNullOrEmpty(sheetClass.MainKey))
			{
				mainKey = GetMemberValue(item, sheetClass.MainKey).ToString();
			}

			bool result = true;
			
			for (int j = 0; j < varList.Count; j++)
			{
			
				if (varList[j].Type == "list" && string.IsNullOrEmpty(varList[j].SplitStr))
				{
					SheetClass tempSheetClass = allSheetClassDic[varList[j].ListSheetName];
					ReadData(item, tempSheetClass, allSheetClassDic, sheetDataDic, mainKey);
				}
				else if (varList[j].Type == "list")
				{
					SheetClass tempSheetClass = allSheetClassDic[varList[j].ListSheetName];
					string valueBase = GetSplitStrList(item, varList[j], tempSheetClass);
					if (valueBase != null)
					{
						rowData.RowDataDic.Add(varList[j].Col, valueBase);
					}
					else
					{
						result = false;
					}
				}
				else if (varList[j].Type == "listStr" || varList[j].Type == "listFloat" ||
				         varList[j].Type == "listInt" || varList[j].Type == "listBool")
				{
					string valueBase = GetSplitBaseList(item, varList[j]);
					if (valueBase != null)
					{
						rowData.RowDataDic.Add(varList[j].Col, valueBase);
					}
					else
					{
						result = false;
					}
				}
				else
				{
					object value = GetMemberValue(item, varList[j].Name);
					if (value != null)
					{
						rowData.RowDataDic.Add(varList[j].Col, value.ToString());
					}
					else
					{
						result = false;
					}
				}

				if (result == false)
				{
					Debug.LogError(varList[j].Name + "反射出来为空！ -> " + sheetClass.Name);
				}
			}

			string key = varClass.ListSheetName;
			if (sheetDataDic.ContainsKey(key))
			{
				sheetDataDic[key].AllData.Add(rowData);
			}
			else
			{
				sheetData.AllData.Add(rowData);
				sheetDataDic.Add(key, sheetData);
			}
		}

	}

	
	/// <summary>
	/// 获取本身是一个类的列表  但是数据比较少  没办法确定父级结构的
	/// </summary>
	/// <returns></returns>
	private static string GetSplitStrList(object data, VarClass varClass, SheetClass sheetClass)
	{
		string str = "";
		string split = varClass.SplitStr;
		string classSplit = sheetClass.SplitStr;

		if (string.IsNullOrEmpty(split) || string.IsNullOrEmpty(classSplit))
		{
			Debug.LogError(varClass.Name + "分割符为空！");
			return str;
		}

		object dataList = GetMemberValue(data, varClass.Name);
		int listCount = Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",
			BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));
		for (int i = 0; i < listCount; i++)
		{
			object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
				null, dataList, new object[] {i});
			
			for (int j = 0; j < sheetClass.VarList.Count; j++)
			{
				object insideData = GetMemberValue(item, sheetClass.VarList[j].Name);


				str += insideData.ToString();
				
				if (j != (sheetClass.VarList.Count - 1))
				{
					str += classSplit.Replace("\\n", "\n");
				}
			}
			if (i != (listCount - 1))
			{
				str += split.Replace("\\n", "\n");
			}
		}

		return str;
	}
	
	
	/// <summary>
	/// 获取基础list里面的所有值
	/// </summary>
	/// <returns></returns>
	private static string GetSplitBaseList(object data, VarClass varClass)
	{
		string str = "";
		if (string.IsNullOrEmpty(varClass.SplitStr))
		{
			Debug.LogError(varClass.Name + "基础list的分割符为空！");
			return str;
		}
		object dataList = GetMemberValue(data, varClass.Name);
		int listCount = Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",
			BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

		for (int i = 0; i < listCount; i++)
		{
			object item = dataList.GetType().InvokeMember("get_Item",
				BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] {i});

			str += item.ToString();
			if (i != (listCount - 1))
			{
				str += varClass.SplitStr.Replace("\\n", "\n");
			}
		}

		return str;
	}

	/// <summary>
	/// 递归读取配置
	/// </summary>
	/// <param name="xe"></param>
	private static void ReadXmlNode(XmlElement xmlElement, Dictionary<string, SheetClass> allSheetClassDic,
		int Depth)
	{
		Depth++;
		foreach (XmlNode node in xmlElement.ChildNodes)
		{
			XmlElement xe = node as XmlElement;
			if (xe.GetAttribute("type") == "list")
			{
				XmlElement listEle = (XmlElement)node.FirstChild;
				VarClass parentVar = new VarClass
				{
					Name = xe.GetAttribute("name"),
					Type = xe.GetAttribute("type"),
					Col = xe.GetAttribute("col"),
					DefaultValue = xe.GetAttribute("defaultValue"),
					Foregin = xe.GetAttribute("foregin"),
					SplitStr = xe.GetAttribute("split"),
				};
				if (parentVar.Type == "list")
				{
					parentVar.ListName = ((XmlElement) xe.FirstChild).GetAttribute("name");
					parentVar.ListSheetName = ((XmlElement) xe.FirstChild).GetAttribute("sheetname");
				}
				
				SheetClass sheetClass = new SheetClass
				{
					Name = listEle.GetAttribute("name"),
					SheetName = listEle.GetAttribute("sheetname"),
					SplitStr = listEle.GetAttribute("split"),
					MainKey = listEle.GetAttribute("mainKey"),
					ParentVar = parentVar,
					Depth = Depth,
				};
				if (!string.IsNullOrEmpty(sheetClass.SheetName))
				{
					if (!allSheetClassDic.ContainsKey(sheetClass.SheetName))
					{
						//获取该类下面所有的变量
						foreach (XmlNode insideNode in listEle.ChildNodes)
						{
							XmlElement insideXe = insideNode as XmlElement;
							VarClass varClass = new VarClass
							{
								Name = insideXe.GetAttribute("name"),
								Type = insideXe.GetAttribute("type"),
								Col = insideXe.GetAttribute("col"),
								DefaultValue = insideXe.GetAttribute("defaultValue"),
								Foregin = insideXe.GetAttribute("foregin"),
								SplitStr = insideXe.GetAttribute("split"),
							};
							if (varClass.Type == "list")
							{
								varClass.ListName = ((XmlElement) insideXe.FirstChild).GetAttribute("name");
								varClass.ListSheetName = ((XmlElement) insideXe.FirstChild).GetAttribute("sheetname");
							}
							sheetClass.VarList.Add(varClass);
						}
						allSheetClassDic.Add(sheetClass.SheetName, sheetClass);
					}
				}
				
				ReadXmlNode(listEle, allSheetClassDic, Depth);
			}
		}
	}

	/// <summary>
	/// 判断文件是否被占用
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static bool FileIsUsed(string path)
	{
		bool result = false;
		if (!File.Exists(path))
		{
			result = false;
		}
		else
		{
			FileStream fs = null;
			try
			{
				fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				result = false;
			}
			catch (Exception e)
			{
				result = true;
				Debug.LogError(e);
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}

		return result;
	}
	
	/// <summary>
	/// 反射 new 一个 List
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	private static object CreateList(Type type)
	{
		Type lstType = typeof(List<>);
		Type specType = lstType.MakeGenericType(new Type[] {type});
		return Activator.CreateInstance(specType, new object[] { });
	}
	

	/// <summary>
	/// 反射变量赋值
	/// </summary>
	/// <param name="info">obj 的 熟悉    obj.GetType().GetProperty("Id");</param>
	/// <param name="obj">类</param>
	/// <param name="value">要赋的值</param>
	/// <param name="type"></param>
	private static void SetValue(PropertyInfo info, object obj, string value, string type)
	{
		object val = (object) value;
		if (type == "int")
		{
			val = Convert.ToInt32(value);
		}
		else if (type == "bool")
		{
			val = Convert.ToBoolean(value);
		}
		else if (type == "float")
		{
			val = Convert.ToSingle(value);
		}
		else if (type == "enum")
		{
			val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(value.ToString());
		}

		info.SetValue(obj, val);
	}


	/// <summary>
	/// 反射类里面的变量的具体数值
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="memberName"></param>
	/// <param name="flags"></param>
	/// <returns></returns>
	private static object GetMemberValue(object obj, string memberName,
		BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
	{
		Type type = obj.GetType();
		MemberInfo[] members = type.GetMember(memberName, flags);
		
		switch (members[0].MemberType)
		{
			case MemberTypes.Field:
				return type.GetField(memberName, flags).GetValue(obj);
				break;
			case MemberTypes.Property:
				return type.GetProperty(memberName, flags).GetValue(obj);
				break;
			default:
				break;
		}

		return null;
	}

	/// <summary>
	/// 反射创建类的实例
	/// </summary>
	/// <returns></returns>
	private static object CreateClass(string name)
	{
		object obj = null;
		Type type = null;
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			Type tempType = asm.GetType(name);
			if (tempType != null)
			{
				type = tempType;
				break;
			}
		}

		if (type != null)
		{
			obj = Activator.CreateInstance(type);
		}
		return obj;
	}
	
	/// <summary>
	/// xml 转 binary
	/// </summary>
	/// <param name="name"></param>
	private static void XmlToBinary(string name)
	{
		if (string.IsNullOrEmpty(name))
			return;

		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type tempType = assembly.GetType(name);
				if (tempType != null)
				{
					type = tempType;
					break;
				}
			}

			if (type != null)
			{
				//var temp = Activator.CreateInstance(type);
				string xmlpath = XmlPath + name + ".xml";
				string binarypath = BinaryPath + name + ".bytes";
				object obj = BinarySerializeOpt.XmlDeserialize(xmlpath, type);
				BinarySerializeOpt.BinarySerialize(binarypath, obj);
				Debug.Log(name + "xml 转 binary 成功 ， binary路径为：" + binarypath);
			}
		}
		catch (Exception e)
		{
			Debug.LogError(name + "xml 转 binary 失败");
		}
	}

	/// <summary>
	/// xml 转 protobuf
	/// </summary>
	/// <param name="name"></param>
	private static void XmlToProtobuf(string name)
	{
		if (string.IsNullOrEmpty(name))
			return;

		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type tempType = assembly.GetType(name);
				if (tempType != null)
				{
					type = tempType;
					break;
				}
			}

			if (type != null)
			{
				//var temp = Activator.CreateInstance(type);
				string xmlpath = XmlPath + name + ".xml";
				string protopath = ProtobufPath + name + ".bytes";
				object obj = BinarySerializeOpt.XmlDeserialize(xmlpath, type);
				BinarySerializeOpt.ProtoSerialize(protopath, obj);
				Debug.Log(name + "xml 转 protobuf 成功 ， protopath：" + protopath);
			}
		}
		catch (Exception e)
		{
			Debug.LogError(name + "xml 转 protobuf 失败");
		}
	}
	
	
	/// <summary>
	/// 实际的类转 xml
	/// </summary>
	/// <param name="name"></param>
	private static void ClassToXml(string name)
	{
		if (string.IsNullOrEmpty(name))
			return;
		try
		{
			var temp = CreateClass(name);
#if UNITY_EDITOR
			if (temp is ExcelBase)
			{
				(temp as ExcelBase).Construction();
			}
#endif
			string xmlpath = XmlPath + name + ".xml";
			BinarySerializeOpt.Xmlserialize(xmlpath, temp);
			Debug.Log(name + "类 转 xml 成功 ， xml路径为：" + xmlpath);
		}
		catch (Exception e)
		{
			Debug.LogError(name + "类 转 xml 失败");
		}
		
	}
}

public class SheetClass
{
	//所属父级别 var 变量
	public VarClass ParentVar { get; set; }
	//深度
	public int Depth { get; set; }
	//类名
	public string Name { get; set; }
	//类对应表的sheet名
	public string SheetName { get; set; }
	//主键
	public string MainKey { get; set; }
	//分隔符
	public string SplitStr { get; set; }
	
	//所包含的变量
	public List<VarClass> VarList = new List<VarClass>();
}

public class VarClass
{
	//原类里面变量的名称
	public string Name { get; set; }
	//变量类型
	public string Type { get; set; }
	//变量对应 excel  列
	public string Col { get; set; }
	//变量的默认值
	public string DefaultValue { get; set; }
	//变量是list的话 外联部分列
	public string Foregin { get; set; }
	//分隔符
	public string SplitStr { get; set; }
	//如果自己是list 对应的list类名
	public string ListName { get; set; }
	//如果自己是list 对应的sheet名
	public string ListSheetName { get; set; }
}

public class SheetData
{
	public List<string> AllName = new List<string>();
	public List<string> AllType = new List<string>();

	public List<RowData> AllData = new List<RowData>();
}

public class RowData
{
	public string ParentValue = string.Empty;
	public Dictionary<string, string> RowDataDic = new Dictionary<string, string>();
}



public enum TestEnum
{
	None = 0,
	VAR1,
	VAR2,
	VAR3,
}

public class TestInfo
{
	public int Id { get; set; }
	public string Name{ get; set; }
	public bool IsA{ get; set; }
	
	public float Height { get; set; }
	
	public TestEnum testEnum { get; set; }
	
	public List<string> AllStrList{ get; set; }

	public List<TestInfoTwo> AllTestInfoList{ get; set; }
}

public class TestInfoTwo
{
	public int Id{ get; set; }
	public string Name{ get; set; }
}
















