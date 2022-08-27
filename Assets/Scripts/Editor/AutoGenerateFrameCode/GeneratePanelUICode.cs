using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util;

namespace CustomTool
{
    /// <summary>
    /// 生产 PanelUI 框架代码工具
    /// </summary>
    public class GeneratePanelUICode : EditorWindow
    {

        private static int _lineSpace;

        private static GUIStyle _mainTitle;
        private static GUIStyle _itemTitle;


        private static EditorWindow window;
        [MenuItem("Tools/GeneratePanelUICode %/")]
        public static void OpenWindow()
        {
            window = GetWindow(typeof(GeneratePanelUICode));
            window.minSize = new Vector2(800, 600);
            window.Show();
            Init();
        }

        private static void Close()
        {
            AssetDatabase.Refresh();
            window.Close();
        }

        private static void Init()
        {
            _lineSpace = 15;
            ToolData.Init();
            InitGUIStyle();
        }

        private static void InitGUIStyle()
        {
            _mainTitle = new GUIStyle();
            _mainTitle.alignment = TextAnchor.MiddleCenter;
            _mainTitle.normal.textColor = Color.green;
            _mainTitle.fontSize = 30;
            _mainTitle.fontStyle = FontStyle.Bold;
            
            _itemTitle = new GUIStyle();
            _itemTitle.normal.textColor = Color.yellow;
            _itemTitle.fontSize = 15;
            _itemTitle.fontStyle = FontStyle.Bold;
        }

        private void OnGUI()
        {
            GUILayout.Space(_lineSpace);
            GUILayout.Label("代码生成工具", _mainTitle);

           
            Path();
            SelectContext();

            Panel();
            CreateUI();
        }
        
        private void Path()
        {
            GUILayout.Space(_lineSpace);
            GUILayout.Label("脚本路径", _itemTitle);
            GUILayout.Space(_lineSpace);
            
            
            PathItem("Prefab层路径",ref ToolData._viewPath);
            GetCreater();
            GetPrefabName();
          

            CreateButton("保存路径", () =>
            {
                ToolData.SaveData2Local();
            });
        }


        private void GetCreater()
        {
            InputName("功能开发者：", ref ToolData._creater);
        } 


        private void GetPrefabName()
        {
            if (string.IsNullOrEmpty(ToolData._viewPath)) return;
            string[] temp = ToolData._viewPath.Split('/');  //分割预制体路径
            ToolData._prefabName = temp[temp.Length - 1].Replace(".prefab", ""); //将结尾的 .prefab 去掉
//            Type type = ToolUtil.GetType(ToolData._UIName);
//            if (System.Activator.CreateInstance(type) is PanelBase)
//            {
//                PanelBase panel = System.Activator.CreateInstance(type) as PanelBase;
//                Debug.Log(ToolData.GetContextNameByEnum(panel.GetUiLayer()));
//            }
//            else
//            {
//                ItemBase panel = System.Activator.CreateInstance(type) as ItemBase;
//                Debug.Log("ItemUI");
//            }
        }

        private void CreateUI()
        {
            GUILayout.Space(_lineSpace);
            GUILayout.Label("UI层代码生成", _itemTitle);

            CreateButton("生成UI逻辑脚本", () =>
            {
                if (IsSureContent() == false) return;
                
                //ToolData._UIName = ToolData._prefabName.Replace("Panel","UI"); 
                int length = ToolData._prefabName.Length;
                string last = ToolData._prefabName.Substring(length - 4);
                if (last == "Item")
                {
                    ToolData._UIName = ToolData._prefabName + "UI";
                }
                else
                {
                    //panel 类脚本
                    ToolData._UIName = ToolData._prefabName.Replace("Panel","UI"); 
                }
                
                
                if (ReflectUtil.GetType(ToolData._UIName) != null)
                {
                    Debug.LogError("将要生成的逻辑脚本 <" + ToolData._UIName + "> 已存在 如果是第一次生成此脚本 请修改你的预制体名称  并且重新生成你的 mono 类");
                }
                else
                {

                    FileName fileName = FileName.NONE;
                    bool isChange = true;
                    try
                    {
                        fileName = (FileName)Enum.Parse(typeof(FileName), ToolData._selectdContextName);
                    }
                    catch (Exception e)
                    {
                        isChange = false;
                        throw;
                    }
                    finally
                    {
                        
                    }

                    if (isChange == false)
                    {
                        Debug.LogError("ui 逻辑类 生成时 枚举转换失败   未生成代码");
                        return;
                    }
                    
                    GenerateCode.CreateScripts(
                        ToolData._uiPath + ToolData._selectdContextName.Replace("Panel", "UI") + "/", ToolData._UIName,
                        CodeTemplate.GetUICode(fileName, ToolData._UIName));
                }
                
            });
        }

