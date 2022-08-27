using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Util;

namespace CustomTool
{
    public class CodeTemplate
    {
        public static string GetPanelCode(List<string> componentName)
        {
            var build = new ScriptBuildHelp();
            build.WriteLine("// Creater : " + ToolData._creater);
            build.WriteEmptyLine();
            build.WriteEmptyLine();
            build.WriteUsing("UnityEngine");
            build.WriteUsing("UnityEngine.UI");
            build.WriteUsing("Util");
            build.WriteUsing("UnityEngine.UIElements");
            build.WriteUsing("Button = UnityEngine.UI.Button");
            build.WriteUsing("Image = UnityEngine.UI.Image");
            build.WriteUsing("Slider = UnityEngine.UI.Slider");
            build.WriteUsing("Toggle = UnityEngine.UI.Toggle");
            build.WriteEmptyLine();
            build.WriteClass(ToolData._prefabName, "MonoBehaviour");

            for (int i = 0; i < componentName.Count; i++)
            {
                GetProString(componentName[i], build);
                build.WriteEmptyLine();
            }
            
            return build.ToString();
        }

        private static void GetProString(string componentName, ScriptBuildHelp build)
        {
            string[] splitTemp = componentName.Split('/');
            string[] temp = splitTemp[splitTemp.Length - 1].Split('_');
            string com = temp[0];
            string name = temp[1];
            string component = ToolData.GetComponentByComName(com);
            
            if (string.IsNullOrEmpty(component)) return;
            
            build.IndentTimes++;
            build.WriteLine("private " + component + " _" + name + ";", true);
            build.WritePro("public " + component + " " + name, true);
            build.IndentTimes++;
            build.WriteGet(true);
            build.IndentTimes++;
            build.WriteProContext("if (" + "_" + name + " == null)", true);
            build.IndentTimes++;
            build.WriteLine("_" + name + " = transform.Find(\"" + componentName + "\").GetComponent<" + component + ">();", true);
            build.IndentTimes--;
            build.ToContentEnd();
            build.WriteLine("return " + "_" + name + ";", true);
            build.ToContentEnd();
            build.IndentTimes--;
            build.IndentTimes--;
            build.IndentTimes--;
            build.ToContentEnd();

            if (com == "scrov")
            {
                build.IndentTimes++;
                build.WriteLine("private Transform" + " _Trans" + name + ";", true);
                build.WritePro("public Transform" + " Trans" + name, true);
                build.IndentTimes++;
                build.WriteGet(true);
                build.IndentTimes++;
                build.WriteProContext("if (" + "_Trans" + name + " == null)", true);
                build.IndentTimes++;
                build.WriteLine("_Trans" + name + " = transform.Find(\"" + componentName + "\");", true);
                build.IndentTimes--;
                build.ToContentEnd();
                build.WriteLine("return " + "_Trans" + name + ";", true);
                build.ToContentEnd();
                build.IndentTimes--;
                build.IndentTimes--;
                build.IndentTimes--;
                build.ToContentEnd();
                
                build.IndentTimes++;
                build.WriteLine("private LoopList" + " _LoopList" + name + ";", true);
                build.WritePro("public LoopList" + " LoopList" + name, true);
                build.IndentTimes++;
                build.WriteGet(true);
                build.IndentTimes++;
                build.WriteProContext("if (" + "_LoopList" + name + " == null)", true);
                build.IndentTimes++;
                build.WriteLine("_LoopList" + name + " = transform.Find(\"" + componentName + "\").GetOrAddComponent<LoopList>();", true);
                build.IndentTimes--;
                build.ToContentEnd();
                build.WriteLine("return " + "_LoopList" + name + ";", true);
                build.ToContentEnd();
                build.IndentTimes--;
                build.IndentTimes--;
                build.IndentTimes--;
                build.ToContentEnd();
            }
        }


        public static string GetUICode(FileName contextName, string className)
        {
            var build = new ScriptBuildHelp();
            build.WriteLine("// Creater : " + ToolData._creater);
            build.WriteEmptyLine();
            build.WriteEmptyLine();
            build.WriteUsing("System");
            build.WriteUsing("System.Collections.Generic");
            build.WriteEmptyLine();
            build.WriteEmptyLine();
            build.WriteClass(className, ToolData.GetUILayerBaseName(contextName));
            build.IndentTimes++;
            
            
            List<string> keys = new List<string>();
            keys.Add("override");
            if (contextName == FileName.ItemPanel)
            {
                keys.Add("ItemId");
                build.WriteFun("GetItemId", ScriptBuildHelp.Public, keys);
            }
            else
            {
                keys.Add("UiId");
                build.WriteFun("GetUiId", ScriptBuildHelp.Public, keys);
            }
            build.Back2InsertContent();
            
            
            build.IndentTimes++;
            if (contextName == FileName.ItemPanel)
            {
                build.WriteLine("return ItemId." + className.Replace("UI", "") + ";", true);
            }
            else
            {
                build.WriteLine("return UiId." + className.Replace("UI", "Panel") + ";", true);
            }
            
            return build.ToString();
        }

