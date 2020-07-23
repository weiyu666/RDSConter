using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RegisterDiscoveryService
{
    public class BaseFile
    {
        public string FileName { get; set; }
        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();

        public BaseFile() { }
        public BaseFile(string fileName) { FileName = fileName; }
        public void Create(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) System.IO.File.Create(filePath);
        }
        private void Write(string content, string newLine)
        {
            if (string.IsNullOrEmpty(FileName)) { throw new Exception("FileName不能为空！"); }
            using (System.IO.FileStream fs = new System.IO.FileStream(FileName,
                System.IO.FileMode.OpenOrCreate,
                System.IO.FileAccess.ReadWrite,
                System.IO.FileShare.ReadWrite, 8,
                System.IO.FileOptions.Asynchronous))
            {
                Byte[] dataArray = System.Text.Encoding.Default.GetBytes(content + newLine);
                bool flag = true;
                long slen = dataArray.Length;
                long len = 0;
                while (flag)
                {
                    try
                    {
                        if (len >= fs.Length)
                        {
                            fs.Lock(len, slen);
                            lockDic[len] = slen;
                            flag = false;
                        }
                        else { len = fs.Length; }
                    }
                    catch (Exception)
                    {
                        while (!lockDic.ContainsKey(len)) { len += lockDic[len]; }
                    }
                }
                fs.Seek(len, System.IO.SeekOrigin.Begin);
                fs.Write(dataArray, 0, dataArray.Length);
                fs.Close();
            }
        }
        public void WriteLine(string content)
        {
            this.Write(content, System.Environment.NewLine);
        }
        public void Write(string content)
        {
            this.Write(content, "");
        }
        public string Read(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) throw new Exception("文件不存在");
            StringBuilder result = new StringBuilder();
            using (System.IO.FileStream file = new System.IO.FileStream(filePath,
                System.IO.FileMode.Open,
                System.IO.FileAccess.ReadWrite,
                System.IO.FileShare.ReadWrite, 8,
                System.IO.FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[file.Length];
                file.Seek(0, System.IO.SeekOrigin.Begin);
                file.Read(buffer, 0, buffer.Length);
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                stream.Write(buffer, 0, buffer.Length);
                var content = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                result.Append(content);

                stream.Flush();
                stream.Dispose();
                stream.Close();

                file.Flush();
                file.Close();
            }
            return result.ToString();
        }
    }
    public class LogHelper
    {
        public static bool enable = Config.isLog;
        static BaseFile _baseFile = new BaseFile();
        static string strPath = Config.logPath;


        public static void WriteLog(string logContent)
        {
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string filePath = strPath + "\\" + dateStr + "\\" + string.Format("{0:dd}.txt", DateTime.Now);
            WriteLogs(filePath, logContent);
        }
        public static void WriteLog(string folderName, string logContent)
        {
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string filePath = strPath + "\\" + folderName + "\\" + dateStr + "\\" + string.Format("{0:yyyy-MM-dd_HH}.txt", DateTime.Now);
            WriteLogs(filePath, logContent);
        }
        public static void Error(Exception ex, string folderName = null)
        {
            try
            {
                Exception logEx = ex;
                while (logEx.InnerException != null) logEx = logEx.InnerException;

                StringBuilder log = new StringBuilder();
                log.AppendLine("*************************************************************************************");
                log.AppendLine("异  常:" + logEx.Message + "；");
                log.AppendLine("错误源:" + logEx.Source + "；");
                log.AppendLine("方法名:" + logEx.TargetSite + "；");
                log.AppendLine("堆  栈:" + logEx.StackTrace + "。");
                if (ex.Data != null && ex.Data.Keys.Count > 0)
                {
                    foreach (var dk in ex.Data.Keys)
                    {
                        log.AppendLine(dk.ToString() + ":" + ex.Data[dk].ToString() + "。");
                    }
                }
                log.AppendLine("*************************************************************************************");

                RecordLogHour(folderName, log.ToString());
            }
            catch (Exception) { }
        }
        private static void WriteLogs(string filePath, string msg)
        {
            var path = filePath.Substring(0, filePath.LastIndexOf("\\"));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(filePath)) File.Create(filePath).Close();
            _baseFile.FileName = filePath;
            _baseFile.WriteLine(msg);
        }
        private static void RecordLogHour(string folderName, string msg)
        {
            if (string.IsNullOrEmpty(folderName)) { folderName = "Errorlog"; }
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string filePath = strPath + "\\Error\\" + folderName + "\\" + dateStr + "\\" + String.Format("{0:yyyy-MM-dd_HH}.txt", DateTime.Now);
            _baseFile.FileName = filePath;
            _baseFile.Write(msg);
        }
        }
    }