using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class AlarmFlashState : AlarmSystemState
    {
        internal AlarmFlashState(Context context)
            : base(context, AlarmSystemStateType.AlarmFlash)
        {
            CancelAlarm();
        }

        private CancellationTokenSource cancelAlarm = null;

        internal override void Unlock()
        {
            CancelAlarm();
            base.ChangeStateTo(AlarmSystemStateType.OpenAndUnlocked);
        }

        internal override void GotActive()
        {
            base.GotActive();
            Task.Factory.StartNew(turnOffSound, cancelAlarm.Token);
        }

        private async void turnOffSound()
        {
            try {
                await Task.Delay(TimeSpan.FromSeconds(300), cancelAlarm.Token);
                ChangeStateTo(AlarmSystemStateType.SilentAndOpen);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private void CancelAlarm()
        {
            if (cancelAlarm != null)
            {
                cancelAlarm.Cancel();
                cancelAlarm.Dispose();
            }

            cancelAlarm = new CancellationTokenSource();
        }

        internal override void ShutDown()
        {
            CancelAlarm();
        }
    }
}
