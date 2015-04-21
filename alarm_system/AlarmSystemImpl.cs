using alarm_system.states;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public class AlarmSystemImpl : AlarmSystem, Context
    {

        private Dictionary<AlarmSystemStateType, AlarmSystemState> AlarmSystemStates { get; set; }

        public AlarmSystemImpl()
        {
            AlarmSystemStates = new Dictionary<AlarmSystemStateType, AlarmSystemState>();
            AddState(new OpenAndUnlockedState(this));
            AddState(new ClosedAndUnlockedState(this));
            AddState(new OpenAndLockedState(this));
        }

        public AlarmSystemStateType CurrentStateType { get; private set; }

        public void Open()
        {
            AlarmSystemStates[CurrentStateType].Open();
        }

        public void Close()
        {
            AlarmSystemStates[CurrentStateType].Close();
        }

        public void Lock()
        {
            AlarmSystemStates[CurrentStateType].Lock();
        }

        public void Unlock()
        {
            AlarmSystemStates[CurrentStateType].Unlock();
        }

        public void ChangeState(AlarmSystemStateType oldStateType, AlarmSystemStateType newStateType)
        {
            if (oldStateType == newStateType)
                return;

            CurrentStateType = newStateType;

            if (StateChanged != null)
            {
                StateChanged(this, new StateChangedEventArgs(oldStateType, newStateType));
            }
        }



        private void AddState(AlarmSystemState alarmSystemState)
        {
            AlarmSystemStates.Add(alarmSystemState.StateType, alarmSystemState);
        }


        public event EventHandler<StateChangedEventArgs> StateChanged;
    }
}
