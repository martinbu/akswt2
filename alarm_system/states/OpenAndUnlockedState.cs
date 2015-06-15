using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class OpenAndUnlockedState : AlarmSystemStateBase
    {
        internal OpenAndUnlockedState(Context context)
            : base(context, AlarmSystemState.OpenAndUnlocked)
        {
        }

        internal override void Close()
        {
            base.ChangeStateTo(AlarmSystemState.ClosedAndUnlocked);
        }

        internal override void Lock()
        {
            base.ChangeStateTo(AlarmSystemState.OpenAndLocked);
        }
    }
}
