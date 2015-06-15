using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class SilentAndOpenState : AlarmSystemStateBase
    {
        internal SilentAndOpenState(Context context)
            : base(context, AlarmSystemState.SilentAndOpen)
        {
        }

        internal override void Close()
        {
            base.ChangeStateTo(AlarmSystemState.Armed);
        }

        internal override void Unlock(string pinCode)
        {
            base.ChangeStateToWithPin(AlarmSystemState.OpenAndUnlocked, pinCode);
        }
    }
}
