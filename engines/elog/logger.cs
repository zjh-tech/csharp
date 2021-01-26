using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Framework.ELog
{
    public enum LogLevel
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
    }

    public class LogEvent
    {
        public LogLevel Lvl;
        public string Content;
        public string FileName;
        public int Line;

        public LogEvent(LogLevel lvl, string content, string filename, int line)
        {
            Lvl = lvl;
            Content = content;
            FileName = filename;
            Line = line;
        }
    }
    public class Logger
    {
        private object locker = new object();

        private Queue<LogEvent> logEventQueue = new Queue<LogEvent>();
        private LogLevel logLvl = LogLevel.DEBUG;
        private string[] lvlArrays = { "DEBUG", "INFO", "WARN", "ERROR" };
        string fileDirPrefix;
        private FileStream fileStream = null;
        private DateTime lastTime;

        private Thread thread = null;
        private bool terminate = false;
        private bool consoleFlag = false;

        public bool Init(int lvl, string fileDir, bool console)
        {
            logLvl = (LogLevel)lvl;
            if (logLvl < LogLevel.DEBUG || logLvl > LogLevel.ERROR)
            {
                logLvl = LogLevel.DEBUG;
            }

            consoleFlag = console;
            fileDirPrefix = fileDir;
            lastTime = DateTime.Now;
            EnsureFileExist(lastTime);

            thread = new Thread(LogThreadFunc);
            thread.Start();
            return true;
        }

        public void AddLogEvent(LogLevel lvl, string content, bool async)
        {
            if (terminate)
            {
                return;
            }

            if (logLvl > lvl)
            {
                return;
            }

#if (DEBUG)
            //Need *.pdb
            StackTrace st = new StackTrace(true);
            if (st == null)
            {
                return;
            }

            StackFrame sf = st.GetFrame(2);
            if (sf == null)
            {
                return;
            }
            string filename = sf.GetFileName();
            int line = sf.GetFileLineNumber();
            int index = filename.LastIndexOf('\\');
            string realFileName = filename.Substring(index + 1, filename.Length - index - 1);            
#else
            string realFileName = "";
            int line = 0;
#endif                          
            lock (locker)
            {
                if (async)
                {
                    LogEvent evt = new LogEvent(lvl, content, realFileName, line);
                    logEventQueue.Enqueue(evt);
                }
                else
                {
                    OutPut((int)lvl, content, realFileName, line);
                }
            }
        }

        private void LogThreadFunc()
        {
            bool busy = false;
            while (!terminate)
            {
                busy = false;

                lock (locker)
                {
                    if (logEventQueue.Count != 0)
                    {
                        LogEvent logEvent = logEventQueue.Dequeue();
                        OutPut((int)logEvent.Lvl, logEvent.Content, logEvent.FileName, logEvent.Line);
                        busy = true;
                    }
                }

                if (busy)
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void OutPut(int lvl, string content, string filename, int line)
        {
            DateTime cur = DateTime.Now;
            if (CheckDiffDate(cur))
            {
                if (fileStream != null)
                {
                    fileStream.Flush();
                    fileStream.Close();
                    fileStream = null;
                }
                EnsureFileExist(cur);
            }
            lastTime = cur;

            string month = cur.Month.ToString().PadLeft(2, '0');
            string day = cur.Day.ToString().PadLeft(2, '0');
            string hour = cur.Hour.ToString().PadLeft(2, '0');
            string min = cur.Minute.ToString().PadLeft(2, '0');
            string sec = cur.Second.ToString().PadLeft(2, '0');
#if (DEBUG)
            string str = $"{cur.Year}-{month}-{day} {hour}:{min}:{sec} {lvlArrays[lvl]} [{filename}:{line}]  {content} \n";
#else
            string str = $"{cur.Year}-{month}-{day} {hour}:{min}:{sec} {lvlArrays[lvl]} {content} \n";
#endif

            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
            if (consoleFlag)
            {
                Console.Write(str);
            }
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Flush();
        }

        private bool CheckDiffDate(DateTime now)
        {
            return (lastTime.Year != now.Year) || (lastTime.Month != now.Month) || (lastTime.Day != now.Day) || (lastTime.Hour != now.Hour);
        }

        private void EnsureFileExist(DateTime now)
        {
            string fileDir = $"{fileDirPrefix}/{now.Year}_{now.Month}_{now.Day}";

            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }

            string fileName = $"{now.Year}_{now.Month}_{now.Day}_{now.Hour}";
            string fileFullPath = $"{fileDir}/{fileName}.log";

            if (!File.Exists(fileFullPath))
            {
                fileStream = File.Create(fileFullPath);
            }

            if (fileStream == null)
            {
                fileStream = File.Open(fileFullPath, FileMode.Append);
            }
        }
    }
}
