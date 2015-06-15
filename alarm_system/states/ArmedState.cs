using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ArmedState : AlarmSystemStateBase
    {
        internal ArmedState(Context context)
            : base(context, AlarmSystemState.Armed)
        {
        }

        private int wrongPinCodeCounter = 0;

        internal override void GotActive()
        {
            wrongPinCodeCounter = 0;
        }

        internal override void Unlock(string pinCode)
        {
            wrongPinCodeCounter++;

            if (Context.checkPinCode(pinCode) == PinCheckResult.CORRECT)
            {
                ChangeStateTo(AlarmSystemState.ClosedAndUnlocked);
            }
            else if (wrongPinCodeCounter >= 3)
            {
                ChangeStateTo(AlarmSystemState.AlarmFlashAndSound);
            }
        }

        internal override void Open()
        {
            ChangeStateTo(AlarmSystemState.AlarmFlashAndSound);
        }
    }
}
