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
        internal ArmedState(Context context, int allowedWrongPinCodeCount)
            : base(context, AlarmSystemState.Armed)
        {
            this.ALLOWED_WRONG_PIN_CODE_COUNT = allowedWrongPinCodeCount;
        }

        private int wrongPinCodeCounter = 0;
        private readonly int ALLOWED_WRONG_PIN_CODE_COUNT;

        internal override void Unlock(string pinCode)
        {
            if (Context.CheckPinCode(pinCode) == PinCheckResult.CORRECT)
            {
                wrongPinCodeCounter = 0;
                ChangeStateTo(AlarmSystemState.ClosedAndUnlocked);
                return;
            }
            
            wrongPinCodeCounter++;    
            if (wrongPinCodeCounter >= ALLOWED_WRONG_PIN_CODE_COUNT)
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
