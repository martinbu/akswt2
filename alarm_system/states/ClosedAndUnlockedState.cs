using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ClosedAndUnlockedState : AlarmSystemStateBase
    {
        internal ClosedAndUnlockedState(Context context)
            : base(context, AlarmSystemState.ClosedAndUnlocked)
        {
        }

        internal override void Open()
        {
            base.ChangeStateTo(AlarmSystemState.OpenAndUnlocked);   
        }

        internal override void Lock()
        {
            base.ChangeStateTo(AlarmSystemState.ClosedAndLocked);
        }

        internal override void SetPinCode(string pinCode, string newPinCode)
        {
            switch (Context.SetPinCode(pinCode, newPinCode))
            {
                case PinCheckResult.CORRECT:
                    Context.SendMessage("newPinSet");
                    break;
                case PinCheckResult.ALARM:
                    base.ChangeStateTo(AlarmSystemState.AlarmFlashAndSound);
                    break;
            }
        }
    }
}
