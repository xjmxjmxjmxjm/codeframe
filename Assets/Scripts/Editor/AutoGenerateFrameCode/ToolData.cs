using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace CustomTool
{
    public class ToolData
    {
        /// <summary>
        /// 预制体名称
        /// </summary>
        public static string _prefabName;
        public static string _UIName;
        public static string _panelPath;
        public static string _uiPath;
        
        /// <summary>
        /// 当前选中上下文名称
        /// </summary>
        public static string _selectdContextName;


        public static List<string> _proprotyLst;

        public static string _creater;
        
        
        
        /// <summary>
        /// view 脚本存放路径
        /// </summary>
        public static string _viewPath;
        /// <summary>
        /// service 脚本存放路径
        /// </summary>
        public static string _servicePath;
        /// <summary>
        /// servicemanager   插入自动代码 service
        /// </summary>
        public static string _serviceManagerPath;
        /// <summary>
        /// 各个上下文 Feature 路径
        /// </summary>
        public static string _gameFeaturePath;
        public static string _inputFeaturePath;
        public static string _viewFeaturePath;
        /// <summary>
        /// 系统脚本存放路径
        /// </summary>
        public static string _systemPath;
        /// <summary>
        /// 数据持久化保存路径
        /// </summary>
        public static string _dataPath = "Assets/Scripts/Editor/AutoGenerateFrameCode/Data/";
        public static string _dataFileName = "Data.asset";
        /// <summary>
        /// view脚本后缀名
        /// </summary>
        public static string _viewPostfix = "View";
        /// <summary>
        /// 开发者输入名
        /// </summary>
        public static string _viewName;
        /// <summary>
        /// service脚本后缀名
        /// </summary>
        public static string _servicePostfix = "Service";
        /// <summary>
        /// 开发者输入名
        /// </summary>
        public static string _serviceName;
        /// <summary>
        /// system脚本后缀名
        /// </summary>
        public static string _systemPostfix = "System";
        /// <summary>
        /// 开发者输入名
        /// </summary>
        public static string _systemName;
        /// <summary>
        /// 基础命名空间
        /// </summary>
        public static string _namespaceBase = "Game";
        /// <summary>
        /// entitas 面板中已经定义的上下文名
        /// </summary>
        public static string[] _contextNames;
        /// <summary>
        /// 每个上下文选中状态  key 上下文名   value 是否选择
        /// </summary>
        public static Dictionary<string, bool> _contextSelectState;
        /// <summary>
        /// 开发者输入名
        /// </summary>
        public static string _otherSystemName;
        /// <summary>
        /// 其他系统接口名称
        /// </summary>
        public static string[] _systemInterfaceName =
        {
            "IInitializeSystem",
            "IExecuteSystem",
            "ICleanupSystem",
            "ITearDownSystem"
        };
        /// <summary>
        /// 其他系统接口选中状态  key 上下文名   value 是否选择
        /// </summary>
        public static Dictionary<string, bool> _systemSelectState;
        
        
        public static void Init()
        {
            GetContextName();
            ReadDataFromLocal();
            InitContextSelectState();
            _selectdContextName = _contextNames[0];
                
            InitSystemSelectState();
            
            
            _proprotyLst = new List<string>();
        }
        
        public static void InitContextSelectState()
        {
            _contextSelectState = new Dictionary<string, bool>();

            ResetContextSelectState();
        }
        
        public static void InitSystemSelectState()
        {
            _systemSelectState = new Dictionary<string, bool>();

            foreach (var name in _systemInterfaceName)
            {
                _systemSelectState[name] = false;
            }
        }

        public static void ResetContextSelectState()
        {
            foreach (var name in _contextNames)
            {
                _contextSelectState[name] = false;
            }
        }
        
        
        
        /// <summary>
        /// 获取所有上下文名称
        /// </summary>
        public static void GetContextName()
        {
            
            string[] layer = new string[]
            {
                FileName.BasicPanel.ToString(),
                FileName.TopPanel.ToString(),
                FileName.OverlayTopPanel.ToString(),
                FileName.BottomPanel.ToString(),
                FileName.OverlayPanel.ToString(),
                FileName.TipPanel.ToString(),
                
                FileName.ItemPanel.ToString(),
            };
            _contextNames = layer.ToArray();
        }

        public static string GetUILayerBaseName(FileName name)
        {
            switch (name)
            {
                case FileName.BasicPanel:
                    return "BasicUI"; 
                case FileName.TopPanel:
                    return "TopUI";
                case FileName.OverlayTopPanel:
                    return "OverlayTopUI";
                case FileName.BottomPanel:
                    return "BottomUI";
                case FileName.OverlayPanel:
                    return "OverlayUI";
                case FileName.TipPanel:
                    return "TipUI";
                
                case FileName.ItemPanel:
                    return "ItemBase";
            }

            return string.Empty;
        }
        

        /// <summary>
        /// 传入开发者自己编写的定义的 ui组件名字  来获取unity组件名
        /// </summary>
        /// <param name="comName"></param>
        public static string GetComponentByComName(string comName)
        {
            switch (comName)
            {
                 case "img":
                     return "Image";
                 case "text":
                     return "Text";
                 case "trans":
                     return "Transform";
                 case "obj":
                     return "GameObject";
                 case "btn":
                     return "Button";
                 case "tog":
                     return "Toggle";
                 case "scrob":
                     return "Scrollbar";
                 case "sli":
                     return "Slider";
                 case "dropd":
                     return "Dropdown";
                 case "inputf":
                     return "InputField";
                 case "scrov":
                     return "ScrollView";
            }

            return string.Empty;
        }
        
        /// <summary>
        /// 保存数据到本地
        /// </summary>
        public static void SaveData2Local()
        {
            Directory.CreateDirectory(_dataPath);
            PanelUIData data = new PanelUIData();
            data.ViewPath = _viewPath;
            data.ServicePath = _servicePath;
            data.SystemPath = _systemPath;
            data.ServiceManagerPath = _serviceManagerPath;
            data.GameFeaturePath = _gameFeaturePath;
            data.InputFeaturePath = _inputFeaturePath;
            data.ViewFeaturePath = _viewFeaturePath;
            data.PanelPath = _panelPath;
            data.UIPath = _uiPath;
            data.Creater = _creater;
            AssetDatabase.CreateAsset(data, _dataPath + _dataFileName);
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// 从本地读取数据
        /// </summary>
        public static void ReadDataFromLocal()
        {
            PanelUIData data = AssetDatabase.LoadAssetAtPath<PanelUIData>(_dataPath + _dataFileName);
            if (data == null) return;
            _viewPath = data.ViewPath;
            _servicePath = data.ServicePath;
            _systemPath = data.SystemPath;
            _serviceManagerPath = data.ServiceManagerPath;
            _gameFeaturePath = data.GameFeaturePath;
            _inputFeaturePath = data.InputFeaturePath;
            _viewFeaturePath = data.ViewFeaturePath;
            _panelPath = data.PanelPath;
            _uiPath = data.UIPath;
            _creater = data.Creater;
        }
        
        
    }

    public enum FileName
    {
        NONE,
        BasicPanel,
        TopPanel,
        OverlayTopPanel,
        BottomPanel,
        OverlayPanel,
        TipPanel,
        ItemPanel,
    }
}