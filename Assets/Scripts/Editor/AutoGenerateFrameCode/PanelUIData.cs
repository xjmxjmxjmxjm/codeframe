using UnityEngine;
using System;

namespace CustomTool
{
    [Serializable]
    public class PanelUIData : ScriptableObject  
    {
        /// <summary>
        /// view层路径
        /// </summary>
        public string ViewPath;
        /// <summary>
        /// service层路径
        /// </summary>
        public string ServicePath;
        /// <summary>
        /// system层路径
        /// </summary>
        public string SystemPath;

        /// <summary>
        /// ServiceManager路徑
        /// </summary>
        public string ServiceManagerPath;

        /// <summary>
        /// Feature 路径
        /// </summary>
        public string GameFeaturePath;
        public string InputFeaturePath;
        public string ViewFeaturePath;




        public string PanelPath;
        public string UIPath;
        public string Creater;
    }
}
