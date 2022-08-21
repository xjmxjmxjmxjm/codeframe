using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomTool{
    
    public class ScriptBuildHelp
    {
        public static string Public = "public";
        public static string Private = "private";
        public static string Protected = "protected";
        
        
        private StringBuilder _stringBuilder;
        private string _lineBrake = "\r\n";
        private int _currentIndex = 0;
        public int IndentTimes { get; set; }

        /// <summary>
        /// 货到大括号 所需要缩进的值
        /// </summary>
        private int _backNum
        {
            get { return (GetIndent() + "}" + _lineBrake).Length; }
        }
        
        public ScriptBuildHelp()
        {
            _stringBuilder = new StringBuilder();
            _currentIndex = 0;
            _stringBuilder.Clear();
        }

        private void Write(string context, bool needIndent = false)
        {
            if (needIndent)
            {
                context = GetIndent() + context;
            }

            if (_currentIndex == _stringBuilder.Length)
            {
                _stringBuilder.Append(context);
            }
            else
            {
                _stringBuilder.Insert(_currentIndex, context);
            }

            _currentIndex += context.Length;
        }

        public void WriteLine(string context, bool needIndent = false)
        {
            Write(context + _lineBrake, needIndent);
        }

        private string GetIndent()
        {
            string indent = "";
            for (int i = 0; i < IndentTimes; i++)
            {
                indent += "    ";
            }

            return indent;
        }

        private int WriteCurlyBrackets()
        {
            var start = _lineBrake + GetIndent() + "{" + _lineBrake;
            var end = GetIndent() + "}" + _lineBrake;

            Write(start + end, true);
            return end.Length;
        }

        public void WriteUsing(string nameSpaceName)
        {
            WriteLine("using " + nameSpaceName + ";", false);
        }

        public void WriteEmptyLine()
        {
            WriteLine("");
        }

        public void WriteNameSpace(string name)
        {
            Write("namespace " + name);
            int length = WriteCurlyBrackets();
            Back2InsertContent();
        }
        public void WriteClass(string name, params string[] baseName)
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("public class ");
            temp.Append(name);
            temp.Append(":");
            for (int i = 0; i < baseName.Length; i++)
            {
                temp.Append(baseName[i]);
                if (i < baseName.Length - 1)
                {
                    temp.Append(",");
                }
            }

            Write(temp.ToString(), true);
            int length = WriteCurlyBrackets();
            Back2InsertContent();
        }

        /// <summary>
        /// 写入属性  get
        /// </summary>
        /// <param name="context"></param>
        /// <param name="needIndent"></param>
        public void WritePro(string context, bool needIndent = false)
        {
            WriteLine(context, needIndent);
            
            var start = "{" + _lineBrake;
            var end = GetIndent() + "}" + _lineBrake;
            Write(start + end, true);
            
            Back2InsertContent();
        }
        public void WriteGet(bool needIndent = false)
        {
            WriteLine("get", needIndent);
            
            var start = "{" + _lineBrake;
            var end = GetIndent() + "}" + _lineBrake;
            Write(start + end, true);
            
            Back2InsertContent();
        }
        public void WriteProContext(string context, bool needIndent = false)
        {
            WriteLine(context, needIndent);
            
            var start = "{" + _lineBrake;
            var end = GetIndent() + "}" + _lineBrake;
            Write(start + end, true);
            
            Back2InsertContent();
        }
        
        
        
        
        public void WriteInterface(string name, params string[] baseName)
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("public interface ");
            temp.Append(name);
            temp.Append(":");
            for (int i = 0; i < baseName.Length; i++)
            {
                temp.Append(baseName[i]);
                if (i < baseName.Length - 1)
                {
                    temp.Append(",");
                }
            }

            Write(temp.ToString(), true);
            int length = WriteCurlyBrackets();
            Back2InsertContent();
        }
        public void WriteAbstract(string name, params string[] baseName)
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("public abstract ");
            temp.Append(name);
            temp.Append(":");
            for (int i = 0; i < baseName.Length; i++)
            {
                temp.Append(baseName[i]);
                if (i < baseName.Length - 1)
                {
                    temp.Append(",");
                }
            }

            Write(temp.ToString(), true);
            int length = WriteCurlyBrackets();
            Back2InsertContent();
        }
        
        public void WriteFun(List<string> keyName, string name, string others = "", params string[] paraName)
        {
            WriteFun(name, Public, keyName, others, paraName);
        }
        
        public void WriteFun(string name, string publicState = "public", List<string> keyName = null, string others = "", params string[] paraName)
        {
            StringBuilder keytemp = new StringBuilder();
            if (keyName != null)
            {
                for (int i = 0; i < keyName.Count; i++)
                {
                    keytemp.Append(keyName[i]);
                    if (i < keyName.Count - 1)
                    {
                        
                    }
                    keytemp.Append(" ");
                } 
            }
            
            
            StringBuilder temp = new StringBuilder();
            temp.Append(publicState + " " + keytemp.ToString() + name + "()");
            if (paraName.Length > 0)
            {
                foreach (string s in paraName)
                {
                    temp.Insert(temp.Length - 1, s + ", ");
                }
                temp.Remove(temp.Length - 3, 2);
            }

            temp.Append(" ");
            temp.Append(others);
            Write(temp.ToString(), true);
            WriteCurlyBrackets();
        }

        /// <summary>
        /// 设置光标位置 给大括号内插入内容
        /// </summary>
        /// <param name="num"></param>
        public void Back2InsertContent()
        {
            _currentIndex -= _backNum;
        }
        /// <summary>
        /// 设置光标位置 回到结束大括号外
        /// </summary>
        public void ToContentEnd()
        {
            _currentIndex += _backNum;
        }


        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
     
    }
}