using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class OpenAndUnlockedState : AlarmSystemState
    {
        internal OpenAndUnlockedState(Context context)
            : base(context, AlarmSystemStateType.OpenAndUnlocked)
        {
        }

        internal override void Close()
        {
            base.ChangeStateTo(AlarmSystemStateType.ClosedAndUnlocked);
        }

        internal override void Lock()
        {
            base.ChangeStateTo(AlarmSystemStateType.OpenAndLocked);
        }
    }
}
