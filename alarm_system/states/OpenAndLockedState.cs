using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class OpenAndLockedState : AlarmSystemState
    {
        internal OpenAndLockedState(Context context)
            : base(context, AlarmSystemStateType.OpenAndLocked)
        {
        }

        internal override void Unlock()
        {
            base.ChangeStateTo(AlarmSystemStateType.OpenAndUnlocked);
        }

        internal override void Close()
        {
            base.ChangeStateTo(AlarmSystemStateType.ClosedAndLocked);
        }
    }
}