        public static string GetReactiveSystemCode()
        {
            string className = ToolData._selectdContextName + ToolData._systemName + ToolData._systemPostfix;
            string entityname = ToolData._selectdContextName + "Entity";
            
            var build = new ScriptBuildHelp();
            build.WriteUsing("Entitas");
            build.WriteUsing("System.Collections.Generic");
            build.WriteEmptyLine();
            build.WriteNameSpace(ToolData._namespaceBase);
            build.IndentTimes++;
            build.WriteClass(className, "ReactiveSystem<" + entityname + ">");
            build.IndentTimes++;
            build.WriteLine("protected Contexts _contexts;", true);
            build.WriteEmptyLine();
            //构造函数
            build.WriteFun(new List<string>(), className, " : base(context." + ToolData._selectdContextName.ToLower() + ")", "Contexts context");
            build.Back2InsertContent();
            build.IndentTimes++;
            build.WriteLine("_contexts = context;", true);
            build.IndentTimes--;
            build.ToContentEnd();
            //GetTrigger
            List<string> keys = new List<string>();
            keys.Add("override");
            keys.Add("ICollector<" + entityname + ">");
            build.WriteFun("GetTrigger", ScriptBuildHelp.Protected, keys,"" , "IContext<" + entityname + "> context");
            build.Back2InsertContent();
            build.IndentTimes++;
            build.WriteLine("return context.CreateCollector(" + ToolData._selectdContextName + "Matcher." + ToolData._selectdContextName + ToolData._systemName + ");", true);
            //build.WriteLine("return null;", true);
            build.IndentTimes--;
            build.ToContentEnd();
            //Filter
            List<string> filterkeys = new List<string>();
            filterkeys.Add("override");
            filterkeys.Add("bool");
            build.WriteFun("Filter", ScriptBuildHelp.Protected, filterkeys,"" , entityname + " entity");
            build.Back2InsertContent();
            build.IndentTimes++;
            build.WriteLine("return entity.has" + ToolData._selectdContextName + ToolData._systemName + ";", true);
            //build.WriteLine("return false;", true);
            build.IndentTimes--;
            build.ToContentEnd();
            //Execute
            List<string> executekeys = new List<string>();
            executekeys.Add("override");
            executekeys.Add("void");
            build.WriteFun("Execute", ScriptBuildHelp.Protected, executekeys,"" , "List<" + entityname + "> entities");
            
            return build.ToString();
        }

        public static string GetOtherSystemCode()
        {
            string className = ToolData._selectdContextName + ToolData._otherSystemName + ToolData._systemPostfix;
            List<string> selectSystem = GetSelectedSystem();
            
            var build = new ScriptBuildHelp();
            build.WriteUsing("Entitas");
            build.WriteEmptyLine();
            build.WriteNameSpace(ToolData._namespaceBase);
            build.IndentTimes++;
            build.WriteClass(className, GetSelectedSystem(selectSystem));
            build.IndentTimes++;
            build.WriteLine("public Contexts _contexts;", true);
            build.WriteEmptyLine();
            
            List<string> lst = new List<string>();
            build.WriteFun(lst, className, "", "Contexts context");
            build.Back2InsertContent();
            build.IndentTimes++;
            build.WriteLine("_contexts = context;", true);
            build.IndentTimes--;
            build.ToContentEnd();

            //实现方法
            lst.Clear();
            lst.Add("void");
            List<string> funName = GetFunName(selectSystem);
            for (int i = 0; i < funName.Count; i++)
            {
                
                build.WriteFun(lst, funName[i]);
            }

            return build.ToString();
        }
        
        public static List<string> GetSelectedSystem()
        {
            List<string> lst = new List<string>();
            foreach (KeyValuePair<string, bool> pair in ToolData._systemSelectState)
            {
                if (pair.Value)
                {
                    lst.Add(pair.Key);
                }
            }

            return lst;
        }

        public static string GetSelectedSystem(List<string> selectlst)
        {
            StringBuilder sb = new StringBuilder();
            if (selectlst.Count <= 0)
            {
                return sb.ToString();
            }

            for (int i = 0; i < selectlst.Count; i++)
            {
                sb.Append(selectlst[i]);
                sb.Append(" , ");
            }

            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        public static List<string> GetFunName(List<string> selectlst)
        {
            List<string> temp = new List<string>();

            for (int i = 0; i < selectlst.Count; i++)
            {
                string name = selectlst[i];
                name = name.Substring(1, name.Length - 7);
                temp.Add(name);
            }

            return temp;
        }
    }
}