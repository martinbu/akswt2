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
        void SendMessage(string message);

        PinCheckResult CheckPinCode(string pinCode);
        PinCheckResult SetPinCode(string pinCode, string newPinCode);
    }
}
