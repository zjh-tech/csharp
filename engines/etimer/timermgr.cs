using System;
using System.Collections.Generic;
using Framework.ELog;

namespace Framework.ETimer
{  
    public class TimeMgr : ITimerMgr
    {
        public TimeMgr()
        {
            cur_slot = 0;
            time_wheel_id = 0;
            last_mill_tick = get_mill_second();
            slot_list_array = new List<Timer>[TimerDef.MaxSlotSize];
            for(int i = 0; i < TimerDef.MaxSlotSize; ++i)
            {
                slot_list_array[i] = new List<Timer>(); 
            }
        }

        public bool Update(int loop_count)
        {
            long now = get_mill_second();
            if (now < last_mill_tick)
            {
                Log.ErrorA("[Timer] Time Rollback");
                return false; 
            }

            bool busy = false;
            long delta = now - last_mill_tick;
            if(delta > loop_count)
            {
                delta = loop_count;
                Log.WarnA("[Timer] Time Forward");
            }

            last_mill_tick += delta;

            for (long i = 0; i < delta; ++i) {
                cur_slot++;
                cur_slot = cur_slot % TimerDef.MaxSlotSize;

                List<Timer> cur_list = slot_list_array[cur_slot];
                List<Timer> del_list = new List<Timer>();
                List<Timer> repeat_list = new List<Timer>();                
                
                int cur_list_count = cur_list.Count; 
                for( int j = 0; j < cur_list_count; ++j)
                {
                    Timer timer = cur_list[j]; 
                    if(timer.State != TimerState.Running)
                    {                        
                        del_list.Add(timer);
                        release_timer(timer);
                        continue; 
                    }

                    timer.Rotation--; 
                    if (timer.Rotation < 0)
                    {
                        busy = true;
                        Log.DebugAf("[Timer] Trigger TimeWheelID={0},RegisterUID={1} RegisterEnumID={2} Desc={3}", timer.TimeWheelID, timer.RegisterUID, timer.RegisterEnumID, timer.Desc);
                        del_list.Add(timer);
                        timer.Call();
                        if(timer.RepeatFlag && timer.State == TimerState.Running)
                        {
                            repeat_list.Add(timer);
                        }
                        else
                        {
                            release_timer(timer); 
                        }
                    }
                    else
                    {
                        Log.DebugAf("[Timer] TimeWheelID={0},RegisterUID={1} RegisterEnumID={2} Remain Rotation={3} Desc={4}", timer.TimeWheelID, timer.RegisterUID, timer.RegisterEnumID, timer.Rotation+1,timer.Desc);
                    }
                }

                if(del_list.Count != 0)
                {
                    foreach (var del_timer in del_list)
                    {
                        cur_list.Remove(del_timer);
                    }
                }

                if(repeat_list.Count != 0)
                {                                   
                    foreach (var repeate_timer in repeat_list)
                    {
                        AddTimer(repeate_timer);
                    }
                }
            }
            
            return busy; 
        }        

        public void AddTimer(Timer timer)
        {                       
            timer.State = TimerState.Running;
            timer.Rotation = timer.Delay / TimerDef.MaxSlotSize;
            timer.Slot = (cur_slot + timer.Delay % TimerDef.MaxSlotSize) % TimerDef.MaxSlotSize;
            long temp_rotation = timer.Rotation; 
            if (timer.Slot == cur_slot && timer.Rotation > 0)
            {
                timer.Rotation--; 
            }

            slot_list_array[timer.Slot].Add(timer);

            Log.DebugAf("[Timer] AddTimer TimeWheelID={0},RegisterUID={1} RegisterEnumID={2} CurSlot={3},Slot={4} Rotation={5} Delay={6},Desc={7}", timer.TimeWheelID,timer.RegisterUID,timer.RegisterEnumID,cur_slot,timer.Slot,temp_rotation,timer.Delay,timer.Desc);
        }

        private void release_timer(Timer timer)
        {
            if(timer.State == TimerState.Running)
            {
                Log.DebugAf("[Timer] ReleaseTimer TimeWheelID={0},RegisterUID={1} RegisterEnumID={2} Desc={3} Runing State", timer.TimeWheelID, timer.RegisterUID,timer.RegisterEnumID,timer.Desc);
            }
            else if(timer.State == TimerState.Killed)
            {
                Log.DebugAf("[Timer] ReleaseTimer TimeWheelID={0},RegisterUID={1} RegisterEnumID={2} Desc={3} Killed State", timer.TimeWheelID, timer.RegisterUID, timer.RegisterEnumID, timer.Desc);
            }
            else
            {
                Log.DebugAf("[Timer] ReleaseTimer TimeWheelID={0},RegisterUID={1} RegisterEnumID={2} Desc={3} Unknow State", timer.TimeWheelID, timer.RegisterUID, timer.RegisterEnumID, timer.Desc);
            }

            timer.CbFunc = null;
            timer.CbParas = null; 

            if (timer.TimerRegister != null)
            {
                timer.TimerRegister.RemoveTimer(timer); 
                timer.TimerRegister = null; 
            }
        }

        public UInt64 GetNextWheelID()
        {
            ++time_wheel_id;
            return time_wheel_id;
        }

        public static TimeMgr Instance = new TimeMgr();
        
        private long cur_slot; 
        public long CurSlot
        {
            get { return cur_slot;  }
        }

        private long get_mill_second()
        {
            return DateTime.Now.Ticks / 10000;
        }

        private UInt64 time_wheel_id;
        private long last_mill_tick;
        private List<Timer>[] slot_list_array;
    }
}
