using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public class StateChangedEventArgs : EventArgs
    {
        public AlarmSystemStateType OldStateType { get; private set; }
        public AlarmSystemStateType NewStateType { get; private set; }

        public StateChangedEventArgs(AlarmSystemStateType oldStateType, AlarmSystemStateType newStateType)
        {
            this.OldStateType = oldStateType;
            this.NewStateType = newStateType;
        }
 
    }
}
