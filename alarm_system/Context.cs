using alarm_system.states;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    internal interface Context
    {
        void ChangeState(AlarmSystemStateType oldStateType, AlarmSystemStateType newStateType);
    }
}
