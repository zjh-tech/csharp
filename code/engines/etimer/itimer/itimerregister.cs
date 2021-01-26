using System;

namespace Framework.ETimer
{
    public interface ITimerMgr
    {
        bool Update(int loop_count);
    }

    public delegate void TimerFunc(object[] paras);

    public interface ITimerRegister
    {
        //upper call
        void AddOnceTimer(UInt32 id, long delay, string desc, TimerFunc cb_func, object[] cb_paras = null, bool replace_flag = true);
        void AddRepeatTimer(UInt32 id, long delay, string desc, TimerFunc cb_func, object[] cb_paras = null, bool replace_flag = true);

        bool HasTimer(UInt32 id);

        void KillTimer(UInt32 id);

        void KillAllTimer();

        bool GetRemainTime(UInt32 id, out long remain_time);
                
    }
}
