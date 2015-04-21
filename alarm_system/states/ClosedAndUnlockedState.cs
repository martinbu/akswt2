using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ClosedAndUnlockedState : AlarmSystemState
    {
        internal ClosedAndUnlockedState(Context context)
            : base(context, AlarmSystemStateType.ClosedAndUnlocked)
        {
        }

        internal override void Open()
        {
            base.ChangeStateTo(AlarmSystemStateType.OpenAndUnlocked);   
        }

        internal override void Lock()
        {
            base.ChangeStateTo(AlarmSystemStateType.ClosedAndLocked);
        }
    }
}
