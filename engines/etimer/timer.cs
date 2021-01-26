using System;
using Framework.ELog;

namespace Framework.ETimer
{    
    public enum TimerState
    {
        Invalid = 0,
        Running = 1,
        Killed  = 2,
    }

    public class Timer 
    {
        public void Kill()
        {
            State = TimerState.Killed;
            Log.DebugAf("[Timer] TimeWheelID={0} RegisterUID={1} RegisterEnumID={2} Kill", TimeWheelID, RegisterUID, RegisterEnumID);
        }

        public void Call()
        {
            CbFunc(CbParas);            
        }  
        
        public long GetRemainTime()
        {
            long remaintime = 0; 
            if (State != TimerState.Running)
            {
                return remaintime; 
            }

            long cur_slot = TimeMgr.Instance.CurSlot; 
            if (cur_slot < Slot)
            {
                remaintime = Rotation * TimerDef.MaxSlotSize + Slot - cur_slot; 
            }
            else
            {
                remaintime = Rotation * TimerDef.MaxSlotSize + (TimerDef.MaxSlotSize - cur_slot + Slot); 
            }

            return remaintime; 
        }


        public UInt32  RegisterEnumID = 0;
        public UInt64  RegisterUID = 0;
        public UInt64  TimeWheelID = 0;
        public long    Delay = 0;
        public long    Rotation = 0;
        public long    Slot = 0;
        public TimerState State = TimerState.Invalid;
        public bool RepeatFlag = false;
        public TimerFunc CbFunc = null;
        public object[] CbParas = null;        
        public TimerRegister TimerRegister = null;
        public string Desc = "";
    }
}
