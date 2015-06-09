using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class AlarmFlashAndSoundState : AlarmSystemState
    {
        private readonly int switchToFlashTime;

        internal AlarmFlashAndSoundState(Context context, int switchToFlashTime)
            : base(context, AlarmSystemStateType.AlarmFlashAndSound)
        {
            this.switchToFlashTime = switchToFlashTime;
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
                await Task.Delay(TimeSpan.FromMilliseconds(switchToFlashTime), cancelAlarm.Token);
                ChangeStateTo(AlarmSystemStateType.AlarmFlash);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private void CancelAlarm() {
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
