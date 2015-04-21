using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ClosedAndLockedState : AlarmSystemState
    {
        internal ClosedAndLockedState(Context context)
            : base(context, AlarmSystemStateType.ClosedAndLocked)
        {
        }

        internal override void Unlock()
        {
            base.ChangeStateTo(AlarmSystemStateType.ClosedAndUnlocked);
        }

        internal override void Open()
        {
            base.ChangeStateTo(AlarmSystemStateType.OpenAndLocked);
        }
    }
}
