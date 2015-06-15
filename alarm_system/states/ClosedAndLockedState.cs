using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ClosedAndLockedState : AlarmSystemStateBase
    {
        private readonly int switchToArmedTime;

        internal ClosedAndLockedState(Context context, int switchToArmedTime)
            : base(context, AlarmSystemState.ClosedAndLocked)
        {
            this.switchToArmedTime = switchToArmedTime;
            CancelArmedActivation();
        }

        private CancellationTokenSource cancelArmedActivation = null;

        internal override void Unlock(string pinCode)
        {
            if (Context.checkPinCode(pinCode) == PinCheckResult.CORRECT)
            {
                CancelArmedActivation();
                base.ChangeStateTo(AlarmSystemState.ClosedAndUnlocked);
            }
        }

        internal override void Open()
        {
            CancelArmedActivation();
            base.ChangeStateTo(AlarmSystemState.OpenAndLocked);
        }

        internal override void GotActive()
        {
            base.GotActive();
            Task.Factory.StartNew(armeSystem, cancelArmedActivation.Token);
        }

        private async void armeSystem()
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(switchToArmedTime), cancelArmedActivation.Token);
                ChangeStateTo(AlarmSystemState.Armed);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private void CancelArmedActivation()
        {
            if (cancelArmedActivation != null)
            {
                cancelArmedActivation.Cancel();
                cancelArmedActivation.Dispose();
            }

            cancelArmedActivation = new CancellationTokenSource();
        }

        internal override void ShutDown()
        {
            CancelArmedActivation();
        }
    }
}
