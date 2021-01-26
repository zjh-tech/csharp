using System;
using System.Collections.Generic;

namespace Framework.ETimer
{
    public class TimerRegister : ITimerRegister 
    {
        public void AddOnceTimer(UInt32 id,long delay,string desc,TimerFunc cb_func, object[] cb_paras = null, bool replace_flag = true)
        {
            add_timer(id, delay, desc, cb_func, cb_paras, replace_flag, false); 
        }
        public void AddRepeatTimer(UInt32 id, long delay, string desc, TimerFunc cb_func, object[] cb_paras = null, bool replace_flag = true)
        {
            add_timer(id, delay, desc, cb_func, cb_paras, replace_flag, true);
        }

        public bool HasTimer(UInt32 id)
        {
            return timer_dict.ContainsKey(id);
        }

        public void KillTimer(UInt32 id)
        {
            if (timer_dict.ContainsKey(id))
            {
                timer_dict[id].Kill();
                timer_dict.Remove(id);
            }
        }

        public void KillAllTimer()
        {
            foreach(Timer timer in timer_dict.Values)
            {
                timer.Kill();
            }
            timer_dict.Clear();
        }

        public bool GetRemainTime(UInt32 id, out long remain_time)
        {
            remain_time = 0;
            if (!timer_dict.ContainsKey(id))
            {
                return false; 
            }

            remain_time = timer_dict[id].GetRemainTime();
            return true; 
        }        
        public void RemoveTimer(Timer timer)
        {            
            if (!timer_dict.ContainsKey(timer.RegisterEnumID))
            {
                return; 
            }

            if(timer_dict[timer.RegisterEnumID].RegisterUID != timer.RegisterUID)
            {
                return; 
            }

            timer_dict.Remove(timer.RegisterEnumID);
        }

        private Timer create_timer(UInt32 register_enum_id, long delay, bool repeat_flag, TimerFunc cb_func, object[] cb_paras, string desc)
        {
            Timer timer = new Timer();
            timer.TimeWheelID = TimeMgr.Instance.GetNextWheelID();
            timer.RegisterEnumID = register_enum_id;
            ++register_id;
            timer.RegisterUID = register_id;
            timer.Delay = delay;
            timer.RepeatFlag = repeat_flag;
            timer.CbFunc = cb_func;
            timer.CbParas = cb_paras;
            timer.TimerRegister = this;
            timer.Desc = desc; 
            return timer; 
        }

        private bool add_timer(UInt32 id, long delay, string desc,TimerFunc cb_func, object[] cb_paras, bool replace_flag,bool repeat_flag)
        {
            if(delay == TimerDef.NovalidDelayMill)
            {
                return false; 
            }

            bool exist_flag = timer_dict.ContainsKey(id);
            if(exist_flag && !replace_flag)
            {
                return true;
            }

            if(exist_flag && replace_flag)
            {
                KillTimer(id);
            }

            Timer timer = create_timer(id, delay, repeat_flag, cb_func, cb_paras,desc);            
            if(delay == 0)
            {
                timer.Call();
                return true; 
            }

            timer_dict[id] = timer;
            TimeMgr.Instance.AddTimer(timer); 
            return true; 
        }

        private UInt64 register_id = 0;
        private Dictionary<UInt32, Timer> timer_dict = new Dictionary<uint, Timer>(); 
    }
}
