using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CustomTool
{
    public class GenerateCode
    {
        public static void InitId(string unique)
        {
            string path = "Assets/Scripts/UIFrame/Const/UiId.cs";
            if (File.Exists(path))
            {
                string context = File.ReadAllText(path);
                int index = context.IndexOf(unique);
                //int newindex = context.IndexOf("new", index);
                if (context.Contains(ToolData._prefabName))
                {
                    Debug.LogError("find repeat Enum Name:" + ToolData._prefabName);
                }
                else
                {
                    context = context.Insert(index + unique.Length, "\n    " + ToolData._prefabName + ",");
                    File.WriteAllText(path, context, Encoding.UTF8);
                }
            }
            else
            {
                Debug.LogError("cant find service  filepath:" + path);
            }
        }
        
        public static void InitSystem(string contextName, string className, params string[] systemName)
        {
            string path = "";
            switch (contextName)
            {
                case "Game":
                    path = ToolData._gameFeaturePath;  
                    break;
                case "Input":
                    path = ToolData._inputFeaturePath;
                    break;
            }
            
            if (string.IsNullOrEmpty(path)) return;

            for (int i = 0; i < systemName.Length; i++)
            {
                SetSystem(path, systemName[i], className);
            }
        }
        
        public static void SetSystem(string path, string systemName, string className)
        {
            string content = File.ReadAllText(path);
            string unique = "//[" + systemName + " unique]//";
            int index = content.IndexOf(unique);
            if (index < 0)
            {
                Debug.LogError("cant find index systemName:" + systemName);
                return;
            }

            content = content.Insert(index + unique.Length, "\n            Add(new " + className + "(contexts));");
            File.WriteAllText(path, content, Encoding.UTF8);
        }
        
        public static void CreateScripts(string path, string className,string context)
        {
            if (Directory.Exists(path))
            {
                File.WriteAllText(path + "/" + className + ".cs", context, Encoding.UTF8);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("path:" + path + "is not exists");
            }
        }
        
    }
}