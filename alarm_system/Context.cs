using alarm_system.states;
using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    internal interface Context
    {
        void ChangeState(AlarmSystemState oldStateType, AlarmSystemState newStateType);

        PinCheckResult checkPinCode(string pinCode);
    }
}
