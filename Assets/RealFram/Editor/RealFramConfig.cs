using UnityEngine;
using UnityEditor;

public class RealFramConfig : ScriptableObject
{

	//打包生成AB包配置表的二进制路径
	public string m_ABBytePath;
	public string m_ABByteRoot;
	
	//打包的默认名称
	//public string m_AppName;

	//xml文件夹路径
	public string m_XmlPath;
	
	//二进制文件夹路径
	public string m_BinaryPath;
	
	//脚本文件夹路径
	public string m_ScriptsPath;
	
	//protobuf 文件夹路径
	public string m_ProtobufPath;
}

[CustomEditor(typeof(RealFramConfig))]
public class RealFramConfigInspector : Editor
{
	//打包生成AB包配置表的二进制路径
	public SerializedProperty m_ABBytePath;
	public SerializedProperty m_ABByteRoot;
	
	//打包的默认名称
	//public SerializedProperty m_AppName;

	//xml文件夹路径
	public SerializedProperty m_XmlPath;
	
	//二进制文件夹路径
	public SerializedProperty m_BinaryPath;
	
	//脚本文件夹路径
	public SerializedProperty m_ScriptsPath;
	
	//protobuf 文件夹路径
	public SerializedProperty m_ProtobufPath;

	private void OnEnable()
	{
		m_ABBytePath = serializedObject.FindProperty("m_ABBytePath");
		m_ABByteRoot = serializedObject.FindProperty("m_ABByteRoot");
		//m_AppName = serializedObject.FindProperty("m_AppName");
		m_XmlPath = serializedObject.FindProperty("m_XmlPath");
		m_BinaryPath = serializedObject.FindProperty("m_BinaryPath");
		m_ScriptsPath = serializedObject.FindProperty("m_ScriptsPath");
		m_ProtobufPath = serializedObject.FindProperty("m_ProtobufPath");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(m_ABBytePath, new GUIContent("ab包二进制路径"));
		GUILayout.Space(5);
		
		EditorGUILayout.PropertyField(m_ABByteRoot, new GUIContent("ab包二进制根目录"));
		GUILayout.Space(5);
		
		//EditorGUILayout.PropertyField(m_AppName, new GUIContent("包名称"));
		//GUILayout.Space(5);
		
		EditorGUILayout.PropertyField(m_XmlPath, new GUIContent("xml路径"));
		GUILayout.Space(5);
		
		EditorGUILayout.PropertyField(m_BinaryPath, new GUIContent("二进制路径"));
		GUILayout.Space(5);
		
		EditorGUILayout.PropertyField(m_ScriptsPath, new GUIContent("脚本路径"));
		GUILayout.Space(5);
		
		
		EditorGUILayout.PropertyField(m_ProtobufPath, new GUIContent("protobuf路径"));
		GUILayout.Space(5);

		serializedObject.ApplyModifiedProperties();
	}
}


public class RealConfig
{
	private const string RealFramPath = "Assets/RealFram/Editor/RealFramConfig.asset";
	
	public static RealFramConfig GetRealFram()
	{
		RealFramConfig realConfig = AssetDatabase.LoadAssetAtPath<RealFramConfig>(RealFramPath);
		return realConfig;
	}
}