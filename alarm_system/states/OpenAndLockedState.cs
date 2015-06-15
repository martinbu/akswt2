using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class OpenAndLockedState : AlarmSystemStateBase
    {
        internal OpenAndLockedState(Context context)
            : base(context, AlarmSystemState.OpenAndLocked)
        {
        }

        internal override void Unlock(string pinCode)
        {
            base.ChangeStateToWithPin(AlarmSystemState.OpenAndUnlocked, pinCode);
        }

        internal override void Close()
        {
            base.ChangeStateTo(AlarmSystemState.ClosedAndLocked);
        }
    }
}
