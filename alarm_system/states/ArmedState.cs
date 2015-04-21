using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ArmedState : AlarmSystemState
    {
        internal ArmedState(Context context)
            : base(context, AlarmSystemStateType.Armed)
        {
        }

        internal override void Unlock()
        {
            ChangeStateTo(AlarmSystemStateType.ClosedAndUnlocked);
        }

        internal override void Open()
        {
            ChangeStateTo(AlarmSystemStateType.AlarmFlashAndSound);
        }
    }
}
