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
        }

        private CancellationTokenSource cancelArmedActivation = new CancellationTokenSource();

        internal override void Unlock()
        {
            cancelArmedActivation.Cancel();
            base.ChangeStateTo(AlarmSystemStateType.ClosedAndUnlocked);
        }

        internal override void Open()
        {
            cancelArmedActivation.Cancel();
            base.ChangeStateTo(AlarmSystemStateType.OpenAndLocked);
        }

        internal override void GotActive()
        {
            base.GotActive();
            Task.Factory.StartNew(armeSystem, cancelArmedActivation.Token);
        }

        private async void armeSystem()
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            ChangeStateTo(AlarmSystemStateType.Armed);
        }
    }
}
