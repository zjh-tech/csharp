namespace Framework.ELog
{
    public interface ILog
    {
        bool Init(int lvl, string fileDir);

        void Debug(string content);
        void Info(string content);
        void Warn(string content);
        void Error(string content);


        void Debugf(string format, params object[] args);
        void Infof(string format, params object[] args);
        void Warnf(string format, params object[] args);
        void Errorf(string format, params object[] args);


        void DebugA(string content);
        void InfoA(string content);
        void WarnA(string content);
        void ErrorA(string content);


        void DebugAf(string format, params object[] args);
        void InfoAf(string format, params object[] args);
        void WarnAf(string format, params object[] args);
        void ErrorAf(string format, params object[] args);
    }
}