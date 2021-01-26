using System; 

namespace Framework.ELog
{
    public class Log
    {
        public static Logger logger = null;

        public static bool Init(int lvl, string fileDir, bool console)
        {
            logger = new Logger();
            return logger.Init(lvl, fileDir, console);
        }

        public static void Debug(string content)
        {
            logger.AddLogEvent(LogLevel.DEBUG, content, false);
        }

        public static void Info(string content)
        {
            logger.AddLogEvent(LogLevel.INFO, content, false);
        }

        public static void Warn(string content)
        {
            logger.AddLogEvent(LogLevel.WARN, content, false);
        }

        public static void Error(string content)
        {
            logger.AddLogEvent(LogLevel.ERROR, content, false);
        }

        public static void Debugf(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.DEBUG, string.Format(format, args), false);
        }

        public static void Infof(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.INFO, string.Format(format, args), false);
        }

        public static void Warnf(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.WARN, string.Format(format, args), false);
        }

        public static void Errorf(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.ERROR, string.Format(format, args), false);
        }

        public static void DebugA(string content)
        {
            logger.AddLogEvent(LogLevel.DEBUG, content, true);
        }

        public static void InfoA(string content)
        {
            logger.AddLogEvent(LogLevel.INFO, content, true);
        }

        public static void WarnA(string content)
        {
            logger.AddLogEvent(LogLevel.WARN, content, true);
        }

        public static void ErrorA(string content)
        {
            logger.AddLogEvent(LogLevel.ERROR, content, true);
        }

        public static void DebugAf(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.DEBUG, string.Format(format, args), true);
        }

        public static void InfoAf(string format, params object[] args)
        {            
            logger.AddLogEvent(LogLevel.INFO, string.Format(format, args), true);            
        }

        public static void WarnAf(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.WARN, string.Format(format, args), true);
        }

        public static void ErrorAf(string format, params object[] args)
        {
            logger.AddLogEvent(LogLevel.ERROR, string.Format(format, args), true);
        }
    }
}
