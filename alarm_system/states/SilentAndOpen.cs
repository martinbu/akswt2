using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class SilentAndOpenState : AlarmSystemState
    {
        internal SilentAndOpenState(Context context)
            : base(context, AlarmSystemStateType.SilentAndOpen)
        {
        }

        internal override void Close()
        {
            base.ChangeStateTo(AlarmSystemStateType.Armed);
        }

        internal override void Unlock()
        {
            base.ChangeStateTo(AlarmSystemStateType.OpenAndUnlocked);
        }
    }
}
