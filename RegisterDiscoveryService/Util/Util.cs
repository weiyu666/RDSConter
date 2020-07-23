using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace RegisterDiscoveryService
{
    public class Util
    {
        #region To<T> 类型转换
        /// <summary>
        /// if the type is basic type, value types, enums or timespan.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBasicType(Type type)
        {
            if (typeof(IConvertible).Equals(type.GetInterface(typeof(IConvertible).FullName))) return true;
            if (type.IsEnum) return true;
            switch (type.Name.ToLower())
            {
                case "timespan": return true;
            }
            return false;
        }

        /// <summary>
        /// 将输入的值转换为指定的类型并返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T To<T>(object value)
        {
            try
            {
                Type type = typeof(T);
                //对于不能顺利转换的,采用Parse方法进行构造
                switch (type.Name.ToLower())
                {
                    case "timespan": return (T)(object)TimeSpan.Parse(value.ToString());
                }
                if (type.IsEnum) return (T)Enum.Parse(type, value.ToString());

                if (typeof(IConvertible).Equals(type.GetInterface(typeof(IConvertible).FullName)))
                    return (T)System.Convert.ChangeType(value, typeof(T));
                return (T)value;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 将指定值转换为指定类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static object To(object value, Type type)
        {
            try
            {
                if (type == typeof(object)) return value;
                if (type.IsInstanceOfType(value)) return value;
                return Convert.ChangeType(value, type);
            }
            catch { return null; }
        }
        #endregion

        #region GetEnums:获得枚举的集合
        /// <summary>
        /// 获得枚举的集合
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns></returns>
        public static List<T> GetEnums<T>()
        {
            string[] names = Enum.GetNames(typeof(T));
            List<T> ls = new List<T>();
            foreach (string name in names)
                ls.Add(To<T>(name));
            return ls;
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 将一个对象进行XML序列化
        /// </summary>
        /// <param name="ObjectToSerialize">要进行序列化的对象</param>
        /// <returns></returns>
        public static string XmlSerialize(object ObjectToSerialize)
        {
            if (ObjectToSerialize == null) return "";
            StringWriter sw = new StringWriter();
            XmlSerializer ser = new XmlSerializer(ObjectToSerialize.GetType());
            ser.Serialize(sw, ObjectToSerialize);
            string formatted = sw.ToString();
            sw.Close();
            return formatted;
        }

        /// <summary>
        /// 将一串序列化文本反序列化为对象
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static T XmlDeSerialize<T>(string SerializedText)
        {
            if (string.IsNullOrEmpty(SerializedText)) return default(T);
            StringReader sr = new StringReader(SerializedText);
            XmlSerializer ser = new XmlSerializer(typeof(T));
            object obj = ser.Deserialize(sr);
            sr.Close();
            return To<T>(obj);
        }

        /// <summary>
        /// 执行二进制序列化
        /// </summary>
        /// <param name="ObjectToSerialize"></param>
        /// <returns></returns>
        public static byte[] BinarySerialize(object ObjectToSerialize)
        {
            if (ObjectToSerialize == null) return null;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, ObjectToSerialize);
            byte[] bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }

        /// <summary>
        /// 将一串序列化字节反序列化为对象
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static T BinaryDeSerialize<T>(byte[] SerializedBytes)
        {
            if (SerializedBytes == null || SerializedBytes.Length <= 0) return default(T);
            MemoryStream ms = new MemoryStream(SerializedBytes);
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(ms);
            ms.Close();
            return To<T>(obj);
        }

        /// <summary>
        /// 将一串序列化字节反序列化为对象
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="SerializedBytes"></param>
        /// <returns></returns>
        public static object BinaryDeSerialize(Type TargetType, byte[] SerializedBytes)
        {
            if (SerializedBytes == null || SerializedBytes.Length <= 0) return null;
            MemoryStream ms = new MemoryStream(SerializedBytes);
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(ms);
            ms.Close();
            return To(obj, TargetType);
        }

        /// <summary>
        /// 将二制数据进行Base64编码。
        /// </summary>
        /// <param name="binaryData"></param>
        /// <returns></returns>
        public static string Base64Encode(byte[] binaryData)
        {
            return Convert.ToBase64String(binaryData);
        }

        public static byte[] Base64Decode(string base64)
        {
            return Convert.FromBase64String(base64);
        }


        #endregion

        /// <summary>
        /// dump an object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Dump(object value)
        {
            return Dump(value, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string Dump(object obj, string padding)
        {
            if (obj == null) return "null";
            var t = obj.GetType();
            if (IsBasicType(t)) return obj.ToString();
            var next_padding = padding + "  ";

            var new_line = !string.IsNullOrEmpty(padding);

            var sb = new StringBuilder();
            var dict = obj as IDictionary;
            if (dict != null)
            {
                foreach (var k in dict.Keys)
                {
                    if (new_line) { sb.AppendLine(); new_line = false; }
                    sb.AppendFormat("{0}{1}: {2}\r\n", padding, k, Dump(dict[k], next_padding));
                }
                return sb.ToString();
            }
            var enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                bool first_ele = true;
                bool basic_type = false;
                foreach (var v in enumerable)
                {
                    if (first_ele)
                    {
                        basic_type = v == null || IsBasicType(v.GetType());
                        first_ele = false;
                        if (basic_type) sb.Append(v);
                        else
                        {
                            sb.AppendLine();
                            sb.AppendLine(Dump(v, next_padding));
                        }
                    }
                    else
                    {
                        if (basic_type) sb.Append("," + v);
                        else sb.AppendLine(Dump(v, next_padding));
                    }
                }
                return sb.ToString();
            }

            var properties = t.GetProperties();
            foreach (var p in properties)
            {
                if (p.Name == "Item") continue;
                sb.AppendFormat("{0}{1}:{2}\r\n", padding, p.Name, Dump(p.GetValue(obj), next_padding));
            }
            var fields = t.GetFields();
            foreach (var f in fields)
            {
                sb.AppendFormat("{0}{1}:{2}\r\n", padding, f.Name, Dump(f.GetValue(obj), next_padding));
            }
            return sb.ToString();
        }

        private static string _applicationDirectory;
        public static string ApplicationDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationDirectory))
                    ApplicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
                return _applicationDirectory;
            }
            set
            {
                if (value != null && value.EndsWith("\\"))
                    value = value.Remove(value.Length - 1, 1);
                _applicationDirectory = value;
            }
        }

        public static string GetFilePath(string file)
        {
            return string.Format("{0}\\{1}", ApplicationDirectory, file);
        }

        public static string GetConfigPath(string name)
        {
            return string.Format("{0}\\{1}.xml", ApplicationDirectory, name);
        }

        public static string LoadConfig(string name)
        {
            var path = GetConfigPath(name);
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public static string LoadFile(string filename)
        {
            var path = GetFilePath(filename);
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public static void SaveConfig(string name, string xml)
        {
            File.WriteAllText(GetConfigPath(name), xml, Encoding.UTF8);
        }

        public static void SaveFile(string filename, string content)
        {
            File.WriteAllText(GetFilePath(filename), content, Encoding.UTF8);
        }

        public static List<string> GetConfigFiles(string prefix)
        {
            var files = Directory.GetFiles(ApplicationDirectory, prefix + "*.xml");
            var ret = new List<string>();
            foreach (var f in files)
            {
                var s = f.LastIndexOf('\\') + 1;
                var l = f.LastIndexOf(".xml") - s;
                ret.Add(f.Substring(s, l));
            }
            return ret;
        }


        public static byte[] CharToBytes(char v) { return BitConverter.GetBytes(v); }
        public static byte[] ByteToBytes(byte v) { return new byte[] { v }; }
        public static byte[] SByteToBytes(sbyte v) { return new byte[] { (byte)v }; }
        public static byte[] UInt16ToBytes(UInt16 v) { return BitConverter.GetBytes(v); }
        public static byte[] Int16ToBytes(Int16 v) { return BitConverter.GetBytes(v); }
        public static byte[] UInt32ToBytes(UInt32 v) { return BitConverter.GetBytes(v); }
        public static byte[] Int32ToBytes(Int32 v) { return BitConverter.GetBytes(v); }
        public static byte[] UInt64ToBytes(UInt64 v) { return BitConverter.GetBytes(v); }
        public static byte[] Int64ToBytes(Int64 v) { return BitConverter.GetBytes(v); }
        public static byte[] SingleToBytes(Single v) { return BitConverter.GetBytes(v); }
        public static byte[] DoubleToBytes(Double v) { return BitConverter.GetBytes(v); }
        public static byte[] BooleanToBytes(Boolean v) { return BitConverter.GetBytes(v); }

        public static Dictionary<string, object> ToDictionary(object obj)
        {
            var ret = new Dictionary<string, object>();
            var props = obj.GetType().GetProperties();
            foreach (var p in props)
            {
                ret.Add(p.Name, p.GetValue(obj));
            }
            var fields = obj.GetType().GetFields();
            foreach (var f in fields)
            {
                ret.Add(f.Name, f.GetValue(obj));
            }
            return ret;
        }

        public static List<Dictionary<string, object>> ToDictionary(DataTable table)
        {
            var ret = new List<Dictionary<string, object>>();
            if (table == null || table.Rows.Count == 0) return ret;
            for (var i = 0; i < table.Rows.Count; i++)
            {
                var dict = new Dictionary<string, object>();
                var row = table.Rows[i];
                for (var j = 0; j < table.Columns.Count; j++)
                {
                    dict[table.Columns[j].ColumnName] = row[table.Columns[j].ColumnName];
                }
                ret.Add(dict);
            }
            return ret;
        }

        public static Dictionary<string, object> ToDictionary(DataRow row)
        {
            var ret = new Dictionary<string, object>();
            if (row == null || row.Table == null) return ret;
            for (var j = 0; j < row.Table.Columns.Count; j++)
            {
                ret[row.Table.Columns[j].ColumnName] = row[row.Table.Columns[j].ColumnName];
            }
            return ret;
        }


        /// <summary>
        /// 将字典输出成：
        /// key1:value1
        /// key2:value2
        /// ...
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pad">填充至指定的长度，大于0时不增加换行</param>
        /// <returns></returns>
        public static string DictionaryToString(IDictionary dict, int pad)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                var str = "";
                if (dict[key].GetType().Equals(typeof(byte)))
                {
                    str = string.Format("{0}: {1:X2}", key, dict[key]);
                }
                else if (dict[key] is Array)
                {
                    str = key + ": ";
                    var arr = dict[key] as Array;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        var v = arr.GetValue(i);
                        if (v.GetType().Equals(typeof(Byte))) str += string.Format("{0:X2} ", v);
                        else if (v.GetType().Equals(typeof(Char))) str += (char)v == '\0' ? '*' : (char)v;
                        else if (i < arr.Length - 1) str += string.Format("{0}, ", v);
                        else str += string.Format("{0}", v);
                    }
                }
                else
                {
                    str = key + ": " + dict[key];
                }
                if (pad > 0)
                {
                    Text.PadRight(ref str, pad);
                    sb.Append(str);
                }
                else
                {
                    sb.AppendLine(str);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Export to app_dir/data folder
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name">file name, not include the path</param>
        /// <returns></returns>
        public static bool ExportCSV(DataTable data, string name)
        {
            try
            {
                var sb = new StringBuilder();
                for (var i = 0; i < data.Columns.Count; i++)
                {
                    var col = data.Columns[i];
                    if (i == 0) sb.Append(col.ColumnName);
                    else sb.Append("," + col.ColumnName);
                }
                sb.Append("\r\n");
                for (var r = 0; r < data.Rows.Count; r++)
                {
                    var row = data.Rows[r];
                    for (var i = 0; i < data.Columns.Count; i++)
                    {
                        if (i == 0) sb.Append(row[i]);
                        else sb.Append("," + row[i]);
                    }
                    sb.Append("\r\n");
                }
                if (!name.EndsWith(".csv")) name += ".csv";
                var path = ApplicationDirectory + "\\Data";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                if (name.IndexOf('\\') > 0)
                    File.WriteAllText(name, sb.ToString(), Encoding.UTF8);
                else
                    File.WriteAllText(path + "\\" + name, sb.ToString(), Encoding.UTF8);

                return true;
            }
            catch { return false; }
        }

    }
}
