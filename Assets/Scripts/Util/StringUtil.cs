

using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    /// <summary>
    /// 一般函数操作类
    /// </summary>
    public class StringUtil
    {
        private static StringBuilder _sb;

        private static StringBuilder _StringBuilder
        {
            get
            {
                if (_sb == null)
                {
                    _sb = new StringBuilder();
                }

                return _sb;
            }
        }
        
        public static string GetMergeStr(params string[] strLst)
        {
            _StringBuilder.Clear();
            int length = strLst.Length;
            for (int i = 0; i < length; i++)
            {
                _StringBuilder.Append(strLst[i]);
            }

            return _StringBuilder.ToString();
        }
        
        public static string GetMergeStrAddChar(string mergeChar = "", params string[] strLst)
        {
            _StringBuilder.Clear();
            int length = strLst.Length;
            for (int i = 0; i < length; i++)
            {
                _StringBuilder.Append(strLst[i]);
                if (i < length - 1)
                {
                    _StringBuilder.Append(mergeChar);
                }
            }

            return _StringBuilder.ToString();
        }
        
        public static string GetMergeStrAddChar(List<string> strLst, string mergeChar = "")
        {
            _StringBuilder.Clear();
            int length = strLst.Count;
            for (int i = 0; i < length; i++)
            {
                _StringBuilder.Append(strLst[i]);
                if (i < length - 1)
                {
                    _StringBuilder.Append(mergeChar);
                }
            }

            return _StringBuilder.ToString();
        }


        public static string GetReultRemoveStr(string old, string removeStr)
        {
            return old.Replace(removeStr, "");
        }
        
        
    }
}