        private void Panel()
        {
            GUILayout.Space(_lineSpace);
            GUILayout.Label("Panel层代码生成", _itemTitle);
            //InputName("代码名称", ref ToolData._viewName);

           
            
            CreateButton("生成Panel脚本", () =>
            {


                if (IsSureContent() == false) return;
                
                
                List<string> lst = GetPrefabAllNeedNode();
                
                GenerateCode.CreateScripts(ToolData._panelPath + ToolData._selectdContextName + "/", ToolData._prefabName, CodeTemplate.GetPanelCode(lst));
                if (ToolData._selectdContextName == FileName.ItemPanel.ToString())
                {
                    GenerateCode.InitId("//#create item#");
                }
                else
                {
                    GenerateCode.InitId("//#create panel#");
                }

   


                
                AssetDatabase.Refresh();
            });
        }


        /// <summary>
        /// 判断上下文是否选择正确
        /// </summary>
        /// <returns></returns>
        private bool IsSureContent()
        {
            int length = ToolData._prefabName.Length;
            string last = string.Empty;
            //如果选择的是 item文件夹 的上下文
            if (ToolData._selectdContextName == FileName.ItemPanel.ToString())
            {
                last = ToolData._prefabName.Substring(length - 5);
                if (last == "Panel")
                {
                    Debug.LogError("上下文选择错误  没有生成代码");
                    return false;
                }
            }
            else
            {
                last = ToolData._prefabName.Substring(length - 4);
                if (last == "Item")
                {
                    Debug.LogError("上下文选择错误  没有生成代码  当前应该选择 ItemPanel 上下文");
                    return false;
                }
            }

            return true;
        }
        

        private List<string> GetPrefabAllNeedNode()
        {
            List<string> lst = new List<string>();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ToolData._viewPath);
            string srcName = prefab.name;
            GetAllChildren(prefab.transform, srcName, lst);

            for (int i = 0; i < lst.Count; i++)
            {
                string[] temp = lst[i].Split('/');
                if (temp[0] == srcName)
                {
                    lst[i] = lst[i].Remove(0, srcName.Length + 1);
                }
            }

            return lst;
        }

        private void GetAllChildren(Transform trans, string name, List<string> lst)
        {
            if (trans.childCount <= 0)
            {
                return;
            }
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform child = trans.GetChild(i);
                
                string[] temp = child.name.Split('_');

                string mergeName = name + "/" + child.name;
                
                if (temp != null && temp.Length == 2)
                {
                    string com = temp[0];
                    string component = ToolData.GetComponentByComName(com);
                    if (!string.IsNullOrEmpty(component))
                    {
                        lst.Add(mergeName);
                    }
                }
                
                GetAllChildren(child, mergeName, lst);
            }

        }


        private void SelectContext()
        {
            GUILayout.Space(_lineSpace);
            GUILayout.Label("选择生成系统的上下文", _itemTitle);
            GUILayout.Space(_lineSpace);
            
            GUILayout.BeginHorizontal();
            foreach (KeyValuePair<string, bool> pair in ToolData._contextSelectState)
            {
                if (GUILayout.Toggle(pair.Value, pair.Key) && pair.Value == false)
                {
                    ToolData._selectdContextName = pair.Key;
                }
            }
            GUILayout.EndHorizontal();
            ToggleGroup(ToolData._selectdContextName);
            GUILayout.Space(_lineSpace);
        }

        /// <summary>
        /// 输入要生成脚本的主名称
        /// </summary>
        private void InputName(string titleName, ref string name)
        {
            GUILayout.Label(titleName);
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(250));
            name = EditorGUI.TextField(rect, name);
        }

        private void CreateButton(string buttonName, Action cbk)
        {
            if (GUILayout.Button(buttonName,GUILayout.Width(250)))
            {
                Close();
                cbk?.Invoke();
            }
        }

        private static void ToggleGroup(string name)
        {
            if (ToolData._contextSelectState.ContainsKey(name))
            {
                ToolData.ResetContextSelectState();
                ToolData._contextSelectState[name] = true;
            }
        }

        /// <summary>
        /// 路径UI显示及输入
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        private void PathItem(string name, ref string path)
        {
            GUILayout.Label(name);
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(250));
            path = EditorGUI.TextField(rect, path);
            DragToGetPath(rect, ref path);
        }

        /// <summary>
        /// 拖动文件夹获取路径
        /// </summary>
        private void DragToGetPath(Rect rect, ref string path)
        {
            if ((Event.current.type == EventType.DragUpdated
                 || Event.current.type == EventType.DragExited)
                && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    path = DragAndDrop.paths[0];
                }
            }
        }
        
    }
}
