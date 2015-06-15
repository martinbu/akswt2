using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system_common
{
    public class StateChangedEventArgs : EventArgs
    {
        public AlarmSystemState OldStateType { get; private set; }
        public AlarmSystemState NewStateType { get; private set; }

        public StateChangedEventArgs(AlarmSystemState oldStateType, AlarmSystemState newStateType)
        {
            this.OldStateType = oldStateType;
            this.NewStateType = newStateType;
        }
 
    }
}
