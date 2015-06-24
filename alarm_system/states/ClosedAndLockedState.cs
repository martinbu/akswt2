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
        private CancellationTokenSource cancelArmedActivation = null;
        private readonly TimeSpan switchToArmedTime;

        internal ClosedAndLockedState(Context context, TimeSpan switchToArmedTime)
            : base(context, AlarmSystemState.ClosedAndLocked)
        {
            this.switchToArmedTime = switchToArmedTime;
            CancelArmedActivation();
        }

        internal override void Unlock(string pinCode)
        {
            if (Context.CheckPinCode(pinCode) == PinCheckResult.CORRECT)
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
                await Task.Delay(switchToArmedTime, cancelArmedActivation.Token);
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
