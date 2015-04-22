using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class ClosedAndLockedState : AlarmSystemState
    {
        internal ClosedAndLockedState(Context context)
            : base(context, AlarmSystemStateType.ClosedAndLocked)
        {
            CancelArmedActivation();
        }

        private CancellationTokenSource cancelArmedActivation = null;

        internal override void Unlock()
        {
            CancelArmedActivation();
            base.ChangeStateTo(AlarmSystemStateType.ClosedAndUnlocked);
        }

        internal override void Open()
        {
            CancelArmedActivation();
            base.ChangeStateTo(AlarmSystemStateType.OpenAndLocked);
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
                await Task.Delay(TimeSpan.FromSeconds(20), cancelArmedActivation.Token);
                ChangeStateTo(AlarmSystemStateType.Armed);
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
